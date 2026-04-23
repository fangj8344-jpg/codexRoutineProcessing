using Prism.Ioc;
using Prism.Modularity;
using UtilityTools.Core.Interface;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.MultiAxisTest.Model;
using UtilityTools.Modules.MultiAxisTest.Services;
using UtilityTools.Modules.MotorTest.Service;
using UtilityTools.Modules.MultiAxisTest.Views;

namespace UtilityTools.Modules.MultiAxisTest
{
    /// <summary>
    /// 多轴电机测试模块
    /// </summary>
    [CustomModule(ModuleName = "MultiAxisTest", Title = "多轴测试", Tip = "多轴电机测试", Icon = "AxisArrowInfo")]
    public class MultiAxisTestModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<MultiAxisWorkflowState>();
            containerRegistry.Register<ITestReportService>(container => container.Resolve<MultiAxisWorkflowState>());
            containerRegistry.Register<IFirmwareUpgradeWorkflowState>(container => container.Resolve<MultiAxisWorkflowState>());
            containerRegistry.RegisterSingleton<IFirmwareUpgradeCoordinator, MultiAxisFirmwareUpgradeFlow>();

            // 入口壳：MultiAxisTestView（内部 region 控制配置/扫码/测试）
            containerRegistry.RegisterForNavigation<MultiAxisTestView, ViewModels.MultiAxisTestShellViewModel>();
            containerRegistry.RegisterForNavigation<MultiAxisScanView, ViewModels.MultiAxisScanViewModel>();
            containerRegistry.RegisterForNavigation<MultiAxisRunView, ViewModels.MultiAxisRunViewModel>();
        }
    }
}

