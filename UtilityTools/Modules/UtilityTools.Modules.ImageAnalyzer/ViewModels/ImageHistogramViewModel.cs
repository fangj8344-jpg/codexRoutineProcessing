using NLog;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using OxyPlot.Wpf;
using Prism.Commands;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.ImageAnalyzer.Model;
using UtilityTools.Views;

namespace UtilityTools.Modules.ImageAnalyzer.ViewModels
{
    public class ImageHistogramViewModel : ViewModelBase
    {
        public ImageHistogramViewModel(IContainerProvider containerProvider) : base(containerProvider)
        {
            InitCommand();
            InitProperty();
        }

        #region ------------Field------------
        private LinearAxis _xAxis;
        private LinearAxis _yAxis;
        private LineSeries _imghistSeries;
        private ImgHistInfo _imgHistInfo;
        #endregion


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

        public DelegateCommand SelectRoiCommand { get; set; }
        private void SelectRoi()
        {

        }


        public DelegateCommand<object> DelImgHistPercentageCommand { get; set; }
        private void DelImgHistPercentage(object o)
        {
            var info = o as ImgHistInfoVo;
            ImgHistInfoListVo.Remove(info);
            UpdateAnnotations();
        }

        public DelegateCommand<object> ImageMouseLeftButtonDownEventCommand { get; set; }
        public DelegateCommand<object> ImageMouseLeftButtonUpEventCommand { get; set; }
        public DelegateCommand<object> ImageMouseMoveEventCommand { get; set; }
        private void ImageMouseLeftButtonDownEvent(object o)
        {

        }

        private void ImageMouseLeftButtonUpEvent(object o)
        {
            var e = o as MouseButtonEventArgs;
        }

        public void ImageMouseMoveEvent(object o)
        {
            var e = o as System.Windows.Input.MouseEventArgs;

            var endPoint = e.GetPosition(ImageViewer.Image);
        }

        private ObservableCollection<ImgHistInfoVo> _imgHistInfoList;
        public ObservableCollection<ImgHistInfoVo> ImgHistInfoListVo
        {
            get { return _imgHistInfoList; }
            set { _imgHistInfoList = value; RaisePropertyChanged(); }
        }


        private int _imghistPercentage;
        public int ImghistPercentage
        {
            get { return _imghistPercentage; }
            set
            {
                if (value > 100) { value = 100; }
                if (value <= 0) { value = 1; }

                _imghistPercentage = value;
                RaisePropertyChanged();
            }
        }

        private BitmapImage _srcBitmap;
        public BitmapImage SrcBitmap
        {
            get { return _srcBitmap; }
            set { _srcBitmap = value; RaisePropertyChanged(); }
        }

        private PlotModel _imgInfoPlotModel;
        public PlotModel ImgInfoPlotModel
        {
            get { return _imgInfoPlotModel; }
            set { _imgInfoPlotModel = value; RaisePropertyChanged(); }
        }


        public InkCanvas InkCanvas { get; set; }
        public ImageViewer ImageViewer { get; set; }

        public DelegateCommand AddImgHistPercentageCommand { get; set; }

        private void AddImgHistPercentage()
        {
            if (FilePath is null) { return; }

            int percentage = ImghistPercentage;

            if (_imgHistInfoList.Select(info => info.PixelSpanPercentage).ToHashSet().Contains(percentage))
            {
                MessageBox.Show($"重复的区间: {percentage}%", "错误", MessageBoxButton.OK); return;
            }

            AddImgHistInfo(percentage);
        }

        #endregion

        #region PublicMethod

        public void SaveResult()
        {
            if (FilePath is null) { return; }

            var p = new PathHelper(FilePath);
            var outdir = p.parent / $"{p.stem}.ImageAnalyzer";

            var records = _imgHistInfoList.Select(info => new { info.PixelSpanPercentage, info.PixelSpan }).ToList();
            CsvFileHelper.Write(outdir / $"{p.stem}.ImageAnalyzer.histogram.csv", records, new[] { "区间", "跨度" });

            _imgHistInfo.SaveReportImage(outdir / $"{p.stem}.ImageAnalyzer.png");

            var pngExporter = new PngExporter();
            pngExporter.ExportToFile(ImgInfoPlotModel, outdir / $"{p.stem}.ImageAnalyzer.histogram.png");

            if (MessageBox.Show("是否打开文件夹?", "保存成功", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
                psi.Arguments = $"/e,/select,{outdir}";
                System.Diagnostics.Process.Start(psi);
            }
        }

        public void UpdateImage(string imagePath)
        {
            try
            {
                FilePath = imagePath;

                // 加载图像
                var img = new BitmapImage();
                img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad; // 设置为不占用的方式
                img.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute); // 指定图片文件路径
                img.EndInit();
                SrcBitmap = img;

                // 清理状态信息
                _imghistSeries.Points.Clear();
                ImgHistInfoListVo.Clear();

                _imgHistInfo = new ImgHistInfo(imagePath);
                _imghistSeries.Points.AddRange(_imgHistInfo.Points);

                // 默认添加96%区间像素信息
                AddImgHistInfo(96);
            }
            catch (Exception ex) 
            {
                LogManager.GetCurrentClassLogger().Error(ex.Message);
            }
        }
        #endregion


