
using Prism.Ioc;
using Prism.Modularity;
using System.Reflection;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.FDC12CHVBox.Model;
using UtilityTools.Modules.FDC12CHVBox.ViewModels;
using UtilityTools.Modules.FDC12CHVBox.Views;


namespace UtilityTools.Modules.FDC12CHVBox
{

    /// <summary>
    /// Fudan 12-Channel Customized High-Voltage Box
    /// </summary>
    [CustomModule(ModuleName = "FDC12CHVBox", Title = "定制12路高压箱", Tip = "串口通讯", Icon = "AxisArrowInfo")]
    public class FDC12CHVBoxModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<FDC12CHVBoxView, FDC12CHVBoxViewModel>();
            containerRegistry.RegisterSingleton<FDC12CHVBoxModel>();
        }
    }
}
