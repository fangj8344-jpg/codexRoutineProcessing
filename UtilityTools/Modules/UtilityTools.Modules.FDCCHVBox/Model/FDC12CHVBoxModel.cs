using CsvHelper;
using HarfBuzzSharp;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using UtilityTools.Core.Model;
using UtilityTools.Modules.FDC12CHVBox.Protocol;
using UtilityTools.Services.Services;
using static UtilityTools.Modules.FDC12CHVBox.Protocol.FDC12CHVBoxProtocol;

namespace UtilityTools.Modules.FDC12CHVBox.Model
{
    public class FDC12CHVBoxModel : BindableBase
    {
        public FDC12CHVBoxModel()
        {
            Init();
        }
        private ushort _maxHV = 30000;
        private System.Timers.Timer _setHvTimer;
        private System.Timers.Timer _timer;
        
        private List<string> listTime;
        private bool _isTimeQuery = true;
        private bool _isSetHvLineShow = true;
        public bool IsSetHvLineShow
        {
            get { return _isSetHvLineShow; }
            set 
            {
                _isSetHvLineShow = value;
                if (value == true)
                {
                    for (int i = 0; i < HVBoxModels.Count; i++)
                    {

                        if (HVBoxModels[i].IsCheck && !PlotModel.Series.Contains(HVBoxModels[i].SetHVSeries))
                        {
                            PlotModel.Series.Add(HVBoxModels[i].SetHVSeries);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < HVBoxModels.Count; i++)
                    {

                        if (PlotModel.Series.Contains(HVBoxModels[i].SetHVSeries))
                        {
                            PlotModel.Series.Remove(HVBoxModels[i].SetHVSeries);
                        }
                    }

                }
                PlotModel.InvalidatePlot(true);
                RaisePropertyChanged(); 
            }
        }

        private bool _isHvLineShow = true;
        public bool IsHvLineShow
        {
            get { return _isHvLineShow; }
            set 
            {
                _isHvLineShow = value;
                if (value == true)
                {
                    for (int i = 0; i < HVBoxModels.Count; i++)
                    {

                        if (HVBoxModels[i].IsCheck && !PlotModel.Series.Contains(HVBoxModels[i].HVlineSeries))
                        {
                            PlotModel.Series.Add(HVBoxModels[i].HVlineSeries);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < HVBoxModels.Count; i++)
                    {

                        if (PlotModel.Series.Contains(HVBoxModels[i].HVlineSeries))
                        {
                            PlotModel.Series.Remove(HVBoxModels[i].HVlineSeries);
                        }
                    }

                }
                PlotModel.InvalidatePlot(true);
                RaisePropertyChanged();
            }
        }

        private bool _isILineShow = true;
        public bool IsILineShow
        {
            get { return _isILineShow; }
            set 
            {
                _isILineShow = value;
                if (value == true)
                {
                    for (int i = 0; i < HVBoxModels.Count; i++)
                    {

                        if (HVBoxModels[i].IsCheck && !PlotModel.Series.Contains(HVBoxModels[i].IlineSeries))
                        {
                            PlotModel.Series.Add(HVBoxModels[i].IlineSeries);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < HVBoxModels.Count; i++)
                    {

                        if (PlotModel.Series.Contains(HVBoxModels[i].IlineSeries))
                        {
                            PlotModel.Series.Remove(HVBoxModels[i].IlineSeries);
                        }
                    }
                }
                PlotModel.InvalidatePlot(true);
                RaisePropertyChanged(); }
        }
        private bool _isRealtimeRefresh = true;
        public bool IsRealtimeRefresh
        {
            get { return _isRealtimeRefresh; }
            set { _isRealtimeRefresh = value; RaisePropertyChanged(); }
        }

        private bool _isNeedRefresh = false;
        public bool IsNeedRefresh
        {
            get { return _isNeedRefresh; }
            set { _isNeedRefresh = value; }
        }
        private int _endBufferHv = 0;
       
        public int EndBufferHv
        {
            get { return _endBufferHv; }
            private set
            {
                if (value < 0)
                {
                    _endBufferHv = 0;
                }
                else if (value > _maxHV)
                {
                    _endBufferHv = _maxHV;
                }
                else 
                {
                    _endBufferHv = value;
                }
                
            }
        }
        private int _startBufferHv = 0;
        public int StartBufferHv
        {
            get { return _startBufferHv; }
            private set
            {
                if (value < 0)
                {
                    _startBufferHv = 0;
                }
                else if (value > _maxHV)
                {
                    _startBufferHv = _maxHV;
                }
                else
                {
                    _startBufferHv = value;
                }

            }
        }
        private bool _isAllCheck;
        public bool IsAllCheck
        {
            get { return _isAllCheck; }
            set
            {
                _isAllCheck = value;
                RaisePropertyChanged(); }
        }
        private string _localIp;
        public string LocalIp
        {
            get { return _localIp; }
            set { _localIp = value; RaisePropertyChanged(); }
        }
        private int _portStart = 0;
        public int PortStart
        {
            get { return _portStart; }
            set { _portStart = value; RaisePropertyChanged(); }
        }
        private string _remoteIp = "192.168.1.253";
        public string RemoteIp
        {
            get { return _remoteIp; }
            set { _remoteIp = value; RaisePropertyChanged(); }
        }
        private int _remotePort = 1030;
        public int RemotePort
        {
            get { return _remotePort; }
            set { _remotePort = value; RaisePropertyChanged(); }
        }
        private int _writeHv = 0;
        public int WriteHv
        {
            get { return _writeHv; }
            set
            {
                if (value < 0)
                {
                    _writeHv = 0;
                }
                else if (value > _maxHV)
                {
                    _writeHv = _maxHV;
                }
                else
                {
                    _writeHv = value;
                }
                RaisePropertyChanged();
            }
        }
        private ushort _setHV;
        /// <summary>
        /// 设置的高压值
        /// </summary>
        public ushort SetHV
        {
            get { return _setHV; }
            set { _setHV = value; RaisePropertyChanged(); }
        }
        private ushort _wirteAllStep = 5;
        public ushort WriteAllStep
        {
            get { return _wirteAllStep; }
            set { _wirteAllStep = value; RaisePropertyChanged(); }
        }
        
     
        private PlotModel _plotModel;
        /// <summary>
        /// 电压电流信息表
        /// </summary>
        public PlotModel PlotModel
        {
            get { return _plotModel; }
            set { _plotModel = value; RaisePropertyChanged(); }
        }
        private ObservableCollection<HVBoxModel> _hVBoxModels;

        public ObservableCollection<HVBoxModel> HVBoxModels
        {
            get { return _hVBoxModels; }
            set { _hVBoxModels = value; RaisePropertyChanged(); }
        }
        private UInt32 _timerInterval = 1000;
        public UInt32 TimerInterval
        {
            get { return _timerInterval; }
            set { _timerInterval = value; RaisePropertyChanged(); }
        }
        [JsonIgnore]
        public DelegateCommand SetHvCommand { get; set; }
        [JsonIgnore]
        public DelegateCommand ConnectCommand { get; set; }
        [JsonIgnore]
        public DelegateCommand SetHvStepParameterCommand { get; set; }
        [JsonIgnore]
        public DelegateCommand AutoAdjustCommand { get; set; }
        public DelegateCommand ExportMultipleSeriesToCsvCommand { get; set; }
        public DelegateCommand ClearMonitorCommand { get; set; }
        public DelegateCommand CloseOutPutCommand { get; set; }
        public DelegateCommand IsAllCheckChangeCommand { get; set; }
        public DelegateCommand TestCommand { get; set; }
        public DelegateCommand SetHvParameterCommand { get; set; }
        public DelegateCommand SetAllHvInitCommand { get; set; }
        public DelegateCommand SaveParameterCommand { get; set; }
        public DelegateCommand RefreshPlotCommand { get; set; }

        private void Init()
        {

            _localIp = NetMethodModel.GetLocalIPAddress().ToString();
            HVBoxModels = new ObservableCollection<HVBoxModel>();
            PlotModel = new PlotModel() { Title = "电流电压曲线" };
            PlotModel.Legends.Add(new Legend());
            PlotModel.Axes.Add(new DateTimeAxis() { Title = "时间", Position = AxisPosition.Bottom });
            PlotModel.Axes.Add(new LinearAxis() { Title = "数值", Position = AxisPosition.Left });
            for (byte i = 0; i < 12; i++)
            {
                var HVBoxModel = new HVBoxModel((byte)(i + 1), _localIp, PortStart, _remoteIp, _remotePort + i, this);
                HVBoxModels.Add(HVBoxModel);
            }
            SetHvCommand = new DelegateCommand(SetHv);
            SetHvStepParameterCommand = new DelegateCommand(SetHvStepParameter);
            ConnectCommand = new DelegateCommand(Connect);
            AutoAdjustCommand = new DelegateCommand(AutoAdjust);
            ExportMultipleSeriesToCsvCommand = new DelegateCommand(ExportToCsv);
            ClearMonitorCommand = new DelegateCommand(ClearMonitor);
            CloseOutPutCommand = new DelegateCommand(CloseOutPut);
            IsAllCheckChangeCommand = new DelegateCommand(IsAllCheckChange);
            SaveParameterCommand = new DelegateCommand(SaveParameter);
            SetHvParameterCommand = new DelegateCommand(SetHvParameter);
            SetAllHvInitCommand = new DelegateCommand(SetAllHvInit);
            RefreshPlotCommand = new DelegateCommand(RefreshPlot);
            listTime = new List<string>();
            var SaveParametersModel = new SaveParametersModel();
            SaveParametersModel.LoadParameter(this);
        }

        public void InitSetHVTimer()
        {
            // 1. 创建定时器，设置间隔时间（单位：毫秒，此处为 1000ms = 1秒）
            if (_setHvTimer == null)
            {
                _setHvTimer = new System.Timers.Timer(1000);
            }
            if (_setHvTimer.Enabled)
            {
                _setHvTimer.Stop();
            }
            // 2. 绑定定时触发的事件
            _setHvTimer.Elapsed += SetHvTimerElapsed;

            // 3. 设置是否重复触发（true = 循环触发，false = 只触发一次）
            _setHvTimer.AutoReset = true;

            // 4. 启动定时器
            _setHvTimer.Enabled = true;
        }
        public void StopSetHvTimer()
        {
            _setHvTimer?.Stop();
            _setHvTimer?.Dispose();
            _setHvTimer = null;

        }
        private void SetHvTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                int count = 0;
                for (int i=0;i<HVBoxModels.Count;i++)
                {
                    if (HVBoxModels[i].IsEnable && !HVBoxModels[i].IsSetHvDone)
                    {
                        HVBoxModels[i].SetHvBySetp();
                        count++;
                    } 
                }
                if (count == 0)
                {
                    StopSetHvTimer();
                    InitTimer();
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"设置所有高压出现错误 {ex}");
            }
            finally
            {
                if (!_isTimeQuery)
                {
                    QueryHvMessage();
                }
               
            }
        }
       
        // 定时触发的方法
        public void InitTimer()
        {
            _isTimeQuery = true;
            // 1. 创建定时器，设置间隔时间（单位：毫秒，此处为 1000ms = 1秒）
            if (_timer == null)
            {
                _timer = new System.Timers.Timer(1000);
            }
            // 2. 绑定定时触发的事件
            _timer.Elapsed += OnTimerElapsed;

            // 3. 设置是否重复触发（true = 循环触发，false = 只触发一次）
            _timer.AutoReset = true;

            // 4. 启动定时器
            _timer.Enabled = true;
        }
        public void StopTimer()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
            _isTimeQuery = false;
        }
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            IsNeedRefresh = true;
            for (int i = 0; i < HVBoxModels.Count; i++)
            {
                if (_isTimeQuery)
                {

                    HVBoxModels[i].Entity?.GetHvReadCommand();
                    HVBoxModels[i].Entity?.GetHvInitCommand();

                }
                else
                {
                    return;
                }  
            }
          
        }
        private void QueryHvMessage()
        {
            IsNeedRefresh = true;
            for (int i = 0; i < HVBoxModels.Count; i++)
            {
                HVBoxModels[i].Entity?.GetHvReadCommand();
                HVBoxModels[i].Entity?.GetHvInitCommand();
            }
        }
        
