using CsvHelper;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using OxyPlot.Wpf;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Dialog;
using UtilityTools.Modules.MotorTest.Protocol;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.Services.Services;

namespace UtilityTools.Modules.MotorTest.Model
{
    public class ThreeAxisTestModel:BindableBase
    {
        public ThreeAxisTestModel(IContainerProvider containerProvider)
        {
            _containerProvider = containerProvider;
         

            _parser = new SelfMotorParser();
            _parser.PacketReceivedEvent += Parser_PacketReceivedEvent;

            SerialPortService = new SerialPortService();
            SerialPortService.UpdateResponse += SerialPortService_UpdateResponse;
            SerialPortService.IsBinary = true;
            var netUdp = new UdpNetAsyncDevice();
            netUdp.Name = "升级网口";
            netUdp.DeviceInstance.TargetIp = "192.168.1.88";
            netUdp.DeviceInstance.TargetPort = 5003;
            netUdp.DeviceInstance.HostIp = "192.168.1.33";
            netUdp.DeviceInstance.HostPort = 6585;
            netUdp.IsBinary = true;
            NetUdpService = netUdp;
            NetUdpService.UpdateResponse += NetUdpService_UpdateResponse;
            Init();
        }
        
        private IContainerProvider _containerProvider;
        private readonly IDialogHostService _dialogHostService;
        private SelfMotorParser _parser;
        private TaskCompletionSource<string> _waitingReply;
        
        private event EventHandler<SelfMotorPacket> _xAxisReturnEvent;
        private event EventHandler<SelfMotorPacket> _yAxisReturnEvent;
        private event EventHandler<SelfMotorPacket> _zAxisReturnEvent;
        private event EventHandler<SelfMotorPacket> _tAxisReturnEvent;
        private event EventHandler<SelfMotorPacket> _rAxisReturnEvent;



        private FiveAxisModel _xAxis;
        /// <summary> 
        /// x轴
        /// </summary>
        public FiveAxisModel XAxis
        {
            get { return _xAxis; }
            set { _xAxis = value; RaisePropertyChanged(); }
        }
        private FiveAxisModel _yAxis;
        /// <summary>
        /// Y轴
        /// </summary>
        public FiveAxisModel YAxis
        {
            get { return _yAxis; }
            set { _yAxis = value; RaisePropertyChanged(); }
        }
        private FiveAxisModel _zAxis;
        /// <summary>
        /// Z轴
        /// </summary>
        public FiveAxisModel ZAxis
        {
            get { return _zAxis; }
            set { _zAxis = value; RaisePropertyChanged(); }
        }
        private FiveAxisModel _tAxis;
        /// <summary>
        /// T轴
        /// </summary>
        public FiveAxisModel TAxis
        {
            get { return _tAxis; }
            set { _tAxis = value; RaisePropertyChanged(); }
        }

        private FiveAxisModel _rAxis;
        /// <summary>
        /// R轴
        /// </summary>
        public FiveAxisModel RAxis
        {
            get { return _rAxis; }
            set { _rAxis = value; RaisePropertyChanged(); }
        }

