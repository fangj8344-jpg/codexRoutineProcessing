using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.HvController.Model;
using UtilityTools.Modules.HvController.Views;

namespace UtilityTools.Modules.HvController
{
    [CustomModule(ModuleName = "HvController", Title = "高压控制", Tip = "串口通讯", Icon = "FlashTriangle")]
    public class HvControllerModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<HvControllerView>();
            containerRegistry.RegisterSingleton<HvModel>();
        }
    }
}