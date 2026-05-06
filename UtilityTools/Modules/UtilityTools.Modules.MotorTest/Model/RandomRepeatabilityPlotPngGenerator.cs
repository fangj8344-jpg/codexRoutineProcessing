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

        public static byte[]? ExportSpeedDistanceScatter(IEnumerable<RandomMoveTripAxisRecord>? trips, string title)
        {
            var list = trips?.ToList();
            if (list == null || list.Count == 0)
                return null;

            var model = new PlotModel { Title = title };
            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "行程距离 (μm)",
                MinimumPadding = 0.05
            });
            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "平均速度 (μm/s)",
                MinimumPadding = 0.05
            });

            var scatter = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 2.8,
                MarkerFill = OxyColor.FromArgb(180, 30, 136, 229)
            };
            foreach (var t in list)
            {
                scatter.Points.Add(new ScatterPoint(t.DistanceUm, t.AvgSpeedUmPerSec));
            }
            model.Series.Add(scatter);
            return ExportToPngBytes(model, 960, 420);
        }

        public static byte[]? ExportDiffBoxPlot(IEnumerable<RandomMoveTripAxisRecord>? trips, string title)
        {
            var list = trips?.ToList();
            if (list == null || list.Count == 0)
                return null;

            var grouped = list
                .GroupBy(t => t.PointIndex)
                .OrderBy(g => g.Key)
                .ToList();
            if (grouped.Count == 0)
                return null;

            var model = new PlotModel { Title = title };
            var xAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Title = "点序号",
                Angle = 45
            };
            foreach (var g in grouped)
                xAxis.Labels.Add(g.Key.ToString());
            model.Axes.Add(xAxis);
            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "差值 (μm)",
                MinimumPadding = 0.05
            });

            var boxSeries = new BoxPlotSeries
            {
                Fill = OxyColor.FromArgb(160, 255, 183, 77),
                Stroke = OxyColor.FromArgb(220, 230, 81, 0),
                MedianPointSize = 2
            };

            for (int i = 0; i < grouped.Count; i++)
            {
                var diffs = grouped[i]
                    .Select(t => t.ActualTargetUm - t.TargetUm)
                    .OrderBy(v => v)
                    .ToList();
                if (diffs.Count == 0)
                    continue;

                double min = diffs.First();
                double max = diffs.Last();
                double median = PercentileSorted(diffs, 0.5);
                double q1 = PercentileSorted(diffs, 0.25);
                double q3 = PercentileSorted(diffs, 0.75);
                boxSeries.Items.Add(new BoxPlotItem(i, min, q1, median, q3, max));
            }

            model.Series.Add(boxSeries);
            return ExportToPngBytes(model, 960, 460);
        }

        private static double PercentileSorted(IReadOnlyList<double> sortedValues, double p)
        {
            if (sortedValues.Count == 0)
                return 0;
            if (sortedValues.Count == 1)
                return sortedValues[0];

            double index = (sortedValues.Count - 1) * p;
            int lo = (int)Math.Floor(index);
            int hi = (int)Math.Ceiling(index);
            if (lo == hi)
                return sortedValues[lo];
            double weight = index - lo;
            return sortedValues[lo] * (1 - weight) + sortedValues[hi] * weight;
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