        private PlotModel _motorplotModel;
        /// <summary>
        /// 电机位置坐标轴
        /// </summary>
        public PlotModel MotorplotModel
        {
            get { return _motorplotModel; }
            set { _motorplotModel = value; RaisePropertyChanged(); }
        }
        private PlotModel _motorSpeedplotModel;
        /// <summary>
        /// 电机速度坐标轴
        /// </summary>
        public PlotModel MotorSpeedplotModel
        {
            get { return _motorSpeedplotModel; }
            set { _motorSpeedplotModel = value; RaisePropertyChanged(); }
        }
        private Queue<byte[]> _byteQueue;
        /// <summary>
        /// 普通队列
        /// </summary>
        public Queue<byte[]> ByteQueue
        {
            get { return _byteQueue; }
            set { _byteQueue = value; RaisePropertyChanged(); }
        }
        private Queue<byte[]> _importantByteQueue;
        /// <summary>
        /// 重要队列
        /// </summary>
        public Queue<byte[]> ImportantByteQueue
        {
            get { return _importantByteQueue; }
            set { _importantByteQueue = value; RaisePropertyChanged(); }
        }
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
        private void Init()
        {
            ClearMonitorCommand = new DelegateCommand<string>(ClearMonitor);
            TestMotorDelegateCommand = new DelegateCommand(TestMotor);
            AutoAdjustCommand = new DelegateCommand<string> (AutoAdjust);
            SaveToFileCommand = new DelegateCommand<string>(SaveToFile);
            SmoothnessDetectionCommand = new DelegateCommand (SmoothnessDetection);
            MoveCommand = new DelegateCommand<string>(Move);
            ByteQueue = new Queue<byte[]>();
            ImportantByteQueue = new Queue<byte[]>();
            XAxis = new FiveAxisModel( _containerProvider,EnumMotorId.MOTOR_2, EnumMotorModel.MOTOR_x, "X轴") { };
            YAxis = new FiveAxisModel(_containerProvider,EnumMotorId.MOTOR_1, EnumMotorModel.MOTOR_y, "Y轴") { };
            ZAxis = new FiveAxisModel(_containerProvider,EnumMotorId.MOTOR_4, EnumMotorModel.MOTOR_z, "Z轴") { };
            TAxis = new FiveAxisModel(_containerProvider, EnumMotorId.MOTOR_3, EnumMotorModel.MOTOR_t, "T轴") { };
            RAxis = new FiveAxisModel(_containerProvider, EnumMotorId.MOTOR_5, EnumMotorModel.MOTOR_r, "R轴") { };
            XAxis.AddCmdEvent += AddCmd;
            XAxis.AddImportantCmdEvent += AddImportant;
            YAxis.AddCmdEvent += AddCmd;
            YAxis.AddImportantCmdEvent += AddImportant;
            ZAxis.AddCmdEvent += AddCmd;
            ZAxis.AddImportantCmdEvent += AddImportant;
            TAxis.AddCmdEvent += AddCmd;
            TAxis.AddImportantCmdEvent += AddImportant;
            RAxis.AddCmdEvent += AddCmd;
            RAxis.AddImportantCmdEvent += AddImportant;
            _xAxisReturnEvent += XAxis.Parser_PacketReceivedEvent;
            _yAxisReturnEvent += YAxis.Parser_PacketReceivedEvent;
            _zAxisReturnEvent += ZAxis.Parser_PacketReceivedEvent;
            _tAxisReturnEvent += TAxis.Parser_PacketReceivedEvent;
            _rAxisReturnEvent += RAxis.Parser_PacketReceivedEvent;

            MotorplotModel = new PlotModel();
            MotorplotModel.Legends.Add(new Legend());
            MotorplotModel.Axes.Add(new DateTimeAxis() { Title = "时间", Position = AxisPosition.Bottom });
            MotorplotModel.Axes.Add(new LinearAxis() { Title = "位置(脉冲)", Position = AxisPosition.Left });
            MotorplotModel.Series.Add(XAxis.PosLine);
            MotorplotModel.Series.Add(YAxis.PosLine);
            MotorplotModel.Series.Add(ZAxis.PosLine);
            MotorplotModel.Series.Add(TAxis.PosLine);
            MotorplotModel.Series.Add(RAxis.PosLine);

            MotorSpeedplotModel = new PlotModel();
            MotorSpeedplotModel.Legends.Add(new Legend());
            MotorSpeedplotModel.Axes.Add(new DateTimeAxis() { Title = "时间", Position = AxisPosition.Bottom });
            MotorSpeedplotModel.Axes.Add(new LinearAxis() { Title = "速度(脉冲/秒)", Position = AxisPosition.Left });
            MotorSpeedplotModel.Series.Add(XAxis.SpeedLine);
            MotorSpeedplotModel.Series.Add(YAxis.SpeedLine);
            MotorSpeedplotModel.Series.Add(ZAxis.SpeedLine);
            MotorSpeedplotModel.Series.Add(TAxis.SpeedLine);
            MotorSpeedplotModel.Series.Add(RAxis.SpeedLine);


        }

