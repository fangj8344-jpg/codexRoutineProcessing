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
    /// Interaction logic for ViewA.xaml
    /// </summary>
    public partial class ImageAnalyzerView : UserControl
    {
        public ImageAnalyzerView()
        {
            InitializeComponent();

            this.Loaded += ImageAnalyzerView_Loaded;
        }

        private void ImageAnalyzerView_Loaded(object sender, RoutedEventArgs e)
        {
            var dataContext = this.DataContext as ImageAnalyzerViewModel;
            if (dataContext == null) { return; }

            dataContext.InkCanvas = this.inkCanvas;
            dataContext.ImageViewer = this.imageViewer;
        }
    }
}
