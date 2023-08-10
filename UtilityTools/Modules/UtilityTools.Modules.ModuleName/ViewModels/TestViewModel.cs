using Prism.Ioc;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Mvvm;
using UtilityTools.Services.Interfaces;

namespace UtilityTools.Modules.ModuleName.ViewModels
{
    public class TestViewModel : RegionViewModelBase
    {
        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public TestViewModel(IContainerProvider containerProvider, IMessageService messageService) :
            base(containerProvider)
        {
            Message = messageService.GetMessage();
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            //do something
        }
    }
}
