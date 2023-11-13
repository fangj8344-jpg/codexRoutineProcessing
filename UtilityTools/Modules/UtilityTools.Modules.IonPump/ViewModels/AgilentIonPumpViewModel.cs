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
using UtilityTools.Modules.IonPump.Model;
using UtilityTools.Services.Interfaces;
using UtilityTools.Services.Interfaces.IServices;


namespace UtilityTools.Modules.IonPump.ViewModels
{
    public class AgilentIonPumpViewModel : RegionViewModelBase
    {
        public AgilentIonPumpViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider)
        {
            _dialogHostService = dialogHostService;

            InitCommand();
            InitProperty();
        }


        #region ------------Field------------

        private readonly IDialogHostService _dialogHostService;

        #endregion

        #region ------------Property------------
        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set { _isConnected = value; RaisePropertyChanged(); }
        }


        private IAsynRWService _service;

        /// <summary>
        /// 异步通信服务
        /// </summary>

        public IAsynRWService Service

        {

            get { return _service; }

            set { _service = value; RaisePropertyChanged(); }

        }
        #endregion


        #region Command
        public DelegateCommand ShowDeviceCommand { get; set; }
        private async void ShowDevice()

        {

            DialogParameters parameter = new DialogParameters();

            parameter.Add("Value", Service);

            var diaglogResult = await this._dialogHostService.ShowDialog("SerialPortView", parameter, CommonModel.IonPumpRegionName);

            if (diaglogResult == null)

                return;

            if (diaglogResult.Result == ButtonResult.OK && diaglogResult.Parameters.ContainsKey("Value"))

            {

                var value = diaglogResult.Parameters.GetValue<IAsynRWService>("Value");

                if (value != null)

                {

                    Service = value;

                    IsConnected = Service.IsOpen;

                }

            }

        }

        #endregion


        private void InitCommand()
        {
            ShowDeviceCommand = new DelegateCommand(ShowDevice);

        }

        private void InitProperty()
        {

        }
    }
}
