using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.IonPump.Views;

namespace UtilityTools.Modules.IonPump
{
    [CustomModule(ModuleName = "IonPump", Title = "离子泵", Tip = "串口通讯", Icon = "Pump")]
    public class IonPumpModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<IonPumpView>();
        }
    }
}