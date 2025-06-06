using Prism.Ioc;
using Prism.Modularity;
using UtilityTools.Core.Mvvm;
using Zeptools.Modules.AuthorityManager.Views;

namespace Zeptools.Modules.AuthorityManager
{
    [CustomModule(ModuleName = "AuthorityManager", Title = "授权管理", Tip = "授权管理模块", Icon = "KeyVariant")]
    public class AuthorityManagerModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider) { }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<AuthorityManagerView>();
        }
    }
} 