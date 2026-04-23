using System;
using System.IO;
using Microsoft.ML.OnnxRuntime;
using NLog;
using UtilityTools.Modules.NavigationImagePositioning.Model;

namespace UtilityTools.Modules.NavigationImagePositioning.Services
{
    /// <summary>
    /// 导航图 YOLO ONNX 推理：持有 <see cref="InferenceSession"/>，调用
    /// <see cref="UltralyticsYoloV8OnnxPipeline"/> 做与 Ultralytics 示例一致的 letterbox/解码/NMS。
    /// </summary>
    public sealed class NavigationYoloInferenceService : INavigationYoloInferenceService
    {
        private static readonly ILogger _log = LogManager.GetCurrentClassLogger();
        private InferenceSession? _session;

        public bool IsModelLoaded => _session != null;

        public void LoadModel(string onnxPath)
        {
            if (string.IsNullOrWhiteSpace(onnxPath))
            {
                throw new ArgumentException("模型路径无效。", nameof(onnxPath));
            }

            if (!File.Exists(onnxPath))
            {
                throw new FileNotFoundException("未找到 ONNX 文件。", onnxPath);
            }

            _session?.Dispose();
            _session = new InferenceSession(onnxPath);
            _log.Info("已加载导航图 YOLO 模型: {0}", onnxPath);
        }

        public void UnloadModel()
        {
            if (_session == null)
            {
                return;
            }

            _session.Dispose();
            _session = null;
            _log.Info("已卸载导航图 YOLO 模型。");
        }

        public NavigationDetectOutcome DetectFromFile(string imagePath)
        {
            if (_session == null)
            {
                _log.Warn("未加载模型，跳过检测。");
                return new NavigationDetectOutcome
                {
                    Detections = Array.Empty<NavigationDetectionResult>()
                };
            }

            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                _log.Warn("图像路径无效: {0}", imagePath);
                return new NavigationDetectOutcome
                {
                    Detections = Array.Empty<NavigationDetectionResult>()
                };
            }

            try
            {
                return UltralyticsYoloV8OnnxPipeline.Run(
                    _session,
                    imagePath,
                    SampleStageYoloLabels.All);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "ONNX 推理或后处理失败");
                return new NavigationDetectOutcome
                {
                    Detections = Array.Empty<NavigationDetectionResult>()
                };
            }
        }

        /// <summary>实现 <see cref="IDisposable"/>，与模块生命周期一致时释放原生会话资源。</summary>
        public void Dispose()
        {
            UnloadModel();
        }
    }
}
