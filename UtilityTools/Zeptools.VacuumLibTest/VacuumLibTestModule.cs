using Prism.Ioc;
using Prism.Modularity;
using System.Reflection;
using UtilityTools.Core.Mvvm;
using UtilityTools.VacuumLibTest.Views;

namespace Zeptools.VacuumLibTest
{
    [CustomModule(ModuleName = "VacuumLibTest", Title = "真空控制板测试", Tip = "串口通讯的真空监控板", Icon = "RobotVacuumVariant")]
    public class VacuumLibTestModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<VacuumLibTestView>();
        }
    }

}