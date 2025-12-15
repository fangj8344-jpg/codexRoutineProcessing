
using Prism.Ioc;
using Prism.Modularity;
using System.Reflection;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.ButterflyValveTest.Dialog.ViewModels;
using UtilityTools.Modules.ButterflyValveTest.Dialog.Views;
using UtilityTools.Modules.ButterflyValveTest.Model;
using UtilityTools.Modules.ButterflyValveTest.Views;

namespace UtilityTools.Modules.ButterflyValveTest
{
    [CustomModule(ModuleName = "ButterflyValveTest", Title = "蝶阀测试工具", Tip = "串口通信", Icon = "FlashTriangle")]
    public class ButterflyValveTestModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ButterflyValveTestView>();
            containerRegistry.RegisterSingleton<ButterflyValveTestModel>();
            containerRegistry.RegisterDialog<WaitPrpgressBarView, WaitPrpgressBarViewModel>();
        }
    }

}
