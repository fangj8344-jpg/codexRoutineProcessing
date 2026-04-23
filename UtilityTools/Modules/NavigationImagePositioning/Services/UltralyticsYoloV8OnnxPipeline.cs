using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using NLog;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Windows.Media;
using UtilityTools.Modules.NavigationImagePositioning.Model;

namespace UtilityTools.Modules.NavigationImagePositioning.Services
{
    /// <summary>
    /// 与 Ultralytics 官方 <c>examples/YOLOv8-ONNXRuntime/main.py</c> 一致的预处理/后处理（letterbox、transpose、pad、gain、NMS、画框）。
    /// 输出张量支持 <c>[1,4+nc,N]</c> 或 <c>[1,N,4+nc]</c>，类别数 <see cref="SampleStageYoloLabels.All"/> 可配置。
    /// </summary>
    internal static class UltralyticsYoloV8OnnxPipeline
    {
        private static readonly ILogger _log = LogManager.GetCurrentClassLogger();

        private const float DefaultConf = 0.5f;
        private const float DefaultIou = 0.5f;

        /// <summary>读图 → letterbox 至模型输入尺寸 → 推理 → 后处理 + NMS → 在原 BGR 上画框并转为 WPF 位图。</summary>
        public static NavigationDetectOutcome Run(
            InferenceSession session,
            string imagePath,
            IReadOnlyList<string> classNames,
            float confThreshold = DefaultConf,
            float iouThreshold = DefaultIou)
        {
            using var bgr = Cv2.ImRead(imagePath, ImreadModes.Color);
            if (bgr.Empty())
            {
                _log.Warn("无法读取图像: {0}", imagePath);
                return new NavigationDetectOutcome
                {
                    Detections = Array.Empty<NavigationDetectionResult>(),
                    AnnotatedImage = null
                };
            }

            int origW = bgr.Cols, origH = bgr.Rows;
            var inputName = session.InputMetadata.Keys.First();
            var inMeta = session.InputMetadata[inputName];
            int inputH = PickFixedDim(inMeta.Dimensions, 2, 640);
            int inputW = PickFixedDim(inMeta.Dimensions, 3, 640);

            using (var rgb = new Mat())
            {
                Cv2.CvtColor(bgr, rgb, ColorConversionCodes.BGR2RGB);
                var (padded, padTop, padLeft, gain) = Letterbox(rgb, inputW, inputH);
                using (padded)
                {
                    var inputTensor = PaddedToNchwTensor(padded);
                    using (var output = session.Run(new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(inputName, inputTensor) }))
                    {
                        var first = output.First();
                        if (first.Value is not DenseTensor<float> dense)
                        {
                            _log.Error("ONNX 第一个输出不是 float 张量。");
                            return PackFailure(bgr);
                        }

                        if (!TryParseOutput(dense, out var n, out var cTotal, out var getter))
                        {
                            _log.Error("不支持的检测输出布局，张量维: {0}", string.Join(',', dense.Dimensions.ToArray()));
                            return PackFailure(bgr);
                        }

                        // 与 main.py: outputs[:,0]-= pad[1], outputs[:,1]-= pad[0]（pad 为 (top,left)）
                        var dets = PostProcessYoloV8(
                            n,
                            cTotal,
                            getter,
                            classNames,
                            confThreshold,
                            iouThreshold,
                            padTop,
                            padLeft,
                            gain,
                            origW,
                            origH);
                        return DrawAndPack(bgr, dets);
                    }
                }
            }
        }

        /// <summary>从 ONNX 输入 shape 中读取固定高/宽；动态维（0 或 -1）时退回默认 640。</summary>
        private static int PickFixedDim(int[]? dims, int index, int fallback)
        {
            if (dims == null || index >= dims.Length)
            {
                return fallback;
            }

            return dims[index] > 0 ? (int)dims[index] : fallback;
        }

