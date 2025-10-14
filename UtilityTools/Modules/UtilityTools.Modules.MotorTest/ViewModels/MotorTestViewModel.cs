using OxyPlot;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.MotorTest.Model;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Modules.MotorTest.ViewModels
{
    public class MotorTestViewModel:RegionViewModelBase
    {

        #region ------------Constructor------------
        public MotorTestViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService)
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

        private MotorTestModel _model;

        public MotorTestModel Model
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
            Model = containerProvider.Resolve<MotorTestModel>();
        }

        /// <summary>
        /// 显示设备连接弹窗
        /// </summary>
        private async void ShowDevice()
        {
            DialogParameters parameter = new DialogParameters();
            parameter.Add("Value", Model.SerialPortService);
            var diaglogResult = await this._dialogHostService.ShowDialog("SerialPortView", parameter, CommonModel.MotorTestRegionName);
            if (diaglogResult == null)
                return;
            if (diaglogResult.Result == ButtonResult.OK && diaglogResult.Parameters.ContainsKey("Value"))
            {
                var value = diaglogResult.Parameters.GetValue<IAsynRWService>("Value");
                if (value != null)
                {
                    Model.SerialPortService = value;
                    IsConnected = Model.SerialPortService.IsOpen;
                    if (IsConnected == true)
                    {
                        Model._getMotorStateTimer.Start();
                        Model.ThreadProc();
                    }
                    else
                    {
                        Model._getMotorStateTimer.Stop();
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
            var diaglogResult = await this._dialogHostService.ShowDialog("NetConfigView", parameter, CommonModel.MotorTestRegionName);
            if (diaglogResult == null)
                return;
            if (diaglogResult.Result == ButtonResult.OK && diaglogResult.Parameters.ContainsKey("Value"))
            {
                var value = diaglogResult.Parameters.GetValue<IAsynRWService>("Value");
                if (value != null)
                {
                    Model.NetUdpService = value;
                    NetIsConnected = Model.NetUdpService.IsOpen;
                    if (NetIsConnected == true)
                    {
                        Model._getMotorStateTimer.Start();
                        Model.ThreadProc();
                    }
                    else
                    {
                        Model._getMotorStateTimer.Stop();
                    }
                }
            }
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }

}
