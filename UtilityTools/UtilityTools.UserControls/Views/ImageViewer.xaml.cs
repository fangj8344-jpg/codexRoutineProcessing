using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UtilityTools.UserControls.Views
{
    /// <summary>
    /// ImageViewer.xaml 的交互逻辑
    /// </summary>
    public partial class ImageViewer : UserControl
    {
        #region Constructor
        public ImageViewer()
        {
            InitializeComponent();

            // 注册事件处理程序
            this.SizeChanged += ImageViewer_SizeChanged;
            image.MouseLeftButtonDown += Image_MouseLeftButtonDown;
            image.MouseWheel += Image_MouseWheel;
            image.MouseMove += Image_MouseMove;
            image.MouseLeftButtonDown += Image_MouseLeftButtonDown;
            image.MouseLeftButtonUp += Image_MouseLeftButtonUp;
        }

        ~ImageViewer()
        {

        }
        #endregion

        #region Field
        private Point _startPoint;
        #endregion

        #region Property

        public BitmapSource ImageSource
        {
            get { return (BitmapSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(BitmapSource), typeof(ImageViewer), new PropertyMetadata(null, OnImageSourcePropertyChanged));

        private static void OnImageSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageViewer viewer = (ImageViewer)d;
            if (viewer != null)
            {
                if (e.NewValue != null)
                {
                    viewer.image.Source = (BitmapSource)e.NewValue;
                    //viewer.ResizeImageToFit();
                    if (e.OldValue == null)
                        viewer.ResizeImageToFit();
                }
            }
        }
        #endregion

        #region PrivateMethod
        private void ResizeImageToFit()
        {
            if (ImageSource == null)
                return;

            double min = Math.Min(grid.ActualWidth, grid.ActualHeight);

            image.Width = min;
            image.Height = min;

            scaleTransform.ScaleX = 1;
            scaleTransform.ScaleY = 1;
            translateTransform.X = 0;
            translateTransform.Y = 0;
        }
        #endregion


        #region Event
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 记录鼠标按下的位置
            _startPoint = e.GetPosition(image);
            image.CaptureMouse();
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // 释放鼠标并阻止捕获
            image.ReleaseMouseCapture();
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            // 判断是否正在拖动
            if (image.IsMouseCaptured)
            {
                if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                {
                    Point endPoint = e.GetPosition(image);
                    //左键按住才能拖拽
                    translateTransform.X += (endPoint.X - _startPoint.X) * scaleTransform.ScaleX;
                    translateTransform.Y += (endPoint.Y - _startPoint.Y) * scaleTransform.ScaleY;
                }
            }
        }

        private void ImageViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // 调整大小以适应窗口
            ResizeImageToFit();
        }

        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var image = sender as Image;
            Point centerPoint = e.MouseDevice.GetPosition(image);

            double scale = (double)e.Delta / 2000.0;//放大倍数

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                //按下Ctrl键，缩放速度加快十倍
                scale = (double)e.Delta / 200.0;
            }
            if (this.scaleTransform.ScaleX + scale < 0.3 && scaleTransform.ScaleY + scale < 0.3 && e.Delta < 0)
            {
                //限制缩放比例

                //设置缩放倍数
                scaleTransform.ScaleX = 0.3;
                scaleTransform.ScaleY = 0.3;

                //设置缩放的中心点
                scaleTransform.CenterX = centerPoint.X;
                scaleTransform.CenterY = centerPoint.Y;
            }
            else
            {
                //设置缩放倍数
                scaleTransform.CenterX = centerPoint.X;
                scaleTransform.CenterY = centerPoint.Y;
                scaleTransform.ScaleX += scale;
                scaleTransform.ScaleY += scale;
                //设置缩放的中心点
                Point centerPointAfterScale = e.MouseDevice.GetPosition(image);
                translateTransform.X += (centerPointAfterScale.X - centerPoint.X) * scaleTransform.ScaleX;
                translateTransform.Y += (centerPointAfterScale.Y - centerPoint.Y) * scaleTransform.ScaleY;
            }
        }
        #endregion
    }
}
