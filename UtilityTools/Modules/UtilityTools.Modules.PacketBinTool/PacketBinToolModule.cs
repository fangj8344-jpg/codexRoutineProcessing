using Prism.Ioc;
using Prism.Modularity;
using System;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.PacketBinTool.ViewModels;
using UtilityTools.Modules.PacketBinTool.Views;

namespace UtilityTools.Modules.PacketBinTool
{
    [CustomModule(ModuleName = "PacketBinTool", Title = "打包工具", Tip = "", Icon = "")]
    public class PacketBinToolModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<PacketBinToolView,PacketBinToolViewModel>();
          
        }
    }
}
