using CsvHelper;
using Newtonsoft.Json;
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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using UtilityTools.Core.Dialog;
using UtilityTools.Modules.MotorTest.Protocol;
using UtilityTools.Modules.MotorTest.SQLite;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.Services.Services;

namespace UtilityTools.Modules.MotorTest.Model
{
    public class ThreeAxisTestModel : BindableBase
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
        private Stopwatch _stopwatch;
        public System.Timers.Timer _getMotorStateTimer;
        private event EventHandler<SelfMotorPacket> _xAxisReturnEvent;
        private event EventHandler<SelfMotorPacket> _yAxisReturnEvent;
        private event EventHandler<SelfMotorPacket> _zAxisReturnEvent;
        private event EventHandler<SelfMotorPacket> _tAxisReturnEvent;
        private event EventHandler<SelfMotorPacket> _rAxisReturnEvent;
        private BackgroundWorker _work;
        private bool _isPerformance = false;
        private bool _isTest = false;
        private EnumMotorInquiry _testMotorId;
        private bool _isSpeedMode = false;
        public bool IsSpeedMode
        {
            get { return _isSpeedMode; }
            set { _isSpeedMode = value;  RaisePropertyChanged(); }
        }
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
            TestMotorDelegateCommand = new DelegateCommand<string>(TestMotor);
            AutoAdjustCommand = new DelegateCommand<string>(AutoAdjust);
            SaveToFileCommand = new DelegateCommand<string>(SaveToFile);
            SmoothnessDetectionCommand = new DelegateCommand<string>(SmoothnessDetection);
            MoveCommand = new DelegateCommand<string>(Move);
            SQLiteTestCommand = new DelegateCommand(SQLiteTest);
            TestPerformanceCommand = new DelegateCommand(TestPerformance);
            CloseTestPerformanceCommand = new DelegateCommand(CloseTestPerformance);
            SaveDataFileCommand = new DelegateCommand(SaveDataFile);
            ReadDataFileCommand = new DelegateCommand(ReadDataFile);
            CloseSlimitedCommand = new DelegateCommand(CloseSlimited); 
            ByteQueue = new Queue<byte[]>();
            ImportantByteQueue = new Queue<byte[]>();
            XAxis = new FiveAxisModel(_containerProvider, Protocol.EnumMotorId.MOTOR_2, EnumMotorModel.MOTOR_x, "X轴") { };
            YAxis = new FiveAxisModel(_containerProvider, EnumMotorId.MOTOR_1, EnumMotorModel.MOTOR_y, "Y轴") { };
            ZAxis = new FiveAxisModel(_containerProvider, EnumMotorId.MOTOR_4, EnumMotorModel.MOTOR_z, "Z轴") { };
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
            XAxis.PiontPlotModel = MotorplotModel;
            XAxis.SpeedPlotModel = MotorSpeedplotModel;
            YAxis.PiontPlotModel = MotorplotModel;
            YAxis.SpeedPlotModel = MotorSpeedplotModel;
            ZAxis.PiontPlotModel = MotorplotModel;
            ZAxis.SpeedPlotModel = MotorSpeedplotModel;
            TAxis.PiontPlotModel = MotorplotModel;
            TAxis.SpeedPlotModel = MotorSpeedplotModel;
            RAxis.PiontPlotModel = MotorplotModel;
            RAxis.SpeedPlotModel = MotorSpeedplotModel;

