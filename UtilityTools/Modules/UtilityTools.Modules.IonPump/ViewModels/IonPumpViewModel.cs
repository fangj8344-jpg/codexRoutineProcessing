using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Mvvm;

namespace UtilityTools.Modules.IonPump.ViewModels
{
    public class IonPumpViewModel : RegionViewModelBase
    {

        public IonPumpViewModel(IDialogHostService dialogHostService, IContainerProvider containerProvider) : base(containerProvider)
        {
            _dialogHostService = dialogHostService;
            _containerProvider = containerProvider;

            InitCommand();
            InitProperty();
        }

        #region ------------Field------------
        private readonly IDialogHostService _dialogHostService;
        private readonly IContainerProvider _containerProvider;
        #endregion

        #region ------------Property------------
        private AgilentIonPumpViewModel _agilentIonPumpViewModel;
        public AgilentIonPumpViewModel AgilentIonPumpVM
        {
            get { return _agilentIonPumpViewModel; }
            set { _agilentIonPumpViewModel = value; RaisePropertyChanged(); }
        }
        #endregion


        #region ------------PrivateMethod------------
        private void InitCommand()
        {
        }

        private void InitProperty()
        {
            _agilentIonPumpViewModel = new AgilentIonPumpViewModel(this._containerProvider, this._dialogHostService);
        }
        #endregion



    }
}
