using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Extension;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Interface;
using UtilityTools.Modules.MotorTest.Service;
using UtilityTools.Modules.OtaTool.Protocol;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Modules.MultiAxisTest.Services
{
    internal sealed class OtaPackageInfo
    {
        public string SourceUrl { get; set; } = string.Empty;
        public string LocalZipPath { get; set; } = string.Empty;
        public DevelopmentBoardMessage BoardMessage { get; set; } = new DevelopmentBoardMessage();
        public byte[] UpdateData { get; set; } = Array.Empty<byte>();
        public uint MaxFrameCount { get; set; }
        public ushort UpdateDataCrc { get; set; }
    }

    internal sealed class DeviceVersionInfo
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public ushort Crc { get; set; }
        public string VersionText => $"v{Major}.{Minor}";
    }

    /// <summary>
    /// 多轴测试内独立固件流程：
    /// 目录解析 -> 版本比对 -> 人工确认 -> OTA 执行 -> 复核。
    /// </summary>
    public class MultiAxisFirmwareUpgradeFlow : IFirmwareUpgradeCoordinator
    {
        private readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(20)
        };
        private static readonly object UpgradeFlowLogLock = new();

        public async Task<FirmwareCheckResult> CheckAndUpgradeIfNeededAsync(
            IAsynRWService serialService,
            IAsynRWService netService,
            string indexUrl,
            IDialogHostService dialogHostService,
            string dialogHostName,
            CancellationToken ct = default)
        {
            string flowLogPath = CreateUpgradeFlowLogPath();
            var result = new FirmwareCheckResult();
            WriteUpgradeFlowLog(flowLogPath, $"流程开始：indexUrl={indexUrl}");
            if ((serialService?.IsOpen != true) && (netService?.IsOpen != true))
            {
                result.Message = "设备未连接，跳过固件检查。";
                WriteUpgradeFlowLog(flowLogPath, "设备未连接，流程结束。");
                return result;
            }

            OtaPackageInfo? package = null;
            try
            {
                WriteUpgradeFlowLog(flowLogPath, "步骤1：下载并解析最新升级包。");
                package = await DownloadLatestPackageAsync(indexUrl, ct);
                WriteUpgradeFlowLog(flowLogPath,
                    $"升级包解析完成：sourceUrl={package.SourceUrl}, firmwareName={package.BoardMessage.FileName}, updateDataLength={package.UpdateData.Length}, frameCount={package.MaxFrameCount}, crc={package.UpdateDataCrc}");
                result.LatestPackageUrl = package.SourceUrl;
                result.LatestVersionText = package.BoardMessage.VersionNumber ?? string.Empty;

                if (!package.BoardMessage.DeviceID.HasValue)
                {
                    result.Message = "最新固件包未包含有效 DeviceID，无法比对。";
                    WriteUpgradeFlowLog(flowLogPath, "升级包缺少 DeviceID，流程终止。");
                    return result;
                }

                WriteUpgradeFlowLog(flowLogPath, $"步骤2：查询设备当前版本，deviceId={package.BoardMessage.DeviceID.Value}");
                var deviceVersion = await QueryDeviceVersionAsync(
                    serialService,
                    netService,
                    package.BoardMessage.DeviceID.Value,
                    ct);
                result.CurrentVersionText = deviceVersion.VersionText;
                WriteUpgradeFlowLog(flowLogPath,
                    $"设备版本查询完成：currentVersion={result.CurrentVersionText}, currentCrc={deviceVersion.Crc}");

                bool isLatest = IsDeviceVersionLatest(deviceVersion, package);
                result.IsLatest = isLatest;
                if (isLatest)
                {
                    result.Message = $"固件已是最新版本（当前 {result.CurrentVersionText}）。";
                    WriteUpgradeFlowLog(flowLogPath, "比对结果：设备已是最新版本，流程结束。");
                    return result;
                }

                string displayUrl = BuildDisplayUrl(result.LatestPackageUrl);
                string promptText =
                    $"检测到固件可升级：\n" +
                    $"当前版本：{result.CurrentVersionText} (CRC={deviceVersion.Crc})\n" +
                    $"最新版本：{result.LatestVersionText} (CRC={package.UpdateDataCrc})\n\n" +
                    $"固件地址：{displayUrl}\n\n" +
                    $"是否立即升级？";
                var question = await dialogHostService.Question("固件升级提示", promptText, dialogHostName);
                if (question.Result != Prism.Services.Dialogs.ButtonResult.OK)
                {
                    result.UserSkippedUpgrade = true;
                    result.Message = "用户选择暂不升级。";
                    WriteUpgradeFlowLog(flowLogPath, "用户取消升级，流程结束。");
                    return result;
                }

                result.UpgradeTriggered = true;
                WriteUpgradeFlowLog(flowLogPath, "步骤3：开始执行 OTA 升级流程。");
                var upgradeOutcome = await RunUpgradeWorkflowAsync(serialService, netService, package, ct);
                bool upgraded = upgradeOutcome.Success;
                result.UpgradeSucceeded = upgraded;
                if (upgraded)
                {
                    WriteUpgradeFlowLog(flowLogPath, "步骤4：升级完成，复查设备版本。");
                    var refreshed = await QueryDeviceVersionAsync(serialService, netService, package.BoardMessage.DeviceID.Value, ct);
                    result.CurrentVersionText = refreshed.VersionText;
                    result.IsLatest = true;
                    WriteUpgradeFlowLog(flowLogPath,
                        $"复查完成：currentVersion={result.CurrentVersionText}, currentCrc={refreshed.Crc}");
                }
                result.Message = upgraded ? "固件升级成功。" : $"[升级后复核失败] {upgradeOutcome.Reason}";
                WriteUpgradeFlowLog(flowLogPath, $"流程结束：upgraded={upgraded}, message={result.Message}");
                return result;
            }
            catch (OperationCanceledException)
            {
                WriteUpgradeFlowLog(flowLogPath, "流程取消：OperationCanceledException。");
                throw;
            }
            catch (Exception ex)
            {
                result.Message = BuildClassifiedFailureMessage(ex);
                WriteUpgradeFlowLog(flowLogPath, $"流程异常：{ex}");
                return result;
            }
            finally
            {
                TryDeleteTempPackage(package);
                WriteUpgradeFlowLog(flowLogPath, "流程收尾：临时包清理结束。");
            }
        }

        private async Task<OtaPackageInfo> DownloadLatestPackageAsync(string indexUrl, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(indexUrl))
                throw new InvalidOperationException("固件索引地址为空。");

            string tempZip = Path.Combine(Path.GetTempPath(), $"multi_axis_fw_{Guid.NewGuid():N}.zip");
            string sourceTag;

            bool isHttp =
                Uri.TryCreate(indexUrl, UriKind.Absolute, out var indexUri) &&
                (string.Equals(indexUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                 || string.Equals(indexUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase));
            if (isHttp)
            {
                var html = await _httpClient.GetStringAsync(indexUri!, ct);
                var links = ExtractLinks(html)
                    .Select(link => BuildAbsoluteUri(indexUri!, link))
                    .Where(uri => uri != null && uri.AbsoluteUri.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    .Select(uri => uri!)
                    .Distinct()
                    .ToList();

                if (links.Count == 0)
                    throw new InvalidOperationException($"目录未找到 zip 固件包：{indexUrl}");

                var ordered = links
                    .Select(uri => new
                    {
                        Uri = uri,
                        Version = TryParseVersionFromName(Uri.UnescapeDataString(Path.GetFileName(uri.LocalPath)))
                    })
                    .OrderByDescending(x => x.Version != null)
                    .ThenByDescending(x => x.Version)
                    .ThenByDescending(x => x.Uri.AbsoluteUri)
                    .ToList();

                var selected = ordered.First().Uri;
                using var response = await _httpClient.GetAsync(selected, ct);
                response.EnsureSuccessStatusCode();
                await using (var fs = File.Create(tempZip))
                {
                    await response.Content.CopyToAsync(fs, ct);
                }
                sourceTag = selected.AbsoluteUri;
            }
            else
            {
                string directory = NormalizeDirectoryPath(indexUrl);
                if (!Directory.Exists(directory))
                    throw new InvalidOperationException($"固件目录不存在：{directory}");

                var candidates = Directory.EnumerateFiles(directory, "*.zip", SearchOption.TopDirectoryOnly)
                    .Select(path => new
                    {
                        Path = path,
                        Version = TryParseVersionFromName(Path.GetFileName(path)),
                        LastWrite = File.GetLastWriteTimeUtc(path)
                    })
                    .OrderByDescending(x => x.Version != null)
                    .ThenByDescending(x => x.Version)
                    .ThenByDescending(x => x.LastWrite)
                    .ThenByDescending(x => x.Path)
                    .ToList();

                if (candidates.Count == 0)
                    throw new InvalidOperationException($"目录未找到 zip 固件包：{directory}");

                var selectedPath = candidates.First().Path;
                File.Copy(selectedPath, tempZip, true);
                sourceTag = selectedPath;
            }

            var package = ParsePackage(tempZip);
            package.SourceUrl = sourceTag;
            return package;
        }

        private static string NormalizeDirectoryPath(string path)
        {
            if (Uri.TryCreate(path, UriKind.Absolute, out var uri) && uri.IsFile)
                return uri.LocalPath;
            return path.Trim().TrimEnd('\\', '/');
        }

        private static IEnumerable<string> ExtractLinks(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                yield break;

            var regex = new Regex("href\\s*=\\s*['\\\"](?<u>[^'\\\"]+)['\\\"]", RegexOptions.IgnoreCase);
            foreach (Match m in regex.Matches(html))
            {
                var link = m.Groups["u"].Value.Trim();
                if (string.IsNullOrWhiteSpace(link) || link.StartsWith("#") || link.StartsWith(".."))
                    continue;
                yield return link;
            }
        }

        private static Uri? BuildAbsoluteUri(Uri baseUri, string href)
        {
            try
            {
                if (Uri.TryCreate(href, UriKind.Absolute, out var abs))
                    return abs;
                return new Uri(baseUri, href);
            }
            catch
            {
                return null;
            }
        }

        private static string BuildDisplayUrl(string rawUrl)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
                return "--";
            string decoded = Uri.UnescapeDataString(rawUrl);
            if (decoded.Length <= 96)
                return decoded;

            string fileName = Path.GetFileName(decoded);
            if (!string.IsNullOrWhiteSpace(fileName))
                return $".../{fileName}";
            return decoded.Substring(0, 96) + "...";
        }

        private static Version? TryParseVersionFromName(string fileName)
        {
            var match = Regex.Match(fileName ?? string.Empty, @"(?i)v?(?<a>\d+)\.(?<b>\d+)(?:\.(?<c>\d+))?");
            if (!match.Success) return null;
            int a = int.Parse(match.Groups["a"].Value);
            int b = int.Parse(match.Groups["b"].Value);
            int c = match.Groups["c"].Success ? int.Parse(match.Groups["c"].Value) : 0;
            return new Version(a, b, c);
        }

        private static OtaPackageInfo ParsePackage(string zipPath)
        {
            string flowLogPath = CreateUpgradeFlowLogPath();
            WriteUpgradeFlowLog(flowLogPath, $"解析升级包开始：zipPath={zipPath}");
            using var archive = ZipFile.OpenRead(zipPath);
            var msgEntry = archive.Entries.FirstOrDefault(e =>
                e.FullName.EndsWith("DevelopmentBoardMessage.json", StringComparison.OrdinalIgnoreCase));
            if (msgEntry == null)
            {
                WriteUpgradeFlowLog(flowLogPath, "解析失败：缺少 DevelopmentBoardMessage.json");
                throw new InvalidOperationException("固件包缺少 DevelopmentBoardMessage.json。");
            }

            DevelopmentBoardMessage? msg;
            using (var sr = new StreamReader(msgEntry.Open()))
            {
                var json = sr.ReadToEnd();
                WriteUpgradeFlowLog(flowLogPath, $"读取升级描述成功：jsonLength={json.Length}");
                msg = JsonSerializer.Deserialize<DevelopmentBoardMessage>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            if (msg == null || string.IsNullOrWhiteSpace(msg.FileName))
            {
                WriteUpgradeFlowLog(flowLogPath, "解析失败：升级描述无效或缺少 FileName");
                throw new InvalidOperationException("升级信息文件无效，无法读取固件文件名。");
            }
            WriteUpgradeFlowLog(flowLogPath,
                $"升级描述解析：fileName={msg.FileName}, version={msg.VersionNumber}, deviceId={msg.DeviceID}");

            var fwEntry = archive.Entries.FirstOrDefault(e =>
                e.FullName.EndsWith(msg.FileName, StringComparison.OrdinalIgnoreCase));
            if (fwEntry == null)
            {
                WriteUpgradeFlowLog(flowLogPath, $"解析失败：zip 中未找到固件文件 {msg.FileName}");
                throw new InvalidOperationException($"固件包缺少固件文件：{msg.FileName}");
            }

            byte[] rawData;
            using (var ms = new MemoryStream())
            {
                using var s = fwEntry.Open();
                s.CopyTo(ms);
                rawData = ms.ToArray();
            }
            WriteUpgradeFlowLog(flowLogPath, $"固件文件读取成功：fileName={msg.FileName}, rawLength={rawData.Length}");

            uint maxFrameCount = (uint)((rawData.Length + 31) / 32);
            var updateData = new byte[maxFrameCount * 32];
            Array.Fill(updateData, (byte)0xFF);
            Array.Copy(rawData, updateData, rawData.Length);
            var crc = CRCHelper.Data_GetCRC16(updateData, 0, updateData.Length);
            WriteUpgradeFlowLog(flowLogPath,
                $"解析升级包完成：paddedLength={updateData.Length}, maxFrameCount={maxFrameCount}, crc={crc}");

            return new OtaPackageInfo
            {
                LocalZipPath = zipPath,
                BoardMessage = msg,
                UpdateData = updateData,
                MaxFrameCount = maxFrameCount,
                UpdateDataCrc = crc
            };
        }

        private static bool IsDeviceVersionLatest(DeviceVersionInfo device, OtaPackageInfo package)
        {
            Version? deviceVersion = new Version(device.Major, device.Minor);
            Version? packageVersion = TryParseVersionFromName(package.BoardMessage.VersionNumber ?? string.Empty);
            if (packageVersion != null && deviceVersion != null)
            {
                if (deviceVersion.CompareTo(new Version(packageVersion.Major, packageVersion.Minor)) >= 0
                    && device.Crc == package.UpdateDataCrc)
                {
                    return true;
                }
            }

            return device.Crc == package.UpdateDataCrc;
        }

        private static void TryDeleteTempPackage(OtaPackageInfo? package)
        {
            try
            {
                if (package != null && !string.IsNullOrWhiteSpace(package.LocalZipPath) && File.Exists(package.LocalZipPath))
                    File.Delete(package.LocalZipPath);
            }
            catch
            {
            }
        }

        private async Task<DeviceVersionInfo> QueryDeviceVersionAsync(
            IAsynRWService serialService,
            IAsynRWService netService,
            int deviceId,
            CancellationToken ct)
        {
            using var session = new OtaSession(serialService, netService);
            var cmd = OtaProtocol.GetUpdateFrameInfoCmd(deviceId);
            await session.SendAsync(cmd, ct);
            var packet = await session.WaitPacketAsync(EnumOtaCommandType.OTA_GET_UPGRADE_FMV, 5000, ct);
            return ParseDeviceVersion(packet);
        }

        private static DeviceVersionInfo ParseDeviceVersion(OtaToolDataPacket packet)
        {
            if (packet.DataSource == null || packet.DataSource.Length < 8)
                throw new InvalidOperationException("设备版本回包长度不足。");

            return new DeviceVersionInfo
            {
                Major = packet.DataSource[0],
                Minor = packet.DataSource[1],
                Crc = BitConverter.ToUInt16(packet.DataSource, 6)
            };
        }

        private async Task<(bool Success, string Reason)> RunUpgradeWorkflowAsync(
            IAsynRWService serialService,
            IAsynRWService netService,
            OtaPackageInfo package,
            CancellationToken ct)
        {
            string flowLogPath = CreateUpgradeFlowLogPath();
            if (!package.BoardMessage.DeviceID.HasValue)
                throw new InvalidOperationException("固件包 DeviceID 为空，无法升级。");

            int deviceId = package.BoardMessage.DeviceID.Value;
            WriteUpgradeFlowLog(flowLogPath,
                $"升级流程开始：deviceId={deviceId}, updateDataLength={package.UpdateData.Length}, frameCount={package.MaxFrameCount}, crc={package.UpdateDataCrc}");
            using var session = new OtaSession(serialService, netService);

            var request = OtaProtocol.GetRequestOtaCmd((uint)package.UpdateData.Length, package.MaxFrameCount, deviceId, package.UpdateDataCrc);
            await session.SendAsync(request, ct);
            await session.WaitPacketAsync(EnumOtaCommandType.OTA_REQUEST, 5000, ct);
            WriteUpgradeFlowLog(flowLogPath, "请求升级通过：收到 OTA_REQUEST 回包。");

            for (uint i = 0; i < package.MaxFrameCount; i++)
            {
                var data = new byte[32];
                Array.Copy(package.UpdateData, i * 32, data, 0, 32);
                var transfer = OtaProtocol.GetTransferOtaCmd(i, data, deviceId);
                await session.SendAsync(transfer, ct);
                var ack = await session.WaitPacketAsync(EnumOtaCommandType.OTA_TRANSFER, 5000, ct);
                if (ack.DataSource.Length >= 4)
                {
                    uint ackId = BitConverter.ToUInt32(ack.DataSource, 0);
                    if (ackId != i)
                    {
                        WriteUpgradeFlowLog(flowLogPath, $"升级帧确认异常：expected={i}, actual={ackId}");
                        throw new InvalidOperationException($"升级帧确认异常，期望={i}，实际={ackId}");
                    }
                }
                if (i < 3 || i % 100 == 0 || i + 1 == package.MaxFrameCount)
                    WriteUpgradeFlowLog(flowLogPath, $"升级帧进度：{i + 1}/{package.MaxFrameCount}");
            }

            var restart = OtaProtocol.GetRestartCmd(deviceId);
            await session.SendAsync(restart, ct);
            bool restartAckReceived = false;
            try
            {
                // 部分设备收到重启命令后会立即复位，可能来不及回 ACK。
                // 因此这里采用“软等待”：ACK 超时不直接判失败，后续以版本/CRC复核为准。
                await session.WaitPacketAsync(EnumOtaCommandType.OTA_RESTART, 2000, ct);
                restartAckReceived = true;
                WriteUpgradeFlowLog(flowLogPath, "重启命令发送并确认完成（收到 OTA_RESTART ACK）。");
            }
            catch (TimeoutException)
            {
                WriteUpgradeFlowLog(flowLogPath, "重启 ACK 超时：未收到 OTA_RESTART 回包，继续执行重启后复核。");
            }

            // 对齐旧 OTA 流程：重启后进行多次复查，不依赖单次查询结果。
            var (latest, finalVersion) = await VerifyAfterRestartWithRetriesAsync(
                serialService,
                netService,
                deviceId,
                package,
                flowLogPath,
                ct);
            WriteUpgradeFlowLog(flowLogPath,
                $"升级后最终复核：version={finalVersion.VersionText}, crc={finalVersion.Crc}, isLatest={latest}, restartAckReceived={restartAckReceived}");
            if (latest)
                return (true, "升级并复核通过。");

            string reason =
                $"重启后复核未通过：设备版本={finalVersion.VersionText}, 设备CRC={finalVersion.Crc}, 目标CRC={package.UpdateDataCrc}。";
            return (false, reason);
        }

        private async Task<(bool isLatest, DeviceVersionInfo finalVersion)> VerifyAfterRestartWithRetriesAsync(
            IAsynRWService serialService,
            IAsynRWService netService,
            int deviceId,
            OtaPackageInfo package,
            string flowLogPath,
            CancellationToken ct)
        {
            const int maxAttempts = 10;
            const int intervalMs = 1000;
            DeviceVersionInfo? lastVersion = null;
            Exception? lastException = null;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                ct.ThrowIfCancellationRequested();
                if (attempt > 1)
                    await Task.Delay(intervalMs, ct);

                try
                {
                    var version = await QueryDeviceVersionAsync(serialService, netService, deviceId, ct);
                    lastVersion = version;
                    bool latest = IsDeviceVersionLatest(version, package);
                    WriteUpgradeFlowLog(flowLogPath,
                        $"重启后复核第{attempt}/{maxAttempts}次：version={version.VersionText}, crc={version.Crc}, isLatest={latest}");
                    if (latest)
                        return (true, version);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    lastException = ex;
                    WriteUpgradeFlowLog(flowLogPath,
                        $"重启后复核第{attempt}/{maxAttempts}次失败：{ex.Message}");
                }
            }

            if (lastVersion != null)
                return (false, lastVersion);

            throw new InvalidOperationException(
                $"重启后复核连续失败（{maxAttempts}次），最后异常：{lastException?.Message ?? "未知错误"}",
                lastException);
        }

        private static string CreateUpgradeFlowLogPath()
        {
            string dir = Path.Combine(AppContext.BaseDirectory, "报告", "固件升级", "流程日志");
            Directory.CreateDirectory(dir);
            string file = $"固件升级流程_{DateTime.Now:yyyyMMdd}.log";
            return Path.Combine(dir, file);
        }

        private static void WriteUpgradeFlowLog(string filePath, string message)
        {
            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
            lock (UpgradeFlowLogLock)
            {
                File.AppendAllText(filePath, line + Environment.NewLine);
            }
        }

        private static string BuildClassifiedFailureMessage(Exception ex)
        {
            string msg = ex.Message ?? "未知错误";
            if (ex is TimeoutException)
                return $"[通信超时] {msg}";
            if (ex is HttpRequestException)
                return $"[下载失败] {msg}";
            if (ex is IOException || ex is UnauthorizedAccessException)
                return $"[文件读写失败] {msg}";
            if (ex is InvalidOperationException)
                return $"[协议或流程异常] {msg}";
            return $"[未知异常] {msg}";
        }

        private sealed class OtaSession : IDisposable
        {
            private readonly IAsynRWService _serialService;
            private readonly IAsynRWService _netService;
            private readonly OtaToolProtocolParser _parserSerial;
            private readonly OtaToolProtocolParser _parserNet;
            private TaskCompletionSource<OtaToolDataPacket>? _waiter;
            private EnumOtaCommandType _expectedCmdType;

            public OtaSession(IAsynRWService serialService, IAsynRWService netService)
            {
                _serialService = serialService;
                _netService = netService;
                _parserSerial = new OtaToolProtocolParser { Service = serialService };
                _parserNet = new OtaToolProtocolParser { Service = netService };
                _parserSerial.PacketReceivedEvent += OnPacketReceived;
                _parserNet.PacketReceivedEvent += OnPacketReceived;
                _serialService.UpdateResponse += SerialUpdateResponse;
                _netService.UpdateResponse += NetUpdateResponse;
            }

            public Task SendAsync(byte[] cmd, CancellationToken ct)
            {
                if (_netService?.IsOpen == true)
                {
                    _netService.SendMsg(cmd);
                    return Task.CompletedTask;
                }
                if (_serialService?.IsOpen == true)
                {
                    _serialService.SendMsg(cmd);
                    return Task.CompletedTask;
                }
                throw new InvalidOperationException("未连接任何通信链路，无法发送升级命令。");
            }

            public async Task<OtaToolDataPacket> WaitPacketAsync(EnumOtaCommandType cmdType, int timeoutMs, CancellationToken ct)
            {
                _expectedCmdType = cmdType;
                _waiter = new TaskCompletionSource<OtaToolDataPacket>(TaskCreationOptions.RunContinuationsAsynchronously);

                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeoutCts.CancelAfter(timeoutMs);
                using var reg = timeoutCts.Token.Register(() =>
                {
                    _waiter?.TrySetException(new TimeoutException($"等待回包超时：{cmdType}"));
                });

                return await _waiter.Task;
            }

            private void OnPacketReceived(object? sender, OtaToolDataPacket packet)
            {
                if (_waiter == null) return;
                if (packet.CmdType != _expectedCmdType) return;
                _waiter.TrySetResult(packet);
            }

            private void SerialUpdateResponse(object? sender, byte[] e) => _parserSerial.ReceiveBytes(e);
            private void NetUpdateResponse(object? sender, byte[] e) => _parserNet.ReceiveBytes(e);

            public void Dispose()
            {
                _serialService.UpdateResponse -= SerialUpdateResponse;
                _netService.UpdateResponse -= NetUpdateResponse;
                _parserSerial.PacketReceivedEvent -= OnPacketReceived;
                _parserNet.PacketReceivedEvent -= OnPacketReceived;
            }
        }
    }
}
