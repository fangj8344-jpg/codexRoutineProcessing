using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Extension;
using UtilityTools.Core.Interface;
using UtilityTools.Modules.MotorTest.Model;
using UtilityTools.Modules.MotorTest.Service;
using UtilityTools.Modules.MotorTest.Views;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.Services.Services;

namespace UtilityTools.Modules.MotorTest.ViewModels
{
    public class ThreeAxisTestModelWindowsViewModel : BindableBase
    {
        public ThreeAxisTestModelWindowsViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService)
        {
            _containerProvider = containerProvider;
            _dialogHostService = dialogHostService;
            try { _firmwareUpgradeCoordinator = _containerProvider.Resolve<IFirmwareUpgradeCoordinator>(); } catch { }
            try { _firmwareWorkflowState = _containerProvider.Resolve<IFirmwareUpgradeWorkflowState>(); } catch { }
            InitProperty();
            InitCommand();
        }
    


        #region ------------Field------------
        private readonly IDialogHostService _dialogHostService;
        private IContainerProvider _containerProvider;
        private readonly IFirmwareUpgradeCoordinator? _firmwareUpgradeCoordinator;
        private readonly IFirmwareUpgradeWorkflowState? _firmwareWorkflowState;
        private bool _firstFlashRunning;
        private bool _upgradeFirmwareRunning;
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        private const string StToolBaseUrl = "http://60.173.16.125:9090/%E5%AE%89%E8%A3%85%E6%96%87%E4%BB%B6/st%E5%B7%A5%E5%85%B7/STM32%20ST-LINK%20Utility/STM32%20ST-LINK%20Utility/";
        private const string FirmwareDirUrl = "http://60.173.16.125:9090/%E5%AE%89%E8%A3%85%E6%96%87%E4%BB%B6/st%E5%B7%A5%E5%85%B7/%E7%94%B5%E6%9C%BA%E5%9B%BA%E4%BB%B6/";
        private static readonly object FirstFlashLogLock = new();
        public string name = "scccc";
        #endregion

        #region ------------Property------------
        private bool _firstFlashInProgress;
        public bool FirstFlashInProgress
        {
            get => _firstFlashInProgress;
            set => SetProperty(ref _firstFlashInProgress, value);
        }

        private double _firstFlashProgressValue;
        public double FirstFlashProgressValue
        {
            get => _firstFlashProgressValue;
            set => SetProperty(ref _firstFlashProgressValue, value);
        }

        private string _firstFlashStatusText = "首刷待命";
        public string FirstFlashStatusText
        {
            get => _firstFlashStatusText;
            set => SetProperty(ref _firstFlashStatusText, value);
        }

