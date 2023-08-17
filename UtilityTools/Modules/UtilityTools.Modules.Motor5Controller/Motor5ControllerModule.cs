using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.Motor5Controller.Model;
using UtilityTools.Modules.Motor5Controller.Views;

namespace UtilityTools.Modules.Motor5Controller
{
    [CustomModule(ModuleName = "Motor5Controller", Title = "五轴电机控制", Tip = "串口通讯", Icon = "AxisArrowInfo")]
    public class Motor5ControllerModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<Motor5ControllerView>();
            containerRegistry.RegisterSingleton<OneModel>();
        }
    }
}