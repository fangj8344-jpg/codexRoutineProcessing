#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2025   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.AutoFocusTest.ViewModels
 * 唯一标识：721e3a63-abd0-45fa-b9ff-9633a951295a
 * 文件名：AutoFocusTestViewModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2025/4/28 14:29:57
 * 版本：V1.0.0
 * 描述：
 *
 * ----------------------------------------------------------------
 * 修改人：
 * 时间：
 * 修改说明：
 *
 * 版本：V1.0.1
 *----------------------------------------------------------------*/
#endregion

using Microsoft.Win32;
using OpenCvSharp;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.AutoFocusTest.Model;

namespace UtilityTools.Modules.AutoFocusTest.ViewModels
{
    public class LineSeriesInfo : BindableBase
    {
        public LineSeriesInfo(PlotModel plot, string titleName)
        {
            Datas = new List<IntegerChartData>();
            _lineSeries = new LineSeries() { Title = titleName, RenderInLegend = true };
            _lineSeries.ItemsSource = Datas;
            _lineSeries.DataFieldX = "Key";
            _lineSeries.DataFieldY = "Value";
            plot.Series.Add(_lineSeries);
            BindingOperations.EnableCollectionSynchronization(Datas, _locker);
        }

        private LineSeries _lineSeries;
        private readonly object _locker = new object();

        private List<IntegerChartData> _datas;

        public List<IntegerChartData> Datas
        {
            get { return _datas; }
            set { _datas = value; RaisePropertyChanged(); }
        }

        public void Add(IntegerChartData data) 
        {
            if(Datas != null)
                Datas.Add(data);
        }

        public void Clear()
        {
            if (Datas != null)
                Datas.Clear();
        }
    }

    public class AutoFocusTestViewModel : RegionViewModelBase
    {
        #region ------------Constructor------------
        public AutoFocusTestViewModel(IDialogHostService dialogHostService, IContainerProvider containerProvider)
            : base(containerProvider)
        {
            this._containerProvider = containerProvider;
            this._dialogHostService = dialogHostService;

            LoadImagePathCommand = new DelegateCommand(LoadImagePath);
            BindingOperations.EnableCollectionSynchronization(BindItems, _locker);

            InitPlot();
        }
        #endregion

        #region ------------Field------------
        private IDialogHostService _dialogHostService;
        private IContainerProvider _containerProvider;
        private BackgroundWorker _worker;
        private readonly object _locker = new object();
        #endregion

        #region ------------Property------------

        private PlotModel _resultPlot;
        /// <summary>
        /// 自动标定结果
        /// </summary>
        public PlotModel ResultPlot
        {
            get { return _resultPlot; }
            set { _resultPlot = value; RaisePropertyChanged(); }
        }

