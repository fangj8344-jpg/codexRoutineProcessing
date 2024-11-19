using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Reflection;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.OtaTool.Model;
using UtilityTools.Modules.OtaTool.Views;

namespace UtilityTools.Modules.OtaTool
{
    [CustomModule(ModuleName = "OtaTool", Title = "远程升级工具", Tip = "串口/网络通讯", Icon = "FlashTriangle")]
    public class OtaToolModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<OtaToolView>(); 
            containerRegistry.RegisterSingleton<OtaModel>();
        }
    }
}
