#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.TemperatureController.Model
 * 唯一标识：fbb267fd-38f5-4d23-bdbb-0dac8edb3da4
 * 文件名：ControllerModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/10/23 16:43:23
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

using CsvHelper;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using OxyPlot.Wpf;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using ScottPlot.Drawing.Colormaps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Extension;
using UtilityTools.Core.Model;
using UtilityTools.Modules.TemperatureController.Protocol;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.Services.Services;

namespace UtilityTools.Modules.TemperatureController.Model
{
    public class ControllerModel : BindableBase
    {
        #region ------------Constructor------------
        public ControllerModel(IContainerProvider containerProvider)
        {
            _containerProvider = containerProvider;
            _dialogHostService = containerProvider.Resolve<IDialogHostService>();

            _parser = new TemperatureControllerParser();
            _parser.PacketReceivedEvent += Parser_PacketReceivedEvent;

            SerialPortService = new SerialPortService();
            SerialPortService.UpdateResponse += SerialPortService_UpdateResponse;
            SerialPortService.IsBinary = true;
            NetUdpService = new UdpNetAsyncDevice();
            NetUdpService.UpdateResponse += NetUdpService_UpdateResponse;
            Pid = new PidModel();

            SelectCtrlMode();
            SelectSensorType();

            DeviceStatusCommand = new DelegateCommand(DeviceStatus);
            WorkTypeCommand = new DelegateCommand(WorkType);
            StartWorkCommand = new DelegateCommand(StartWork);
            ClearMonitorCommand = new DelegateCommand(ClearMonitor);
            AutoAdjustComamnd = new DelegateCommand(AutoAdjust);
            SaveToFileCommand = new DelegateCommand(SaveToFile);
            SetControlparmsCommand = new DelegateCommand(SetControlparms);
            SetPIDparmsCommand = new DelegateCommand(SetPIDparms);

            // 初始化图表信息
            TempPlotModel = new PlotModel();
            TempPlotModel.Legends.Add(new Legend());

            TempPlotModel.Axes.Add(new LinearAxis() { Title = "时间", Position = OxyPlot.Axes.AxisPosition.Bottom });
            TempPlotModel.Axes.Add(new LogarithmicAxis() { Title = "温度", Position = OxyPlot.Axes.AxisPosition.Left });

            _temp = new LineSeries() { Title = "Vac1", RenderInLegend = true };
            TempPlotModel.Series.Add(_temp);
        }

        #endregion

        #region ------------Field------------
        private IContainerProvider _containerProvider;
        private readonly IDialogHostService _dialogHostService;

        private TemperatureControllerParser _parser;
        private System.Timers.Timer _pidTimer;
        private System.Timers.Timer _manualTimer;

        LineSeries _temp;
        #endregion

        #region ------------Property------------
        private IAsynRWService _serialPortService;
        /// <summary>
        /// 串口异步通信服务
        /// </summary>
        public IAsynRWService SerialPortService
        {
            get { return _serialPortService; }
            set { _serialPortService = value; RaisePropertyChanged(); }
        }

        private IAsynRWService _netUdpService;
        /// <summary>
        /// 串口异步通信服务
        /// </summary>
        public IAsynRWService NetUdpService
        {
            get { return _netUdpService; }
            set { _netUdpService = value; RaisePropertyChanged(); }
        }

        private bool _isKelvin = false;
        /// <summary>
        /// 是否使用开氏温度
        /// </summary>
        public bool IsKelvin
        {
            get { return _isKelvin; }
            set
            {
                if (_isKelvin == value)
                    return;

                _isKelvin = value;
                RaisePropertyChanged();

                if (value)
                {
                    _readTemperature = ReadTemperature + 273.15f;
                    RaisePropertyChanged("ReadTemperature");

                    _targetTemperature = TargetTemperature + 273.15f;
                    RaisePropertyChanged("TargetTemperature");
                }
                else
                {
                    _readTemperature = ReadTemperature - 273.15f;
                    RaisePropertyChanged("ReadTemperature");

                    _targetTemperature = TargetTemperature - 273.15f;
                    RaisePropertyChanged("TargetTemperature");
                }

            }
        }

