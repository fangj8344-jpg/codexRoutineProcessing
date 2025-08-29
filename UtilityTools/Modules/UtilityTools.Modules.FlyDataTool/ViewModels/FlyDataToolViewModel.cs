#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.FlyDataTool.ViewModels
 * 唯一标识：419c2284-31b3-477d-b7cf-4f1c6f92df05
 * 文件名：FlyDataViewModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/3/21 16:22:13
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
using OxyPlot.Wpf;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Model;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.FlyDataTool.Model;

namespace UtilityTools.Modules.FlyDataTool.ViewModels
{
    public class FlyDataToolViewModel : RegionViewModelBase
    {
        #region ------------Constructor------------
        public FlyDataToolViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService)
            : base(containerProvider)
        {
            _dialogHostService = dialogHostService;
            InitProperty();
            InitCommand();
        }

        #endregion

        #region ------------Field------------
        private readonly IDialogHostService _dialogHostService;

        private Random _random = new Random();
        List<int> _dataIndex = new List<int>();
        #endregion

        #region ------------Property------------
        private string _filePath = "";
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; RaisePropertyChanged(); }
        }

        private PlotModel _flyDataPlotModel;
        /// <summary>
        /// 飞行数据图表模型
        /// </summary>
        public PlotModel FlyDataPlotModel
        {
            get { return _flyDataPlotModel; }
            set { _flyDataPlotModel = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<FlyDataModel> _flyDataModels;
        /// <summary>
        /// 累计飞行数据集合
        /// </summary>
        public ObservableCollection<FlyDataModel> FlyDataModels
        {
            get { return _flyDataModels; }
            set { _flyDataModels = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<SingleOptModel> _singleFlyDataModels;
        /// <summary>
        /// 单次飞行数据集合
        /// </summary>
        public ObservableCollection<SingleOptModel> SingleFlyDataModels
        {
            get { return _singleFlyDataModels; }
            set { _singleFlyDataModels = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<FlyDataModel> _curFlyDataModels;
        /// <summary>
        /// 当前选中的飞行数据集合
        /// </summary>
        public ObservableCollection<FlyDataModel> CurFlyDataModels
        {
            get { return _curFlyDataModels; }
            set { _curFlyDataModels = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<LineTypeModel> _lineTypeModels;
        /// <summary>
        /// 图表线类型集合
        /// </summary>
        public ObservableCollection<LineTypeModel> LineTypeModels
        {
            get { return _lineTypeModels; }
            set { _lineTypeModels = value; RaisePropertyChanged(); }
        }

        private double _allFlyTime;
        /// <summary>
        /// 累计飞行时间
        /// </summary>
        public double AllFlyTime
        {
            get { return _allFlyTime; }
            set { _allFlyTime = value; RaisePropertyChanged(); }
        }

        private double _maxSingleTime;
        /// <summary>
        /// 最大单次时间
        /// </summary>
        public double MaxSingleTime
        {
            get { return _maxSingleTime; }
            set { _maxSingleTime = value; RaisePropertyChanged(); }
        }

        private bool _isAxisXShift;
        /// <summary>
        /// X轴坐标含义是否变换，true表示时间，false表示索引
        /// </summary>
        public bool IsAxisXShift
        {
            get { return _isAxisXShift; }
            set 
            { 
                _isAxisXShift = value;
                RaisePropertyChanged();

                CreateAllLine();
            }
        }
        #endregion

        #region ------------Command------------
        /// <summary>
        /// 选择文件指令
        /// </summary>
        public DelegateCommand SelectFileCommand { get; set; }

        /// <summary>
        /// 选择所有飞行数据指令
        /// </summary>
        public DelegateCommand SelectAllFlyDataCommand { get; set; }

        /// <summary>
        /// 选择一次飞行过程指令
        /// </summary>
        public DelegateCommand<object> SelectSingleCommand { get; set; }

        /// <summary>
        /// 图表自适应显示指令
        /// </summary>
        public DelegateCommand PlotAutoAdjustCommand { get; set; }

        /// <summary>
        /// 保存图表指令
        /// </summary>
        public DelegateCommand SavePlotCommand { get; set; }
        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        /// <summary>
        /// 初始化指令
        /// </summary>
        private void InitCommand()
        {
            SelectFileCommand = new DelegateCommand(SelectFile);
            SelectAllFlyDataCommand = new DelegateCommand(SelectAllFlyData);
            SelectSingleCommand = new DelegateCommand<object>(SelectSingle);
            PlotAutoAdjustCommand = new DelegateCommand(PlotAutoAdjust);
            SavePlotCommand = new DelegateCommand(SavePlot);
        }

        /// <summary>
        /// 初始化属性
        /// </summary>
        private void InitProperty()
        {
            FlyDataPlotModel = new PlotModel();
            FlyDataModels = new ObservableCollection<FlyDataModel>();
            SingleFlyDataModels = new ObservableCollection<SingleOptModel>();
            CurFlyDataModels = new ObservableCollection<FlyDataModel>();
            LineTypeModels = new ObservableCollection<LineTypeModel>();

            LineTypeModels.Add(new LineTypeModel() { Name = "加速电压", Field = "AccVol", YAxisKey = "Y1", AxisType = 0, MarkerType = 0 });
            LineTypeModels.Add(new LineTypeModel() { Name = "灯丝电流", Field = "FilaCur", YAxisKey = "Y2", AxisType = 0, MarkerType = 0 });
            //LineTypeModels.Add(new LineTypeModel() { Name = "灯丝电阻", Field="FilaR", YAxisKey = "Y3", AxisType = 0, MarkerType = 0 });
            LineTypeModels.Add(new LineTypeModel() { Name = "栅极电压", Field = "GridVol", YAxisKey = "Y4", AxisType = 0, MarkerType = 0 });

        }

        /// <summary>
        /// 选择文件
        /// </summary>
        private void SelectFile()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Title = "请选择飞行数据文件";
            dialog.Filter = "飞行数据文件|*.csv;*.xlsx";
            var result = dialog.ShowDialog();
            if (result != null && result == true)
            {
                FilePath = dialog.FileName;
                Application.Current.Dispatcher.InvokeAsync(() => LoadFile());
            }
        }

        /// <summary>
        /// 选择显示所有飞行数据
        /// </summary>
        private void SelectAllFlyData()
        {
            CurFlyDataModels = FlyDataModels;
            for (int j = 0; j < CurFlyDataModels.Count; j++)
            {
                CurFlyDataModels[j].Index = j;
            }
            CreateAllLine();
        }

        /// <summary>
        /// 选择显示一次或多次飞行数据
        /// </summary>
        private void SelectSingle(object obj)
        {
            var SelectionChanged = (System.Windows.Controls.SelectionChangedEventArgs)obj;
            var Items = SelectionChanged.AddedItems;
            var RemoveItems = SelectionChanged.RemovedItems;

            foreach (var addItem in Items)
            {
                var model = addItem as SingleOptModel;
                if (model != null)
                {
                    var index = model.FlyIndex - 1;
                    if (!_dataIndex.Contains(index))
                    {
                        _dataIndex.Add(index);
                    }
                }
            }

            foreach (var removeItem in RemoveItems)
            {
                var model = removeItem as SingleOptModel;
                if (model != null)
                {
                    var index = model.FlyIndex - 1;
                    if (_dataIndex.Contains(index))
                    {
                        _dataIndex.Remove(index);
                    }
                }
            }

            _dataIndex.Sort();
            CurFlyDataModels = new ObservableCollection<FlyDataModel>();
            foreach (var dataindex in _dataIndex)
            {
                SingleOptModel model = this.SingleFlyDataModels[dataindex];
                if (model == null)
                    return;
                for (int i = 0; i < model.FlyDataModels.Count; i++)
                {
                    CurFlyDataModels.Add(model.FlyDataModels[i]);
                }
            }
            for (int j = 0; j < CurFlyDataModels.Count; j++)
            {
                CurFlyDataModels[j].Index = j;
            }

            CreateAllLine();

            //if (Items.Count > 0 || RemoveItems.Count > 0)
            //{
            //    var Item = Items.Count > 0 ? Items[0] : RemoveItems[0];
            //    var Model = (SingleOptModel)Item;
            //    var Index = Model.FlyIndex - 1;
            //    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            //    {
            //        //键盘ctrl键触发时
            //        if (!_dataIndex.Contains(Index))
            //        {
            //            _dataIndex.Add(Index);
            //        }
            //        else
            //        {
            //            _dataIndex.Remove(Index);
            //        }
            //        _dataIndex.Sort();
            //        CurFlyDataModels = new ObservableCollection<FlyDataModel>();
            //        foreach (var dataindex in _dataIndex)
            //        {
            //            SingleOptModel model = this.SingleFlyDataModels[dataindex];
            //            if (model == null)
            //                return;
            //            for (int i = 0; i < model.FlyDataModels.Count; i++)
            //            {
            //                CurFlyDataModels.Add(model.FlyDataModels[i]);
            //            }
            //        }
            //        for (int j = 0; j < CurFlyDataModels.Count; j++)
            //        {
            //            CurFlyDataModels[j].Index = j;
            //        }

            //        CreateAllLine();
            //    }
            //    else
            //    {
            //        if (Index is int i)
            //        {
            //            _dataIndex.Clear();
            //            _dataIndex.Add(Index);
            //            if (this.SingleFlyDataModels.Count <= i || i < 0)
            //                return;
            //            SingleOptModel model = this.SingleFlyDataModels[i];
            //            if (model == null)
            //                return;
            //            CurFlyDataModels = model.FlyDataModels;
            //            for (int j = 0; j < CurFlyDataModels.Count; j++)
            //            {
            //                CurFlyDataModels[j].Index = j;
            //            }
            //            CreateAllLine();
            //        }
            //    }
            //}
        }

        /// <summary>
        /// 图表显示自适应
        /// </summary>
        private void PlotAutoAdjust()
        {
            foreach (var axis in FlyDataPlotModel.Axes)
                axis.Reset();
            FlyDataPlotModel.InvalidatePlot(true);
        }

        /// <summary>
        /// 保存图表
        /// </summary>
        private void SavePlot() 
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "请选择保存的路径";
            dialog.Filter = "图片 |*.png";
            dialog.DefaultExt = "png";
            var result = dialog.ShowDialog();
            if (result != null && result == true)
            {
                var filePath = dialog.FileName;
                var pngExporter = new PngExporter() { Width = (int)FlyDataPlotModel.Width, Height = (int)FlyDataPlotModel.Height };
                pngExporter.ExportToFile(FlyDataPlotModel, filePath);
            }
        }

        /// <summary>
        /// 加载文件
        /// </summary>
        private void LoadFile()
        {
            var table = ExcelHelper.GetDataGrid(FilePath);
            if (table == null)
                return;
            if (FlyDataModels == null)
                FlyDataModels = new();
            FlyDataModels.Clear();
            ObservableCollection<SingleOptModel> tmpModel = new();

            FlyDataModel lastModel = null;
            SingleOptModel processModel = null;
            for (int i = 0, index = 0; i < table.Rows.Count; i++)
            {
                if (string.IsNullOrEmpty(table.Rows[i][0].ToString()))
                {
                    continue;
                }
                var model = new FlyDataModel();
                model.Index = i;
                var str = table.Rows[i][0].ToString().Trim();
                if (double.TryParse(str, out double time))
                {
                    model.Time = TimeHelper.GetTimeFromLabview(time);
                }
                else
                {
                    continue;
                }
                str = table.Rows[i][1].ToString().Trim();
                if (double.TryParse(str, out double acc))
                {
                    model.AccVol = acc;
                }
                else
                {
                    continue;
                }
                str = table.Rows[i][2].ToString().Trim();
                if (double.TryParse(str, out double filaCur))
                {
                    model.FilaCur = filaCur;
                }
                else
                {
                    continue;
                }
                str = table.Rows[i][3].ToString().Trim();
                if (double.TryParse(str, out double filaR))
                {
                    model.FilaR = filaR;
                }
                else
                {
                    continue;
                }
                str = table.Rows[i][4].ToString().Trim();
                if (double.TryParse(str, out double gridVol))
                {
                    model.GridVol = gridVol;
                }
                else
                {
                    continue;
                }

                FlyDataModels.Add(model);
                if (lastModel == null)
                {
                    lastModel = model;
                    processModel = new() { FlyIndex = index + 1, FlyDataModels = new(), Title = $"第{index + 1}次开枪" };
                    processModel.FlyDataModels.Add(model);
                }
                else
                {
                    var dif = model.Time - lastModel.Time;
                    if (dif.TotalSeconds < 180)
                    {
                        //认为是一次开枪过程
                        processModel.FlyDataModels.Add(model);
                    }
                    else
                    {
                        tmpModel.Add(processModel);
                        index++;
                        processModel = new() { FlyIndex = index + 1, FlyDataModels = new(), Title = $"第{index + 1}次开枪" };
                        processModel.FlyDataModels.Add(model);
                    }
                    lastModel = model;
                }
            }
            if (tmpModel != null)
                tmpModel.Add(processModel);

            this.CurFlyDataModels = FlyDataModels;
            ParseFlyData(ref tmpModel);
            CreateAllLine();

            this.SingleFlyDataModels = tmpModel;
        }

        /// <summary>
        /// 解析参数
        /// </summary>
        /// <param name="models"></param>
        private void ParseFlyData(ref ObservableCollection<SingleOptModel> models)
        {
            double allTime = 0;
            double maxTime = 0;
            foreach (var p in models)
            {
                SingleOptModel process = p as SingleOptModel;
                if (process == null)
                    continue;
                // 计算单次开枪时间
                if (process.FlyDataModels.Count == 1)
                {
                    process.BeginTime = process.FlyDataModels[0].Time - TimeSpan.FromSeconds(60);
                    process.EndTime = process.FlyDataModels[0].Time;
                }
                else
                {
                    process.BeginTime = process.FlyDataModels.First<FlyDataModel>().Time;
                    process.EndTime = process.FlyDataModels.Last<FlyDataModel>().Time;
                }
                var span = process.EndTime - process.BeginTime;
                process.FlyTime = span.TotalSeconds;
                allTime += process.FlyTime;
                if (maxTime < process.FlyTime)
                    maxTime = process.FlyTime;
                // 计算平均电阻
                double allR = 0.0;
                foreach (var model in process.FlyDataModels)
                {
                    allR += model.FilaR;
                }
                process.AveFilaR = allR / process.FlyDataModels.Count;
            }
            this.AllFlyTime = allTime;
            this.MaxSingleTime = maxTime;
        }

        /// <summary>
        /// 绘制图表
        /// </summary>
        public void CreateAllLine()
        {
            var model = new PlotModel
            {
                //Title = "飞行数据曲线图", //图表的Titile
                //Subtitle = "折线图" //图表的说明
            };

            model.Legends.Add(new Legend()
            {
                LegendPlacement = LegendPlacement.Inside,
                LegendPosition = LegendPosition.TopRight,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendBorderThickness = 0,
                LegendTextColor = OxyColors.LightGray
            });

            // 默认线条
            var series = new LineSeries
            {
                Title = "灯丝电阻", //线的说明
                //MarkerType = MarkerType.Circle //标记点 的类型、形状
            };
            series.ItemsSource = CurFlyDataModels;
            series.YAxisKey = "Y0";
            series.DataFieldX = IsAxisXShift ? "Time" : "Index";
            series.DataFieldY = "FilaR";

            model.Series.Add(series);//将线添加到图标的容器中
            model.Axes.Add(new LinearAxis() { Key = "Y0", Position = AxisPosition.Right });
            if (IsAxisXShift)
            {
                model.Axes.Add(new DateTimeAxis() { Position = AxisPosition.Bottom, Angle = 45, StringFormat = "yy-MM-dd HH:mm:ss" });
            }
            else
            {
                model.Axes.Add(new LinearAxis() { Position = AxisPosition.Bottom });
            }


            double distance = 0;
            var list = LineTypeModels.Where(p => p.IsChecked);
            if (list.Count() > 0)
            {
                foreach (var item in list)
                {
                    series = null;
                    switch (item.MarkerType)
                    {
                        case 0:
                            series = new LineSeries
                            {
                                Title = item.Name, //线的说明
                                MarkerType = MarkerType.Circle //标记点 的类型、形状
                            };
                            break;
                        case 1:
                            series = new LineSeries
                            {
                                Title = item.Name, //线的说明
                                MarkerType = MarkerType.Square //标记点 的类型、形状
                            };
                            break;
                        case 2:
                            series = new LineSeries
                            {
                                Title = item.Name, //线的说明
                                MarkerType = MarkerType.Diamond //标记点 的类型、形状
                            };
                            break;
                        case 3:
                            series = new LineSeries
                            {
                                Title = item.Name, //线的说明
                                MarkerType = MarkerType.Triangle //标记点 的类型、形状
                            };
                            break;
                        case 4:
                            series = new LineSeries
                            {
                                Title = item.Name, //线的说明
                                MarkerType = MarkerType.Cross //标记点 的类型、形状
                            };
                            break;
                        case 5:
                            series = new LineSeries
                            {
                                Title = item.Name, //线的说明
                                MarkerType = MarkerType.Star //标记点 的类型、形状
                            };
                            break;
                        default:
                            break;
                    }
                    if (series == null)
                        continue;
                    series.ItemsSource = CurFlyDataModels;
                    series.YAxisKey = item.YAxisKey;
                    series.DataFieldX = IsAxisXShift ? "Time" : "Index";
                    series.DataFieldY = item.Field;
                    model.Series.Add(series);//将线添加到图标的容器中
                    switch (item.AxisType)
                    {
                        case 0:
                            model.Axes.Add(new LinearAxis()
                            {
                                Key = item.YAxisKey,
                                Position = AxisPosition.Left,
                                AxisDistance = distance
                            });
                            break;
                        case 1:
                            model.Axes.Add(new LogarithmicAxis()
                            {
                                Key = item.YAxisKey,
                                Position = AxisPosition.Left,
                                AxisDistance = distance
                            });
                            break;
                        case 2:
                            model.Axes.Add(new MagnitudeAxis()
                            {
                                Key = item.YAxisKey,
                                Position = AxisPosition.Left,
                                AxisDistance = distance
                            });
                            break;
                    }
                    distance += 50;
                }
            }

            this.FlyDataPlotModel = model;//赋值
        }

        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