        private bool _isConnected;
        /// <summary>
        /// 设备是否连接
        /// </summary>
        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                RaisePropertyChanged();
            }
        }

        private bool _netIsConnected;
        /// <summary>
        /// 网络设备是否连接
        /// </summary>
        public bool NetIsConnected
        {
            get { return _netIsConnected; }
            set
            {
                _netIsConnected = value;
                RaisePropertyChanged();
            }
        }

        private ThreeAxisTestModel _model;

        public ThreeAxisTestModel Model
        {
            get { return _model; }
            set { _model = value; RaisePropertyChanged(); }
        }


        #endregion

        #region ------------Command------------
        public DelegateCommand ShowDeviceCommand { get; set; }

        public DelegateCommand ShowNetDeviceCommand { get; set; }

        public DelegateCommand FirstFlashFirmwareCommand { get; set; }
        public DelegateCommand UpgradeFirmwareCommand { get; set; }
        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        /// <summary>
        /// 初始化指令
        /// </summary>
        private void InitCommand()
        {
            ShowDeviceCommand = new DelegateCommand(ShowDevice);
            ShowNetDeviceCommand = new DelegateCommand(ShowNetDevice);
            FirstFlashFirmwareCommand = new DelegateCommand(async () => await RunFirstFlashFirmwareAsync(), () => !_firstFlashRunning)
                .ObservesProperty(() => IsConnected)
                .ObservesProperty(() => NetIsConnected);
            UpgradeFirmwareCommand = new DelegateCommand(async () => await RunZipFirmwareUpgradeAsync(), () => !_upgradeFirmwareRunning)
                .ObservesProperty(() => IsConnected)
                .ObservesProperty(() => NetIsConnected);
        }

        /// <summary>
        /// 初始化属性
        /// </summary>
        private void InitProperty()
        {
            Model = new ThreeAxisTestModel(_containerProvider);
        }

        /// <summary>
        /// 显示设备连接弹窗
        /// </summary>
        private async void ShowDevice()
        {
            DialogParameters parameter = new DialogParameters();
            parameter.Add("Value", Model.SerialPortService);
            var diaglogResult = await this._dialogHostService.ShowDialog("SerialPortView", parameter, CommonModel.ThreeAxisTestModelWindowsViewModelRegionName);
            if (diaglogResult == null)
                return;
            if (diaglogResult.Result == ButtonResult.OK && diaglogResult.Parameters.ContainsKey("Value"))
            {
                var value = diaglogResult.Parameters.GetValue<IAsynRWService>("Value");
                if (value is SerialPortService serialPortValue)
                {
                    Model.SerialPortService = serialPortValue;
                    IsConnected = Model.SerialPortService.IsOpen;
                    if (IsConnected == true)
                    {
                        Model.QueryStatusTask();
                    }
                    else
                    {
                        Model.CloseQueryStatusTask();
                    }
                }
            }
        }

        /// <summary>
        /// 显示设备连接弹窗
        /// </summary>
        private async void ShowNetDevice()
        {
            DialogParameters parameter = new DialogParameters();
            parameter.Add("Value", Model.NetUdpService);
            var diaglogResult = await this._dialogHostService.ShowDialog("NetConfigView", parameter, CommonModel.ThreeAxisTestModelWindowsViewModelRegionName);
            if (diaglogResult == null)
                return;
            if (diaglogResult.Result == ButtonResult.OK && diaglogResult.Parameters.ContainsKey("Value"))
            {
                var value = diaglogResult.Parameters.GetValue<IAsynRWService>("Value");
                if (value is UdpNetAsyncDevice udpValue)
                {

                    Model.NetUdpService = udpValue;
                    NetIsConnected = Model.NetUdpService.IsOpen;
                    if (NetIsConnected == true)
                    {
                        Model.QueryStatusTask();
                    }
                    else
                    {
                        Model.CloseQueryStatusTask();
                    }
                }
            }
        }

        private async Task RunFirstFlashFirmwareAsync()
        {
            if (_firstFlashRunning)
                return;

            _firstFlashRunning = true;
            FirstFlashFirmwareCommand.RaiseCanExecuteChanged();
            string flowLogPath = CreateFirstFlashLogPath();
            FirstFlashInProgress = true;
            UpdateFirstFlashProgress(2, "开始首刷流程...");
            WriteFirstFlashLog(flowLogPath, "========== 首刷流程开始 ==========");

            try
            {
                string toolRoot = Path.Combine(AppContext.BaseDirectory, "Third", "STTools");
                WriteFirstFlashLog(flowLogPath, $"工具根目录: {toolRoot}");
                UpdateFirstFlashProgress(20, "检查本地刷写工具...");

                string cliPath = await EnsureStLinkToolAsync(toolRoot, flowLogPath);
                WriteFirstFlashLog(flowLogPath, $"CLI 路径: {cliPath}");
                UpdateFirstFlashProgress(45, "刷写工具准备完成。");

                string hexPath = await DownloadFirmwareEveryTimeAsync(toolRoot, flowLogPath);
                WriteFirstFlashLog(flowLogPath, $"HEX 路径: {hexPath}");
                UpdateFirstFlashProgress(65, "固件下载完成。");

                if (string.IsNullOrWhiteSpace(cliPath) || string.IsNullOrWhiteSpace(hexPath))
                {
                    WriteFirstFlashLog(flowLogPath, "流程终止: 工具或固件文件路径为空。");
                    UpdateFirstFlashProgress(100, "首刷准备失败。");
                    await _dialogHostService.Information(
                        "首刷固件",
                        "首刷失败",
                        CommonModel.ThreeAxisTestModelWindowsViewModelRegionName);
                    return;
                }

                string args = $"-c SWD UR -P \"{hexPath}\" -V -Rst";
                var startInfo = new ProcessStartInfo
                {
                    FileName = cliPath,
                    Arguments = args,
                    WorkingDirectory = Path.GetDirectoryName(cliPath) ?? AppContext.BaseDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                WriteFirstFlashLog(flowLogPath, $"执行命令: {startInfo.FileName} {startInfo.Arguments}");
                UpdateFirstFlashProgress(75, "开始执行 ST-LINK 刷写...");

                using var process = new Process { StartInfo = startInfo };
                process.Start();
                string stdOut = await process.StandardOutput.ReadToEndAsync();
                string stdErr = await process.StandardError.ReadToEndAsync();
                await Task.Run(() => process.WaitForExit());
                UpdateFirstFlashProgress(95, "刷写命令执行完成，整理结果...");
                WriteFirstFlashLog(flowLogPath, $"CLI 退出码: {process.ExitCode}");
                if (!string.IsNullOrWhiteSpace(stdOut))
                    WriteFirstFlashLog(flowLogPath, $"CLI 标准输出:\n{stdOut}");
                if (!string.IsNullOrWhiteSpace(stdErr))
                    WriteFirstFlashLog(flowLogPath, $"CLI 错误输出:\n{stdErr}");

                string logText = string.IsNullOrWhiteSpace(stdErr) ? stdOut : $"{stdOut}\n{stdErr}";
                string showText = string.IsNullOrWhiteSpace(logText) ? "未获取到烧录日志，请手动检查 ST-LINK 连接状态。" : logText;
                if (showText.Length > 2000)
                    showText = showText.Substring(0, 2000) + "\n...(日志已截断)";

                if (process.ExitCode == 0)
                {
                    WriteFirstFlashLog(flowLogPath, "流程结果: 首刷成功。");
                    UpdateFirstFlashProgress(100, "首刷成功。");
                    await _dialogHostService.Information(
                        "首刷固件",
                        "首刷成功",
                        CommonModel.ThreeAxisTestModelWindowsViewModelRegionName);
                }
                else
                {
                    WriteFirstFlashLog(flowLogPath, $"流程结果: 首刷失败，ExitCode={process.ExitCode}。");
                    UpdateFirstFlashProgress(100, $"首刷失败（ExitCode={process.ExitCode}）。");
                    await _dialogHostService.Information(
                        "首刷固件",
                        "首刷失败",
                        CommonModel.ThreeAxisTestModelWindowsViewModelRegionName);
                }
            }
            catch (Exception ex)
            {
                WriteFirstFlashLog(flowLogPath, $"流程异常: {ex}");
                UpdateFirstFlashProgress(100, "首刷异常中断。");
                await _dialogHostService.Information(
                    "首刷固件",
                    "首刷失败",
                    CommonModel.ThreeAxisTestModelWindowsViewModelRegionName);
            }
            finally
            {
                WriteFirstFlashLog(flowLogPath, "========== 首刷流程结束 ==========\n");
                FirstFlashInProgress = false;
                _firstFlashRunning = false;
                FirstFlashFirmwareCommand.RaiseCanExecuteChanged();
            }
        }

        private async Task RunZipFirmwareUpgradeAsync()
        {
            if (_upgradeFirmwareRunning)
                return;

            _upgradeFirmwareRunning = true;
            UpgradeFirmwareCommand.RaiseCanExecuteChanged();
            try
            {
                if (_firmwareUpgradeCoordinator == null || _firmwareWorkflowState == null)
                {
                    await _dialogHostService.Information(
                        "升级固件",
                        "升级功能未初始化",
                        CommonModel.ThreeAxisTestModelWindowsViewModelRegionName);
                    return;
                }
                if ((Model.SerialPortService?.IsOpen != true) && (Model.NetUdpService?.IsOpen != true))
                {
                    await _dialogHostService.Information(
                        "升级固件",
                        "请先连接串口或网口设备",
                        CommonModel.ThreeAxisTestModelWindowsViewModelRegionName);
                    return;
                }

                var result = await _firmwareUpgradeCoordinator.CheckAndUpgradeIfNeededAsync(
                    Model.SerialPortService,
                    Model.NetUdpService,
                    _firmwareWorkflowState.FirmwareIndexUrl,
                    _dialogHostService,
                    CommonModel.ThreeAxisTestModelWindowsViewModelRegionName);

                _firmwareWorkflowState.FirmwareCheckCompleted = true;
                _firmwareWorkflowState.FirmwareUpgradeRequired = !result.IsLatest;
                _firmwareWorkflowState.FirmwareUpgradeSkipped = result.UserSkippedUpgrade;
                _firmwareWorkflowState.FirmwareCheckMessage = result.Message;
                _firmwareWorkflowState.CurrentFirmwareVersion = result.CurrentVersionText;
                _firmwareWorkflowState.LatestFirmwareVersion = result.LatestVersionText;
                _firmwareWorkflowState.LatestFirmwareUrl = result.LatestPackageUrl;

                await _dialogHostService.Information(
                    "升级固件",
                    (result.IsLatest || result.UpgradeSucceeded) ? "升级成功" : "升级失败",
                    CommonModel.ThreeAxisTestModelWindowsViewModelRegionName);
            }
            catch
            {
                await _dialogHostService.Information(
                    "升级固件",
                    "升级失败",
                    CommonModel.ThreeAxisTestModelWindowsViewModelRegionName);
            }
            finally
            {
                _upgradeFirmwareRunning = false;
                UpgradeFirmwareCommand.RaiseCanExecuteChanged();
            }
        }

        private void UpdateFirstFlashProgress(double value, string status)
        {
            FirstFlashProgressValue = value;
            FirstFlashStatusText = status;
        }

        private static async Task DownloadFileAsync(string url, string targetPath, string flowLogPath)
        {
            WriteFirstFlashLog(flowLogPath, $"开始下载: {url}");
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath) ?? AppContext.BaseDirectory);
            using var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            await using var fs = File.Create(targetPath);
            await response.Content.CopyToAsync(fs);
            WriteFirstFlashLog(flowLogPath, $"下载完成: {targetPath}");
        }

        private static async Task<string> EnsureStLinkToolAsync(string toolRoot, string flowLogPath)
        {
            string toolDir = Path.Combine(toolRoot, "st_link_utility");
            Directory.CreateDirectory(toolDir);
            string cliPath = Path.Combine(toolDir, "ST-LINK_CLI.exe");
            if (File.Exists(cliPath))
            {
                WriteFirstFlashLog(flowLogPath, "检测到本地 CLI，开始校验 loader 资源完整性。");
                await EnsureLoaderResourcesAsync(toolDir, flowLogPath);
                return cliPath;
            }

            WriteFirstFlashLog(flowLogPath, "本地未找到 CLI，开始从云端下载工具。");

            var toolFiles = new[]
            {
                "ST-LINK%20Utility/ST-LINK_CLI.exe",
                "ST-LINK%20Utility/STLinkUSBDriver.dll",
                "ST-LINK%20Utility/STM32%20ST-LINK%20Utility.exe",
                "ST-LINK%20Utility/ST-LinkUpgrade.exe",
                "ST-LINK%20Utility/advapi32.dll",
                "ST-LINK%20Utility/comctl32.dll",
            };

            foreach (var item in toolFiles)
            {
                string fileName = Uri.UnescapeDataString(item.Split('/').Last());
                string localPath = Path.Combine(toolDir, fileName);
                string url = StToolBaseUrl + item;
                await DownloadFileAsync(url, localPath, flowLogPath);
            }

            await EnsureLoaderResourcesAsync(toolDir, flowLogPath);

            string driverDir = Path.Combine(toolDir, "ST-LINK_USB_V2_1_Driver");
            Directory.CreateDirectory(driverDir);
            var driverFiles = new[]
            {
                "dpinst_amd64.exe",
                "dpinst_x86.exe",
                "stlink_VCP.inf",
                "stlink_dbg_winusb.inf",
                "stlink_winusb_install.bat",
                "stlinkdbgwinusb_x64.cat",
                "stlinkdbgwinusb_x86.cat",
                "stlinkvcp_x64.cat",
                "stlinkvcp_x86.cat",
            };
            foreach (var file in driverFiles)
            {
                string url = StToolBaseUrl + "ST-LINK_USB_V2_1_Driver/" + file;
                await DownloadFileAsync(url, Path.Combine(driverDir, file), flowLogPath);
            }

            WriteFirstFlashLog(flowLogPath, "工具下载完成。");
            return cliPath;
        }

        private static async Task EnsureLoaderResourcesAsync(string toolDir, string flowLogPath)
        {
            // ST-LINK_CLI 在烧写时依赖同目录下的 FlashLoader / ExternalLoader 资源。
            // 若缺失会报 "The elf loader file is not accessible."（ExitCode=12）。
            string flashLoaderDir = Path.Combine(toolDir, "FlashLoader");
            string externalLoaderDir = Path.Combine(toolDir, "ExternalLoader");
            bool flashLoaderReady = Directory.Exists(flashLoaderDir) && Directory.EnumerateFiles(flashLoaderDir, "*", SearchOption.AllDirectories).Any();
            bool externalLoaderReady = Directory.Exists(externalLoaderDir) && Directory.EnumerateFiles(externalLoaderDir, "*", SearchOption.AllDirectories).Any();

            if (flashLoaderReady && externalLoaderReady)
            {
                WriteFirstFlashLog(flowLogPath, "Loader 资源已存在，跳过同步。");
                return;
            }

            WriteFirstFlashLog(flowLogPath, "Loader 资源缺失，开始从云端补齐 FlashLoader/ExternalLoader。");
            await SyncDirectoryFromIndexAsync(
                StToolBaseUrl + "ST-LINK%20Utility/FlashLoader/",
                flashLoaderDir,
                flowLogPath);
            await SyncDirectoryFromIndexAsync(
                StToolBaseUrl + "ST-LINK%20Utility/ExternalLoader/",
                externalLoaderDir,
                flowLogPath);
            WriteFirstFlashLog(flowLogPath, "Loader 资源补齐完成。");
        }

        private static async Task<string> DownloadFirmwareEveryTimeAsync(string toolRoot, string flowLogPath)
        {
            string firmwareDir = Path.Combine(toolRoot, "motor_firmware");
            Directory.CreateDirectory(firmwareDir);

            WriteFirstFlashLog(flowLogPath, $"请求固件目录: {FirmwareDirUrl}");
            string html = await _httpClient.GetStringAsync(FirmwareDirUrl);
            var matches = Regex.Matches(html, "href\\s*=\\s*\"(?<u>[^\"]+\\.hex)\"", RegexOptions.IgnoreCase);
            if (matches.Count == 0)
                throw new InvalidOperationException("云端固件目录未找到 .hex 文件。");

            // 每次刷写都重新下载目录中最后一个 hex（一般为最新文件）。
            string href = matches[matches.Count - 1].Groups["u"].Value.Trim();
            string fileName = Uri.UnescapeDataString(Path.GetFileName(href));
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "latest.hex";

            string localHex = Path.Combine(firmwareDir, fileName);
            string downloadUrl = new Uri(new Uri(FirmwareDirUrl), href).ToString();
            WriteFirstFlashLog(flowLogPath, $"选中固件: {fileName}");
            await DownloadFileAsync(downloadUrl, localHex, flowLogPath);
            return localHex;
        }

        private static string CreateFirstFlashLogPath()
        {
            string dir = Path.Combine(AppContext.BaseDirectory, "报告", "固件升级", "首刷日志");
            Directory.CreateDirectory(dir);
            string file = $"首刷流程_{DateTime.Now:yyyyMMdd}.log";
            return Path.Combine(dir, file);
        }

        private static async Task SyncDirectoryFromIndexAsync(string indexUrl, string localDir, string flowLogPath)
        {
            Directory.CreateDirectory(localDir);
            WriteFirstFlashLog(flowLogPath, $"同步目录: {indexUrl}");
            string html = await _httpClient.GetStringAsync(indexUrl);

            // Apache 样式目录页：<a href="xxx">；筛掉 ../
            var linkMatches = Regex.Matches(html, "href\\s*=\\s*\"(?<u>[^\"]+)\"", RegexOptions.IgnoreCase);
            foreach (Match m in linkMatches)
            {
                string href = m.Groups["u"].Value.Trim();
                if (string.IsNullOrWhiteSpace(href) || href == "../")
                    continue;

                var nextUri = new Uri(new Uri(indexUrl), href);
                bool isDir = href.EndsWith("/", StringComparison.Ordinal);
                if (isDir)
                {
                    string subName = Uri.UnescapeDataString(href.TrimEnd('/'));
                    string subLocal = Path.Combine(localDir, subName);
                    await SyncDirectoryFromIndexAsync(nextUri.ToString(), subLocal, flowLogPath);
                }
                else
                {
                    string fileName = Uri.UnescapeDataString(Path.GetFileName(href));
                    if (string.IsNullOrWhiteSpace(fileName))
                        continue;
                    string localPath = Path.Combine(localDir, fileName);
                    await DownloadFileAsync(nextUri.ToString(), localPath, flowLogPath);
                }
            }
        }

        private static void WriteFirstFlashLog(string filePath, string message)
        {
            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
            lock (FirstFlashLogLock)
            {
                File.AppendAllText(filePath, line + Environment.NewLine);
            }
        }
        #endregion
    }
}
