using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using OxyPlot.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using UtilityTools.Modules.InstrumentDataRetriever.Entity;
using UtilityTools.Modules.InstrumentDataRetriever.Services;

namespace UtilityTools.Modules.InstrumentDataRetriever.ViewModels
{
    public class InstrumentDataRetrieverViewModel : BindableBase
    {
        private readonly IInstrumentDataService _dataService;
        private ObservableCollection<InstrumentInfo> _instrumentData;
        private string _databaseStatus;
        private DateTime? _startTime;
        private DateTime? _endTime;
        private bool _isGunEnabled;
        private bool _isSEHighVoltageEnabled;
        private bool _isVacuumPlotVisible = true;
        private bool _isTemperaturePlotVisible = true;
        private bool _isSEPlotVisible = true;
        private bool _isHighVoltagePlotVisible = true;
        private PlotModel _vacuumPlotModel;
        private PlotModel _temperaturePlotModel;
        private PlotModel _sePlotModel;
        private PlotModel _highVoltagePlotModel;

        public InstrumentDataRetrieverViewModel(IInstrumentDataService dataService)
        {
            _dataService = dataService;
            InstrumentData = new ObservableCollection<InstrumentInfo>();
            
            // 初始化命令
            SelectDatabaseCommand = new DelegateCommand(ExecuteSelectDatabase);
            DisconnectDatabaseCommand = new DelegateCommand(ExecuteDisconnectDatabase);
            SearchCommand = new DelegateCommand(ExecuteSearch);
            GetLatestDataCommand = new DelegateCommand(ExecuteGetLatestData);
            ToggleVacuumPlotCommand = new DelegateCommand(() => IsVacuumPlotVisible = !IsVacuumPlotVisible);
            ToggleTemperaturePlotCommand = new DelegateCommand(() => IsTemperaturePlotVisible = !IsTemperaturePlotVisible);
            ToggleSEPlotCommand = new DelegateCommand(() => IsSEPlotVisible = !IsSEPlotVisible);
            ToggleHighVoltagePlotCommand = new DelegateCommand(() => IsHighVoltagePlotVisible = !IsHighVoltagePlotVisible);
            ExportPlotCommand = new DelegateCommand(ExecuteExportPlot);
            AutoAdjustCommand = new DelegateCommand<object>(AutoAdjust);
            SavePlotCommand = new DelegateCommand<object>(SavePlot);
            // 初始化图表
            InitializePlotModels();
        }

        #region Properties

        public ObservableCollection<InstrumentInfo> InstrumentData
        {
            get => _instrumentData;
            set => SetProperty(ref _instrumentData, value);
        }

        public string DatabaseStatus
        {
            get => _databaseStatus;
            set => SetProperty(ref _databaseStatus, value);
        }

        public DateTime? StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        public DateTime? EndTime
        {
            get => _endTime;
            set => SetProperty(ref _endTime, value);
        }

        public bool IsGunEnabled
        {
            get => _isGunEnabled;
            set => SetProperty(ref _isGunEnabled, value);
        }

        public bool IsSEHighVoltageEnabled
        {
            get => _isSEHighVoltageEnabled;
            set => SetProperty(ref _isSEHighVoltageEnabled, value);
        }

        public bool IsConnected => _dataService.IsConnected;

        private string _dbFilePath;

        public string DbFilePath
        {
            get { return _dbFilePath; }
            set { _dbFilePath = value; RaisePropertyChanged(); }
        }


        public bool IsVacuumPlotVisible
        {
            get => _isVacuumPlotVisible;
            set => SetProperty(ref _isVacuumPlotVisible, value);
        }

        public bool IsTemperaturePlotVisible
        {
            get => _isTemperaturePlotVisible;
            set => SetProperty(ref _isTemperaturePlotVisible, value);
        }

        public bool IsSEPlotVisible
        {
            get => _isSEPlotVisible;
            set => SetProperty(ref _isSEPlotVisible, value);
        }

        public bool IsHighVoltagePlotVisible
        {
            get => _isHighVoltagePlotVisible;
            set => SetProperty(ref _isHighVoltagePlotVisible, value);
        }

