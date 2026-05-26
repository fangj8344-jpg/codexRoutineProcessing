using Prism.Ioc;
using Prism.Modularity;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.KeyboardTest.Views;

namespace UtilityTools.Modules.KeyboardTest
{
    [CustomModule(ModuleName = "KeyboardTest", Title = "键盘测试", Tip = "CSSEC雷达键盘HID测试", Icon = "Keyboard")]
    public class KeyboardTestModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<KeyboardTestView>();
        }
    }
}
