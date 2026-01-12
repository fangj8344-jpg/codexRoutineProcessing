using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.FDC12CHVBox.Model;

namespace UtilityTools.Modules.FDC12CHVBox.ViewModels
{
    public class FDC12CHVBoxViewModel: RegionViewModelBase
    {

        private string _version = "1.41";
        public string Version
        {
            get { return _version; }
            set { _version = value; RaisePropertyChanged(); }
        }
        public FDC12CHVBoxViewModel(IContainerProvider containerProvider)
          : base(containerProvider)
        {
            Model = containerProvider.Resolve<FDC12CHVBoxModel>();
        }
        private FDC12CHVBoxModel _model;
        public FDC12CHVBoxModel Model
        {
            get { return _model; }
            set { _model = value; RaisePropertyChanged(); }
        }
    }
}
