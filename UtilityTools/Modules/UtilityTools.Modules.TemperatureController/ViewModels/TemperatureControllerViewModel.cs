#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.TemperatureController.ViewModels
 * 唯一标识：7606eb56-e540-4f61-bf18-724c9e043c45
 * 文件名：TemperatureControllerViewModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/10/23 15:30:12
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.TemperatureController.Model;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Modules.TemperatureController.ViewModels
{
    public class TemperatureControllerViewModel : RegionViewModelBase
    {
        #region ------------Constructor------------
        public TemperatureControllerViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService)
            : base(containerProvider)
        {
            _dialogHostService = dialogHostService;
            InitProperty();
            InitCommand();
        }
        #endregion

        #region ------------Field------------
        private readonly IDialogHostService _dialogHostService;
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

        private ControllerModel _model;

        public ControllerModel Model
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
            Model = containerProvider.Resolve<ControllerModel>();
        }

        /// <summary>
        /// 显示设备连接弹窗
        /// </summary>
        private async void ShowDevice()
        {
            DialogParameters parameter = new DialogParameters();
            parameter.Add("Value", Model.SerialPortService);
            var diaglogResult = await this._dialogHostService.ShowDialog("SerialPortView", parameter, CommonModel.TemperatureControllerRegionName);
            if (diaglogResult == null)
                return;
            if (diaglogResult.Result == ButtonResult.OK && diaglogResult.Parameters.ContainsKey("Value"))
            {
                var value = diaglogResult.Parameters.GetValue<IAsynRWService>("Value");
                if (value != null)
                {
                    Model.SerialPortService = value;
                    IsConnected = Model.SerialPortService.IsOpen;
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
            var diaglogResult = await this._dialogHostService.ShowDialog("NetConfigView", parameter, CommonModel.TemperatureControllerRegionName);
            if (diaglogResult == null)
                return;
            if (diaglogResult.Result == ButtonResult.OK && diaglogResult.Parameters.ContainsKey("Value"))
            {
                var value = diaglogResult.Parameters.GetValue<IAsynRWService>("Value");
                if (value != null)
                {
                    Model.NetUdpService = value;
                    NetIsConnected = Model.NetUdpService.IsOpen;
                }
            }
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
