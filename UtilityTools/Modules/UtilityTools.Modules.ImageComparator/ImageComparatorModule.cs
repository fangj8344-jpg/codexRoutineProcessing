using Prism.Ioc;
using Prism.Modularity;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.ImageComparator.Views;
using UtilityTools.Modules.ImageComparator.ViewModels;

namespace UtilityTools.Modules.ImageComparator
{
    [CustomModule(ModuleName = "ImageComparator", Title = "图像比较工具", Tip = "比较两张图片的重叠区域", Icon = "ImageCompare")]
    public class ImageComparatorModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ImageComparatorView, ImageComparatorViewModel>();
        }
    }
} 