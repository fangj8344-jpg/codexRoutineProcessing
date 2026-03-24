using Prism.Ioc;
using Prism.Services.Dialogs;
using UtilityTools.Core.Dialog;
using UtilityTools.Modules.MotorTest.ViewModels;

namespace UtilityTools.Modules.MultiAxisTest.ViewModels
{
    /// <summary>
    /// 复用三轴测试窗口的 ViewModel 逻辑
    /// </summary>
    public class MultiAxisTestViewModel : ThreeAxisTestModelWindowsViewModel
    {
        public MultiAxisTestViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService)
            : base(containerProvider, dialogHostService)
        {
        }
    }
}

