
using Prism.Ioc;
using Prism.Modularity;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.NewMotor5Controller.Model;
using UtilityTools.Modules.NewMotor5Controller.Views;

namespace UtilityTools.Modules.NewMotor5Controller
{
    [CustomModule(ModuleName = "NewMotor5ControllerModule", Title = "电机测试", Tip = "串口通讯", Icon = "AxisArrowInfo")]
    public class NewMotor5ControllerModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<NewMotor5ControllerView>();
            containerRegistry.RegisterSingleton<FiveAxisTestModel>();
        }
    }
}