        private uint _readDeviceStatus;
        /// <summary>
        /// 读到的PID调节电流
        /// </summary>
        public uint ReadDeviceStatus
        {
            get { return _readDeviceStatus; }
            set { _readDeviceStatus = value; RaisePropertyChanged(); }
        }

        private float _readPidCurrent;
        /// <summary>
        /// 读到的PID调节电流
        /// </summary>
        public float ReadPidCurrent
        {
            get { return _readPidCurrent; }
            set { _readPidCurrent = value; RaisePropertyChanged(); }
        }

        private float _readManualCurrent;
        /// <summary>
        /// 读到的手动控制电流
        /// </summary>
        public float ReadManualCurrent
        {
            get { return _readManualCurrent; }
            set
            {
                _readManualCurrent = value;
                RaisePropertyChanged();
            }
        }

        private float _readTemperature;
        /// <summary>
        /// 读到的实时温度，为开氏温度
        /// </summary>
        public float ReadTemperature
        {
            get { return _readTemperature; }
            set
            {
                _readTemperature = value;
                RaisePropertyChanged();
            }
        }

        private float _readTargetTemperature;
        /// <summary>
        /// 读到的目标温度，为开氏温度
        /// </summary>
        public float ReadTargetTemperature
        {
            get { return _readTargetTemperature; }
            set
            {
                _readTargetTemperature = value;
                RaisePropertyChanged();
            }
        }

        private uint _readTemperatureUnit;
        /// <summary>
        /// 读到的温度单位
        /// </summary>
        public uint ReadTemperatureUnit
        {
            get { return _readTemperatureUnit; }
            set
            {
                _readTemperatureUnit = value;
                RaisePropertyChanged();
            }
        }

        private uint _readCtrlMode;
        /// <summary>
        /// 读到的控制模式
        /// </summary>
        public uint ReadCtrlMode
        {
            get { return _readCtrlMode; }
            set
            {
                _readCtrlMode = value;
                RaisePropertyChanged();
            }
        }

        private float _targetCurrent;
        /// <summary>
        /// 读到的目标电流
        /// </summary>
        public float TargetCurrent
        {
            get { return _targetCurrent; }
            set { _targetCurrent = value; RaisePropertyChanged(); }
        }

        private float _readMaxCurrent;
        /// <summary>
        /// 读到的电流最大值
        /// </summary>
        public float ReadMaxCurrent
        {
            get { return _readMaxCurrent; }
            set { _readMaxCurrent = value; RaisePropertyChanged(); }
        }

        private float _readCurrent;
        /// <summary>
        /// 读到的反馈电流
        /// </summary>
        public float ReadCurrent
        {
            get { return _readCurrent; }
            set { _readCurrent = value; RaisePropertyChanged(); }
        }

        private float _readTemperatureRate;
        /// <summary>
        /// 读到的控温速率
        /// </summary>
        public float ReadTemperatureRate
        {
            get { return _readTemperatureRate; }
            set { _readTemperatureRate = value; RaisePropertyChanged(); }
        }

        private float _readTemperatureCorrection;
        /// <summary>
        /// 读到的温度修正值
        /// </summary>
        public float ReadTemperatureCorrection
        {
            get { return _readTemperatureCorrection; }
            set { _readTemperatureCorrection = value; RaisePropertyChanged(); }
        }

        private float _readTimeCtrl;
        /// <summary>
        /// 读到的反馈电流
        /// </summary>
        public float ReadTimeCtrl
        {
            get { return _readTimeCtrl; }
            set { _readTimeCtrl = value; RaisePropertyChanged(); }
        }

        private float _targetTemperature;
        /// <summary>
        /// 设置的目标温度，为开氏温度
        /// </summary>
        public float TargetTemperature
        {
            get { return _targetTemperature; }
            set { _targetTemperature = value; RaisePropertyChanged(); }
        }

        private float _setCurrent;
        /// <summary>
        /// 电流的设定值
        /// </summary>
        public float SetCurrent
        {
            get { return _setCurrent; }
            set { _setCurrent = value; RaisePropertyChanged(); }
        }

