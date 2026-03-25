using UserControl = System.Windows.Controls.UserControl;
using Prism.Regions;
using UtilityTools.Core;

namespace UtilityTools.Modules.MultiAxisTest.Views
{
    /// <summary>
    /// MultiAxisTestView.xaml 的交互逻辑
    /// </summary>
    public partial class MultiAxisTestView : UserControl
    {
        public MultiAxisTestView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // 进入模块时默认跳到配置页
            var rm = RegionManager.GetRegionManager(this);
            if (rm?.Regions.ContainsRegionWithName(RegionNames.ContentRegion) == true)
            {
                rm.Regions[RegionNames.ContentRegion].RequestNavigate(nameof(MultiAxisScanView));
            }
        }
    }
}

