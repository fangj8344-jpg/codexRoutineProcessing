using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System.Reflection;
using System;
using UtilityTools.Core;
using UtilityTools.Modules.ModuleName.Views;
using UtilityTools.Core.Interface;

namespace UtilityTools.Modules.ModuleName
{
    public class ModuleNameModule : IModuleBase
    {
        private readonly IRegionManager _regionManager;

        public ModuleNameModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public static string ModuleName => "Test";

        public void OnInitialized(IContainerProvider containerProvider)
        {
            //_regionManager.RequestNavigate(RegionNames.ContentRegion, "ViewA");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ViewA>();
            containerRegistry.RegisterForNavigation<TestView>();
        }

    }
}