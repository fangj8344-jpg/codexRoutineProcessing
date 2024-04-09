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
using UtilityTools.Modules.FZBPumpController.Model;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Modules.FZBPumpController.ViewModels
{
    internal class FZBPumpControllerViewModel: RegionViewModelBase
    {
        public FZBPumpControllerViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) :base(containerProvider)
        {
            _dialogHostService = dialogHostService;
            _containerProvider = containerProvider;
            InitProperty();
            InitCommand();
        }

        private readonly IDialogHostService _dialogHostService;
        private readonly IContainerProvider _containerProvider;

        public DelegateCommand ShowDeviceCommand { get; set; }
        
        private FZBModel _model;
        public FZBModel Model  
        {
            get { return _model; }
            set { _model = value; RaisePropertyChanged(); }
        }

        private bool _isConnected;
        /// <summary>
        /// 设备是否连接
        /// </summary>
        public bool IsConnected
        {
            get { return _isConnected; }
            set { _isConnected = value; RaisePropertyChanged(); }
        }
        private void InitCommand()
        {
            ShowDeviceCommand = new DelegateCommand(ShowDevice);

        }
        private void InitProperty()
        {
            Model = new FZBModel(containerProvider);
        }

        private async void ShowDevice()
        {
            DialogParameters parameter = new DialogParameters();
            parameter.Add("Value", Model.SerialPortService);
            var diaglogResult = await this._dialogHostService.ShowDialog("SerialPortView", parameter, CommonModel.HvControllerRegionName);
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
    }
}