        #region ------------PrivateMethod------------
        private void InitCommand()
        {
            SelectRoiCommand = new DelegateCommand(SelectRoi);
            AddImgHistPercentageCommand = new DelegateCommand(AddImgHistPercentage);
            DelImgHistPercentageCommand = new DelegateCommand<object>(DelImgHistPercentage);
            ImageMouseLeftButtonDownEventCommand = new DelegateCommand<object>(ImageMouseLeftButtonDownEvent);
            ImageMouseLeftButtonUpEventCommand = new DelegateCommand<object>(ImageMouseLeftButtonUpEvent);
            ImageMouseMoveEventCommand = new DelegateCommand<object>(ImageMouseMoveEvent);
        }

        private void InitProperty()
        {
            var pm = new PlotModel();
            pm.DefaultFont = "SimHei";
            pm.Legends.Add(new Legend());
            _imghistSeries = new LineSeries() { Title = "Histgram", TrackerKey = "TrackerHistgram", RenderInLegend = false };
            _yAxis = new LinearAxis { Position = AxisPosition.Left, Title = "计数", AxislineStyle = LineStyle.Solid, MajorGridlineStyle = LineStyle.Dot, FontSize = 12 };
            _xAxis = new LinearAxis { Position = AxisPosition.Bottom, Title = "灰度值", AxislineStyle = LineStyle.Solid, MajorGridlineStyle = LineStyle.Dot, FontSize = 12 };
            pm.Axes.Add(_xAxis);
            pm.Axes.Add(_yAxis);
            pm.Series.Add(_imghistSeries);
            ImgInfoPlotModel = pm;

            ImghistPercentage = 96;
            ImgHistInfoListVo = new ObservableCollection<ImgHistInfoVo>();
        }




        private void AddImgHistInfo(int percentage = 96)
        {
            ImgHistInfoVo info = _imgHistInfo.GetHistInfo(percentage);
            ImgHistInfoListVo.Add(info);

            UpdateAnnotations();
        }

        private void UpdateAnnotations()
        {
            ImgInfoPlotModel.Annotations.Clear();

            List<ImgHistInfoVo> infos = new List<ImgHistInfoVo>(ImgHistInfoListVo);
            infos = infos.OrderBy(info => info.PixelSpanPercentage).ToList();
            for (int i = 0; i < infos.Count; i++)
            {
                AddAnnotation(infos[i], _imgHistInfo.MaxY * (1 + 0.05 * (i + 1)));
            }

            // 更新坐标轴显示
            ImgInfoPlotModel.Axes.Clear();
            var padX = (_imgHistInfo.MaxX - _imgHistInfo.MinX) * 0.1;
            _xAxis.Minimum = _imgHistInfo.MinX - padX;
            _xAxis.Maximum = _imgHistInfo.MaxX + padX;
            var maxY = _imgHistInfo.MaxY * (1 + 0.05 * infos.Count);
            var padY = maxY * 0.1;
            _yAxis.Minimum = 0 - padY;
            _yAxis.Maximum = maxY + padY;
            ImgInfoPlotModel.Axes.Add(_xAxis);
            ImgInfoPlotModel.Axes.Add(_yAxis);

            ImgInfoPlotModel.ResetAllAxes();
            ImgInfoPlotModel.InvalidatePlot(true);
        }

        private void AddAnnotation(ImgHistInfoVo info, double Y)
        {
            var la_hori = new LineAnnotation();
            la_hori.Type = LineAnnotationType.Horizontal;
            la_hori.Y = Y;
            la_hori.Color = OxyColors.Green;
            la_hori.MinimumX = info.LeftX;
            la_hori.MaximumX = info.RightX;
            ImgInfoPlotModel.Annotations.Add(la_hori);

            var ta = new TextAnnotation();
            ta.TextPosition = new DataPoint((info.RightX + info.LeftX) / 2, Y);
            ta.Text = info.Desc;
            ta.TextColor = OxyColors.Black;
            ta.StrokeThickness = 0;
            ImgInfoPlotModel.Annotations.Add(ta);

            var la_vert1 = new LineAnnotation();
            la_vert1.Type = LineAnnotationType.Vertical;
            la_vert1.Color = OxyColors.Red;
            la_vert1.X = info.LeftX;
            la_vert1.MinimumY = info.LeftY;
            la_vert1.MaximumY = Y;
            ImgInfoPlotModel.Annotations.Add(la_vert1);

            var la_vert2 = new LineAnnotation();
            la_vert2.Type = LineAnnotationType.Vertical;
            la_vert2.Color = OxyColors.Red;
            la_vert2.X = info.RightX;
            la_vert2.MinimumY = info.RightY;
            la_vert2.MaximumY = Y;
            ImgInfoPlotModel.Annotations.Add(la_vert2);
        }

        #endregion
    }
}