        public PlotModel VacuumPlotModel
        {
            get => _vacuumPlotModel;
            set => SetProperty(ref _vacuumPlotModel, value);
        }

        public PlotModel TemperaturePlotModel
        {
            get => _temperaturePlotModel;
            set => SetProperty(ref _temperaturePlotModel, value);
        }

        public PlotModel SEPlotModel
        {
            get => _sePlotModel;
            set => SetProperty(ref _sePlotModel, value);
        }

        public PlotModel HighVoltagePlotModel
        {
            get => _highVoltagePlotModel;
            set => SetProperty(ref _highVoltagePlotModel, value);
        }

        #endregion

        #region Commands

        public ICommand SelectDatabaseCommand { get; }
        public ICommand DisconnectDatabaseCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand GetLatestDataCommand { get; }
        public ICommand ToggleVacuumPlotCommand { get; }
        public ICommand ToggleTemperaturePlotCommand { get; }
        public ICommand ToggleSEPlotCommand { get; }
        public ICommand ToggleHighVoltagePlotCommand { get; }
        public ICommand ExportPlotCommand { get; }
        public ICommand AutoAdjustCommand { get; }
        public ICommand SavePlotCommand { get; }
        #endregion

        #region Private Methods

        private async void ExecuteSelectDatabase()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "SQLite Database (*.db)|*.db|All files (*.*)|*.*",
                Title = "选择数据库文件"
            };

