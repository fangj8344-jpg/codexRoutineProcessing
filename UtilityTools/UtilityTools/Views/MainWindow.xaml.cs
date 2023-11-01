using MaterialDesignThemes.Wpf;
using Prism.Events;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using UtilityTools.Core.Extension;

namespace UtilityTools.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(IEventAggregator aggregator)
        {
            InitializeComponent();

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
    }
}
