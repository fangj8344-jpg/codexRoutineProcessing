using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.ImageMagic.Views;
using UtilityTools.Modules.ImageMagic.ViewModels;

namespace UtilityTools.Modules.ImageMagic
{
    [CustomModule(ModuleName = "ImageMagic", Title = "图片查看", Tip = "图片查看器工具", Icon = "ImageMultiple")]
    public class ImageMagicModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ImageMagicView, ImageMagicViewModel>();
        }
    }
}