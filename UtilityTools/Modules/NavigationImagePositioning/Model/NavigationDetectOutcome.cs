using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace UtilityTools.Modules.NavigationImagePositioning.Model
{
    /// <summary>
    /// 一次推理的检测结果 + 可显示的叠加图（原图上的框）。
    /// </summary>
    public sealed class NavigationDetectOutcome
    {
        public IReadOnlyList<NavigationDetectionResult> Detections { get; init; } = new List<NavigationDetectionResult>();

        /// <summary>在原图尺寸上绘制检测框后的图像（失败时为 null）。</summary>
        public BitmapSource? AnnotatedImage { get; init; }
    }
}
