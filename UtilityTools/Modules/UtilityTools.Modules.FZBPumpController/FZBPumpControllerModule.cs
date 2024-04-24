using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Reflection;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.FZBPumpController.Views;

namespace UtilityTools.Modules.FZBPumpController
{
    [CustomModule(ModuleName = "FZBPumpController", Title = "涡轮泵控制", Tip = "串口通讯", Icon = "FlashTriangle")]
    public class FZBPumpControllerModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<FZBPumpControllerView>();
        }
    }
}
