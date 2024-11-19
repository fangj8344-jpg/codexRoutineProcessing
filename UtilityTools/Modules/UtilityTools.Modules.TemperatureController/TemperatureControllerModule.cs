using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Reflection;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.TemperatureController.Model;
using UtilityTools.Modules.TemperatureController.Views;

namespace UtilityTools.Modules.TemperatureController
{
    [CustomModule(ModuleName = "TemperatureController", Title = "ÎÂ¿ØÒÇ", Tip = "´®¿Ú/ÍøÂçÍ¨Ñ¶", Icon = "Thermometer")]
    public class TemperatureControllerModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<TemperatureControllerView>();
            containerRegistry.RegisterSingleton<ControllerModel>();
        }
    }
}
