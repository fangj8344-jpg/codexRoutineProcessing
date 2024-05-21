using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Reflection;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.CCSTool.Views;

namespace UtilityTools.Modules.CCSTool
{
    [CustomModule(ModuleName = "CCSTool", Title = "CCS控制工具", Tip = "串口通信", Icon = "FlashTriangle")]
    public class CCSToolModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<CCSToolView>();
        }
    }
}
