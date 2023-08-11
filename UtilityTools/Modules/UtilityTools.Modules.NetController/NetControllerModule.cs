using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using UtilityTools.Modules.NetController.Model;
using UtilityTools.Modules.NetController.Views;

namespace UtilityTools.Modules.NetController
{
    public class NetControllerModule : IModule
    {
        public static string ModuleName => "NetController";

        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<NetControllerView>();

            containerRegistry.RegisterSingleton<CCSModel>();
            containerRegistry.RegisterSingleton<DacModel>();
            containerRegistry.RegisterSingleton<LightModel>();
            containerRegistry.RegisterSingleton<RelayModel>();
            containerRegistry.RegisterSingleton<TemperatureModel>();
            containerRegistry.RegisterSingleton<VacuumModel>();
        }
    }
}