        private float _maxCurrent;
        /// <summary>
        /// 电流最大值
        /// </summary>
        public float MaxCurrent
        {
            get { return _maxCurrent; }
            set { _maxCurrent = value; RaisePropertyChanged(); }
        }

        private PidModel _pid;
        /// <summary>
        /// PID调节模型
        /// </summary>
        public PidModel Pid
        {
            get { return _pid; }
            set { _pid = value; RaisePropertyChanged(); }
        }

        private SteadyCoolPIDModel _steadyCoolPID;
        /// <summary>
        /// 稳态降温PID
        /// </summary>
        public SteadyCoolPIDModel SteadyCoolPID
        {
            get { return _steadyCoolPID; }
            set { _steadyCoolPID = value; RaisePropertyChanged(); }
        }


        private float _temperatureRate;
        /// <summary>
        /// 设置温度速率
        /// </summary>
        public float TemperatureRate
        {
            get { return _temperatureRate; }
            set { _temperatureRate = value; RaisePropertyChanged(); }
        }

        private float _temperatureCorrection;
        /// <summary>
        /// 设置温度修正值
        /// </summary>
        public float TemperatureCorrection
        {
            get { return _temperatureCorrection; }
            set { _temperatureCorrection = value; RaisePropertyChanged(); }
        }

        private float _timeCtrl;
        /// <summary>
        /// 设置定时控制
        /// </summary>
        public float TimeCtrl
        {
            get { return _timeCtrl; }
            set { _timeCtrl = value; RaisePropertyChanged(); }
        }

        private bool _isDeviceStatus = false;
        /// <summary>
        /// 设备状态
        /// </summary>
        public bool IsDeviceStatus
        {
            get { return _isDeviceStatus; }
            set
            {
                if (_isDeviceStatus == value)
                    return;

                _isDeviceStatus = value;
                RaisePropertyChanged();
            }
        }

        private List<string> _controlMode;
        /// <summary>
        /// 控制模式
        /// </summary>
        public List<string> ControlMode
        {
            get { return _controlMode; }
            set
            {
                _controlMode = value;
                RaisePropertyChanged();
            }
        }

        private string _selectedControlMode;
        /// <summary>
        /// 当前选中的控制模式
        /// </summary>
        public string SelectedControlMode
        {
            get { return _selectedControlMode; }
            set
            {
                _selectedControlMode = value;
                RaisePropertyChanged();
            }
        }

        private List<string> _sensorType;
        /// <summary>
        /// 传感器类型
        /// </summary>
        public List<string> SensorType
        {
            get { return _sensorType; }
            set
            {
                _sensorType = value;
                RaisePropertyChanged();
            }
        }

        private string _selectedSensorType;
        /// <summary>
        /// 当前选中的控制模式传感器
        /// </summary>
        public string SelectedSensorType
        {
            get { return _selectedSensorType; }
            set
            {
                _selectedSensorType = value;
                RaisePropertyChanged();
            }
        }

        private bool _isManual = false;
        /// <summary>
        /// 是否为手动模式
        /// </summary>
        public bool IsManual
        {
            get { return _isManual; }
            set
            {
                if (_isManual == value)
                    return;

                _isManual = value;
                RaisePropertyChanged();

                if (BtnContent.Contains("关闭"))
                {
                    if (value)
                    {
                        StopPidTimer();
                        StartManualTimer();
                    }
                    else
                    {
                        StopManualTimer();
                        StartPidTimer();
                    }
                }
            }
        }

        private string _btnContent = "启动";

        public string BtnContent
        {
            get { return _btnContent; }
            set { _btnContent = value; RaisePropertyChanged(); }
        }


        private PlotModel _tempPlotModel;
        /// <summary>
        /// 真空图表模型
        /// </summary>
        public PlotModel TempPlotModel
        {
            get { return _tempPlotModel; }
            set { _tempPlotModel = value; RaisePropertyChanged(); }
        }

        private float _temperatureRange;
        /// <summary>
        /// 触发温度控制的范围（绝对值）也就是温度距离多少时开始精确PID控制，单位℃
        /// </summary>
        public float TemperatureRange
        {
            get { return _temperatureRange; }
            set { _temperatureRange = value; RaisePropertyChanged(); }
        }