        private string _filePath;
        /// <summary>
        /// 文件夹名称
        /// </summary>
        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; RaisePropertyChanged(); }
        }

        private bool _completed = true;
        /// <summary>
        /// 是否已经分析完成
        /// </summary>
        public bool Completed
        {
            get { return _completed; }
            set { _completed = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<ImgListBindData> _bindItems = new ObservableCollection<ImgListBindData>();
        /// <summary>
        /// 绑定的资源元素集合
        /// </summary>
        public ObservableCollection<ImgListBindData> BindItems
        {
            get { return _bindItems; }
            set { _bindItems = value; RaisePropertyChanged(); }
        }

        private LineSeriesInfo _tenengrad;

        public LineSeriesInfo Tenengrad
        {
            get { return _tenengrad; }
            set { _tenengrad = value; RaisePropertyChanged(); }
        }

        private LineSeriesInfo _laplacian;

        public LineSeriesInfo Laplacian
        {
            get { return _laplacian; }
            set { _laplacian = value; RaisePropertyChanged(); }
        }

        private LineSeriesInfo _brenner;

        public LineSeriesInfo Brenner
        {
            get { return _brenner; }
            set { _brenner = value; RaisePropertyChanged(); }
        }

        private LineSeriesInfo _multiScale;

        public LineSeriesInfo MultiScale
        {
            get { return _multiScale; }
            set { _multiScale = value; RaisePropertyChanged(); }
        }

        private LineSeriesInfo _noise;

        public LineSeriesInfo Noise
        {
            get { return _noise; }
            set { _noise = value; RaisePropertyChanged(); }
        }

        #endregion

        #region ------------Command------------
        public DelegateCommand LoadImagePathCommand { get; set; }

        private void LoadImagePath()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                FilePath = dialog.SelectedPath;
                StartAnalysis(FilePath);
            }
        }
        #endregion

        #region ------------PublicMethod------------
        public void StartAnalysis(string filePath)
        {
            StopAnalysis();

            _worker = new BackgroundWorker();
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += BackgroundWorker_DoWork;
            _worker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
            _worker.RunWorkerAsync(filePath);
        }

        public void StopAnalysis()
        {
            if (_worker != null)
            {
                _worker.CancelAsync();
            }
        }
        #endregion

        #region ------------PrivateMethod------------
        private void InitPlot()
        {
            // 初始化图表信息
            ResultPlot = new PlotModel();
            ResultPlot.Legends.Add(new Legend());

            ResultPlot.Axes.Add(new LinearAxis() { Title = "物镜值", Position = OxyPlot.Axes.AxisPosition.Bottom });
            ResultPlot.Axes.Add(new LogarithmicAxis() { Title = "计算结果", Position = OxyPlot.Axes.AxisPosition.Left });

            Tenengrad = new LineSeriesInfo(ResultPlot, "Tenengrad");
            Laplacian = new LineSeriesInfo(ResultPlot, "Laplacian");
            Brenner = new LineSeriesInfo(ResultPlot, "Brenner");
            MultiScale = new LineSeriesInfo(ResultPlot, "MultiScale");
            Noise = new LineSeriesInfo(ResultPlot, "Noise");
        }

        /// <summary>
        /// 受支持的图片格式
        /// </summary>
        private List<string> supportedPicType = new List<string>()
        {
            ".jpeg",
            ".jpg",
            ".png",
            ".bmp",
            ".tiff",
        };
        /// <summary>
        /// 判断该扩展名是否是受支持的图片类型
        /// </summary>
        private bool IsBmpSupport(string ext)
        {
            return supportedPicType.FindAll((c) => c.Contains(ext.ToLower())).Count > 0;
        }

        /// <summary>
        /// 从文件路径中获取文件名
        /// </summary>
        private static String GetFileName(String fileName)
        {
            if (fileName == null || fileName == "")
                return "";
            return fileName.Substring(fileName.LastIndexOf("\\") + 1);
        }
        /// <summary>
        /// 获得文件后缀名
        /// </summary>
        private static String GetEndFile(String fileName)
        {
            return fileName.Substring(fileName.LastIndexOf(".") + 1);
        }

        /// <summary>
        /// 获得没有后缀的文件名
        /// </summary>
        private static string GetFileNameEx(String fileName)
        {
            try
            {
                return GetFileName(fileName).Substring(0, GetFileName(fileName).LastIndexOf(GetEndFile(fileName)) - 1);
            }
            catch { return ""; }
        }

        private bool IsValidImage(String fileName)
        {
            var list = fileName.Split('_');
            return list.Count() == 4;
        }

        private int GetObValue(string fileName)
        {
            var list = fileName.Split('_');
            if (list.Length == 4 && int.TryParse(list[1], out int value))
            {
                return value;
            }

            return 0;
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var bw = sender as BackgroundWorker;

            Completed = false;
            var filePath = e.Argument as string;
            DirectoryInfo theFolder = new DirectoryInfo(filePath);
            if (!theFolder.Exists)
            {
                e.Result = false;
                return;
            }

            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                BindItems.Clear();
                Tenengrad.Clear();
                Laplacian.Clear();
                Brenner.Clear();
                MultiScale.Clear();
                Noise.Clear();
            }));

            var files = new DirectoryInfo(filePath).GetFiles();
            var sortedFiles = files.OrderByDescending(f => f.LastWriteTime);

            foreach (var file in sortedFiles)
            {
                if (bw?.CancellationPending == true)
                {
                    e.Cancel = true;
                    return;
                }

                if (IsBmpSupport(file.Extension) && IsValidImage(file.Name))
                {
                    var tmpBindData = new ImgListBindData();
                    tmpBindData.FileName = GetFileNameEx(file.Name);
                    tmpBindData.ObValue = GetObValue(tmpBindData.FileName);
                    tmpBindData.FilePath = file.FullName;
                    tmpBindData.ImgWidth = 250;
                    tmpBindData.ImgHeight = 250;
                    BindItems.Add(tmpBindData);


                    Mat gray = new Mat(file.FullName, ImreadModes.LoadGdal);
                    Cv2.MinMaxLoc(gray, out double min, out double max);

                    if (min > 0 && max < 0xFFF)
                    {
                        Cv2.MedianBlur(gray, gray, 5);
                        var tenengrad = AutoFocusMethod.CalculateTenengrad(gray);
                        var laplacian = AutoFocusMethod.Laplacian(gray);
                        var brenner = AutoFocusMethod.CalculateBrenner(gray);
                        var multi = AutoFocusMethod.MultiScaleSharpness(gray);
                        var noise = AutoFocusMethod.ComputeNoise(gray);

                        Tenengrad.Add(new IntegerChartData(tmpBindData.ObValue, tenengrad));
                        Laplacian.Add(new IntegerChartData(tmpBindData.ObValue, laplacian));
                        Brenner.Add(new IntegerChartData(tmpBindData.ObValue, brenner));
                        MultiScale.Add(new IntegerChartData(tmpBindData.ObValue, multi));
                        Noise.Add(new IntegerChartData(tmpBindData.ObValue, noise));

                        ResultPlot.InvalidatePlot(true);
                    }
                }


            }
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Completed = true;
            ResultPlot.InvalidatePlot(true);
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
