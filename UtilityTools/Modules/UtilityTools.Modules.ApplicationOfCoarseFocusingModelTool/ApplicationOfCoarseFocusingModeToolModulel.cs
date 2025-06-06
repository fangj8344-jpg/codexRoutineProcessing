using UtilityTools.Modules.ApplicationOfCoarseFocusingModelTool.Views;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Reflection;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.ApplicationOfCoarseFocusingModelTool.ViewModels;

namespace UtilityTools.Modules.ApplicationOfCoarseFocusingModelTool
{
    [CustomModule(ModuleName = "ApplicationOfCoarseFocusingModelTool", Title = "粗焦距模型应用工具", Tip = "", Icon = "")]
    public class ApplicationOfCoarseFocusingModeToolModulel: IModule
    {

        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ApplicationOfCoarseFocusingModelToolView, ApplicationOfCoarseFocusingModelToolViewModel>();
        }
    }
}