            if (dialog.ShowDialog() == true)
            {
                DbFilePath = dialog.FileName;
                var success = await _dataService.ConnectToDatabaseAsync(dialog.FileName);
                DatabaseStatus = success ? "已连接到数据库" : "连接数据库失败";
                RaisePropertyChanged(nameof(IsConnected));
                var latestData = await _dataService.GetAllAsync();
                if (latestData != null)
                {
                    InstrumentData.Clear();
                    InstrumentData.AddRange(latestData);
                    UpdatePlotModels();
                }
            }
        }

        private async void ExecuteDisconnectDatabase()
        {
            if(IsConnected) 
            {
                await _dataService.DisconnectAsync();
                DatabaseStatus = "未连接数据库";
                RaisePropertyChanged(nameof(IsConnected));
                InstrumentData.Clear();
                UpdatePlotModels();
            }
        }

        private async void ExecuteSearch()
        {
            if (!IsConnected) return;

            var data = await _dataService.GetByConditionsAsync(
                StartTime,
                EndTime,
                IsGunEnabled ? (bool?)true : null,
                IsSEHighVoltageEnabled ? (bool?)true : null
            );

            InstrumentData.Clear();
            foreach (var item in data)
            {
                InstrumentData.Add(item);
            }

            UpdatePlotModels();
        }

        private async void ExecuteGetLatestData()
        {
            if (!IsConnected) return;

            var latestData = await _dataService.GetAllAsync();
            if (latestData != null)
            {
                InstrumentData.Clear();
                InstrumentData.AddRange(latestData);
                UpdatePlotModels();
            }
        }

        private void ExecuteExportPlot()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "PNG Image (*.png)|*.png",
                Title = "导出图表"
            };

            if (dialog.ShowDialog() == true)
            {
                // 导出当前可见的图表
                if (IsVacuumPlotVisible)
                    ExportPlotToFile(VacuumPlotModel, dialog.FileName);
                if (IsTemperaturePlotVisible)
                    ExportPlotToFile(TemperaturePlotModel, dialog.FileName);
                if (IsSEPlotVisible)
                    ExportPlotToFile(SEPlotModel, dialog.FileName);
                if (IsHighVoltagePlotVisible)
                    ExportPlotToFile(HighVoltagePlotModel, dialog.FileName);
            }
        }

        private void AutoAdjust(object obj)
        {
            switch(obj.ToString()) 
            {
                case "真空图表":
                    {
                        foreach (var axis in VacuumPlotModel.Axes)
                            axis.Reset();
                        VacuumPlotModel.InvalidatePlot(true);
                    }
                    break;
                case "温度图表":
                    {
                        foreach (var axis in TemperaturePlotModel.Axes)
                            axis.Reset();
                        TemperaturePlotModel.InvalidatePlot(true);
                    }
                    break;
                case "SE图表":
                    {
                        foreach (var axis in SEPlotModel.Axes)
                            axis.Reset();
                        SEPlotModel.InvalidatePlot(true);
                    }
                    break;
                case "高压箱图表":
                    {
                        foreach (var axis in HighVoltagePlotModel.Axes)
                            axis.Reset();
                        HighVoltagePlotModel.InvalidatePlot(true);
                    }
                    break;
                default:
                    break;
            }
        }

        private void SavePlot(object obj)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "请选择保存的路径";
            dialog.Filter = "图片 |*.png";
            dialog.DefaultExt = "png";
            var result = dialog.ShowDialog();
            if (result != null && result == true)
            {
                var filePath = dialog.FileName;
                switch (obj.ToString())
                {
                    case "真空图表":
                        {
                            var pngExporter = new PngExporter() { Width = (int)VacuumPlotModel.Width, Height = (int)VacuumPlotModel.Height };
                            pngExporter.ExportToFile(VacuumPlotModel, filePath);
                        }
                        break;
                    case "温度图表":
                        {
                            var pngExporter = new PngExporter() { Width = (int)TemperaturePlotModel.Width, Height = (int)TemperaturePlotModel.Height };
                            pngExporter.ExportToFile(TemperaturePlotModel, filePath);
                        }
                        break;
                    case "SE图表":
                        {
                            var pngExporter = new PngExporter() { Width = (int)SEPlotModel.Width, Height = (int)SEPlotModel.Height };
                            pngExporter.ExportToFile(SEPlotModel, filePath);
                        }
                        break;
                    case "高压箱图表":
                        {
                            var pngExporter = new PngExporter() { Width = (int)HighVoltagePlotModel.Width, Height = (int)HighVoltagePlotModel.Height };
                            pngExporter.ExportToFile(HighVoltagePlotModel, filePath);
                        }
                        break;
                    default:
                        break;
                }
                
            }
            
        }

        private void InitializePlotModels()
        {
            // 初始化真空图表
            VacuumPlotModel = new PlotModel { Title = "真空数据"};
            VacuumPlotModel.Legends.Add(new Legend());
            VacuumPlotModel.Series.Add(new LineSeries { Title = "枪头真空", RenderInLegend = true });
            VacuumPlotModel.Series.Add(new LineSeries { Title = "样品仓真空", RenderInLegend = true });

            // 初始化温度图表
            TemperaturePlotModel = new PlotModel { Title = "温度数据" };
            TemperaturePlotModel.Legends.Add(new Legend());
            TemperaturePlotModel.Series.Add(new LineSeries { Title = "温度1", RenderInLegend = true });
            TemperaturePlotModel.Series.Add(new LineSeries { Title = "温度2", RenderInLegend = true });

            // 初始化SE图表
            SEPlotModel = new PlotModel { Title = "SE数据" };
            SEPlotModel.Legends.Add(new Legend());
            SEPlotModel.Series.Add(new LineSeries { Title = "SE增益", RenderInLegend = true });
            SEPlotModel.Series.Add(new LineSeries { Title = "采集电压", RenderInLegend = true });
            SEPlotModel.Series.Add(new LineSeries { Title = "倍增体电压", RenderInLegend = true });
            SEPlotModel.Series.Add(new LineSeries { Title = "减速电压", RenderInLegend = true });
            SEPlotModel.Series.Add(new LineSeries { Title = "采集电流", RenderInLegend = true });
            SEPlotModel.Series.Add(new LineSeries { Title = "倍增体电流", RenderInLegend = true });
            SEPlotModel.Series.Add(new LineSeries { Title = "减速电流", RenderInLegend = true });

            // 初始化高压箱图表
            HighVoltagePlotModel = new PlotModel { Title = "高压箱数据" };
            HighVoltagePlotModel.Legends.Add(new Legend());
            HighVoltagePlotModel.Series.Add(new LineSeries { Title = "加速电压", RenderInLegend = true });
            HighVoltagePlotModel.Series.Add(new LineSeries { Title = "灯丝电流", RenderInLegend = true });
            HighVoltagePlotModel.Series.Add(new LineSeries { Title = "灯丝电阻", RenderInLegend = true });
            HighVoltagePlotModel.Series.Add(new LineSeries { Title = "栅极电压", RenderInLegend = true });
        }

        private void UpdatePlotModels()
        {
            // 更新真空图表
            var gunVacuumSeries = (LineSeries)VacuumPlotModel.Series[0];
            var samVacuumSeries = (LineSeries)VacuumPlotModel.Series[1];
            gunVacuumSeries.Points.Clear();
            samVacuumSeries.Points.Clear();

            // 更新温度图表
            var temp1Series = (LineSeries)TemperaturePlotModel.Series[0];
            var temp2Series = (LineSeries)TemperaturePlotModel.Series[1];
            temp1Series.Points.Clear();
            temp2Series.Points.Clear();

            // 更新SE图表
            var seGainSeries = (LineSeries)SEPlotModel.Series[0];
            var collectVolSeries = (LineSeries)SEPlotModel.Series[1];
            var scinVolSeries = (LineSeries)SEPlotModel.Series[2];
            var decVolSeries = (LineSeries)SEPlotModel.Series[3];
            var collectCurSeries = (LineSeries)SEPlotModel.Series[4];
            var scinCurSeries = (LineSeries)SEPlotModel.Series[5];
            var decCurSeries = (LineSeries)SEPlotModel.Series[6];
            seGainSeries.Points.Clear();
            collectVolSeries.Points.Clear();
            scinVolSeries.Points.Clear();
            decVolSeries.Points.Clear();
            collectCurSeries.Points.Clear();
            scinCurSeries.Points.Clear();
            decCurSeries.Points.Clear();

            // 更新高压箱图表
            var accVolSeries = (LineSeries)HighVoltagePlotModel.Series[0];
            var emissCurSeries = (LineSeries)HighVoltagePlotModel.Series[1];
            var filaRSeries = (LineSeries)HighVoltagePlotModel.Series[2];
            var gridVolSeries = (LineSeries)HighVoltagePlotModel.Series[3];
            accVolSeries.Points.Clear();
            emissCurSeries.Points.Clear();
            filaRSeries.Points.Clear();
            gridVolSeries.Points.Clear();

            foreach (var data in InstrumentData)
            {
                var time = DateTimeAxis.ToDouble(data.Time);

                // 添加真空数据点
                gunVacuumSeries.Points.Add(new DataPoint(time, data.GunVacuum));
                samVacuumSeries.Points.Add(new DataPoint(time, data.SamVacuum));

                // 添加温度数据点
                temp1Series.Points.Add(new DataPoint(time, data.Temperature1));
                temp2Series.Points.Add(new DataPoint(time, data.Temperature2));

                // 添加SE数据点
                seGainSeries.Points.Add(new DataPoint(time, data.SeGain));
                collectVolSeries.Points.Add(new DataPoint(time, data.CollectVol));
                scinVolSeries.Points.Add(new DataPoint(time, data.ScinVol));
                decVolSeries.Points.Add(new DataPoint(time, data.DecVol));
                collectCurSeries.Points.Add(new DataPoint(time, data.CollectCur));
                scinCurSeries.Points.Add(new DataPoint(time, data.ScinCur));
                decCurSeries.Points.Add(new DataPoint(time, data.DecCur));

                // 添加高压箱数据点
                accVolSeries.Points.Add(new DataPoint(time, data.AccVol));
                emissCurSeries.Points.Add(new DataPoint(time, data.EmissCur));
                filaRSeries.Points.Add(new DataPoint(time, data.FilaR));
                gridVolSeries.Points.Add(new DataPoint(time, data.GridVol));
            }

            // 更新所有图表
            VacuumPlotModel.InvalidatePlot(true);
            TemperaturePlotModel.InvalidatePlot(true);
            SEPlotModel.InvalidatePlot(true);
            HighVoltagePlotModel.InvalidatePlot(true);
        }

        private void ExportPlotToFile(PlotModel model, string filePath)
        {
            var pngExporter = new PngExporter { Width = 800, Height = 600 };
            pngExporter.ExportToFile(model, filePath);
        }

        #endregion
    }
} 