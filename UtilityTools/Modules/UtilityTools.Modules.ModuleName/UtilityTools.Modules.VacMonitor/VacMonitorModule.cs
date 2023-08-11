using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using UtilityTools.Modules.VacMonitor.Views;

namespace UtilityTools.Modules.VacMonitor
{
    public class VacMonitorModule : IModule
    {
        public static string ModuleName => "VacMonitor";

        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<VacMonitorView>();
        }
    }
}