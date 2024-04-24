using Prism.Ioc;
using Prism.Modularity;
using System.Reflection;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.CmdTestTool.Model;
using UtilityTools.Modules.CmdTestTool.Views;

namespace UtilityTools.Modules.CmdTestTool
{
    [CustomModule(ModuleName = "CmdTestTool", Title = "指令测试工具", Tip = "UDP通信", Icon = "AxisArrowInfo")]
    public class CmdTestToolModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<CmdTestToolView>();
            containerRegistry.RegisterSingleton<CmdModel>();
        }
    }
}