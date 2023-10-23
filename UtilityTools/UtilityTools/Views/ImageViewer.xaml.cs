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

namespace UtilityTools.Views
{
    /// <summary>
    /// ImageViewer.xaml 的交互逻辑
    /// </summary>
    public partial class ImageViewer : UserControl
    {
        public ImageViewer()
        {
            InitializeComponent();

            this.SizeChanged += ImageViewer_SizeChanged;
        }


        public Image Image { get { return image; } }

        public BitmapImage ImageSource
        {
            get { return (BitmapImage)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(BitmapImage), typeof(ImageViewer), new PropertyMetadata(null, OnImageSourcePropertyChanged));
        private static void OnImageSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageViewer viewer = (ImageViewer)d;
            if (viewer != null)
            {
                if (e.NewValue != null)
                {
                    viewer.image.Source = (BitmapImage)e.NewValue;
                    //viewer.ResizeImageToFit();
                    if (e.OldValue == null)
                        viewer.ResizeImageToFit();
                }
            }
        }

        private void ImageViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeImageToFit();
        }

        private void ResizeImageToFit()
        {
            if (ImageSource == null)
                return;

            double min = Math.Min(grid.ActualWidth, grid.ActualHeight);

            image.Width = min;
            image.Height = min;

            grid.Width = min;
            grid.Height = min;

            //scaleTransform.ScaleX = 1;
            //scaleTransform.ScaleY = 1;
            //translateTransform.X = 0;
            //translateTransform.Y = 0;
        }
    }
}
