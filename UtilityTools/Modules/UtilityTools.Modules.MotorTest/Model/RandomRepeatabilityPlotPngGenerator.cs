using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UtilityTools.Modules.MotorTest.Model
{
    /// <summary>
    /// 随机重复精度报告用：生成可嵌入 Excel 的 PNG 字节（经临时文件，避免依赖额外渲染包）。
    /// </summary>
    internal static class RandomRepeatabilityPlotPngGenerator
    {
        public static byte[]? ExportDiffCurve(IEnumerable<RandomMoveTripAxisRecord>? trips, string title)
        {
            var list = trips?.ToList();
            if (list == null || list.Count == 0)
                return null;

            var model = new PlotModel { Title = title };
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "样本序号", MinimumPadding = 0, AbsoluteMinimum = 0 });
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "差值 (μm)", MinimumPadding = 0.05 });

            var line = new LineSeries
            {
                Title = "实际−目标",
                Color = OxyColor.FromArgb(220, 255, 111, 0),
                StrokeThickness = 1.8,
                MarkerType = MarkerType.Circle,
                MarkerSize = 2.2,
                MarkerFill = OxyColor.FromArgb(220, 255, 111, 0)
            };
            for (int i = 0; i < list.Count; i++)
            {
                double diff = list[i].ActualTargetUm - list[i].TargetUm;
                line.Points.Add(new DataPoint(i + 1, diff));
            }
            model.Series.Add(line);

            var zeroLine = new LineSeries { Color = OxyColors.Gray, StrokeThickness = 1, LineStyle = LineStyle.Dash };
            zeroLine.Points.Add(new DataPoint(1, 0));
            zeroLine.Points.Add(new DataPoint(Math.Max(1, list.Count), 0));
            model.Series.Add(zeroLine);

            return ExportToPngBytes(model, 960, 420);
        }

        public static byte[]? ExportHistogram(IEnumerable<HistogramBinRecord>? bins, string title, string binAxisTitle)
        {
            var list = bins?.ToList();
            if (list == null || list.Count == 0)
                return null;

            var model = new PlotModel { Title = title };
            var categoryAxis = new CategoryAxis
            {
                Position = AxisPosition.Left,
                Title = binAxisTitle,
                Angle = 45,
                GapWidth = 0.35
            };
            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "计数",
                MinimumPadding = 0,
                AbsoluteMinimum = 0
            };
            model.Axes.Add(categoryAxis);
            model.Axes.Add(valueAxis);

            var series = new BarSeries
            {
                FillColor = OxyColor.FromArgb(220, 66, 165, 245),
                StrokeColor = OxyColors.DarkBlue,
                StrokeThickness = 1
            };
            for (int i = 0; i < list.Count; i++)
            {
                var b = list[i];
                categoryAxis.Labels.Add($"{b.MinValue:F1}–{b.MaxValue:F1}");
                series.Items.Add(new BarItem(b.Count));
            }
            model.Series.Add(series);

            return ExportToPngBytes(model, 960, 520);
        }

        private static byte[]? ExportToPngBytes(PlotModel model, int width, int height)
        {
            string temp = Path.Combine(Path.GetTempPath(), "zep_rr_" + Guid.NewGuid().ToString("N") + ".png");
            try
            {
                var exporter = new PngExporter { Width = width, Height = height };
                exporter.ExportToFile(model, temp);
                return File.ReadAllBytes(temp);
            }
            catch
            {
                return null;
            }
            finally
            {
                try { if (File.Exists(temp)) File.Delete(temp); } catch { /* ignore */ }
            }
        }
    }
}
