using System.Globalization;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace UtilityTools.Modules.MotorTest.Model
{
    /// <summary>
    /// 使用 Open XML SDK（免费、无 GemBox 段落限制）生成随机重复精度测试 Word 报告。
    /// </summary>
    internal static class RandomRepeatabilityOpenXmlReport
    {
        public static void Save(
            string docxPath,
            RandomRepeatabilityTestResult? xResult,
            RandomRepeatabilityTestResult? yResult,
            string? plotPositionPngPath,
            string? plotSpeedPngPath,
            string? histXDistancePngPath,
            string? histYDistancePngPath,
            string? histXSpeedPngPath,
            string? histYSpeedPngPath,
            IReadOnlyList<string> csvFileNameHints)
        {
            using var wordDoc = WordprocessingDocument.Create(docxPath, WordprocessingDocumentType.Document);
            var mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new Document(new Body());
            var body = mainPart.Document.Body!;

            AddTitleParagraph(body, "随机重复精度测试报告");
            AddParagraph(body, $"生成时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            AddParagraph(body,
                $"测试摘要：X轴标准差={xResult?.AvgStdX:F3} μm，Y轴标准差={yResult?.AvgStdX:F3} μm");

            AddOverviewTable(body, "X轴概览", xResult);
            AddOverviewTable(body, "Y轴概览", yResult);

            string csvNote = csvFileNameHints.Count == 0
                ? "明细 CSV：本次未写出文件（可能集合为空）。"
                : "明细 CSV（与 Word 同目录）：" + string.Join("；", csvFileNameHints);
            AddParagraph(body, csvNote);

            AddPlotSection(mainPart, body, "位置曲线图", plotPositionPngPath, 1U);
            AddPlotSection(mainPart, body, "速度曲线图", plotSpeedPngPath, 2U);

            AddParagraph(body, "距离 / 速度直方图（OxyPlot）");
            AddPlotSection(mainPart, body, "X轴距离分箱直方图", histXDistancePngPath, 3U);
            AddPlotSection(mainPart, body, "Y轴距离分箱直方图", histYDistancePngPath, 4U);
            AddPlotSection(mainPart, body, "X轴速度分箱直方图", histXSpeedPngPath, 5U);
            AddPlotSection(mainPart, body, "Y轴速度分箱直方图", histYSpeedPngPath, 6U);

            AddTargetPointsTable(body, "X轴目标点", xResult?.TargetPoints);
            AddTargetPointsTable(body, "Y轴目标点", yResult?.TargetPoints);
            AddPointStatsTable(body, "X轴点位统计", xResult?.PointStatsX);
            AddPointStatsTable(body, "Y轴点位统计", yResult?.PointStatsX);
            AddMoveTripsTable(body, "X轴行程明细", xResult?.MoveTripsX);
            AddMoveTripsTable(body, "Y轴行程明细", yResult?.MoveTripsX);
            AddHistogramTable(body, "X轴距离分箱", xResult?.DistanceHistogramX);
            AddHistogramTable(body, "Y轴距离分箱", yResult?.DistanceHistogramX);
            AddHistogramTable(body, "X轴速度分箱", xResult?.SpeedHistogramX);
            AddHistogramTable(body, "Y轴速度分箱", yResult?.SpeedHistogramX);

            mainPart.Document.Save();
        }

        private static void AddTitleParagraph(Body body, string text)
        {
            body.AppendChild(new Paragraph(
                new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
                new Run(
                    new RunProperties(
                        new Bold(),
                        new FontSize { Val = "40" },
                        new RunFonts { Ascii = "Microsoft YaHei", HighAnsi = "Microsoft YaHei", EastAsia = "Microsoft YaHei" }),
                    new Text(text) { Space = SpaceProcessingModeValues.Preserve })));
        }

        private static void AddParagraph(Body body, string text)
        {
            body.AppendChild(new Paragraph(
                new Run(
                    new RunProperties(new RunFonts { Ascii = "Microsoft YaHei", HighAnsi = "Microsoft YaHei", EastAsia = "Microsoft YaHei" }),
                    new Text(text) { Space = SpaceProcessingModeValues.Preserve })));
        }

        private static void AddOverviewTable(Body body, string title, RandomRepeatabilityTestResult? result)
        {
            AddParagraph(body, title);
            var rows = new List<string[]>
            {
                new[] { "平均标准差(μm)", $"{result?.AvgStdX:F3}" },
                new[] { "平均移动时间(ms)", $"{result?.AvgMoveTimeMs:F1}" },
                new[] { "目标点数量", $"{result?.TargetPoints.Count ?? 0}" },
                new[] { "行程记录数量", $"{result?.MoveTripsX.Count ?? 0}" },
                new[] { "测试完成", $"{result?.IsTestCompleted ?? false}" }
            };
            AppendBorderedTable(body, new[] { "项", "值" }, rows);
        }

        private static void AddTargetPointsTable(Body body, string title, IEnumerable<RandomTargetPointRecord>? points)
        {
            AddParagraph(body, title);
            var source = points?.ToList() ?? new List<RandomTargetPointRecord>();
            var headers = new[] { "序号", "目标X(μm)", "目标Y(μm)", "目标X脉冲", "目标Y脉冲" };
            if (source.Count == 0)
            {
                AppendBorderedTable(body, headers, new[] { new[] { "-", "-", "-", "-", "-" } });
                return;
            }

            var rows = source.Select(p => new[]
            {
                p.Index.ToString(CultureInfo.InvariantCulture),
                p.TargetXUm.ToString("F3", CultureInfo.InvariantCulture),
                p.TargetYUm.ToString("F3", CultureInfo.InvariantCulture),
                p.TargetXPulse.ToString(CultureInfo.InvariantCulture),
                p.TargetYPulse.ToString(CultureInfo.InvariantCulture)
            });
            AppendBorderedTable(body, headers, rows);
        }

        private static void AddPointStatsTable(Body body, string title, IEnumerable<RandomPointStatAxisRecord>? stats)
        {
            AddParagraph(body, title);
            var source = stats?.ToList() ?? new List<RandomPointStatAxisRecord>();
            var headers = new[] { "点序号", "样本数", "目标(μm)", "均值(μm)", "标准差(μm)" };
            if (source.Count == 0)
            {
                AppendBorderedTable(body, headers, new[] { new[] { "-", "-", "-", "-", "-" } });
                return;
            }

            var rows = source.Select(s => new[]
            {
                s.PointIndex.ToString(CultureInfo.InvariantCulture),
                s.SampleCount.ToString(CultureInfo.InvariantCulture),
                s.TargetUm.ToString("F3", CultureInfo.InvariantCulture),
                s.MeanUm.ToString("F3", CultureInfo.InvariantCulture),
                s.StdUm.ToString("F3", CultureInfo.InvariantCulture)
            });
            AppendBorderedTable(body, headers, rows);
        }

        private static void AddMoveTripsTable(Body body, string title, IEnumerable<RandomMoveTripAxisRecord>? trips)
        {
            AddParagraph(body, title);
            var source = trips?.ToList() ?? new List<RandomMoveTripAxisRecord>();
            var headers = new[]
            {
                "序号", "点序号", "计划起点(μm)", "实际起点(μm)", "目标(μm)", "实际到位(μm)", "移动时间(ms)", "行程(μm)", "平均速度(μm/s)"
            };
            if (source.Count == 0)
            {
                AppendBorderedTable(body, headers, new[] { new[] { "-", "-", "-", "-", "-", "-", "-", "-", "-" } });
                return;
            }

            var rows = source.Select(t => new[]
            {
                t.SeqNo.ToString(CultureInfo.InvariantCulture),
                t.PointIndex.ToString(CultureInfo.InvariantCulture),
                t.PlannedStartUm.ToString("F3", CultureInfo.InvariantCulture),
                t.ActualStartUm.ToString("F3", CultureInfo.InvariantCulture),
                t.TargetUm.ToString("F3", CultureInfo.InvariantCulture),
                t.ActualTargetUm.ToString("F3", CultureInfo.InvariantCulture),
                t.MoveTimeMs.ToString("F1", CultureInfo.InvariantCulture),
                t.DistanceUm.ToString("F1", CultureInfo.InvariantCulture),
                t.AvgSpeedUmPerSec.ToString("F1", CultureInfo.InvariantCulture)
            });
            AppendBorderedTable(body, headers, rows);
        }

        private static void AddHistogramTable(Body body, string title, IEnumerable<HistogramBinRecord>? bins)
        {
            AddParagraph(body, title);
            var source = bins?.ToList() ?? new List<HistogramBinRecord>();
            var headers = new[] { "序号", "下限", "上限", "计数" };
            if (source.Count == 0)
            {
                AppendBorderedTable(body, headers, new[] { new[] { "-", "-", "-", "-" } });
                return;
            }

            var rows = source.Select((b, i) => new[]
            {
                (i + 1).ToString(CultureInfo.InvariantCulture),
                b.MinValue.ToString("F3", CultureInfo.InvariantCulture),
                b.MaxValue.ToString("F3", CultureInfo.InvariantCulture),
                b.Count.ToString(CultureInfo.InvariantCulture)
            });
            AppendBorderedTable(body, headers, rows);
        }

        private static TableBorders CreateTableBorders()
        {
            return new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 4U, Color = "000000" },
                new BottomBorder { Val = BorderValues.Single, Size = 4U, Color = "000000" },
                new LeftBorder { Val = BorderValues.Single, Size = 4U, Color = "000000" },
                new RightBorder { Val = BorderValues.Single, Size = 4U, Color = "000000" },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4U, Color = "000000" },
                new InsideVerticalBorder { Val = BorderValues.Single, Size = 4U, Color = "000000" });
        }

        private static void AppendBorderedTable(Body body, IReadOnlyList<string> headers, IEnumerable<string[]> dataRows)
        {
            var table = new Table();
            table.AppendChild(new TableProperties(
                new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct },
                CreateTableBorders()));

            var headerRow = new TableRow();
            foreach (var h in headers)
            {
                headerRow.AppendChild(new TableCell(
                    new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Auto }),
                    new Paragraph(
                        new Run(
                            new RunProperties(new Bold(), new RunFonts { Ascii = "Microsoft YaHei", EastAsia = "Microsoft YaHei" }),
                            new Text(h) { Space = SpaceProcessingModeValues.Preserve }))));
            }

            table.AppendChild(headerRow);

            foreach (var row in dataRows)
            {
                var tr = new TableRow();
                for (int i = 0; i < headers.Count; i++)
                {
                    var cellText = i < row.Length ? row[i] : string.Empty;
                    tr.AppendChild(new TableCell(
                        new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Auto }),
                        new Paragraph(
                            new Run(
                                new RunProperties(new RunFonts { Ascii = "Microsoft YaHei", EastAsia = "Microsoft YaHei" }),
                                new Text(cellText) { Space = SpaceProcessingModeValues.Preserve }))));
                }

                table.AppendChild(tr);
            }

            body.AppendChild(table);
        }

        private static void AddPlotSection(MainDocumentPart mainPart, Body body, string title, string? pngPath, uint drawingDocPropId)
        {
            AddParagraph(body, title);
            if (string.IsNullOrWhiteSpace(pngPath) || !File.Exists(pngPath))
            {
                AddParagraph(body, "（图像导出失败或无图像数据）");
                return;
            }

            body.AppendChild(CreateImageParagraph(mainPart, pngPath, drawingDocPropId, imageWidthPx: 1280, imageHeightPx: 720));
        }

        /// <summary>将像素尺寸转为 EMU（96 DPI）。</summary>
        private static (long cx, long cy) PixelsToEmu(int widthPx, int heightPx)
        {
            long cx = widthPx * 914400L / 96L;
            long cy = heightPx * 914400L / 96L;
            return (cx, cy);
        }

        private static Paragraph CreateImageParagraph(
            MainDocumentPart mainPart,
            string imagePath,
            uint drawingDocPropId,
            int imageWidthPx,
            int imageHeightPx)
        {
            var imagePart = mainPart.AddImagePart(ImagePartType.Png);
            using (var fs = File.OpenRead(imagePath))
                imagePart.FeedData(fs);
            string relationshipId = mainPart.GetIdOfPart(imagePart);

            var (cx, cy) = PixelsToEmu(imageWidthPx, imageHeightPx);

            var picture = new PIC.Picture(
                new PIC.NonVisualPictureProperties(
                    new PIC.NonVisualDrawingProperties { Id = 0U, Name = Path.GetFileName(imagePath) },
                    new PIC.NonVisualPictureDrawingProperties(new A.PictureLocks { NoChangeAspect = true })),
                new PIC.BlipFill(
                    new A.Blip { Embed = relationshipId, CompressionState = A.BlipCompressionValues.Print },
                    new A.Stretch(new A.FillRectangle())),
                new PIC.ShapeProperties(
                    new A.Transform2D(
                        new A.Offset { X = 0L, Y = 0L },
                        new A.Extents { Cx = cx, Cy = cy }),
                    new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }));

            var graphicData = new A.GraphicData(picture)
            {
                Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture"
            };

            var graphic = new A.Graphic(graphicData);

            var inline = new DW.Inline(
                new DW.Extent { Cx = cx, Cy = cy },
                new DW.DocProperties { Id = drawingDocPropId, Name = "Plot" },
                new DW.NonVisualGraphicFrameDrawingProperties(new A.GraphicFrameLocks { NoChangeAspect = true }),
                graphic)
            {
                DistanceFromTop = 0U,
                DistanceFromBottom = 0U,
                DistanceFromLeft = 0U,
                DistanceFromRight = 0U
            };

            return new Paragraph(new Run(new Drawing(inline)));
        }
    }
}
