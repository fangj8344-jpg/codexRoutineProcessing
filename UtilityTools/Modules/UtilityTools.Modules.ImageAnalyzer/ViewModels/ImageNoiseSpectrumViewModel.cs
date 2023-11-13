using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using Prism.Commands;
using Prism.Ioc;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.ImageAnalyzer.Model;

namespace UtilityTools.Modules.ImageAnalyzer.ViewModels
{
    public class ImageNoiseSpectrumViewModel : ViewModelBase
    {
        public ImageNoiseSpectrumViewModel(IContainerProvider containerProvider) : base(containerProvider)
        {
            InitCommand();
            InitProperty();
        }

        #region ------------Property------------
        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region PublicMethod
        public void SaveResult()
        {
            if (FilePath is null) { return; }

            var p = new PathHelper(FilePath);
            var outdir = p.parent / $"{p.stem}.ImageAnalyzer";

            var records_row = _rowSpectrumPeakList.Select(info => new { info.ID, info.Freq, info.Magnitude }).ToList();
            CsvFileHelper.Write(outdir / $"{p.stem}.ImageAnalyzer.spectrum_row.csv", records_row, new[] { "序号", "频率", "幅值" });

            var records_columns = _columnSpectrumPeakList.Select(info => new { info.ID, info.Freq, info.Magnitude }).ToList();
            CsvFileHelper.Write(outdir / $"{p.stem}.ImageAnalyzer.spectrum_column.csv", records_columns, new[] { "序号", "频率", "幅值" });


            var pngExporter = new PngExporter();
            pngExporter.ExportToFile(RowSpectrumMeanOxyModel, outdir / $"{p.stem}.ImageAnalyzer.spectrum_row.png");
            pngExporter.ExportToFile(ColumnSpectrumMeanOxyModel, outdir / $"{p.stem}.ImageAnalyzer.spectrum_column.png");
        }
        #endregion


        #region PrivateMethod
        private void InitCommand()
        {
            DelRowPeakCommand = new DelegateCommand<object>(DelRowPeak);
            DelColumnPeakCommand = new DelegateCommand<object>(DelColumnPeak);
        }

        private void InitProperty()
        {
            RowSpectrumMeanOxyModel = new PlotModel() { DefaultFont = "SimHei", Title = "高频谱图" };

            var pmRowSpectrum = new PlotModel();
            RowSpectrumOxyModel = pmRowSpectrum;

            ColumnSpectrumMeanOxyModel = new PlotModel() { DefaultFont = "SimHei", Title = "低频谱图" }; ;

            var pmColumnSpectrum = new PlotModel();
            ColumnSpectrumOxyModel = pmColumnSpectrum;

            RowSpectrumPeakList = new ObservableCollection<SpectrumPeakVo>();
            ColumnSpectrumPeakList = new ObservableCollection<SpectrumPeakVo>();
        }
        #endregion


        private PlotModel _rowSpectrumMeanOxyModel;
        public PlotModel RowSpectrumMeanOxyModel
        {
            get { return _rowSpectrumMeanOxyModel; }
            set { _rowSpectrumMeanOxyModel = value; RaisePropertyChanged(); }
        }

        private PlotModel _rowSpectrumOxyModel;

        public PlotModel RowSpectrumOxyModel
        {
            get { return _rowSpectrumOxyModel; }
            set { _rowSpectrumOxyModel = value; RaisePropertyChanged(); }
        }

        private PlotModel _columnSpectrumOxyModel;

        public PlotModel ColumnSpectrumOxyModel
        {
            get { return _columnSpectrumOxyModel; }
            set { _columnSpectrumOxyModel = value; RaisePropertyChanged(); }
        }


