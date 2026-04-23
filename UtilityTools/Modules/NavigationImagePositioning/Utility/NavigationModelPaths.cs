using System.IO;

namespace UtilityTools.Modules.NavigationImagePositioning.Utility
{
    /// <summary>
    /// 主程序输出目录下的 NavigationImagePositioning/OnnxModels（与 UtilityTools 主 exe 同级的子目录结构）。
    /// </summary>
    public static class NavigationModelPaths
    {
        /// <summary>与主程序 exe 同基目录下的子文件夹名，用于与其它模块的模型目录隔离。</summary>
        public const string AppSubFolder = "NavigationImagePositioning";
        /// <summary>存放 <c>*.onnx</c> 的文件夹名，完整路径为 BaseDirectory / AppSubFolder / OnnxFolderName。</summary>
        public const string OnnxFolderName = "OnnxModels";

        /// <summary>返回当前进程基目录下 Navigation 模块的 ONNX 模型目录（不存在时由 ViewModel 侧尝试创建）。</summary>
        public static string GetOnnxDirectory()
        {
            return Path.Combine(AppContext.BaseDirectory, AppSubFolder, OnnxFolderName);
        }
    }
}
