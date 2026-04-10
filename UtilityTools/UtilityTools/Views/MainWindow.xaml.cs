using MaterialDesignThemes.Wpf;
using Prism.Events;
using System;
using System.Diagnostics;
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
        private readonly TimeSpan _inputThreshold = TimeSpan.FromMilliseconds(200);

        //声明 Prism 的事件聚合器013157/001252/ZP030506J08/260317001

        private readonly IEventAggregator _eventAggregator;

        //通过依赖注入获取IEventAggregator
        public MainWindow(IEventAggregator aggregator)
        {
            InitializeComponent();
            _eventAggregator = aggregator;

            this.PreviewKeyDown += Window_PreviewKeyDown;
            

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
            DateTime now = DateTime.Now;
            if ((now - _lastInputTime) > _inputThreshold)
            {
                _barcodeBuffer.Clear();
            }

            if (e.Key == Key.Enter)
            {
                if ((now - _lastInputTime) <= _inputThreshold && _barcodeBuffer.Length > 0)
                {
                    string finalBarcode = _barcodeBuffer.ToString();
                    
                    _barcodeBuffer.Clear();
                    e.Handled = true;

                    string[] parts = finalBarcode.Split('/');
                    if (parts.Length >= 4)
                    {
                        _eventAggregator.GetEvent<BarcodeScannedEvent>().Publish(parts);
                    }

                }
            }
            else
            {
                bool isShiftDown = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
                if (TryConvertKeyToBarcodeChar(e.Key, isShiftDown, out char ch))
                {
                    _barcodeBuffer.Append(ch);
                    _lastInputTime = now;
                }
            }

        }



    }
}
