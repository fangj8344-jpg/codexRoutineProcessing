using HarfBuzzSharp;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class FDC12CHVBoxModel:BindableBase
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
        private string _localIp;
        public string LocalIp
        {
            get { return _localIp; }
            set { _localIp = value; RaisePropertyChanged(); }
        }
        private int _portStart = 6000;
        public int PortStart
        {
            get { return _portStart; }
            set { _portStart = value; RaisePropertyChanged(); }
        }
        private string _remoteIp = "192.168.1.253";
        public string RemoteIp
        {
            get { return _remoteIp; }
            set { _remoteIp = value;RaisePropertyChanged(); }
        }
        private int _remotePort = 1030;
        public int RemotePort
        {
            get { return _remotePort; }
            set { _remotePort = value; RaisePropertyChanged(); }
        }
        private ushort _writeHV = 0;
        public ushort WriteHV
        {
            get { return _writeHV; }
            set
            {
                if (value > _maxHV)
                {
                    _writeHV = _maxHV;
                }
                else
                {
                    _writeHV = value;
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
        private ushort _wirteStep = 5;
        public ushort WriteStep
        {
            get { return _wirteStep; }
            set {
                if (WriteStep > _maxStep)
                {
                    _wirteStep = _maxStep;
                }
                else if (WriteStep < _minStep)
                {
                    _wirteStep = _minStep;
                }
                else
                {
                    _wirteStep = value;
                }
                RaisePropertyChanged(); }
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
        private void Init()
        {

            _localIp = NetMethodModel.GetLocalIPAddress().ToString();
            HVBoxModels = new ObservableCollection<HVBoxModel>();
            PlotModel = new PlotModel() { Title = "电流电压曲线"};
            PlotModel.Legends.Add(new Legend());
            PlotModel.Axes.Add(new DateTimeAxis() { Title="时间",Position=AxisPosition.Bottom});
            PlotModel.Axes.Add(new LinearAxis() { Title = "数值", Position = AxisPosition.Left });
            for (byte i = 0; i < 12; i++)
            {
                var HVBoxModel = new HVBoxModel((byte)(i+1), _localIp, _portStart + i, _remoteIp, _remotePort + i,this);
                HVBoxModels.Add(HVBoxModel);
                PlotModel.Series.Add(HVBoxModel.IlineSeries);
                PlotModel.Series.Add(HVBoxModel.HVlineSeries);
            }
            SetHvCommand = new DelegateCommand(SetHv);
            SetHvStepCommand = new DelegateCommand(SetHvStep);
            ConnectCommand = new DelegateCommand(Connect);
            AutoAdjustCommand = new DelegateCommand(AutoAdjust);
            ExportMultipleSeriesToCsvCommand = new DelegateCommand(ExportMultipleSeriesToCsv);
            ClearMonitorCommand = new DelegateCommand(ClearMonitor);
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
            _setHvCount++;
            if (_direction)
            {
                if (_setHvCount >= _setHvTotalCount)
                {
                    _startBufferHv = _endBufferHv;
                    for (int i = 0; i < HVBoxModels.Count; i++)
                    {
                        HVBoxModels[i].Entity.SetHvCommand((ushort)(_endBufferHv));
                    }
                }
                else
                {
                    _startBufferHv = (ushort)(_startBufferHv + SetStep);
                    for (int i = 0; i < HVBoxModels.Count; i++)
                    {
                        HVBoxModels[i].Entity.SetHvCommand((ushort)(_startBufferHv));
                    }
                }
            }
            else
            {
                if (_setHvCount >= _setHvTotalCount)
                {
                    _startBufferHv = _endBufferHv;
                    for (int i = 0; i < HVBoxModels.Count; i++)
                    {
                        HVBoxModels[i].Entity.SetHvCommand((ushort)(_endBufferHv));
                    }     
                }
                else
                {
                    _startBufferHv = (ushort)(_startBufferHv - SetStep);
                    for (int i = 0; i < HVBoxModels.Count; i++)
                    {
                        HVBoxModels[i].Entity.SetHvCommand((ushort)(_startBufferHv));
                    }
                }
            }
            if (_setHvCount >= _setHvTotalCount)
            {
                StopSetHvTimer();
            }
        }
       
        private void SetHv()
        {
            _setHvCount = 0;
            _startBufferHv = (ushort)HVBoxModels[0].ReadHV;
            _endBufferHv = WriteHV;
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
            SetStep = WriteStep;
        }
        private void Connect()
        {
            for (int i = 0; i < HVBoxModels.Count; i++)
            {

                HVBoxModels[i].InitConnect(_portStart + i, NetMethodModel.GetLocalIPAddress().ToString(), _remotePort + i, _remoteIp);
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
            List<List<HvMessage>> allLineSeries = new List<List<HvMessage>>();
            List<string> head = new List<string>();
            for (int i = 0; i < HVBoxModels.Count; i++)
            {
                var list = HVBoxModels[i].HvMessages.ToList();
                allLineSeries.Add(list);
                head.Add(HVBoxModels[i].Channel.ToString() + "时间");
                head.Add(HVBoxModels[i].Channel.ToString() + "电压");
                head.Add(HVBoxModels[i].Channel.ToString() + "电流");
            }
       
            // 4. 打开保存文件对话框
            string savePath = GetSavePath();
            if (string.IsNullOrEmpty(savePath)) return;
            try
            {
                var csvLines = new List<string>();

               
                csvLines.Add(string.Join(",", head));

                // 6. 构建数据行（按时间排序）
                foreach (var timeEntry in allLineSeries)
                {
                    var rowParts = new List<string>();
                    foreach (var series in timeEntry)
                    {
                        rowParts.Add(series.DateTime.ToString("yyyy年MM月dd日 HH:mm:ss"));
                        rowParts.Add(series.HV.ToString());
                        rowParts.Add(series.I.ToString());
                    }
                    csvLines.Add(string.Join(",", rowParts));
                }

                // 7. 写入文件
                File.WriteAllLines(savePath, csvLines, System.Text.Encoding.UTF8);
                ShowMessage($"所有 {allLineSeries.Count} 个系列的数据已成功导出到：{savePath}");
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

    }
}
