using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.ImageAnalyzer.Views;

namespace UtilityTools.Modules.ImageAnalyzer
{

    [CustomModule(ModuleName = "ImageAnalyzer", Title = "图像分析", Tip = "图像统计信息分析", Icon = "ImageMultiple ")]
    public class ImageAnalyzerModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ImageAnalyzerView>();
        }
    }
}