        private void AddCmd(object? sender, byte[] e)
        {
            ByteQueue.Enqueue(e);
        }
        private void AddImportant(object? sender, byte[] e)
        {
            ImportantByteQueue.Enqueue(e);
        }
        
        public DelegateCommand TestMotorDelegateCommand { get; set; } 
        /// <summary>
        /// 基础测试
        /// </summary>
        private void TestMotor()
        {
            Task.Run(() => 
            {
                XAxis.TestAll();
            });
            Task.Run(() =>
            {
                YAxis.TestAll();
            });
            Task.Run(() => 
            {
                ZAxis.TestAll();
            });
        }
        public DelegateCommand SmoothnessDetectionCommand { get; set; }
        private void SmoothnessDetection()
        {
            Task.Run(() =>
            {
                XAxis.TestSmoothnessDetection();
            });
            Task.Run(() =>
            {
                YAxis.TestSmoothnessDetection();
            });
            Task.Run(() =>
            {
                ZAxis.TestSmoothnessDetection();
            });
            Task.Run(() =>
            {
                TAxis.TestSmoothnessDetection();
            });
            Task.Run(() =>
            {
                RAxis.TestSmoothnessDetection();
            });
        }
        public DelegateCommand<string> MoveCommand { get; set; }
        private void Move(string direction)
        {
            switch (direction)
            {
                case "x_n": XAxis.Move("n"); break;
                case "x_p": XAxis.Move("p"); break;
                case "y_n": YAxis.Move("n"); break;
                case "y_p": YAxis.Move("p"); break;
                case "z_n": ZAxis.Move("n"); break;
                case "z_p": ZAxis.Move("p"); break;
                case "t_p": TAxis.Move("p"); break;
                case "t_n": TAxis.Move("n"); break;
                case "r_p": RAxis.Move("p"); break;
                case "r_n": RAxis.Move("n"); break;
                case "stop":
                    {
                        XAxis.Move("s");
                        YAxis.Move("s");
                        ZAxis.Move("s"); 
                        TAxis.Move("s");
                        RAxis.Move("s");
                    }
                    break;
            }
        }
        public DelegateCommand<string> ClearMonitorCommand { get; set; }
        /// <summary>
        /// 清除监控数据
        /// </summary>
        private void ClearMonitor(string parameter)
        {
            if (parameter == "point")
            {
                foreach (var series in MotorplotModel.Series)
                {
                    var line = series as LineSeries;
                    if (line != null)
                    {
                        line.Points.Clear();
                    }
                    XAxis.PlotViewPointMessages.Clear();
                    YAxis.PlotViewPointMessages.Clear();
                    ZAxis.PlotViewPointMessages.Clear();
                    TAxis.PlotViewPointMessages.Clear();
                    RAxis.PlotViewPointMessages.Clear();

                }

                MotorplotModel.InvalidatePlot(true);
            }
            else if (parameter == "speed")
            {
                foreach (var series in MotorSpeedplotModel.Series)
                {
                    var line = series as LineSeries;
                    if (line != null)
                    {
                        line.Points.Clear();
                    }
                    XAxis.plotViewSpeedMessages.Clear();
                    YAxis.plotViewSpeedMessages.Clear();
                    ZAxis.plotViewSpeedMessages.Clear();
                    TAxis.plotViewSpeedMessages.Clear();
                    RAxis.plotViewSpeedMessages.Clear();
                }
                MotorSpeedplotModel.InvalidatePlot(true);
            }

        }
        public DelegateCommand<string> AutoAdjustCommand { get; set; }
        /// <summary>
        /// 自动调节监控数据
        /// </summary>
        private void AutoAdjust(string parameter)
        {
            if (parameter == "point")
            {
                foreach (var axis in MotorplotModel.Axes)
                    axis.Reset();
                MotorplotModel.InvalidatePlot(true);
            }
            else if (parameter == "speed")
            {
                foreach (var axis in MotorSpeedplotModel.Axes)
                    axis.Reset();
                MotorSpeedplotModel.InvalidatePlot(true);
            }

        }
        public DelegateCommand<string> SaveToFileCommand { get; set; }
        /// <summary>
        /// 保存到数据
        /// </summary>
        private void SaveToFile(string parameter)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
            {
                var path = dialog.SelectedPath;

                SaveToFile(path, parameter);
            }
        }