        private void SetHvParameter()
        {
            
            for (int i = 0; i < HVBoxModels.Count; i++)
            {
                if (HVBoxModels[i].IsCheck)
                {
                    HVBoxModels[i].WriteHV = WriteHv;

                } 
            }
        }
        private void SetHvStepParameter()
        {
            
            for (int i = 0; i < HVBoxModels.Count; i++)
            {
                if (HVBoxModels[i].IsCheck)
                {
                    HVBoxModels[i].SetStep = WriteAllStep;
                    HVBoxModels[i].WriteStep = WriteAllStep;
                }
               
            }
        }
        private void SetHv()
        {
            for (int i = 0; i < HVBoxModels.Count; i++)
            {
                HVBoxModels[i].InitSetHv();
            }
            StopSetHvTimer();
            StopTimer();
            InitSetHVTimer();
        }

       
        private void CloseOutPut()
        {
            //停止设置电压
            StopSetHvTimer();
            //停止定时查询
            StopTimer();
            //开启定时问询
            InitTimer();
            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < HVBoxModels.Count; i++)
                {
                    HVBoxModels[i].CloseOutput();
                }
            }
           
        }
        /// <summary>
        /// 全部初始化
        /// </summary>
        private void SetAllHvInit()
        {
            for (int i = 0; i < HVBoxModels.Count; i++)
            {
                HVBoxModels[i].Entity?.SetHvInitCommand();
            }
        }

        private  void Connect()
        {
            for (int i = 0; i < HVBoxModels.Count; i++)
            {
               
                try
                {
                    HVBoxModels[i].InitConnect(0, _localIp, _remotePort + i, _remoteIp);
                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Error($"{i + 1}通道连接出现错误 {ex}");
                }
                

            }
            try
            {
                StopTimer();
                InitTimer();
              
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"初始化定时器出现错误 {ex}");
            }
              
        }
        private void AutoAdjust()
        {
            foreach (var axis in PlotModel.Axes)
            {
                axis.Reset();
                // 通知图表重绘
                PlotModel.InvalidatePlot(true);
            }
        }
        private void IsAllCheckChange()
        {
            if (IsAllCheck)
            {

                for (int i = 0; i < HVBoxModels.Count; i++)
                {
                    HVBoxModels[i].IsCheck = true;
                }


            }
            else
            {
                for (int i = 0; i < HVBoxModels.Count; i++)
                {
                    HVBoxModels[i].IsCheck = false;
                }
            }
        }
        private void ClearMonitor()
        {
            for (int i = 0; i < HVBoxModels.Count; i++)
            {
                HVBoxModels[i].HvMessages.Clear();
                PlotModel.InvalidatePlot(true);
            }
        }
        
        /*
         ///每个通道单独导出的方法
        public async void ExportToCsv()
        {
            string filePath = GetSavePath();

            if (filePath == null)
            {
                return;
            }
            List<HvMessage> allRecords = new List<HvMessage>();
            await Task.Run(() => 
            {
                try
                {
                    for (int i = 0; i < HVBoxModels.Count; i++)
                    {
                        if (HVBoxModels[i].IsEnable)
                        {
                            using var write = new StreamWriter(Path.Combine( filePath, HVBoxModels[i].Channel.ToString() + "通道.csv"));
                            using var csv = new CsvWriter(write, CultureInfo.InvariantCulture);
                            List<HvMessage>  realHvMessages = HVBoxModels[i].HvMessages.Select(oldparameter => new HvMessage() 
                            {
                                DateTime = oldparameter.DateTime,
                                HV = -oldparameter.HV,
                                I = -oldparameter.I,
                                setHv = -oldparameter.setHv,
                            }).ToList();
                            csv.WriteRecords(realHvMessages);
                       
                        }
                    }
                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Error($"导入数据失败：{ex}");
                }
              
            });

            System.Windows.MessageBox.Show($"数据已成功导出到 {filePath}");
        }
        private string GetSavePath()
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();

            // 设置初始路径（可选）
            folderDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // 设置对话框标题
            folderDialog.Description = "请选择要保存的文件夹";

            // 显示对话框
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return folderDialog.SelectedPath;
            }

            return null; // 用户取消选择

        }
        */
        public void ExportToCsv()
        {
            string filePath = GetSavePath();
            if (filePath == null)
            {
                return;
            }
            // 使用 StreamWriter 写入文件，并指定 UTF-8 编码以支持中文等特殊字符
            int maxCount = 0;
            try
            {
                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    var hVBoxModels = HVBoxModels.ToList();
                    string hear = "";
                    // 写入表头
                    for (int i = 0; i < hVBoxModels.Count; i++)
                    {
                        if (hVBoxModels[i].IsCheck == false)
                        {
                            continue;
                        }
                        hear += ($"{hVBoxModels[i].Channel.ToString()}time,");
                        hear += ($"{hVBoxModels[i].Channel.ToString()}SetHv,");
                        hear += ($"{hVBoxModels[i].Channel.ToString()}Hv,");
                        hear += ($"{hVBoxModels[i].Channel.ToString()}I,");
                        if (i == 0)
                        {
                            maxCount = hVBoxModels[i].HvMessages.Count;
                        }
                        else
                        {
                            if (hVBoxModels[i].HvMessages.Count > maxCount)
                            {
                                maxCount = hVBoxModels[i].HvMessages.Count;
                            }
                        }
                    }
                    writer.WriteLine(hear);
                    // 写入内容
                    string content = "";
                    for (int i = 0; i < maxCount; i++)
                    {
                        content = "";
                        for (int j = 0; j < hVBoxModels.Count; j++)
                        {
                            if (hVBoxModels[j].IsCheck)
                            {
                                if (hVBoxModels[j].HvMessages != null && hVBoxModels[j].HvMessages.Count >= i + 1)
                                {
                                    var sethv = -hVBoxModels[j].HvMessages[i].setHv;
                                    var hv = -hVBoxModels[j].HvMessages[i].HV;
                                    var hi = -hVBoxModels[j].HvMessages[i].I;
                                    content += $"{hVBoxModels[j].HvMessages[i].DateTime.ToString("O")},";
                                    content += $"{sethv.ToString()},";
                                    content += $"{hv.ToString()},";
                                    content += $"{hi.ToString()},";
                                }
                                else
                                {
                                    content += $",";
                                    content += $",";
                                    content += $",";
                                    content += $",";
                                }
                            }
                        }
                        writer.WriteLine($"{content}");
                    }
                }
                System.Windows.MessageBox.Show($"数据已成功导出到 {filePath}");
            }
            catch (Exception ex) 
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"导出数据失败：{ex}");
            }
            
        }
        private string GetSavePath()
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "导出多个系列数据",
                Filter = "CSV 文件 (*.csv)|*.csv|所有文件 (*.*)|*.*",
                DefaultExt = ".csv",
                FileName = $"高压数据_{DateTime.Now:yyyyMMddHHmmss}"
            };
            return saveDialog.ShowDialog() == true ? saveDialog.FileName : null;


        }
        private void ShowMessage(string message)
        {
            System.Windows.MessageBox.Show(message);
            
        }
       
      
      
        private void SaveParameter()
        {
            var save = new  SaveParametersModel();
            save.SaveParameter(this);
        }
       
        private void RefreshPlot()
        {
            PlotModel.InvalidatePlot(true);
        }
        
    }
}
