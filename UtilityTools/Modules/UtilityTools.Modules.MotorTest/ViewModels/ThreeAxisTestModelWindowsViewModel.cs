using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private bool _firmwareCheckRunning;
        public string name = "scccc";
        #endregion

        #region ------------Property------------
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
                        await TryRunFirmwareCheckAsync();
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
                        await TryRunFirmwareCheckAsync();
                    }
                    else
                    {
                        Model.CloseQueryStatusTask();
                    }
                }
            }
        }

        private async Task TryRunFirmwareCheckAsync()
        {
            if (_firmwareUpgradeCoordinator == null || _firmwareWorkflowState == null)
                return;
            if (_firmwareCheckRunning || _firmwareWorkflowState.FirmwareCheckCompleted)
                return;
            if ((Model.SerialPortService?.IsOpen != true) && (Model.NetUdpService?.IsOpen != true))
                return;

            _firmwareCheckRunning = true;
            _firmwareWorkflowState.FirmwareUpgradeInProgress = true;
            try
            {
                var checkResult = await _firmwareUpgradeCoordinator.CheckAndUpgradeIfNeededAsync(
                    Model.SerialPortService,
                    Model.NetUdpService,
                    _firmwareWorkflowState.FirmwareIndexUrl,
                    _dialogHostService,
                    CommonModel.ThreeAxisTestModelWindowsViewModelRegionName);

                _firmwareWorkflowState.FirmwareCheckCompleted = true;
                _firmwareWorkflowState.FirmwareUpgradeRequired = !checkResult.IsLatest;
                _firmwareWorkflowState.FirmwareUpgradeSkipped = checkResult.UserSkippedUpgrade;
                _firmwareWorkflowState.FirmwareCheckMessage = checkResult.Message;
                _firmwareWorkflowState.CurrentFirmwareVersion = checkResult.CurrentVersionText;
                _firmwareWorkflowState.LatestFirmwareVersion = checkResult.LatestVersionText;
                _firmwareWorkflowState.LatestFirmwareUrl = checkResult.LatestPackageUrl;

                if (!checkResult.IsLatest && checkResult.UserSkippedUpgrade)
                {
                    await _dialogHostService.Information(
                        "固件提醒",
                        $"检测到固件非最新，已按你的选择暂不升级。\n{checkResult.Message}",
                        CommonModel.ThreeAxisTestModelWindowsViewModelRegionName);
                }
                else if (!checkResult.IsLatest && !checkResult.UpgradeSucceeded)
                {
                    await _dialogHostService.Information(
                        "固件提醒",
                        $"固件未更新到最新版本。\n{checkResult.Message}",
                        CommonModel.ThreeAxisTestModelWindowsViewModelRegionName);
                }
            }
            catch (Exception ex)
            {
                _firmwareWorkflowState.FirmwareCheckCompleted = true;
                _firmwareWorkflowState.FirmwareUpgradeRequired = false;
                _firmwareWorkflowState.FirmwareUpgradeSkipped = true;
                _firmwareWorkflowState.FirmwareCheckMessage = $"固件检查失败：{ex.Message}";
            }
            finally
            {
                _firmwareWorkflowState.FirmwareUpgradeInProgress = false;
                _firmwareCheckRunning = false;
            }
        }
        #endregion
    }
}
