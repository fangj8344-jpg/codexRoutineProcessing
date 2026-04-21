using System.Windows;
using System.Windows.Input;
using UtilityTools.Modules.MultiAxisTest.ViewModels;
using UserControl = System.Windows.Controls.UserControl;

namespace UtilityTools.Modules.MultiAxisTest.Views
{
    public partial class MultiAxisScanView : UserControl
    {
        private Window _hostWindow;

        public MultiAxisScanView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _hostWindow = Window.GetWindow(this);
            if (_hostWindow != null)
                _hostWindow.PreviewKeyDown += OnHostWindowPreviewKeyDown;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_hostWindow != null)
            {
                _hostWindow.PreviewKeyDown -= OnHostWindowPreviewKeyDown;
                _hostWindow = null;
            }
        }

        /// <summary>
        /// 开发用：本页显示时按 Ctrl+Shift+T，无真实扫码进入测试（占位数据）。不向最终用户展示入口。
        /// </summary>
        private void OnHostWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != (ModifierKeys.Control | ModifierKeys.Shift) || e.Key != Key.T)
                return;
            if (DataContext is MultiAxisScanViewModel vm)
            {
                vm.EnterTestWithoutScanCommand.Execute();
                e.Handled = true;
            }
        }
    }
}
