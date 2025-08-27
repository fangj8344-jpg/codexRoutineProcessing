
using Prism.Ioc;
using Prism.Modularity;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.NewMotor5Controller.Entity;
using UtilityTools.Modules.NewMotor5Controller.Views;

namespace UtilityTools.Modules.NewMotor5Controller
{
    
    [CustomModule(ModuleName = "NewMotor5Control", Title = "新五轴电机控制", Tip = "", Icon = "Ethernet")]
    public class NewMotor5ControlModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
           
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<NewMotor5ControlView>();
            containerRegistry.RegisterSingleton<MotorEntity>();
        }
    }
    

}
