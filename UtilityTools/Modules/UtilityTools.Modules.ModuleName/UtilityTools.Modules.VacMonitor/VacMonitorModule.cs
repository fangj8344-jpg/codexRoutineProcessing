using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.VacMonitor.Views;

namespace UtilityTools.Modules.VacMonitor
{
    [CustomModule(ModuleName = "VacMonitor", Title = "真空监控", Tip = "串口通讯的真空监控板", Icon = "RobotVacuumVariant")]
    public class VacMonitorModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<VacMonitorView>();
        }
    }
}