using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using UtilityTools.Modules.NetController.Model;
using UtilityTools.Modules.NetController.Views;

namespace UtilityTools.Modules.NetController
{
    [Module(ModuleName = "NetController")]
    public class NetControllerModule : IModule
    {
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