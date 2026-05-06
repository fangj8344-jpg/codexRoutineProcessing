using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using A = DocumentFormat.OpenXml.Drawing;
using S = DocumentFormat.OpenXml.Spreadsheet;
using Xdr = DocumentFormat.OpenXml.Drawing.Spreadsheet;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace UtilityTools.Modules.MotorTest.Model
{
    internal static class RandomRepeatabilityExcelReport
    {
        /// <summary>
        /// 单轴结果中行程/统计/直方图均存放在 *X 命名集合内（见 RandomRepeatabilityTestItem）。
        /// </summary>
        public static void Save(
            string xlsxPath,
            RandomRepeatabilityTestResult? xResult,
            RandomRepeatabilityTestResult? yResult,
            int fastQueryIntervalMs,
            int normalQueryIntervalMs,
            IReadOnlyList<(string Title, byte[] Png)>? embeddedChartPngs)
        {
            int chartCount = embeddedChartPngs?.Count ?? 0;
            using var doc = SpreadsheetDocument.Create(xlsxPath, SpreadsheetDocumentType.Workbook);
            var wbPart = doc.AddWorkbookPart();
            wbPart.Workbook = new Workbook();
            uint separatorStyleIndex = EnsureSeparatorCellStyle(wbPart);
            var sheets = wbPart.Workbook.AppendChild(new Sheets());

            uint sheetId = 1;
            AddOverviewAndStatsSheet(
                wbPart,
                sheets,
                ref sheetId,
                xResult,
                yResult,
                fastQueryIntervalMs,
                normalQueryIntervalMs,
                chartCount);

            AddPointCoordinateCombinedSheet(wbPart, sheets, ref sheetId, xResult?.TargetPoints, yResult?.TargetPoints);

            AddMoveTripSheet(wbPart, sheets, ref sheetId, "X轴行程明细", xResult?.MoveTripsX);
            AddMoveTripSheet(wbPart, sheets, ref sheetId, "Y轴行程明细", yResult?.MoveTripsX);

            AddHistogramCombinedSheet(wbPart, sheets, ref sheetId, "距离分箱(XY)", xResult?.DistanceHistogramX, yResult?.DistanceHistogramX, separatorStyleIndex);
            AddHistogramCombinedSheet(wbPart, sheets, ref sheetId, "速度分箱(XY)", xResult?.SpeedHistogramX, yResult?.SpeedHistogramX, separatorStyleIndex);

            AddDiffSheet(wbPart, sheets, ref sheetId, "X轴差值", xResult?.MoveTripsX);
            AddDiffSheet(wbPart, sheets, ref sheetId, "Y轴差值", yResult?.MoveTripsX);

            AddPointStatsCombinedSheet(wbPart, sheets, ref sheetId, xResult, yResult, separatorStyleIndex);

            AddEmbeddedChartsSheet(wbPart, sheets, ref sheetId, embeddedChartPngs);

            wbPart.Workbook.Save();
        }

        private static void AddOverviewAndStatsSheet(
            WorkbookPart wbPart,
            Sheets sheets,
            ref uint sheetId,
            RandomRepeatabilityTestResult? xResult,
            RandomRepeatabilityTestResult? yResult,
            int fastQueryIntervalMs,
            int normalQueryIntervalMs,
            int embeddedChartCount)
        {
            var wsPart = wbPart.AddNewPart<WorksheetPart>();
            var data = new SheetData();
            AttachWorksheet(
                wsPart,
                data,
                freezeTopRow: false,
                autoFilterReference: null,
                columnWidths: new[] { 28d, 90d, 22d });

            sheets.Append(new Sheet { Id = wbPart.GetIdOfPart(wsPart), SheetId = sheetId++, Name = "总览统计" });

            AddTextRow(data, "生成时间", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            AddTextRow(
                data,
                "快速问询",
                $"测试过程中状态轮询已切换为 {fastQueryIntervalMs}ms（结束后恢复 {normalQueryIntervalMs}ms），便于行程与速度统计。");
            AddTextRow(
                data,
                "测试摘要",
                $"X轴平均重复标准差={F3(xResult?.AvgStdX)} μm，Y轴={F3(yResult?.AvgStdX)} μm");
            AddEmptyRow(data);

            AddTitleOnlyRow(data, "表结构说明");
            AddTextRow(data, "① 目标点坐标(XY)", "合并后的目标点列表（按点序号对齐 X/Y 目标坐标与脉冲）");
            AddTextRow(data, "② X/Y轴行程明细", "起点/实际起点/目标/实际到位/移动时间，以及行程长度与平均速度(μm/s)");
            AddTextRow(data, "③ 其余工作表", "差值（大表分轴）+ 分箱/点统计（小表合并 XY）+ 嵌入图表");
            AddTextRow(data, "导出说明", "仅生成本工作簿文件，不另存 CSV；直方图与差值以工作表数值为准。");
            AddTextRow(
                data,
                "嵌入图表",
                embeddedChartCount > 0
                    ? $"已写入工作表「嵌入图表」（共 {embeddedChartCount} 张 PNG，内嵌于 xlsx）。"
                    : "未生成嵌入图（行程或分箱数据不足时略过）。");
            AddEmptyRow(data);

            AddTitleOnlyRow(data, "X轴概览");
            AddHeaderRow(data, "项", "值");
            AddTextRow(data, "平均重复位置标准差(μm)", F3(xResult?.AvgStdX));
            AddTextRow(data, "平均移动时间(ms)", F1(xResult?.AvgMoveTimeMs));
            AddTextRow(data, "行程平均速度(μm/s)", F3TripMeanSpeed(xResult));
            AddTextRow(data, "生成目标点数量", (xResult?.TargetPoints.Count ?? 0).ToString(CultureInfo.InvariantCulture));
            AddTextRow(data, "行程记录条数", (xResult?.MoveTripsX.Count ?? 0).ToString(CultureInfo.InvariantCulture));
            AddTextRow(data, "测试完成", BoolText(xResult?.IsTestCompleted));
            AddEmptyRow(data);

            AddTitleOnlyRow(data, "Y轴概览");
            AddHeaderRow(data, "项", "值");
            AddTextRow(data, "平均重复位置标准差(μm)", F3(yResult?.AvgStdX));
            AddTextRow(data, "平均移动时间(ms)", F1(yResult?.AvgMoveTimeMs));
            AddTextRow(data, "行程平均速度(μm/s)", F3TripMeanSpeed(yResult));
            AddTextRow(data, "生成目标点数量", (yResult?.TargetPoints.Count ?? 0).ToString(CultureInfo.InvariantCulture));
            AddTextRow(data, "行程记录条数", (yResult?.MoveTripsX.Count ?? 0).ToString(CultureInfo.InvariantCulture));
            AddTextRow(data, "测试完成", BoolText(yResult?.IsTestCompleted));

            AddEmptyRow(data);
            AddTitleOnlyRow(data, "X/Y 对比统计");
            AddHeaderRow(data, "统计项", "X轴", "Y轴");
            AddTextRow3(data, "各目标点到位重复性的平均标准差(μm)", F3(xResult?.AvgStdX), F3(yResult?.AvgStdX));
            AddTextRow3(data, "平均移动时间(ms)", F1(xResult?.AvgMoveTimeMs), F1(yResult?.AvgMoveTimeMs));
            AddTextRow3(data, "行程平均速度(μm/s)", F3TripMeanSpeed(xResult), F3TripMeanSpeed(yResult));
            AddTextRow3(data, "生成目标点个数",
                (xResult?.TargetPoints.Count ?? 0).ToString(CultureInfo.InvariantCulture),
                (yResult?.TargetPoints.Count ?? 0).ToString(CultureInfo.InvariantCulture));
            AddTextRow3(data, "行程条数",
                (xResult?.MoveTripsX.Count ?? 0).ToString(CultureInfo.InvariantCulture),
                (yResult?.MoveTripsX.Count ?? 0).ToString(CultureInfo.InvariantCulture));
            AddTextRow3(data, "测试完成", BoolText(xResult?.IsTestCompleted), BoolText(yResult?.IsTestCompleted));
        }

        private static void AddPointCoordinateCombinedSheet(
            WorkbookPart wbPart,
            Sheets sheets,
            ref uint sheetId,
            IEnumerable<RandomTargetPointRecord>? xPoints,
            IEnumerable<RandomTargetPointRecord>? yPoints)
        {
            var wsPart = wbPart.AddNewPart<WorksheetPart>();
            var data = new SheetData();
            int colCount = 5;
            int xCount = xPoints?.Count() ?? 0;
            int yCount = yPoints?.Count() ?? 0;
            int rowCount = Math.Max(1, Math.Max(xCount, yCount)) + 1;
            string filterRef = $"A1:{ExcelColumnName(colCount)}{Math.Max(1, rowCount)}";
            AttachWorksheet(
                wsPart,
                data,
                freezeTopRow: true,
                autoFilterReference: filterRef,
                columnWidths: new[] { 10d, 18d, 18d, 14d, 14d });
            sheets.Append(new Sheet { Id = wbPart.GetIdOfPart(wsPart), SheetId = sheetId++, Name = "目标点坐标(XY)" });

            AddHeaderRow(data, "点序号", "目标X(μm)", "目标Y(μm)", "脉冲X", "脉冲Y");
            var xDict = (xPoints ?? Array.Empty<RandomTargetPointRecord>()).ToDictionary(p => p.Index);
            var yDict = (yPoints ?? Array.Empty<RandomTargetPointRecord>()).ToDictionary(p => p.Index);
            foreach (var index in xDict.Keys.Union(yDict.Keys).OrderBy(i => i))
            {
                xDict.TryGetValue(index, out var xPoint);
                yDict.TryGetValue(index, out var yPoint);
                double targetX = xPoint?.TargetXUm ?? 0;
                // 单轴测试结果中目标值统一写入 TargetXUm/TargetXPulse，Y轴结果同样如此。
                double targetY = yPoint?.TargetXUm ?? 0;
                double pulseX = xPoint?.TargetXPulse ?? 0;
                double pulseY = yPoint?.TargetXPulse ?? 0;
                AddNumberRow(data, index, targetX, targetY, pulseX, pulseY);
            }
        }

        private static void AddMoveTripSheet(
            WorkbookPart wbPart,
            Sheets sheets,
            ref uint sheetId,
            string sheetName,
            IEnumerable<RandomMoveTripAxisRecord>? trips)
        {
            var wsPart = wbPart.AddNewPart<WorksheetPart>();
            var data = new SheetData();
            const int colCount = 9;
            int dataRows = trips == null ? 0 : trips.Count();
            int rowCount = 1 + dataRows;
            string filterRef = $"A1:{ExcelColumnName(colCount)}{Math.Max(1, rowCount)}";
            AttachWorksheet(
                wsPart,
                data,
                freezeTopRow: true,
                autoFilterReference: filterRef,
                columnWidths: new[] { 9d, 10d, 16d, 16d, 14d, 16d, 14d, 14d, 16d });
            sheets.Append(new Sheet { Id = wbPart.GetIdOfPart(wsPart), SheetId = sheetId++, Name = sheetName });

            AddHeaderRow(data,
                "序号",
                "点序号",
                "计划起点(μm)",
                "实际起点(μm)",
                "目标(μm)",
                "实际到位(μm)",
                "移动时间(ms)",
                "行程(μm)",
                "平均速度(μm/s)");
            if (trips == null) return;
            int rowNo = 1;
            foreach (var t in trips)
                AddNumberRow(data, rowNo++, t.PointIndex, t.PlannedStartUm, t.ActualStartUm, t.TargetUm, t.ActualTargetUm, t.MoveTimeMs, t.DistanceUm, t.AvgSpeedUmPerSec);
        }

        private static void AddHistogramCombinedSheet(
            WorkbookPart wbPart,
            Sheets sheets,
            ref uint sheetId,
            string sheetName,
            IEnumerable<HistogramBinRecord>? xBins,
            IEnumerable<HistogramBinRecord>? yBins,
            uint separatorStyleIndex)
        {
            var wsPart = wbPart.AddNewPart<WorksheetPart>();
            var data = new SheetData();
            var xList = xBins?.ToList() ?? new List<HistogramBinRecord>();
            var yList = yBins?.ToList() ?? new List<HistogramBinRecord>();
            int dataRows = Math.Max(xList.Count, yList.Count);
            int rowCount = 1 + dataRows;
            string filterRef = $"A1:I{Math.Max(1, rowCount)}";
            AttachWorksheet(
                wsPart,
                data,
                freezeTopRow: true,
                autoFilterReference: filterRef,
                columnWidths: new[] { 8d, 14d, 14d, 10d, 3d, 8d, 14d, 14d, 10d });
            sheets.Append(new Sheet { Id = wbPart.GetIdOfPart(wsPart), SheetId = sheetId++, Name = sheetName });

            AddHeaderRow(data, "X序号", "X下限", "X上限", "X计数", "", "Y序号", "Y下限", "Y上限", "Y计数");
            for (int i = 0; i < dataRows; i++)
            {
                var row = new Row();
                var xb = i < xList.Count ? xList[i] : null;
                var yb = i < yList.Count ? yList[i] : null;
                row.Append(xb == null ? TextCell(string.Empty) : NumberCell(xb.BinIndex));
                row.Append(xb == null ? TextCell(string.Empty) : NumberCell(xb.MinValue));
                row.Append(xb == null ? TextCell(string.Empty) : NumberCell(xb.MaxValue));
                row.Append(xb == null ? TextCell(string.Empty) : NumberCell(xb.Count));
                row.Append(SeparatorCell(separatorStyleIndex));
                row.Append(yb == null ? TextCell(string.Empty) : NumberCell(yb.BinIndex));
                row.Append(yb == null ? TextCell(string.Empty) : NumberCell(yb.MinValue));
                row.Append(yb == null ? TextCell(string.Empty) : NumberCell(yb.MaxValue));
                row.Append(yb == null ? TextCell(string.Empty) : NumberCell(yb.Count));
                data.Append(row);
            }
        }

        private static void AddDiffSheet(
            WorkbookPart wbPart,
            Sheets sheets,
            ref uint sheetId,
            string sheetName,
            IEnumerable<RandomMoveTripAxisRecord>? trips)
        {
            var wsPart = wbPart.AddNewPart<WorksheetPart>();
            var data = new SheetData();
            const int colCount = 7;
            int dataRows = trips == null ? 0 : trips.Count();
            int rowCount = 1 + dataRows;
            string filterRef = $"A1:{ExcelColumnName(colCount)}{Math.Max(1, rowCount)}";
            AttachWorksheet(
                wsPart,
                data,
                freezeTopRow: true,
                autoFilterReference: filterRef,
                columnWidths: new[] { 9d, 10d, 14d, 14d, 14d, 14d, 14d });
            sheets.Append(new Sheet { Id = wbPart.GetIdOfPart(wsPart), SheetId = sheetId++, Name = sheetName });

            AddHeaderRow(data, "序号", "点序号", "目标(μm)", "实际(μm)", "差值(μm)", "移动时间(ms)", "平均速度(μm/s)");
            if (trips == null) return;
            foreach (var t in trips)
            {
                double diff = t.ActualTargetUm - t.TargetUm;
                AddNumberRow(data, t.SeqNo, t.PointIndex, t.TargetUm, t.ActualTargetUm, diff, t.MoveTimeMs, t.AvgSpeedUmPerSec);
            }
        }

        private static void AddPointStatsCombinedSheet(
            WorkbookPart wbPart,
            Sheets sheets,
            ref uint sheetId,
            RandomRepeatabilityTestResult? xResult,
            RandomRepeatabilityTestResult? yResult,
            uint separatorStyleIndex)
        {
            var wsPart = wbPart.AddNewPart<WorksheetPart>();
            var data = new SheetData();
            var xStats = xResult?.PointStatsX?.ToList() ?? new List<RandomPointStatAxisRecord>();
            var yStats = yResult?.PointStatsX?.ToList() ?? new List<RandomPointStatAxisRecord>();
            const int colCount = 15;
            int dataRows = Math.Max(xStats.Count, yStats.Count);
            int rowCount = 1 + dataRows;
            string filterRef = $"A1:{ExcelColumnName(colCount)}{Math.Max(1, rowCount)}";
            AttachWorksheet(
                wsPart,
                data,
                freezeTopRow: true,
                autoFilterReference: filterRef,
                columnWidths: new[] { 10d, 10d, 14d, 14d, 14d, 14d, 16d, 3d, 10d, 10d, 14d, 14d, 14d, 14d, 16d });
            sheets.Append(new Sheet { Id = wbPart.GetIdOfPart(wsPart), SheetId = sheetId++, Name = "点统计(XY)" });

            AddHeaderRow(data,
                "X点序号", "X样本数", "X目标(μm)", "X均值(μm)", "X标准差(μm)", "X均值偏差(μm)", "X行程平均速度(μm/s)",
                "",
                "Y点序号", "Y样本数", "Y目标(μm)", "Y均值(μm)", "Y标准差(μm)", "Y均值偏差(μm)", "Y行程平均速度(μm/s)");
            for (int i = 0; i < dataRows; i++)
            {
                var row = new Row();
                var xs = i < xStats.Count ? xStats[i] : null;
                var ys = i < yStats.Count ? yStats[i] : null;
                double xMeanSpeed = xs == null ? double.NaN : MeanTripSpeedUmPerSecAtPoint(xResult?.MoveTripsX, xs.PointIndex);
                double yMeanSpeed = ys == null ? double.NaN : MeanTripSpeedUmPerSecAtPoint(yResult?.MoveTripsX, ys.PointIndex);
                AppendPointStatCells(row, xs, xMeanSpeed);
                row.Append(SeparatorCell(separatorStyleIndex));
                AppendPointStatCells(row, ys, yMeanSpeed);
                data.Append(row);
            }
        }

        /// <summary>该目标点下各次跳转的平均速度均值（μm/s）。</summary>
        private static double MeanTripSpeedUmPerSecAtPoint(IEnumerable<RandomMoveTripAxisRecord>? trips, int pointIndex)
        {
            if (trips == null)
                return double.NaN;
            double sum = 0;
            int n = 0;
            foreach (var t in trips)
            {
                if (t.PointIndex != pointIndex) continue;
                sum += t.AvgSpeedUmPerSec;
                n++;
            }
            return n == 0 ? double.NaN : sum / n;
        }

        private static void AppendPointStatCells(
            Row row,
            RandomPointStatAxisRecord? stat,
            double meanTripSpeedUmPerSec)
        {
            if (stat == null)
            {
                row.Append(TextCell(string.Empty));
                row.Append(TextCell(string.Empty));
                row.Append(TextCell(string.Empty));
                row.Append(TextCell(string.Empty));
                row.Append(TextCell(string.Empty));
                row.Append(TextCell(string.Empty));
                row.Append(TextCell(string.Empty));
                return;
            }

            row.Append(NumberCell(stat.PointIndex));
            row.Append(NumberCell(stat.SampleCount));
            row.Append(NumberCell(stat.TargetUm));
            row.Append(NumberCell(stat.MeanUm));
            row.Append(NumberCell(stat.StdUm));
            row.Append(NumberCell(stat.MeanUm - stat.TargetUm));
            if (double.IsNaN(meanTripSpeedUmPerSec))
                row.Append(TextCell("--"));
            else
                row.Append(NumberCell(meanTripSpeedUmPerSec));
        }

        private static Cell SeparatorCell(uint styleIndex) =>
            new Cell { DataType = CellValues.InlineString, InlineString = new InlineString(new DocumentFormat.OpenXml.Spreadsheet.Text(string.Empty)), StyleIndex = styleIndex };

        private static uint EnsureSeparatorCellStyle(WorkbookPart wbPart)
        {
            var stylesPart = wbPart.WorkbookStylesPart ?? wbPart.AddNewPart<WorkbookStylesPart>();
            if (stylesPart.Stylesheet == null)
            {
                stylesPart.Stylesheet = new S.Stylesheet(
                    new S.Fonts(new S.Font()),
                    new S.Fills(
                        new S.Fill(new S.PatternFill { PatternType = S.PatternValues.None }),
                        new S.Fill(new S.PatternFill { PatternType = S.PatternValues.Gray125 })),
                    new S.Borders(new S.Border()),
                    new S.CellStyleFormats(new S.CellFormat()),
                    new S.CellFormats(new S.CellFormat()));
            }

            var stylesheet = stylesPart.Stylesheet;
            stylesheet.Fills ??= new S.Fills(
                new S.Fill(new S.PatternFill { PatternType = S.PatternValues.None }),
                new S.Fill(new S.PatternFill { PatternType = S.PatternValues.Gray125 }));
            stylesheet.CellFormats ??= new S.CellFormats(new S.CellFormat());

            uint sepFillId = (uint)stylesheet.Fills.Count();
            stylesheet.Fills.Append(new S.Fill(
                new S.PatternFill(
                    new S.ForegroundColor { Rgb = "FFDDEBF7" },
                    new S.BackgroundColor { Indexed = 64U })
                { PatternType = S.PatternValues.Solid }));
            stylesheet.Fills.Count = (uint)stylesheet.Fills.Count();

            uint sepStyleIndex = (uint)stylesheet.CellFormats.Count();
            stylesheet.CellFormats.Append(new S.CellFormat
            {
                FillId = sepFillId,
                ApplyFill = true
            });
            stylesheet.CellFormats.Count = (uint)stylesheet.CellFormats.Count();
            stylesheet.Save();
            return sepStyleIndex;
        }

        private static string F3TripMeanSpeed(RandomRepeatabilityTestResult? r)
        {
            var trips = r?.MoveTripsX;
            if (trips == null || trips.Count == 0)
                return "--";
            double v = trips.Average(t => t.AvgSpeedUmPerSec);
            return v.ToString("F3", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 将 PNG 嵌入独立工作表（图片浮动于单元格之上，Excel 打开即可见）。
        /// </summary>
        private static void AddEmbeddedChartsSheet(
            WorkbookPart wbPart,
            Sheets sheets,
            ref uint sheetId,
            IReadOnlyList<(string Title, byte[] Png)>? embeddedChartPngs)
        {
            if (embeddedChartPngs == null || embeddedChartPngs.Count == 0)
                return;

            var wsPart = wbPart.AddNewPart<WorksheetPart>();
            var data = new SheetData();
            AddTitleOnlyRow(data, "以下为嵌入的曲线 / 直方图（PNG），数据仍以「差值」「距离分箱」「速度分箱」等工作表为准。");
            AddEmptyRow(data);

            var drawingsPart = wsPart.AddNewPart<DrawingsPart>();
            var worksheetDrawing = new Xdr.WorksheetDrawing();

            uint anchorRow1Based = 3;
            uint pictureNvId = 1024;

            foreach (var (title, pngBytes) in embeddedChartPngs)
            {
                if (pngBytes == null || pngBytes.Length == 0)
                    continue;

                using var ms = new MemoryStream(pngBytes, writable: false);
                var imagePart = drawingsPart.AddImagePart(ImagePartType.Png);
                imagePart.FeedData(ms);
                string embedId = drawingsPart.GetIdOfPart(imagePart);

                var (emuW, emuH) = GetPngExtentsEmu(pngBytes);

                var oneCellAnchor = new Xdr.OneCellAnchor(
                    new Xdr.FromMarker(
                        new Xdr.ColumnId("0"),
                        new Xdr.ColumnOffset("0"),
                        new Xdr.RowId((anchorRow1Based - 1).ToString(CultureInfo.InvariantCulture)),
                        new Xdr.RowOffset("0")),
                    new Xdr.Extent { Cx = emuW, Cy = emuH },
                    new Xdr.Picture(
                        new Xdr.NonVisualPictureProperties(
                            new Xdr.NonVisualDrawingProperties
                            {
                                Id = pictureNvId,
                                Name = "Chart " + pictureNvId,
                                Description = title
                            },
                            new Xdr.NonVisualPictureDrawingProperties(new A.PictureLocks { NoChangeAspect = true })),
                        new Xdr.BlipFill(
                            new A.Blip { Embed = embedId, CompressionState = A.BlipCompressionValues.Print },
                            new A.Stretch(new A.FillRectangle())),
                        new Xdr.ShapeProperties(
                            new A.Transform2D(
                                new A.Offset { X = 0, Y = 0 },
                                new A.Extents { Cx = emuW, Cy = emuH }),
                            new A.PresetGeometry { Preset = A.ShapeTypeValues.Rectangle })),
                    new Xdr.ClientData());
                worksheetDrawing.Append(oneCellAnchor);

                pictureNvId++;
                uint rowGap = (uint)Math.Max(28, Math.Ceiling(emuH / 9525.0 / 18.0)) + 4u;
                anchorRow1Based += rowGap;
            }

            drawingsPart.WorksheetDrawing = worksheetDrawing;

            var worksheet = new Worksheet();
            worksheet.Append(new SheetFormatProperties { DefaultRowHeight = 15D });
            worksheet.Append(CreateColumns(new[] { 90d }));
            worksheet.Append(data);
            worksheet.Append(new Drawing { Id = wsPart.GetIdOfPart(drawingsPart) });
            wsPart.Worksheet = worksheet;

            sheets.Append(new Sheet { Id = wbPart.GetIdOfPart(wsPart), SheetId = sheetId++, Name = "嵌入图表" });
        }

        private static (long Cx, long Cy) GetPngExtentsEmu(byte[] png)
        {
            try
            {
                if (png.Length < 24)
                    return (5486400, 2997000);
                int w = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16, 4));
                int h = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20, 4));
                if (w <= 0 || h <= 0)
                    return (5486400, 2997000);
                return (w * 914400L / 96, h * 914400L / 96);
            }
            catch
            {
                return (5486400, 2997000);
            }
        }

        private static void AttachWorksheet(
            WorksheetPart wsPart,
            SheetData data,
            bool freezeTopRow,
            string? autoFilterReference,
            IReadOnlyList<double>? columnWidths = null)
        {
            var worksheet = new Worksheet();
            if (freezeTopRow)
            {
                var pane = new Pane
                {
                    VerticalSplit = 1D,
                    TopLeftCell = "A2",
                    ActivePane = PaneValues.BottomLeft,
                    State = PaneStateValues.Frozen
                };
                var sheetView = new SheetView { WorkbookViewId = 0U };
                sheetView.Append(pane);
                worksheet.Append(new SheetViews(sheetView));
            }

            worksheet.Append(new SheetFormatProperties { DefaultRowHeight = 15D });
            if (columnWidths != null && columnWidths.Count > 0)
                worksheet.Append(CreateColumns(columnWidths));
            worksheet.Append(data);

            if (!string.IsNullOrEmpty(autoFilterReference))
                worksheet.Append(new AutoFilter { Reference = autoFilterReference });

            wsPart.Worksheet = worksheet;
        }

        private static Columns CreateColumns(IReadOnlyList<double> widths)
        {
            var columns = new Columns();
            for (uint i = 0; i < widths.Count; i++)
            {
                columns.Append(new Column
                {
                    Min = i + 1,
                    Max = i + 1,
                    Width = widths[(int)i],
                    CustomWidth = true
                });
            }
            return columns;
        }

        private static string ExcelColumnName(int columnIndex1Based)
        {
            var sb = new StringBuilder();
            int n = columnIndex1Based;
            while (n > 0)
            {
                n--;
                sb.Insert(0, (char)('A' + (n % 26)));
                n /= 26;
            }
            return sb.ToString();
        }

        private static void AddHeaderRow(SheetData data, params string[] values)
        {
            var row = new Row();
            foreach (var v in values) row.Append(TextCell(v));
            data.Append(row);
        }

        private static void AddTitleOnlyRow(SheetData data, string text)
        {
            var row = new Row();
            row.Append(TextCell(text));
            data.Append(row);
        }

        private static void AddTextRow(SheetData data, string left, string right)
        {
            var row = new Row();
            row.Append(TextCell(left));
            row.Append(TextCell(right));
            data.Append(row);
        }

        private static void AddTextRow3(SheetData data, string c1, string c2, string c3)
        {
            var row = new Row();
            row.Append(TextCell(c1));
            row.Append(TextCell(c2));
            row.Append(TextCell(c3));
            data.Append(row);
        }

        private static void AddEmptyRow(SheetData data) => data.Append(new Row());

        private static void AddNumberRow(SheetData data, params double[] values)
        {
            var row = new Row();
            foreach (var v in values) row.Append(NumberCell(v));
            data.Append(row);
        }

        private static Cell TextCell(string value) =>
            new Cell { DataType = CellValues.InlineString, InlineString = new InlineString(new DocumentFormat.OpenXml.Spreadsheet.Text(value ?? string.Empty)) };

        private static Cell NumberCell(double value) =>
            new Cell { DataType = CellValues.Number, CellValue = new CellValue(value.ToString("0.######", CultureInfo.InvariantCulture)) };

        private static string F3(double? value) => value.HasValue ? value.Value.ToString("F3", CultureInfo.InvariantCulture) : "--";
        private static string F1(double? value) => value.HasValue ? value.Value.ToString("F1", CultureInfo.InvariantCulture) : "--";
        private static string BoolText(bool? value) => value == true ? "是" : "否";
    }
}
