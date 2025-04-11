using Prism.Ioc;
using Prism.Modularity;
using System;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.Test485ChipTool.Model;
using UtilityTools.Modules.Test485ChipTool.ViewModels;
using UtilityTools.Modules.Test485ChipTool.Views;

namespace UtilityTools.Modules.Test485ChipTool
{
    [CustomModule(ModuleName = "Test485ChipTool", Title = "485–æ∆¨≤‚ ‘π§æﬂ", Tip = "", Icon = "")]
    public class Test485ChipToolModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<Test485ChipToolView, Test485ChipToolViewModel>();
            containerRegistry.RegisterSingleton<Test485Model>();
        }
    }
}