        private float _minRate;
        /// <summary>
        /// 最小升温速率，单位℃/s
        /// </summary>
        public float MinRate
        {
            get { return _minRate; }
            set { _minRate = value; RaisePropertyChanged(); }
        }

        private float _speedC;
        /// <summary>
        /// 速度控制的衰减变化系数
        /// </summary>
        public float SpeedC
        {
            get { return _speedC; }
            set { _speedC = value; RaisePropertyChanged(); }
        }

        private float _systemC;
        /// <summary>
        /// 速度控制的衰减变化系数
        /// </summary>
        public float SystemC
        {
            get { return _systemC; }
            set { _systemC = value; RaisePropertyChanged(); }
        }


        #endregion

        #region ------------Command------------
        public DelegateCommand WorkTypeCommand { get; set; }
        private void WorkType()
        {
            TemperatureControllerProtocol.SetWorkTypeCommand(0);
        }

        public DelegateCommand DeviceStatusCommand { get; set; }
        private void DeviceStatus()
        {
            if (IsDeviceStatus)
            {
                TemperatureControllerProtocol.SetCentigradeCommand(1);
            }
            else
            {
                TemperatureControllerProtocol.SetCentigradeCommand(0);
            }
        }

        public DelegateCommand StartWorkCommand { get; set; }

        private void StartWork()
        {
            if (IsDeviceStatus)
            {
                if (BtnContent.Contains("启动"))
                {
                    if (!SerialPortService.IsOpen && !NetUdpService.IsOpen)
                    {
                        _dialogHostService.Information("提示", "请连接设备后再开始！", CommonModel.TemperatureControllerRegionName);
                        return;
                    }

                    BtnContent = "停止";
                    if (SerialPortService.IsOpen)
                    {
                        if (ControlMode.Contains("Manual"))
                        {
                            //var cmd = TemperatureControllerProtocol.SetManualCurrentCommand(
                            //    SetCurrent,
                            //    MaxCurrent);
                            var cmdModel = TemperatureControllerProtocol.SetCtrlModeCommand(1);
                            var cmdIout = TemperatureControllerProtocol.SetManualIOutCommand(SetCurrent);
                            //var cmdImax = TemperatureControllerProtocol.SetIMaxCommand(MaxCurrent);
                            SerialPortService.SendMsg(cmdModel);
                            SerialPortService.SendMsg(cmdIout);
                            StartManualTimer();
                        }
                        else
                        {
                            var cmdModel = TemperatureControllerProtocol.SetCtrlModeCommand(0);
                            var cmdPID = TemperatureControllerProtocol.SetPIDparameterCommand(Pid.Kp, Pid.Ki, Pid.Kd);
                            var cmdImax = TemperatureControllerProtocol.SetIMaxCommand(MaxCurrent);
                            SerialPortService.SendMsg(cmdModel);
                            SerialPortService.SendMsg(cmdPID);
                            SerialPortService.SendMsg(cmdImax);
                            if (IsKelvin)
                            {
                                //var cmd = TemperatureControllerProtocol.SetKelvinPidCommand(
                                //    TargetTemperature,
                                //    SetCurrent,
                                //    MaxCurrent,
                                //    Pid.Kp, Pid.Ki, Pid.Kd);
                                var alldata = TemperatureControllerProtocol.SetAllData(1);
                                var cmdtargetTemp = TemperatureControllerProtocol.SetPIDTargetTempCommand(1, TargetTemperature);
                                var cmd = TemperatureControllerProtocol.SetRtimeTempCommand(1);
                                SerialPortService.SendMsg(alldata);
                                SerialPortService.SendMsg(cmdtargetTemp);
                                SerialPortService.SendMsg(cmd);
                            }
                            else
                            {
                                //var cmd = TemperatureControllerProtocol.SetCentigradePidCommand(
                                //    TargetTemperature,
                                //    SetCurrent,
                                //    MaxCurrent,
                                //    Pid.Kp, Pid.Ki, Pid.Kd);
                                var alldata = TemperatureControllerProtocol.SetAllData(1);
                                var cmdtargetTemp = TemperatureControllerProtocol.SetPIDTargetTempCommand(0, TargetTemperature);
                                var cmd = TemperatureControllerProtocol.SetRtimeTempCommand(0);
                                SerialPortService.SendMsg(alldata);
                                SerialPortService.SendMsg(cmdtargetTemp);
                                SerialPortService.SendMsg(cmd);
                            }
                        }

                        if (NetUdpService.IsOpen)
                        {
                            if (IsManual)
                            {
                                var cmd = TemperatureControllerProtocol.SetManualCurrentCommand(
                                    SetCurrent,
                                    MaxCurrent);
                                NetUdpService.SendMsg(cmd);
                                StartManualTimer();
                            }
                            else
                            {
                                if (IsKelvin)
                                {
                                    var cmd = TemperatureControllerProtocol.SetKelvinPidCommand(
                                        TargetTemperature,
                                        SetCurrent,
                                        MaxCurrent,
                                        Pid.Kp, Pid.Ki, Pid.Kd);
                                    NetUdpService.SendMsg(cmd);
                                }
                                else
                                {
                                    var cmd = TemperatureControllerProtocol.SetCentigradePidCommand(
                                        TargetTemperature,
                                        SetCurrent,
                                        MaxCurrent,
                                        Pid.Kp, Pid.Ki, Pid.Kd);
                                    NetUdpService.SendMsg(cmd);
                                }
                            }
                        }

                        StartPidTimer();
                    }
                }
                else
                {
                    BtnContent = "启动";
                    StopManualTimer();
                    StopPidTimer();

                    var cmd = TemperatureControllerProtocol.ReleaseCommand();
                    if (SerialPortService.IsOpen)
                        SerialPortService.SendMsg(cmd);
                    if (NetUdpService.IsOpen)
                        NetUdpService.SendMsg(cmd);
                }
            }
            else
            {
                _dialogHostService.Information("提示", "设备未就绪，请检查设备状态！", CommonModel.TemperatureControllerRegionName);
            }
        }

