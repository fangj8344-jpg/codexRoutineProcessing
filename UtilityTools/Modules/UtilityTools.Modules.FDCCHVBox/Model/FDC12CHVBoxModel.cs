using HarfBuzzSharp;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
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
        private ushort _minStep = 5;
        private ushort _maxStep = 500;
        private ushort _setHvCount = 0;
        private int _setHvTotalCount = 0;
        private ushort _endBufferHv = 0;
        private ushort _startBufferHv = 0;
        private bool _direction = true;
        private System.Timers.Timer _setHvTimer;
        private System.Timers.Timer _timer;
        private int _connectCount;
        private List<string> listTime;
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
        private ushort _writeHv = 0;
        public ushort WriteHv
        {
            get { return _writeHv; }
            set
            {
                _writeHv = value; RaisePropertyChanged();
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

        private ushort _setStep = 5;
        public ushort SetStep
        {
            get { return _setStep; }
            set { _setStep = value; RaisePropertyChanged(); }
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
        [JsonIgnore]
        public DelegateCommand SetHvCommand { get; set; }
        [JsonIgnore]
        public DelegateCommand ConnectCommand { get; set; }
        [JsonIgnore]
        public DelegateCommand SetHvStepCommand { get; set; }
        [JsonIgnore]
        public DelegateCommand AutoAdjustCommand { get; set; }
        public DelegateCommand ExportMultipleSeriesToCsvCommand { get; set; }
        public DelegateCommand ClearMonitorCommand { get; set; }
        public DelegateCommand CloseOutPutCommand { get; set; }
        public DelegateCommand IsAllCheckChangeCommand { get; set; }
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
            SetHvStepCommand = new DelegateCommand(SetHvStep);
            ConnectCommand = new DelegateCommand(Connect);
            AutoAdjustCommand = new DelegateCommand(AutoAdjust);
            ExportMultipleSeriesToCsvCommand = new DelegateCommand(ExportMultipleSeriesToCsv);
            ClearMonitorCommand = new DelegateCommand(ClearMonitor);
            CloseOutPutCommand = new DelegateCommand(CloseOutPut);
            TestCommand = new DelegateCommand(Test);
            IsAllCheckChangeCommand = new DelegateCommand(IsAllCheckChange);    
            listTime = new List<string>();
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
                StopTimer();
                if (_direction)
                {
                    if (_startBufferHv >= _endBufferHv || _startBufferHv + SetStep >= _endBufferHv)
                    {
                        _startBufferHv = _endBufferHv;
                        for (int i = 0; i < HVBoxModels.Count; i++)
                        {
                            HVBoxModels[i].DirectlySetHV((ushort)(_endBufferHv));


                        }
                        StopSetHvTimer();
                        InitTimer();
                        return;
                    }
                    else
                    {
                        _startBufferHv = (ushort)(_startBufferHv + SetStep);
                    }

                    if (_startBufferHv > 41000)
                    {
                        for (int i = 0; i < HVBoxModels.Count; i++)
                        {
                            HVBoxModels[i].DirectlySetHV((ushort)(0));
                        }
                        StopSetHvTimer();
                        InitTimer();
                        return;

                    }
                    else if (_startBufferHv > 30000)
                    {
                        for (int i = 0; i < HVBoxModels.Count; i++)
                        {
                            HVBoxModels[i].DirectlySetHV((ushort)(30000));
                        }
                        StopSetHvTimer();
                        InitTimer();
                        return;
                    }
                    else
                    {
                        for (int i = 0; i < HVBoxModels.Count; i++)
                        {
                            HVBoxModels[i].DirectlySetHV(_startBufferHv);
                        }
                    }
                }
                else
                {
                    if (_endBufferHv >= _startBufferHv || _endBufferHv >= _startBufferHv - SetStep || _startBufferHv > 30000 || _startBufferHv - SetStep > 30000)
                    {
                        _startBufferHv = _endBufferHv;
                        for (int i = 0; i < HVBoxModels.Count; i++)
                        {
                            HVBoxModels[i].DirectlySetHV(_endBufferHv);
                        }
                        StopSetHvTimer();
                        InitTimer();
                        return;
                    }
                    else
                    {
                        _startBufferHv = (ushort)(_startBufferHv - SetStep);
                    }
                    if (_startBufferHv > 41000)
                    {
                        for (int i = 0; i < HVBoxModels.Count; i++)
                        {
                            HVBoxModels[i].DirectlySetHV(0);
                        }
                        StopSetHvTimer();
                        InitTimer();
                    }
                    else if (_startBufferHv > 30000)
                    {
                        for (int i = 0; i < HVBoxModels.Count; i++)
                        {
                            HVBoxModels[i].DirectlySetHV(30000);
                        }
                        StopSetHvTimer();
                        InitTimer();
                    }
                    else
                    {
                        for (int i = 0; i < HVBoxModels.Count; i++)
                        {
                            HVBoxModels[i].DirectlySetHV(_startBufferHv);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"设置所有高压出现错误 {ex}");
            }
            finally
            {
                QueryHvMessage();

            }


        }
        // 定时触发的方法
        public void InitTimer()
        {
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
        }
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            for (int i = 0; i < HVBoxModels.Count; i++)
            {
                HVBoxModels[i].Entity?.GetHvReadCommand();
                HVBoxModels[i].Entity?.GetHvInitCommand();
            }
            _connectCount++;
            if (_connectCount >= 10)
            {
                _connectCount = 0;
                for (int i = 0; i < HVBoxModels.Count; i++)
                {
                    HVBoxModels[i].CheckConnect(); ;
                }

            }
        }
        private void QueryHvMessage()
        {
            for (int i = 0; i < HVBoxModels.Count; i++)
            {
                HVBoxModels[i].Entity?.GetHvReadCommand();
                HVBoxModels[i].Entity?.GetHvInitCommand();
            }
        }
        private void SetHv()
        {
            _setHvCount = 0;
            _startBufferHv = (ushort)HVBoxModels[0].ReadHV;
            _endBufferHv = _writeHv;
            if (_endBufferHv > _startBufferHv)
            {
                _setHvTotalCount = (_endBufferHv - _startBufferHv) / SetStep + 1;
                _direction = true;
            }
            else
            {
                _setHvTotalCount = (_startBufferHv - _endBufferHv) / SetStep + 1;
                _direction = false;
            }
            StopSetHvTimer();
            InitSetHVTimer();
        }

        private void SetHvStep()
        {

            SetStep = WriteAllStep;
        }
        private void CloseOutPut()
        {

            for (int i = 0; i < HVBoxModels.Count; i++)
            {
                HVBoxModels[i].CloseOutput();
            }
        }

        private void Connect()
        {
            for (int i = 0; i < HVBoxModels.Count; i++)
            {

                HVBoxModels[i].InitConnect(0, _localIp, _remotePort + i, _remoteIp);
            }
            InitTimer();
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
        public void ExportMultipleSeriesToCsv()
        {
            // 1. 获取所有 LineSeries 并按 Title 排序（确保顺序一致）
           
          
       
            // 4. 打开保存文件对话框
            string savePath = GetSavePath();
            List<string> csvfile = new List<string>();
            if (string.IsNullOrEmpty(savePath)) return;
            try
            {
                // 6. 构建数据行（按时间排序）
                for (int i = 0; i < HVBoxModels.Count; i++)
                {

                    var timeCsvLines = new List<string>();
                    var setCsvLines1 = new List<string>();
                    var hvCsvLines1 = new List<string>();
                    var iCsvLines1 = new List<string>();
                  

                    if (HVBoxModels[i].HvMessages!=null && HVBoxModels[i].HvMessages.Count>0)
                    {

                        timeCsvLines.Add(HVBoxModels[i].Channel.ToString() + "时间");
                        hvCsvLines1.Add(HVBoxModels[i].Channel.ToString() + "高压");
                        iCsvLines1.Add(HVBoxModels[i].Channel.ToString() + "电流");
                        for (int j = 0; j < HVBoxModels[i].HvMessages.Count; j++)
                        {
                            timeCsvLines.Add(HVBoxModels[i].HvMessages[j].DateTime.ToString("yyyy年MM月dd日 HH:mm:ss"));
                            setCsvLines1.Add(HVBoxModels[i].HvMessages[j].I.ToString());
                            hvCsvLines1.Add(HVBoxModels[i].HvMessages[j].HV.ToString());
                            iCsvLines1.Add(HVBoxModels[i].HvMessages[j].I.ToString());
                           
                        }
                        csvfile.Add(string.Join(",", timeCsvLines));
                        csvfile.Add(string.Join(",", hvCsvLines1));
                        csvfile.Add(string.Join(",", iCsvLines1));
                    }
                }
                                      
                // 7. 写入文件
                File.WriteAllLines(savePath, csvfile, System.Text.Encoding.UTF8);
                ShowMessage($"成功导出到：{savePath}");
            }
            catch (Exception ex)
            {
                ShowMessage($"导出失败：{ex.Message}");
            }
        }       
        private string GetSavePath()
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "导出多个系列数据",
                Filter = "CSV 文件 (*.csv)|*.csv|所有文件 (*.*)|*.*",
                DefaultExt = ".csv",
                FileName = $"MultiSeriesData_{DateTime.Now:yyyyMMddHHmmss}"
            };
            return saveDialog.ShowDialog() == true ? saveDialog.FileName : null;

          
        }

        private void ShowMessage(string message)
        {
            System.Windows.MessageBox.Show(message);
            
        }
        public DelegateCommand TestCommand { get; set; }
        private void Test()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < HVBoxModels.Count; i++)
            {
                HVBoxModels[i].Entity.SetHvCommand(50);
                listTime.Add($"通道{HVBoxModels[i].Channel}:{stopwatch.Elapsed.TotalMilliseconds.ToString()}ms");
            }
            var x = listTime;
            stopwatch.Stop();
        }

    }
}