        private PlotModel _columnSpectrumMeanOxyModel;
        public PlotModel ColumnSpectrumMeanOxyModel
        {
            get { return _columnSpectrumMeanOxyModel; }
            set { _columnSpectrumMeanOxyModel = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<SpectrumPeakVo> _rowSpectrumPeakList;
        public ObservableCollection<SpectrumPeakVo> RowSpectrumPeakList
        {
            get { return _rowSpectrumPeakList; }
            set { _rowSpectrumPeakList = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<SpectrumPeakVo> _columnSpectrumPeakList;
        public ObservableCollection<SpectrumPeakVo> ColumnSpectrumPeakList
        {
            get { return _columnSpectrumPeakList; }
            set { _columnSpectrumPeakList = value; RaisePropertyChanged(); }
        }

        public DelegateCommand<object> DelRowPeakCommand { get; set; }
        private void DelRowPeak(object o)
        {
        }

        public DelegateCommand<object> DelColumnPeakCommand { get; set; }
        private void DelColumnPeak(object o)
        {
        }


        public void UpdateImage(string imagePath)
        {
            FilePath = imagePath;

            var _meta = ImageMetadata.ReadMetadata(imagePath);
            float ad_freq = 40 * 1000000; // 40MHz
            float sample_freq = ad_freq / _meta.AvgLength;

            {
                var info = new ImgSpectrumInfo(imagePath, sample_freq);
                updateSpectrumMean(RowSpectrumMeanOxyModel, RowSpectrumPeakList, info, "RowSpectrum");
                updateSpectrumImg(RowSpectrumOxyModel, info);
            }


            {
                var info = new ImgSpectrumInfo(imagePath, sample_freq, true);
                updateSpectrumMean(ColumnSpectrumMeanOxyModel, ColumnSpectrumPeakList, info, "ColumnSpectrum");
                updateSpectrumImg(ColumnSpectrumOxyModel, info);
            }
        }

        private void updateSpectrumMean(PlotModel pm, ObservableCollection<SpectrumPeakVo> peaks, ImgSpectrumInfo info, string title)
        {
            peaks.Clear();

            pm.Annotations.Clear();
            pm.Series.Clear();
            var series = new LineSeries() { Title = title, RenderInLegend = false };
            series.Points.AddRange(info.Points);
            pm.Series.Add(series);

            var pf = new PeakFinder1D(info.Points.Select(p => p.Y).ToList());
            int id = 1;
            var maxY = info.Points.MaxBy(p => p.Y).Y;
            foreach (var i in pf.FindPeaks(topk: 5))
            {
                peaks.Add(new SpectrumPeakVo() { ID = id++, Freq = info.Points[i].X, Magnitude = info.Points[i].Y });
                pm.Annotations.Add(new PointAnnotation
                {
                    X = info.Points[i].X,
                    Y = info.Points[i].Y,
                    Text = $"{info.Points[i].X:f3}Hz",
                    FontSize = 14,
                    Shape = MarkerType.Cross,
                    Stroke = OxyColors.IndianRed,
                    StrokeThickness = 2,
                    TextVerticalAlignment = VerticalAlignment.Bottom
                });

                maxY = Math.Max(maxY, info.Points[i].Y * 1.2);
            }

            pm.Axes.Clear();
            //pm.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "强度谱(DBFS)", AxislineStyle = LineStyle.Solid, MajorGridlineStyle = LineStyle.Dot, FontSize = 12, Maximum = maxY });
            pm.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "幅值", AxislineStyle = LineStyle.Solid, MajorGridlineStyle = LineStyle.Dot, FontSize = 12, Maximum = maxY });
            pm.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = $"频率(Hz) df={info.FreqResolution:f3}Hz", AxislineStyle = LineStyle.Solid, MajorGridlineStyle = LineStyle.Dot, FontSize = 12 });
            pm.ResetAllAxes();

            pm.InvalidatePlot(true);
        }

        private void updateSpectrumImg(PlotModel pm, ImgSpectrumInfo info)
        {
            int n = info.SpectrumColor.Width;
            float maxY = info.FreqResolution * n / 2;
            pm.Annotations.Clear();
            pm.Annotations.Add(new ImageAnnotation
            {
                ImageSource = new OxyImage(info.SpectrumColor.ToMemoryStream()),
                X = new PlotLength(0, PlotLengthUnit.Data),
                Y = new PlotLength(maxY, PlotLengthUnit.Data),
                Width = new PlotLength(n, PlotLengthUnit.Data),
                Height = new PlotLength(maxY, PlotLengthUnit.Data),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            });

            pm.Axes.Clear();
            string xLabel = info.IsColumnDFT ? "列序号" : "行序号";
            pm.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Minimum = 0, Maximum = n, Title = xLabel });
            pm.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = 0, Maximum = maxY, Title = "频率(Hz)" });
            pm.ResetAllAxes();
            pm.InvalidatePlot(true);
        }
    }
}