        private string SelectCtrlMode()
        {
            ControlMode = new List<string> { "PID", "Manual" };

            // 设置默认选中第一项
            if (ControlMode.Any())
            {
                SelectedControlMode = ControlMode.First();
            }
            return SelectedControlMode;
        }

        private string SelectSensorType()
        {
            SensorType = new List<string> { "PT100", "K型热电偶" };

            // 设置默认选中第一项
            if (SensorType.Any())
            {
                SelectedSensorType = SensorType.First();
            }
            return SelectedSensorType;
        }

        /// <summary>
        /// 图表数据
        /// </summary>
        /// <param name="Temperature">温度值</param>
        private void TemperatureDataChart(float Temperature)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    _temp.Points.Add(DateTimeAxis.CreateDataPoint(DateTime.Now, Temperature - 273.15));
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        TempPlotModel.InvalidatePlot(true);
                    });
                    Thread.Sleep(2000);
                }
            });
        }

        public DelegateCommand ClearMonitorCommand { get; set; }

        private void ClearMonitor()
        {
            _temp.Points.Clear();
            TempPlotModel.InvalidatePlot(true);
        }

        public DelegateCommand AutoAdjustComamnd { get; set; }

        private void AutoAdjust()
        {
            foreach (var axis in TempPlotModel.Axes)
                axis.Reset();
            TempPlotModel.InvalidatePlot(true);
        }

        public DelegateCommand SaveToFileCommand { get; set; }

        private void SaveToFile()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
            {
                var path = dialog.SelectedPath;

                SaveToFile(path);
            }
        }


        private void SaveToFile(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var timeTip = DateTime.Now.ToString("HHmmss");
            PngExporter exporter = new PngExporter();

            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                exporter.ExportToFile(TempPlotModel, $"{path}\\温度记录_{timeTip}.png");
                SaveSeriesToFile(_temp, $"{path}\\温度(K)_{timeTip}.txt");
            }));
        }

        private void SaveSeriesToFile(DataPointSeries series, string filePath)
        {
            string gunReport = string.Empty;
            foreach (var report in series.Points)
            {
                gunReport += $"{report.X}\t{report.Y}\n";
            }
            try
            {
                using (var gunStream = File.OpenWrite(filePath))
                {
                    var gunData = Encoding.UTF8.GetBytes(gunReport);
                    gunStream.Write(gunData, 0, gunData.Length);
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Fatal($"保存图表数据异常：目标路径【{filePath}】，异常原因【{ex.Message}】");
            }
        }

        public DelegateCommand SetControlparmsCommand { get; set; }
        private void SetControlparms()
        {
            TemperatureControllerProtocol.SetControlparms(TemperatureRange, MinRate, SpeedC, SystemC);
        }

        public DelegateCommand SetPIDparmsCommand { get; set; }
        private void SetPIDparms()
        {
            TemperatureControllerProtocol.SetSteadyCoolPIDparms(SteadyCoolPID.SteadySteadyCoolp, SteadyCoolPID.SteadySteadyCooli, SteadyCoolPID.SteadySteadyCoold);
        }
        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        private void StartPidTimer()
        {
            if (_pidTimer == null)
            {
                _pidTimer = new System.Timers.Timer();
                _pidTimer.Elapsed += PidTimer_Elapsed;
                _pidTimer.Interval = 2000;
                _pidTimer.AutoReset = true;
                _pidTimer.Enabled = true;
            }
            _pidTimer.Start();
        }

        private void StopPidTimer()
        {
            if (_pidTimer != null)
            {
                _pidTimer.Stop();
            }
        }

        private void PidTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (IsKelvin)
            {
                var cmd = TemperatureControllerProtocol.SetKelvinCommand(TargetTemperature);
                if(SerialPortService.IsOpen)
                    SerialPortService.SendMsg(cmd);
                if(NetUdpService.IsOpen)
                    NetUdpService.SendMsg(cmd);
            }
            else
            {
                var cmd = TemperatureControllerProtocol.SetCentigradeCommand(TargetTemperature);
                if (SerialPortService.IsOpen)
                    SerialPortService.SendMsg(cmd);
                if (NetUdpService.IsOpen)
                    NetUdpService.SendMsg(cmd);
            }
        }

        private void StartManualTimer()
        {
            var cmd = TemperatureControllerProtocol.StopCommand();
            if (SerialPortService.IsOpen)
                SerialPortService.SendMsg(cmd);
            if (NetUdpService.IsOpen)
                NetUdpService.SendMsg(cmd);

            if (_manualTimer != null)
            {
                _manualTimer = new System.Timers.Timer();
                _manualTimer.Elapsed += ManualTimer_Elapsed;
                _manualTimer.Interval = 100;
                _manualTimer.AutoReset = true;
                _manualTimer.Enabled = true;
            }
            _manualTimer?.Start();
        }

        private void StopManualTimer()
        {
            if (_manualTimer != null)
            {
                _manualTimer.Stop();
            }
        }

        private void ManualTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            var cmd = TemperatureControllerProtocol.SetManualCurrentCommand(
                            SetCurrent,
                            MaxCurrent);
            SerialPortService.SendMsg(cmd);
        }

        private void NetUdpService_UpdateResponse(object? sender, byte[] e)
        {
            _parser.ReceiveBytes(e);
        }

        private void SerialPortService_UpdateResponse(object? sender, byte[] e)
        {
            _parser.ReceiveBytes(e);
        }

        private void Parser_PacketReceivedEvent(object? sender, TemperatureControllerPacket e)
        {
            if (e != null)
            {
                switch (e.CmdType)
                {
                    case EnumTemperatureControllerCommandType.CMD_GET_DATA_ALL:
                        {
                            var data = e.DataSource;
                            ReadTemperatureUnit = BitConverter.ToUInt32(data, 0);
                            var temp = BitConverter.ToSingle(data, 4);
                            ReadTargetTemperature = BitConverter.ToSingle(data, 8);
                            ReadCurrent = BitConverter.ToSingle(data, 12);
                            ReadTimeCtrl = BitConverter.ToSingle(data, 16);
                            ReadCtrlMode = BitConverter.ToUInt32(data, 20);
                            ReadDeviceStatus = BitConverter.ToUInt32(data, 24);
                            ReadTemperature = temp;
                            _temp.Points.Add(new DataPoint(_temp.Points.Count(), temp));
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
