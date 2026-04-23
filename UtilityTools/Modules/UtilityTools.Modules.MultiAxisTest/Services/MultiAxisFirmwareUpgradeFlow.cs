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

        public async Task<FirmwareCheckResult> CheckAndUpgradeIfNeededAsync(
            IAsynRWService serialService,
            IAsynRWService netService,
            string indexUrl,
            IDialogHostService dialogHostService,
            string dialogHostName,
            CancellationToken ct = default)
        {
            var result = new FirmwareCheckResult();
            if ((serialService?.IsOpen != true) && (netService?.IsOpen != true))
            {
                result.Message = "设备未连接，跳过固件检查。";
                return result;
            }

            OtaPackageInfo? package = null;
            try
            {
                package = await DownloadLatestPackageAsync(indexUrl, ct);
                result.LatestPackageUrl = package.SourceUrl;
                result.LatestVersionText = package.BoardMessage.VersionNumber ?? string.Empty;

                if (!package.BoardMessage.DeviceID.HasValue)
                {
                    result.Message = "最新固件包未包含有效 DeviceID，无法比对。";
                    return result;
                }

                var deviceVersion = await QueryDeviceVersionAsync(
                    serialService,
                    netService,
                    package.BoardMessage.DeviceID.Value,
                    ct);
                result.CurrentVersionText = deviceVersion.VersionText;

                bool isLatest = IsDeviceVersionLatest(deviceVersion, package);
                result.IsLatest = isLatest;
                if (isLatest)
                {
                    result.Message = $"固件已是最新版本（当前 {result.CurrentVersionText}）。";
                    return result;
                }

                string promptText =
                    $"检测到固件可升级：\n" +
                    $"当前版本：{result.CurrentVersionText} (CRC={deviceVersion.Crc})\n" +
                    $"最新版本：{result.LatestVersionText} (CRC={package.UpdateDataCrc})\n\n" +
                    $"固件地址：{result.LatestPackageUrl}\n\n" +
                    $"是否立即升级？";
                var question = await dialogHostService.Question("固件升级提示", promptText, dialogHostName);
                if (question.Result != Prism.Services.Dialogs.ButtonResult.OK)
                {
                    result.UserSkippedUpgrade = true;
                    result.Message = "用户选择暂不升级。";
                    return result;
                }

                result.UpgradeTriggered = true;
                bool upgraded = await RunUpgradeWorkflowAsync(serialService, netService, package, ct);
                result.UpgradeSucceeded = upgraded;
                if (upgraded)
                {
                    var refreshed = await QueryDeviceVersionAsync(serialService, netService, package.BoardMessage.DeviceID.Value, ct);
                    result.CurrentVersionText = refreshed.VersionText;
                    result.IsLatest = true;
                }
                result.Message = upgraded ? "固件升级成功。" : "固件升级失败，请查看日志。";
                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                result.Message = $"固件检查异常：{ex.Message}";
                return result;
            }
            finally
            {
                TryDeleteTempPackage(package);
            }
        }

        private async Task<OtaPackageInfo> DownloadLatestPackageAsync(string indexUrl, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(indexUrl))
                throw new InvalidOperationException("固件索引地址为空。");

            var baseUri = new Uri(indexUrl);
            var html = await _httpClient.GetStringAsync(baseUri, ct);
            var links = ExtractLinks(html)
                .Select(link => BuildAbsoluteUri(baseUri, link))
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
            string tempZip = Path.Combine(Path.GetTempPath(), $"multi_axis_fw_{Guid.NewGuid():N}.zip");
            using var response = await _httpClient.GetAsync(selected, ct);
            response.EnsureSuccessStatusCode();
            await using (var fs = File.Create(tempZip))
            {
                await response.Content.CopyToAsync(fs, ct);
            }

            var package = ParsePackage(tempZip);
            package.SourceUrl = selected.AbsoluteUri;
            return package;
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
            using var archive = ZipFile.OpenRead(zipPath);
            var msgEntry = archive.Entries.FirstOrDefault(e =>
                e.FullName.EndsWith("DevelopmentBoardMessage.json", StringComparison.OrdinalIgnoreCase));
            if (msgEntry == null)
                throw new InvalidOperationException("固件包缺少 DevelopmentBoardMessage.json。");

            DevelopmentBoardMessage? msg;
            using (var sr = new StreamReader(msgEntry.Open()))
            {
                var json = sr.ReadToEnd();
                msg = JsonSerializer.Deserialize<DevelopmentBoardMessage>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            if (msg == null || string.IsNullOrWhiteSpace(msg.FileName))
                throw new InvalidOperationException("升级信息文件无效，无法读取固件文件名。");

            var fwEntry = archive.Entries.FirstOrDefault(e =>
                e.FullName.EndsWith(msg.FileName, StringComparison.OrdinalIgnoreCase));
            if (fwEntry == null)
                throw new InvalidOperationException($"固件包缺少固件文件：{msg.FileName}");

            byte[] rawData;
            using (var ms = new MemoryStream())
            {
                using var s = fwEntry.Open();
                s.CopyTo(ms);
                rawData = ms.ToArray();
            }

            uint maxFrameCount = (uint)((rawData.Length + 31) / 32);
            var updateData = new byte[maxFrameCount * 32];
            Array.Fill(updateData, (byte)0xFF);
            Array.Copy(rawData, updateData, rawData.Length);
            var crc = CRCHelper.Data_GetCRC16(updateData, 0, updateData.Length);

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

        private async Task<bool> RunUpgradeWorkflowAsync(
            IAsynRWService serialService,
            IAsynRWService netService,
            OtaPackageInfo package,
            CancellationToken ct)
        {
            if (!package.BoardMessage.DeviceID.HasValue)
                throw new InvalidOperationException("固件包 DeviceID 为空，无法升级。");

            int deviceId = package.BoardMessage.DeviceID.Value;
            using var session = new OtaSession(serialService, netService);

            var request = OtaProtocol.GetRequestOtaCmd((uint)package.UpdateData.Length, package.MaxFrameCount, deviceId, package.UpdateDataCrc);
            await session.SendAsync(request, ct);
            await session.WaitPacketAsync(EnumOtaCommandType.OTA_REQUEST, 5000, ct);

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
                        throw new InvalidOperationException($"升级帧确认异常，期望={i}，实际={ackId}");
                }
            }

            var restart = OtaProtocol.GetRestartCmd(deviceId);
            await session.SendAsync(restart, ct);
            await session.WaitPacketAsync(EnumOtaCommandType.OTA_RESTART, 5000, ct);

            await Task.Delay(1500, ct);
            var after = await QueryDeviceVersionAsync(serialService, netService, deviceId, ct);
            return IsDeviceVersionLatest(after, package);
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
