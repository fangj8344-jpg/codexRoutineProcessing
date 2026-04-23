using UtilityTools.Modules.NavigationImagePositioning.Model;

namespace UtilityTools.Modules.NavigationImagePositioning.Services
{
    /// <summary>
    /// 导航图 YOLO ONNX 推理；负责加载模型、预处理、后处理（含 NMS）。
    /// </summary>
    public interface INavigationYoloInferenceService : IDisposable
    {
        /// <summary>当前是否已创建可用的 ONNX 推理会话（未加载或加载失败为 false）。</summary>
        bool IsModelLoaded { get; }

        /// <summary>从磁盘路径加载并替换当前 ONNX 模型。</summary>
        void LoadModel(string onnxPath);

        /// <summary>释放当前会话，释放后可再次 <see cref="LoadModel"/>。</summary>
        void UnloadModel();

        /// <summary>
        /// 对图像做检测（Letterbox+YOLOv8 后处理+NMS），返回原图坐标与叠加可视化图。
        /// </summary>
        NavigationDetectOutcome DetectFromFile(string imagePath);
    }
}