        private void SaveToFile(string path, string parameter)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var timeTip = DateTime.Now.ToString("HHmmss");
            PngExporter exporter = new PngExporter();

            if (parameter == "point")
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    exporter.ExportToFile(MotorplotModel, $"{path}\\参数曲线_{timeTip}.png");
                    int index = 0;
                    foreach (var series in MotorplotModel.Series)
                    {
                        var line = series as LineSeries;
                        if (line != null)
                        {
                            index++;
                            SaveSeriesToFile(line, $"{path}\\参数{index}_{timeTip}_{parameter}.txt");
                        }
                    }
                }));
            }
            else if (parameter == "speed")
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    exporter.ExportToFile(MotorSpeedplotModel, $"{path}\\参数曲线_{timeTip}.png");
                    int index = 0;
                    foreach (var series in MotorSpeedplotModel.Series)
                    {
                        var line = series as LineSeries;
                        if (line != null)
                        {
                            index++;
                            SaveSeriesToFile(line, $"{path}\\参数{index}_{timeTip}.txt");
                        }
                    }
                }));
            }

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
        public void StartTimer()
        {
            XAxis._getMotorStateTimer.Start();
            YAxis._getMotorStateTimer.Start();
            ZAxis._getMotorStateTimer.Start();
            TAxis._getMotorStateTimer.Start();
            RAxis._getMotorStateTimer.Start();
        }
        public void StopTimer()
        {
            XAxis._getMotorStateTimer.Stop();
            YAxis._getMotorStateTimer.Stop();
            ZAxis._getMotorStateTimer.Stop();
            TAxis._getMotorStateTimer.Stop();
            RAxis._getMotorStateTimer.Stop();
        }
        private void NetUdpService_UpdateResponse(object? sender, byte[] e)
        {
            _parser.ReceiveBytes(e);
        }

        private void SerialPortService_UpdateResponse(object? sender, byte[] e)
        {
            _parser.ReceiveBytes(e);
        }
        private void Parser_PacketReceivedEvent(object? sender, SelfMotorPacket e)
        {
            if (e != null)
            {
                _waitingReply.SetResult("Ready");
                var data = e.DataSource;
                var channel = (EnumMotorId)(data[0]);//电机通道
                switch (channel)
                {
                    case EnumMotorId.MOTOR_1: _yAxisReturnEvent.Invoke(this,e); break;
                    case EnumMotorId.MOTOR_2: _xAxisReturnEvent.Invoke(this,e); break;
                    case EnumMotorId.MOTOR_3: _tAxisReturnEvent.Invoke(this,e); break;
                    case EnumMotorId.MOTOR_4: _zAxisReturnEvent.Invoke(this, e); break;
                    case EnumMotorId.MOTOR_5: _rAxisReturnEvent.Invoke(this, e); break;
                }
            }
        }
       

        public void SendingThread()
        {
            Task.Run(async () =>
            {
                while (NetUdpService.IsOpen || SerialPortService.IsOpen)
                {
                    
                    if (ImportantByteQueue.Count > 0)
                    {
                        var cmd = ImportantByteQueue.Dequeue();
                        if (NetUdpService.IsOpen)
                        {
                            NetUdpService.SendMsg(cmd);
                        }
                        if (SerialPortService.IsOpen)
                        {
                            SerialPortService.SendMsg(cmd);
                        }
                    }
                    else
                    {
                        if (ByteQueue.Count > 0)
                        {
                            var cmd = ByteQueue.Dequeue();
                            if (NetUdpService.IsOpen)
                            {
                                NetUdpService.SendMsg(cmd);
                            }
                            if (SerialPortService.IsOpen)
                            {
                                SerialPortService.SendMsg(cmd);
                            }
                        }
                        _waitingReply = new TaskCompletionSource<string>();
                        try
                        {
                            string result = await _waitingReply.Task.WaitAsync(TimeSpan.FromMilliseconds(300));
                            if (result == "Ready")
                            {
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }
                }

            });
        
        }
       
    }
}
