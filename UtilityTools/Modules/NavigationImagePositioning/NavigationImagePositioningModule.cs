using Prism.Ioc;
using Prism.Modularity;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.NavigationImagePositioning.Services;
using UtilityTools.Modules.NavigationImagePositioning.Views;

namespace UtilityTools.Modules.NavigationImagePositioning
{
    /// <summary>
    /// Prism 功能模块：注册导航图 YOLO 推理服务与「导航图定位」视图，供主程序侧栏进入。
    /// </summary>
    [CustomModule(
        ModuleName = "NavigationImagePositioning",
        Title = "导航图定位",
        Tip = "使用 YOLO ONNX 在导航图中标记钉位与样品台",
        Icon = "FlashTriangle")]
    public class NavigationImagePositioningModule : IModule
    {
        /// <summary>模块加载完成时暂无额外初始化逻辑。</summary>
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        /// <summary>单例注册 YOLO 服务；将本模块主视图注册到 Prism 导航。</summary>
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<INavigationYoloInferenceService, NavigationYoloInferenceService>();
            containerRegistry.RegisterForNavigation<NavigationImagePositioningView>();
        }
    }
}
