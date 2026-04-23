namespace UtilityTools.Modules.NavigationImagePositioning.Model
{
    /// <summary>
    /// 下拉项：展示短文件名，内部用完整路径加载。
    /// </summary>
    public sealed class NavigationOnnxFileItem
    {
        public NavigationOnnxFileItem(string fullPath)
        {
            FullPath = fullPath;
            FileName = System.IO.Path.GetFileName(fullPath);
        }

        /// <summary>仅用于界面展示的文件名（含扩展名）。</summary>
        public string FileName { get; }
        /// <summary>磁盘上的绝对路径，供加载与存在性检查。</summary>
        public string FullPath { get; }
    }
}
