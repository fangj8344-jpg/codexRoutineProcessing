using MaterialDesignThemes.Wpf;
using Prism.Events;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Input;
using UtilityTools.Core.Event;
using UtilityTools.Core.Extension;


namespace UtilityTools.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private StringBuilder _barcodeBuffer = new StringBuilder();
        private DateTime _lastInputTime = DateTime.Now;
        private readonly TimeSpan _inputThreshold = TimeSpan.FromMilliseconds(500);
        private bool _barcodeCaptureEnabled = false;

        //声明 Prism 的事件聚合器013157/001252/ZP030506J08/260317001

        private readonly IEventAggregator _eventAggregator;

        //通过依赖注入获取IEventAggregator
        public MainWindow(IEventAggregator aggregator)
        {
            InitializeComponent();
            _eventAggregator = aggregator;

            // 全局捕获窗口内所有按键（即使子控件已处理），避免必须点击输入框后才能扫码
            this.AddHandler(Keyboard.PreviewKeyDownEvent, new KeyEventHandler(Window_PreviewKeyDown), true);
            

            this.btnMin.Click += BtnMin_Click;
            this.btnMax.Click += BtnMax_Click;
            this.btnClose.Click += BtnClose_Click;
             
            this.ColorZone.MouseMove += ColorZone_MouseMove;
            this.ColorZone.MouseDoubleClick += ColorZone_MouseDoubleClick;

            menuBar.SelectionChanged += (s, e) =>
            {
                this.drawerHost.IsLeftDrawerOpen = false;
            };

            //注册提示消息
            aggregator.RegisterMessage(arg =>
            {
                Snackbar.Dispatcher.BeginInvoke(() =>
                {
                    Snackbar.MessageQueue.Enqueue(arg.Message);
                });
            });

            // 注册等待消息窗口
            aggregator.Register(arg =>
            {
                DialogHost.IsOpen = arg.IsOpen;
                if (DialogHost.IsOpen)
                {
                    DialogHost.DialogContent = new ProgressView(arg.Title);
                }
            });

            // 仅在需要扫码的页面（如扫码页）开启全局扫码捕获
            aggregator.GetEvent<BarcodeCaptureEnabledEvent>().Subscribe(enabled =>
            {
                _barcodeCaptureEnabled = enabled;
                if (!enabled)
                {
                    _barcodeBuffer.Clear();
                }
                else
                {
                    // 中文 IME 开启时，模拟键盘的扫码枪只会把字母交给输入法，导致缓冲区只剩数字和 /（样品台英文型号整段丢失）
                    try
                    {
                        InputMethod.Current.ImeState = InputMethodState.Off;
                        InputLanguageManager.Current.CurrentInputLanguage = new CultureInfo("en-US");
                    }
                    catch
                    {
                        // 无 IME 或非 WPF 输入线程时忽略
                    }
                }
            }, ThreadOption.UIThread);
        }


        private void ColorZone_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void ColorZone_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Process.GetCurrentProcess().Kill();
        }

        private void BtnMax_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void BtnMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }


        private static bool TryConvertKeyToBarcodeChar(Key key, bool isShiftDown, out char ch)
        {
            ch = '\0';

            if (key >= Key.D0 && key <= Key.D9)
            {
                ch = (char)('0' + (key - Key.D0));
                return true;
            }

            if (key >= Key.NumPad0 && key <= Key.NumPad9)
            {
                ch = (char)('0' + (key - Key.NumPad0));
                return true;
            }

            if (key >= Key.A && key <= Key.Z)
            {
                bool isCapsLockOn = Keyboard.IsKeyToggled(Key.CapsLock);
                bool isUpper = isCapsLockOn ^ isShiftDown;
                ch = isUpper
                    ? (char)('A' + (key - Key.A))
                    : (char)('a' + (key - Key.A));
                return true;
            }

            switch (key)
            {
                case Key.Divide: ch = '/'; return true;
                case Key.Subtract: ch = '-'; return true;
                case Key.Decimal: ch = '.'; return true;
                case Key.OemMinus: ch = '-'; return true;
                case Key.OemPlus: ch = '='; return true;
                case Key.OemOpenBrackets: ch = '['; return true;
                case Key.Oem6: ch = ']'; return true;
                case Key.Oem5: ch = '\\'; return true;
                case Key.Oem1: ch = ';'; return true;
                case Key.OemQuotes: ch = '\''; return true;
                case Key.OemComma: ch = ','; return true;
                case Key.OemPeriod: ch = '.'; return true;
                case Key.Oem2: ch = '/'; return true;
                case Key.Oem3: ch = '`'; return true;
                case Key.Space: ch = ' '; return true;
                default: return false;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!_barcodeCaptureEnabled)
            {
                return;
            }

            Key actualKey = e.Key;
            if (actualKey == Key.ImeProcessed)
            {
                actualKey = e.ImeProcessedKey;
            }
            else if (actualKey == Key.System)
            {
                actualKey = e.SystemKey;
            }

            DateTime now = DateTime.Now;
            if ((now - _lastInputTime) > _inputThreshold)
            {
                _barcodeBuffer.Clear();
            }

            if (actualKey == Key.Enter || actualKey == Key.Return)
            {
                if ((now - _lastInputTime) <= _inputThreshold && _barcodeBuffer.Length > 0)
                {
                    string finalBarcode = _barcodeBuffer.ToString();
                    
                    _barcodeBuffer.Clear();
                    e.Handled = true;

                    string[] parts = finalBarcode.Split('/');
                    // 仅识别新格式：采购/生产/物料编码/操作员/电镜/样品台/日期与序号（共 7 段）
                    if (parts.Length >= 7)
                    {
                        _eventAggregator.GetEvent<BarcodeScannedEvent>().Publish(parts);
                    }

                }
            }
            else
            {
                bool isShiftDown = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
                if (TryConvertKeyToBarcodeChar(actualKey, isShiftDown, out char ch))
                {
                    _barcodeBuffer.Append(ch);
                    _lastInputTime = now;
                }
            }

        }



    }
}
