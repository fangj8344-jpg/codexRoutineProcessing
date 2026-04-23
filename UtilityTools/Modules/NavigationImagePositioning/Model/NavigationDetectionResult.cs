namespace UtilityTools.Modules.NavigationImagePositioning.Model
{
    /// <summary>
    /// 单次检测结果（钉、样品台等），框为原图像素坐标。
    /// </summary>
    public sealed class NavigationDetectionResult
    {
        public string Label { get; init; } = "";

        /// <summary>模型类别 id，未使用可为 -1。</summary>
        public int ClassId { get; init; }

        public double Confidence { get; init; }

        /// <summary>边界框左上角的 X（像素，相对原图）。</summary>
        public double Left { get; init; }
        /// <summary>边界框左上角的 Y（像素，相对原图）。</summary>
        public double Top { get; init; }
        /// <summary>边界框宽度（像素）。</summary>
        public double Width { get; init; }
        /// <summary>边界框高度（像素）。</summary>
        public double Height { get; init; }
    }
}
