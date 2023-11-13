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
using UtilityTools.Modules.ImageAnalyzer.ViewModels;

namespace UtilityTools.Modules.ImageAnalyzer.Views
{
    /// <summary>
    /// ImageHistogram.xaml 的交互逻辑
    /// </summary>
    public partial class ImageHistogram : UserControl
    {
        public ImageHistogram()
        {
            InitializeComponent();
            this.Loaded += ImageAnalyzerView_Loaded;
        }

        private void ImageAnalyzerView_Loaded(object sender, RoutedEventArgs e)
        {
            var dataContext = this.DataContext as ImageHistogramViewModel;
            if (dataContext == null) { return; }

            dataContext.InkCanvas = this.inkCanvas;
            dataContext.ImageViewer = this.imageViewer;
        }
    }
}
