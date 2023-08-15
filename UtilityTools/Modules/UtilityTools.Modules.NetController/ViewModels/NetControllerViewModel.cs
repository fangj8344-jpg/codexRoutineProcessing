#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.NetController.ViewModels
 * 唯一标识：d636ff6b-0a6e-400b-8460-6872a380aa4c
 * 文件名：NetControllerViewModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/10 17:19:15
 * 版本：V1.0.0
 * 描述：
 *
 * ----------------------------------------------------------------
 * 修改人：
 * 时间：
 * 修改说明：
 *
 * 版本：V1.0.1
 *----------------------------------------------------------------*/
#endregion

using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Model;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.NetController.Extension;
using UtilityTools.Modules.NetController.Model;
using UtilityTools.Modules.NetController.Protocol;
using UtilityTools.Modules.NetController.Views;
using UtilityTools.Services.Interfaces;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Modules.NetController.ViewModels
{
    public class NetControllerViewModel : RegionViewModelBase
    {
        #region ------------Constructor------------
        public NetControllerViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService)
            : base(containerProvider)
        {
            _dialogHostService = dialogHostService;
            _containerProvider = containerProvider;
            InitProperty();
            InitCommand();

        }
        #endregion

        #region ------------Field------------
        private readonly IDialogHostService _dialogHostService;
        private readonly IContainerProvider _containerProvider;
        private string _message;
        private CCSModel _ccs;
        private DacModel _dac;
        private LightModel _light;
        private RelayModel _relay;
        private TemperatureModel _temperature;
        private VacuumModel _vacuum;
        #endregion

        #region ------------Property------------
        /// <summary>
        /// 测试消息
        /// </summary>
        public string Message
        {
            get { return _message; }
            set { _message = value; RaisePropertyChanged(); }
        }

        private bool _isConnected;
        /// <summary>
        /// 是否已经连接设备
        /// </summary>
        public bool IsConnected
        {
            get { return _isConnected; }
            set { _isConnected = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 通讯服务
        /// </summary>
        public ISyncRWService Service { get; set; }

        /// <summary>
        /// CCS参数模型
        /// </summary>
        public CCSModel CCS
        {
            get { return _ccs; }
            set { _ccs = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// DAC参数模型
        /// </summary>
        public DacModel DAC
        {
            get { return _dac; }
            set { _dac = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 灯带控制模型
        /// </summary>
        public LightModel Light
        {
            get { return _light; }
            set { _light = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 继电器控制模型
        /// </summary>
        public RelayModel Relay
        {
            get { return _relay; }
            set { _relay = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 温度控制模型
        /// </summary>
        public TemperatureModel Temperature
        {
            get { return _temperature; }
            set { _temperature = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 真空模型
        /// </summary>
        public VacuumModel Vacuum
        {
            get { return _vacuum; }
            set { _vacuum = value; RaisePropertyChanged(); }
        }

        #endregion

        #region ------------PublicMethod------------
        public DelegateCommand ShowDeviceCommand { get; set; }
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
        }

        /// <summary>
        /// 初始化属性
        /// </summary>
        private void InitProperty()
        {
            CCS = _containerProvider.Resolve<CCSModel>();
            DAC = _containerProvider.Resolve<DacModel>();
            Light = _containerProvider.Resolve<LightModel>();
            Relay = _containerProvider.Resolve<RelayModel>();
            Temperature = _containerProvider.Resolve<TemperatureModel>();
            Vacuum = _containerProvider.Resolve<VacuumModel>();

            Service = _containerProvider.Resolve<IServiceFactory>().GetSyncRWService("UNCB");
            SetServiceInfo(Service.GetHandle());
        }

        /// <summary>
        /// 初始化属性绑定
        /// </summary>
        private void InitBinding()
        {
            CCS.SetPropertyChangedHandle(OnPropertyChanged);
            DAC.SetPropertyChangedHandle(OnPropertyChanged);
            Relay.SetPropertyChangedHandle(OnPropertyChanged);
            Temperature.SetPropertyChangedHandle(OnPropertyChanged);
            Vacuum.SetPropertyChangedHandle(OnPropertyChanged);
        }

        /// <summary>
        /// 显示设备连接弹窗
        /// </summary>
        private async void ShowDevice()
        {
            DialogParameters parameter = new DialogParameters();
            parameter.Add("Value", Service.GetHandle());
            var diaglogResult = await this._dialogHostService.ShowDialog("NetConfigView", parameter, CommonModel.NetControllerRegionName);
            if (diaglogResult == null)
                return;
            if (diaglogResult.Result == ButtonResult.OK && diaglogResult.Parameters.ContainsKey("Value"))
            {
                Service.SetHandle(diaglogResult.Parameters.GetValue<object>("Value"));
                IsConnected = Service.IsOpen;
            }
        }

        /// <summary>
        /// 设置服务信息
        /// </summary>
        /// <param name="service"></param>
        private void SetServiceInfo(object service)
        {
            if (service is NetConfigModel netConfig)
            {
                netConfig.HostIp = "192.168.1.33";
                netConfig.HostPort = 5005;
                netConfig.TargetIp = "255.255.255.255";
                netConfig.TargetPort = 5000;
                netConfig.ConnectTest = new DelegateConnectTestCommand((config) =>
                {
                    var cmd = NetControllerProtocol.PackBytesHandShake(netConfig.HostIp, netConfig.HostPort);

                });
            }
        }

        /// <summary>
        /// 属性变更回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPropertyChanged(object sender, EventArgs e)
        {
            switch (sender)
            {
                case IntSliderInfoModel intSlider:
                    SetIntSliderValue(intSlider.Type, intSlider.Channel, intSlider.Value);
                    break;
                case ToggleInfoModel toggle:
                    SetToggleValue(toggle.Type, toggle.Channel, toggle.Enable);
                    break;
            }
        }

        /// <summary>
        /// 设置整型滑块参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        private bool SetIntSliderValue(string type, int channel, int value) 
        {
            switch (type)
            {
                case "Focus":
                    Service.SetCCSFocusValue(value);
                    break;
                case "Compress":
                    Service.SetCCSCompressValue(channel, value);
                    break;
                case "Center":
                    Service.SetCCSCenterValue(channel, value);
                    break;
                case "Astig":
                    Service.SetCCSAstigValue(channel, value);
                    break;
                case "DAC":
                    Service.SetCCSCenterValue(channel, value);
                    break;
            }
            return false;
        }

        /// <summary>
        /// 设置开关参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool SetToggleValue(string type, int channle, bool value) 
        {
            switch (type)
            {
                case "CCS":
                    Service.SetCCSRelay(channle, value);
                    break;
                case "Compress":

                    break;
            }
            return false;
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
