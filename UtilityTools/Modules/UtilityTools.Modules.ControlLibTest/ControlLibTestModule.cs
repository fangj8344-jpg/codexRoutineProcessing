using Prism.Ioc;
using Prism.Modularity;
using System;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.ControlLibTest.Views;

namespace UtilityTools.Modules.ControlLibTest
{
    [CustomModule(ModuleName = "ControlLibTest", Title = "通讯库测试", Tip = "串口通讯", Icon = "FlashTriangle")]
    public class ControlLibTestModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ControlLibTestView>();
        }
    }
}