            RegisterTimer();
        }

        private void AddCmd(object? sender, byte[] e)
        {
            ByteQueue.Enqueue(e);
        }
        private void AddImportant(object? sender, byte[] e)
        {
            ImportantByteQueue.Enqueue(e);
        }
        public DelegateCommand SQLiteTestCommand { get; set; }
        private void SQLiteTest()
        {

            SpliteOperate.CreateTable(SpliteOperate.dbName);
            var time = DateTime.Now.ToString();
            SpliteOperate.InsertData(SpliteOperate.dbName, "0001", time, "1.10", "1.09", "两轴", "电机数据", "");
            SpliteOperate.QueryData(SpliteOperate.dbName);
        }

        public DelegateCommand TestPerformanceCommand { get; set; }
        private void TestPerformance()
        {
            _isPerformance = true;
            Task.Run(() =>
            {
                while (_isPerformance)
                {
                    if (_byteQueue.Count == 0)
                    {
                        var getMotor2Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_2);
                        ByteQueue.Enqueue(getMotor2Statuscmd);
                        var getMotor1Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_1);
                        ByteQueue.Enqueue(getMotor1Statuscmd);
                        var getMotor3Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_3);
                        ByteQueue.Enqueue(getMotor3Statuscmd);
                        var getMotor4Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_4);
                        ByteQueue.Enqueue(getMotor4Statuscmd);
                        var getMotor5Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_5);
                        ByteQueue.Enqueue(getMotor5Statuscmd);
                    }
                    Thread.Sleep(1);

                }
            });

        }
        public DelegateCommand CloseTestPerformanceCommand { get; set; }
        private void CloseTestPerformance()
        {
            _isPerformance = false;
        }

        public DelegateCommand<string> TestMotorDelegateCommand { get; set; }
        /// <summary>
        /// 基础测试
        /// </summary>
        private async void TestMotor(string motor)
        {
            await Task.Run(async () =>
            {
                _testMotorId = EnumMotorInquiry.InquiryMotor_all;
                TestInquiry();
                switch (motor)
                {
                    case "x": 
                        _testMotorId = EnumMotorInquiry.InquiryMotor_x;
                        await XAxis.TestAll();
                        break;
                    case "y":
                        _testMotorId = EnumMotorInquiry.InquiryMotor_y;
                        await YAxis.TestAll();
                        break;
                    case "z":
                        _testMotorId = EnumMotorInquiry.InquiryMotor_z;
                        await ZAxis.TestAll(); 
                        break;
                    case "t":
                        _testMotorId = EnumMotorInquiry.InquiryMotor_t;
                        await TAxis.TestAll();
                        break;
                    case "r":
                        _testMotorId = EnumMotorInquiry.InquiryMotor_r;
                        await RAxis.TestAll(); 
                        break;
                    case "all":
                        _testMotorId = EnumMotorInquiry.InquiryMotor_x;
                        await XAxis.TestAll();
                        _testMotorId = EnumMotorInquiry.InquiryMotor_y;
                        await YAxis.TestAll();
                        _testMotorId = EnumMotorInquiry.InquiryMotor_z;
                        await ZAxis.TestAll();
                        _testMotorId = EnumMotorInquiry.InquiryMotor_t;
                        await TAxis.TestAll();
                        _testMotorId = EnumMotorInquiry.InquiryMotor_r;
                        await RAxis.TestAll();
                   
                        break;
                }
                _testMotorId = EnumMotorInquiry.InquiryMotor_null; 
            });
           
        }
        private void TestInquiry()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (_byteQueue.Count == 0)
                    {
                        switch (_testMotorId)
                        {
                            case EnumMotorInquiry.InquiryMotor_x:
                                var getMotor1Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_2);
                                ByteQueue.Enqueue(getMotor1Statuscmd);
                                break;
                            case EnumMotorInquiry.InquiryMotor_y:
                                var getMotor2Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_1);
                                ByteQueue.Enqueue(getMotor2Statuscmd);
                                break;
                            case EnumMotorInquiry.InquiryMotor_z:
                                var getMotor3Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_4);
                                ByteQueue.Enqueue(getMotor3Statuscmd);
                                break;
                            case EnumMotorInquiry.InquiryMotor_t:
                                var getMotor4Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_3);
                                ByteQueue.Enqueue(getMotor4Statuscmd);
                                break;
                            case EnumMotorInquiry.InquiryMotor_r:
                                var getMotor5Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_5);
                                ByteQueue.Enqueue(getMotor5Statuscmd);
                                break;
                            case EnumMotorInquiry.InquiryMotor_all:
                                var getMotor1Statuscmd1 = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_2);
                                ByteQueue.Enqueue(getMotor1Statuscmd1);
                                var getMotor2Statuscmd1 = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_1);
                                ByteQueue.Enqueue(getMotor2Statuscmd1);
                                var getMotor3Statuscmd1 = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_4);
                                ByteQueue.Enqueue(getMotor3Statuscmd1);
                                var getMotor4Statuscmd1 = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_3);
                                ByteQueue.Enqueue(getMotor4Statuscmd1);
                                var getMotor5Statuscmd1 = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_5);
                                ByteQueue.Enqueue(getMotor5Statuscmd1);
                                break;
                            case EnumMotorInquiry.InquiryMotor_null: return;
                        }
                    }
                    Thread.Sleep(1);

                }
            });
        }
        public DelegateCommand<string> SmoothnessDetectionCommand { get; set; }
        private void SmoothnessDetection(string motor)
        {
            Task.Run(async () =>
            {
                _testMotorId = EnumMotorInquiry.InquiryMotor_all;
                TestInquiry();
                switch (motor)
                {
                    case "x":
                        _testMotorId = EnumMotorInquiry.InquiryMotor_x;
                        await XAxis.TestSmoothnessDetection();  
                        break;
                    case "y":
                        _testMotorId = EnumMotorInquiry.InquiryMotor_y;
                        await YAxis.TestSmoothnessDetection();
                        break;
                    case "z":
                        _testMotorId = EnumMotorInquiry.InquiryMotor_z;
                        await ZAxis.TestSmoothnessDetection();
                        break;
                    case "t":
                        _testMotorId = EnumMotorInquiry.InquiryMotor_t;
                        await TAxis.TestSmoothnessDetection();
                        break;
                    case "r":
                        _testMotorId = EnumMotorInquiry.InquiryMotor_r;
                        await RAxis.TestSmoothnessDetection();
                        break;
                    case "all":
                        _testMotorId = EnumMotorInquiry.InquiryMotor_x;
                        await XAxis.TestSmoothnessDetection();
                        _testMotorId = EnumMotorInquiry.InquiryMotor_y;
                        await YAxis.TestSmoothnessDetection();
                        _testMotorId = EnumMotorInquiry.InquiryMotor_z;
                        await ZAxis.TestSmoothnessDetection();
                        _testMotorId = EnumMotorInquiry.InquiryMotor_t;
                        await TAxis.TestSmoothnessDetection();
                        _testMotorId = EnumMotorInquiry.InquiryMotor_r;
                        await RAxis.TestSmoothnessDetection();
                        break;
                }
            });

        }
        public DelegateCommand<string> MoveCommand { get; set; }
        private void Move(string direction)
        {
            if (IsSpeedMode)
            {
                switch (direction)
                {
                    case "x_n": XAxis.SpeedModeMove("n"); break;
                    case "x_p": XAxis.SpeedModeMove("p"); break;
                    case "y_n": YAxis.SpeedModeMove("n"); break;
                    case "y_p": YAxis.SpeedModeMove("p"); break;
                    case "z_n": ZAxis.SpeedModeMove("n"); break;
                    case "z_p": ZAxis.SpeedModeMove("p"); break;
                    case "t_p": TAxis.SpeedModeMove("p"); break;
                    case "t_n": TAxis.SpeedModeMove("n"); break;
                    case "r_p": RAxis.SpeedModeMove("p"); break;
                    case "r_n": RAxis.SpeedModeMove("n"); break;
                    case "stop":
                        {
                            XAxis.SpeedModeMove("s");
                            YAxis.SpeedModeMove("s");
                            ZAxis.SpeedModeMove("s");
                            TAxis.SpeedModeMove("s");
                            RAxis.SpeedModeMove("s");
                        }
                        break;
                }
            }
            else 
            {
                switch (direction)
                {
                    case "x_n": XAxis.PosModeMove("n"); break;
                    case "x_p": XAxis.PosModeMove("p"); break;
                    case "y_n": YAxis.PosModeMove("n"); break;
                    case "y_p": YAxis.PosModeMove("p"); break;
                    case "z_n": ZAxis.PosModeMove("n"); break;
                    case "z_p": ZAxis.PosModeMove("p"); break;
                    case "t_p": TAxis.PosModeMove("p"); break;
                    case "t_n": TAxis.PosModeMove("n"); break;
                    case "r_p": RAxis.PosModeMove("p"); break;
                    case "r_n": RAxis.PosModeMove("n"); break;
                    case "stop":
                        {
                            XAxis.PosModeMove("s");
                            YAxis.PosModeMove("s");
                            ZAxis.PosModeMove("s");
                            TAxis.PosModeMove("s");
                            RAxis.PosModeMove("s");
                        }
                        break;
                }
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
                    XAxis.PlotViewSpeedMessages.Clear();
                    YAxis.PlotViewSpeedMessages.Clear();
                    ZAxis.PlotViewSpeedMessages.Clear();
                    TAxis.PlotViewSpeedMessages.Clear();
                    RAxis.PlotViewSpeedMessages.Clear();
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


        /// <summary>
        /// 定时器事件，问询电机状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QueryMotorStatusTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var getMotor1Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_1);
            ByteQueue.Enqueue(getMotor1Statuscmd);
            var getMotor2Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_2);
            ByteQueue.Enqueue(getMotor2Statuscmd);
            var getMotor3Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_3);
            ByteQueue.Enqueue(getMotor3Statuscmd);
            var getMotor4Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_4);
            ByteQueue.Enqueue(getMotor4Statuscmd);
            var getMotor5Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_5);
            ByteQueue.Enqueue(getMotor5Statuscmd);

        }
        private void RegisterTimer()
        {
            _getMotorStateTimer = new System.Timers.Timer(1000);
            _getMotorStateTimer.AutoReset = true;
            _getMotorStateTimer.Elapsed += QueryMotorStatusTimerElapsed;
        }
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            StopTimer();

            while (_work != null && _work.CancellationPending != true)
            {
                if (ByteQueue.Count == 0)
                {
                    var getMotor1Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_1);
                    ByteQueue.Enqueue(getMotor1Statuscmd);
                    var getMotor2Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_2);
                    ByteQueue.Enqueue(getMotor2Statuscmd);
                    var getMotor3Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_3);
                    ByteQueue.Enqueue(getMotor3Statuscmd);
                    var getMotor4Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_4);
                    ByteQueue.Enqueue(getMotor4Statuscmd);
                    var getMotor5Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_5);
                    ByteQueue.Enqueue(getMotor5Statuscmd);
                }

            }
            e.Cancel = true;
            StartTimer();
        }
        public void StartTimer()
        {
            _getMotorStateTimer.Start();


        }
        public void StopTimer()
        {
            _getMotorStateTimer.Stop();
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
                    case EnumMotorId.MOTOR_1: _yAxisReturnEvent.Invoke(this, e); break;
                    case EnumMotorId.MOTOR_2: _xAxisReturnEvent.Invoke(this, e); break;
                    case EnumMotorId.MOTOR_3: _tAxisReturnEvent.Invoke(this, e); break;
                    case EnumMotorId.MOTOR_4: _zAxisReturnEvent.Invoke(this, e); break;
                    case EnumMotorId.MOTOR_5: _rAxisReturnEvent.Invoke(this, e); break;
                }

            }
        }


        public void SendingThread()
        {
            Task.Run(async () =>
            {
            List<long> times = new List<long>();
            while (NetUdpService.IsOpen)
            {
                if (_stopwatch == null)
                {
                    _stopwatch = new Stopwatch();
                }
                if (_stopwatch.IsRunning == false)
                {
                    _stopwatch.Start();
                }
                if (_importantByteQueue != null && _importantByteQueue.Count > 0)
                {
                    NetUdpService.SendMsg(_importantByteQueue.Dequeue());
                }
                else
                {
                    if (_byteQueue != null && _byteQueue.Count > 0)
                    {
                        NetUdpService.SendMsg(_byteQueue.Dequeue());
                    }
                }
                var time = _stopwatch.ElapsedMilliseconds;
                times.Add(time);
                Debug.WriteLine($"发送时间间隔{time}");

                _waitingReply = new TaskCompletionSource<string>();
                Thread.Sleep(1);
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

                }
            }


                while (SerialPortService.IsOpen)
                {
                    if (_stopwatch == null)
                    {
                        _stopwatch = new Stopwatch();
                    }
                    if (_stopwatch.IsRunning == false)
                    {
                        _stopwatch.Start();
                    }
                    if (_importantByteQueue != null && _importantByteQueue.Count > 0)
                    {
                        SerialPortService.SendMsg(_importantByteQueue.Dequeue());
                    }
                    else
                    {
                        if (_byteQueue != null && _byteQueue.Count > 0)
                        {
                            SerialPortService.SendMsg(_byteQueue.Dequeue());
                        }
                    }
                    var time = _stopwatch.ElapsedMilliseconds;
                    Debug.WriteLine($"发送时间间隔{time}");

                    _waitingReply = new TaskCompletionSource<string>();
                    Thread.Sleep(1);
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

                    }

                }

            });
        

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
        public DelegateCommand SaveDataFileCommand { get; set; }
        private void SaveDataFile()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            var path = dialog.SelectedPath;
            Task.Run(() => 
            {
                XAxis.Serilize(path);
                YAxis.Serilize(path);
                ZAxis.Serilize(path);
                TAxis.Serilize(path);
                RAxis.Serilize(path);
            });
            
        }

        public DelegateCommand ReadDataFileCommand { get; set; }
        private void ReadDataFile()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            var path = dialog.SelectedPath;
            Task.Run(() =>
            {
                XAxis.Deserilize(path);
                YAxis.Deserilize(path);
                ZAxis.Deserilize(path);
                TAxis.Deserilize(path);
                RAxis.Deserilize(path);
            });
        }


        public DelegateCommand CloseSlimitedCommand { get; set; }
        /// <summary>
        /// 关闭x，y，z的软限位
        /// </summary>
        private void CloseSlimited()
        {
            XAxis.CloseSlimited();
            YAxis.CloseSlimited();
            ZAxis.CloseSlimited();
        }

    }
}