        /// <summary>等比缩放后居中填充到网络输入大小，边沿用灰 114 填充；返回上/左 padding 与相对原图的缩放系数 gain。</summary>
        private static (Mat Padded, int Top, int Left, float Gain) Letterbox(Mat rgb, int inW, int inH)
        {
            int w = rgb.Cols, h = rgb.Rows;
            double r = Math.Min((double)inH / h, (double)inW / w);
            int newW = (int)Math.Round(w * r);
            int newH = (int)Math.Round(h * r);
            using (var resized = new Mat())
            {
                Cv2.Resize(rgb, resized, new OpenCvSharp.Size(newW, newH), 0, 0, InterpolationFlags.Linear);
                double dw = (inW - newW) / 2.0, dh = (inH - newH) / 2.0;
                int top = (int)Math.Round(dh - 0.1);
                int bottom = (int)Math.Round(dh + 0.1);
                int left = (int)Math.Round(dw - 0.1);
                int right = (int)Math.Round(dw + 0.1);
                var padded = new Mat();
                Cv2.CopyMakeBorder(resized, padded, top, bottom, left, right, BorderTypes.Constant, new Scalar(114, 114, 114));
                return (padded, top, left, (float)r);
            }
        }

        /// <summary>将 H×W 已 letterbox 的 RGB 图转为 NCHW float、归一化到 0~1（与官方示例一致）。</summary>
        private static DenseTensor<float> PaddedToNchwTensor(Mat padded)
        {
            int h = padded.Rows, w = padded.Cols;
            var data = new float[1 * 3 * h * w];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var v = padded.At<Vec3b>(y, x);
                    data[0 * h * w + y * w + x] = v.Item0 / 255f;
                    data[1 * h * w + y * w + x] = v.Item1 / 255f;
                    data[2 * h * w + y * w + x] = v.Item2 / 255f;
                }
            }

            return new DenseTensor<float>(data, new[] { 1, 3, h, w });
        }

        /// <summary>支持检测头输出 <c>[1,4+nc,N]</c> 或 <c>[1,N,4+nc]</c>，并给出按 (anchorIndex, channel) 读值的委托。</summary>
        private static bool TryParseOutput(
            DenseTensor<float> tensor,
            out int n,
            out int cTotal,
            out Func<int, int, float> read)
        {
            var d = tensor.Dimensions.ToArray();
            read = (_, _) => 0f;
            n = 0;
            cTotal = 0;
            if (d.Length != 3 || d[0] != 1)
            {
                return false;
            }

            // [1, 4+nc, N] 或 [1, N, 4+nc]
            int a = d[1], b = d[2];
            cTotal = Math.Min(a, b);
            n = Math.Max(a, b);
            if (cTotal < 5)
            {
                return false;
            }

            if (a == cTotal)
            {
                read = (row, c) => tensor[0, c, row];
                return true;
            }

            read = (row, c) => tensor[0, row, c];
            return true;
        }

        /// <summary>将网络输出的中心点格式框还原到原图，过滤置信度、做 NMS，并附上类别名。</summary>
        private static IReadOnlyList<NavigationDetectionResult> PostProcessYoloV8(
            int numPred,
            int cTotal,
            Func<int, int, float> get,
            IReadOnlyList<string> classNames,
            float confTh,
            float iouTh,
            int padTop,
            int padLeft,
            float gain,
            int origW,
            int origH)
        {
            int nc = cTotal - 4;
            var boxes = new List<(float L, float T, float W, float H)>();
            var scores = new List<float>();
            var classes = new List<int>();

            for (int i = 0; i < numPred; i++)
            {
                var scoresSpan = new float[nc];
                for (int k = 0; k < nc; k++)
                {
                    scoresSpan[k] = get(i, 4 + k);
                }

                // 与 Ultralytics YOLOv8-ONNXRuntime 示例：多为已落在 [0,1] 的得分；若明显为 logits 再 Sigmoid
                if (scoresSpan.Any(s => s < 0f || s > 1.001f))
                {
                    for (int k = 0; k < nc; k++)
                    {
                        scoresSpan[k] = Sigmoid(scoresSpan[k]);
                    }
                }

                int best = 0;
                float bestScore = scoresSpan[0];
                for (int k = 1; k < nc; k++)
                {
                    if (scoresSpan[k] > bestScore)
                    {
                        bestScore = scoresSpan[k];
                        best = k;
                    }
                }

                if (bestScore < confTh)
                {
                    continue;
                }

                float x = get(i, 0) - padLeft;
                float y = get(i, 1) - padTop;
                float w = get(i, 2);
                float h = get(i, 3);

                float left = (x - w / 2f) / gain;
                float top = (y - h / 2f) / gain;
                float width = w / gain;
                float height = h / gain;

                left = Math.Clamp(left, 0, origW);
                top = Math.Clamp(top, 0, origH);
                width = Math.Clamp(width, 0, origW - left);
                height = Math.Clamp(height, 0, origH - top);

                boxes.Add((left, top, width, height));
                scores.Add(bestScore);
                classes.Add(best);
            }

            var keep = NmsIndices(boxes, scores, iouTh);
            var list = new List<NavigationDetectionResult>(keep.Count);
            foreach (var idx in keep)
            {
                var b = boxes[idx];
                if (b.W < 0.1f && b.H < 0.1f)
                {
                    continue;
                }

                int cid = classes[idx];
                var label = cid >= 0 && cid < classNames.Count ? classNames[cid] : "class_" + cid;
                list.Add(new NavigationDetectionResult
                {
                    ClassId = cid,
                    Label = label,
                    Confidence = scores[idx],
                    Left = b.L,
                    Top = b.T,
                    Width = b.W,
                    Height = b.H
                });
            }

            return list;
        }

        private static float Sigmoid(float x) => 1f / (1f + (float)Math.Exp(-x));

        /// <summary>按分数降序贪心非极大值抑制，框格式为 (left, top, width, height) 的 IoU。</summary>
        private static List<int> NmsIndices(
            IReadOnlyList<(float L, float T, float W, float H)> boxes,
            IReadOnlyList<float> scores,
            float iouTh)
        {
            var order = scores
                .Select((s, i) => (s, i))
                .OrderByDescending(x => x.s)
                .Select(x => x.i)
                .ToList();
            var suppressed = new bool[boxes.Count];
            var keep = new List<int>();
            for (int o = 0; o < order.Count; o++)
            {
                int i = order[o];
                if (suppressed[i])
                {
                    continue;
                }

                keep.Add(i);
                var a = boxes[i];
                for (int o2 = o + 1; o2 < order.Count; o2++)
                {
                    int j = order[o2];
                    if (suppressed[j])
                    {
                        continue;
                    }

                    var b = boxes[j];
                    if (IouLtwth(a, b) > iouTh)
                    {
                        suppressed[j] = true;
                    }
                }
            }

            return keep;
        }

        /// <summary>两矩形均为 LTWH 时的交并比。</summary>
        private static float IouLtwth(
            (float L, float T, float W, float H) a,
            (float L, float T, float W, float H) b)
        {
            float aR = a.L + a.W, aB = a.T + a.H;
            float bR = b.L + b.W, bB = b.T + b.H;
            float il = Math.Max(a.L, b.L);
            float it = Math.Max(a.T, b.T);
            float ir = Math.Min(aR, bR);
            float ib = Math.Min(aB, bB);
            if (ir <= il || ib <= it)
            {
                return 0f;
            }

            float inter = (ir - il) * (ib - it);
            float ua = a.W * a.H + b.W * b.H - inter;
            return inter / Math.Max(ua, 1e-6f);
        }

        /// <summary>在 BGR 副本上画矩形与标签，并生成 WPF 可用的 <see cref="System.Windows.Media.Imaging.WriteableBitmap"/>。</summary>
        private static NavigationDetectOutcome DrawAndPack(Mat bgr, IReadOnlyList<NavigationDetectionResult> dets)
        {
            using (var vis = bgr.Clone())
            {
                var colors = new[] { new Scalar(0, 0, 255), new Scalar(0, 255, 0), new Scalar(255, 0, 0) };
                foreach (var d in dets)
                {
                    int x1 = (int)d.Left, y1 = (int)d.Top;
                    int x2 = (int)(d.Left + d.Width), y2 = (int)(d.Top + d.Height);
                    var col = colors[Math.Abs(d.ClassId) % colors.Length];
                    Cv2.Rectangle(vis, new Point(x1, y1), new Point(x2, y2), col, 2);
                    var text = $"{d.Label} {d.Confidence:F2}";
                    Cv2.PutText(vis, text, new Point(x1, Math.Max(0, y1 - 4)), HersheyFonts.HersheySimplex, 0.5, col, 1);
                }

                return new NavigationDetectOutcome
                {
                    Detections = dets,
                    AnnotatedImage = vis.ToWriteableBitmap()
                };
            }
        }

        /// <summary>推理/解析失败时仍返回原图便于界面展示，检测列表为空。</summary>
        private static NavigationDetectOutcome PackFailure(Mat bgr)
        {
            return new NavigationDetectOutcome
            {
                Detections = Array.Empty<NavigationDetectionResult>(),
                AnnotatedImage = bgr.ToWriteableBitmap()
            };
        }
    }
}
