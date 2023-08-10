using Prism.Ioc;
using Prism.Regions;
using UtilityTools.Core.Mvvm;
using UtilityTools.Services.Interfaces;

namespace UtilityTools.Modules.ModuleName.ViewModels
{
    public class ViewAViewModel : RegionViewModelBase
    {
        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public ViewAViewModel(IContainerProvider containerProvider, IMessageService messageService) :
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
