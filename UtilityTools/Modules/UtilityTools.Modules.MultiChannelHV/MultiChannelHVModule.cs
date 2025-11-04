
using Prism.Ioc;
using Prism.Modularity;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.MultiChannelHV.Model;
using UtilityTools.Modules.MultiChannelHV.ViewModels;
using UtilityTools.Modules.MultiChannelHV.Views;

namespace UtilityTools.Modules.MultiChannelHV
{
    [CustomModule(ModuleName = "MultiChannelHV", Title = "多路高压箱", Tip = "UDP连接的控制板", Icon = "Ethernet")]
    public class MultiChannelHVModule:IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MultiChannelHVView, MultiChannelHVViewModel>();
            containerRegistry.RegisterSingleton<MultiChannelHVModel>();
            // 注册对话框：键名"PasswordDialog"用于后续调用，视图类型是PasswordDialog
            containerRegistry.RegisterDialog<PasswordDialogView, PasswordDialogViewModel>("PasswordDialog");
        }
       
    }

}
