
using Prism.Ioc;
using Prism.Modularity;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.MotorTest.Model;
using UtilityTools.Modules.MotorTest.ViewModels;
using UtilityTools.Modules.MotorTest.Views;

namespace UtilityTools.Modules.MotorTest
{
    [CustomModule(ModuleName = "MotorTest" , Title = "电机测试" , Tip = "串口通讯" ,Icon = "AxisArrowInfo")]
    public class MotorTestModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MotorTestView>();
            containerRegistry.RegisterSingleton<MotorTestModel>();
            containerRegistry.RegisterForNavigation<ThreeAxisTestModelWindowsView, ThreeAxisTestModelWindowsViewModel>();
        }
    }

}
