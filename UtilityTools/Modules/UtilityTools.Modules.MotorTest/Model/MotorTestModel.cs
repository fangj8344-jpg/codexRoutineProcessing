using ImageMagick;
using MaterialDesignThemes.Wpf;
using MathNet.Numerics.Financial;
using MathNet.Numerics.RootFinding;
using MathNet.Numerics.Statistics;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Fluent;
using OpenCvSharp.Features2D;
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using TouchSocket.Rpc.TouchRpc;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Extension;
using UtilityTools.Core.Model;
using UtilityTools.Modules.MotorTest.Entity;
using UtilityTools.Modules.MotorTest.Protocol;
using UtilityTools.Modules.MotorTest.SQLite;
using UtilityTools.Modules.MotorTest.Views;
using UtilityTools.Services.Comms;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.Services.Services;

namespace UtilityTools.Modules.MotorTest.Model
{
    public class MotorTestModel : BindableBase
    {


        #region ------------Constructor------------
        public MotorTestModel(IContainerProvider containerProvider)
        {
            _containerProvider = containerProvider;
            _dialogHostService = containerProvider.Resolve<IDialogHostService>();

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
            RegisterTimer();
        }
        #endregion
        private IContainerProvider _containerProvider;
        private readonly IDialogHostService _dialogHostService;

        private SelfMotorParser _parser;
        private ConcurrentQueue<byte[]> _byteQueue = new ConcurrentQueue<byte[]>();
        private ConcurrentQueue<byte[]> _importantByteQueue = new ConcurrentQueue<byte[]>();
        private string[] _directSubDirs;
        private string _selectLoadDataPath;
        private int _selectIndex;
        public System.Timers.Timer _getMotorStateTimer;
        private System.Timers.Timer _getMotorPosTimer;
        private System.Timers.Timer _StallDetectionTimer;
        private TaskCompletionSource<string> _limitedtcs1;
        private TaskCompletionSource<string> _waitingReply;
        private TaskCompletionSource<string> _runStateUpdate;
  
        private CancellationTokenSource _cts;
        //private List<int> timePosStallDetectionList;
        private Stopwatch _stopwatch;
        private Stopwatch _testTime;
        private long elapsedTime;
        private int[] _limtedPos;
        private BackgroundWorker _work;
        private bool _isPerformance;
        private bool _isDurabilityTest = false;
        private bool _isTest = false;
        private bool __isInLeadScrewTest = false;
        int countNumber = 0;
        private readonly object _lockobj = new object();
       
        LineSeries _xPosLine;
        LineSeries _yPosLine;
        LineSeries _xSpeedPosLine;
        LineSeries _ySpeedPosLine;
        private string _dataPath;

        private double _std;
        public double Std
        {
            get { return _std; }
            set { _std = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 正限位位置
        /// </summary>
        private int _PositiveLimitPosition;
        /// <summary>
        /// 负限位位置
        /// </summary>
        private int _NegativeLimitPosition;
        private MotorTestMessage _TestMessage;

        #region ------------Property------------
        private ObservableCollection<MotorTypeMessage> _motorTypeMessage;
        /// <summary>
        /// 电机类型信息
        /// </summary>
        public ObservableCollection<MotorTypeMessage> MotorTypeMessage
        {
            get { return _motorTypeMessage; }
            set { _motorTypeMessage = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<MotorTestMessage> _motorTestMessages;
        /// <summary>
        /// 电机信息
        /// </summary>
        public ObservableCollection<MotorTestMessage> MotorTestMessages
        {
            get { return _motorTestMessages; }
            set { _motorTestMessages = value; RaisePropertyChanged(); }
        }
        private int _xPointPos;
        public int XPointPos
        {
            get { return _xPointPos; }
            set { _xPointPos = value; RaisePropertyChanged(); }
        }
        private int _yPointPos;
        public int YPointPos
        {
            get { return _yPointPos; }
            set { _yPointPos = value; RaisePropertyChanged(); }
        }
        private double _xPointSpeed;
        public double XPointSpeed
        {
            get { return _xPointSpeed; }
            set { _xPointSpeed = value; RaisePropertyChanged(); }
        }
        private double _yPointSpeed;
        public double YPointSpeed
        {
            get { return _yPointSpeed; }
            set { _yPointSpeed = value; RaisePropertyChanged(); }
        }


        /*
        private List<int> _xPos;

        public List<int> XPos
        {
            get { return _xPos; }
            set { _xPos = value; RaisePropertyChanged(); }
        }
        private List<int> _yPos;

        public List<int> YPos
        {
            get { return _yPos; }
            set { _yPos = value; RaisePropertyChanged(); }
        }

        private List<double> _xSpeeds;
        /// <summary>
        /// x轴速度
        /// </summary>
        public List<double> XSpeeds
        {
            get { return _xSpeeds; }
            set { _xSpeeds = value; RaisePropertyChanged(); }
        }

        private List<double> _ySpeeds;
        /// <summary>
        /// y轴速度
        /// </summary>
        public List<double> YSpeeds
        {
            get { return _ySpeeds; }
            set { _ySpeeds = value; RaisePropertyChanged(); }
        }
        */
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
        public PlotModel MotorSpeedplotModel
        {
            get { return _motorSpeedplotModel; }
            set { _motorSpeedplotModel = value; RaisePropertyChanged(); }
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
        private MotorEntity _motorEntity;
        public MotorEntity MotorEntity
        {
            get { return _motorEntity; }
            set { _motorEntity = value; RaisePropertyChanged(); }
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

        private MotorModel _motorModelX;
        public MotorModel MotorModelX
        {
            get { return _motorModelX; }
            set { _motorModelX = value; RaisePropertyChanged(); }
        }
        private MotorModel _motorModelY;
        public MotorModel MotorModelY
        {
            get { return _motorModelY; }
            set { _motorModelY = value; RaisePropertyChanged(); }
        }
        #endregion

        private EnumMotorOperatingState _operatingState;
        /// <summary>
        /// 电机的运行状态
        /// </summary>
        public EnumMotorOperatingState OperatingState
        {
            get { return _operatingState; }
            set
            {
                _operatingState = value;
                RaisePropertyChanged();
            }
        }
        private bool _isTestMotorMove = false;
        /// <summary>
        /// 是否处于电机位置移动检测中
        /// </summary>
        public bool IsTestMotorMove
        {
            get { return _isTestMotorMove; }
            set
            { _isTestMotorMove = value; RaisePropertyChanged(); }
        }

        private bool _isEncoderDirectionTest = false;
        /// <summary>
        /// 是否处于编码器检测中
        /// </summary>
        public bool IsEncoderDirectionTest
        {
            get { return _isEncoderDirectionTest; }
            set
            { _isEncoderDirectionTest = value; RaisePropertyChanged(); }
        }

        private bool _isLimitSwitchTest = false;
        /// <summary>
        /// 是否处于限位检测中
        /// </summary>
        public bool IsLimitSwitchTest
        {
            get { return _isLimitSwitchTest; }
            set
            { _isLimitSwitchTest = value; RaisePropertyChanged(); }
        }

        private bool _isFullTravelTest = false;
        /// <summary>
        /// 是否处于满行程检测中
        /// </summary>
        public bool IsFullTravelTest
        {
            get { return _isFullTravelTest; }
            set
            { _isFullTravelTest = value; RaisePropertyChanged(); }
        }
        private ObservableCollection<string> _log;
        public ObservableCollection<string> Log
        {
            get { return _log; }
            set { _log = value; RaisePropertyChanged(); }
        }
        private int _loadSize = 100000;
        /// <summary>
        /// 需要加载的点数
        /// </summary>
        public int LoadSize
        {
            get { return _loadSize; }
            set { _loadSize = value; RaisePropertyChanged(); }
        }
        private int headIndex = 0;
        /// <summary>
        /// 目前加载数据的数据库中的第一个索引ID
        /// </summary>
        public int HeadIndex
        {
            get { return headIndex; }
            set { headIndex = value; RaisePropertyChanged(); }
        }
        private int _totalSize;
        /// <summary>
        /// 总数
        /// </summary>
        public int TotalSize
        {
            get { return _totalSize; }
            set { _totalSize = value; RaisePropertyChanged(); }
        }
      
      

        private async Task Init()
        {
        
            MotorModelX = new MotorModel(EnumMotorModel.MOTOR_x);
            MotorModelY = new MotorModel(EnumMotorModel.MOTOR_y);
            MotorplotModel = new PlotModel();
            MotorSpeedplotModel = new PlotModel();
           
            using (var AxisDbContext = new TwoAxisDbContextBase())
            {
                TotalSize = await SpliteOperate.GetPlotViewPointMessageCountAsync(AxisDbContext);
            }
            MotorEntity = new MotorEntity(_importantByteQueue, _byteQueue) { };
            MotorTestMessages = new ObservableCollection<MotorTestMessage>();
            MotorTypeMessage = new ObservableCollection<MotorTypeMessage>
            {
                { new MotorTypeMessage() { Name = "Zem15C" ,MinStrokeRange = 120000,MaxStrokeRange=130000} },
                { new MotorTypeMessage() { Name = "Zem18" ,MinStrokeRange = 120000,MaxStrokeRange=130000} },
                { new MotorTypeMessage() { Name = "Zem20" ,MinStrokeRange = 120000,MaxStrokeRange=130000} }
            };
            _TestMessage = new MotorTestMessage();

            MotorTestMessages.Add(_TestMessage);

            TestAllCommand = new DelegateCommand(TestAll);
            MoveCommand = new DelegateCommand<string>(Move);
            ClearLogCommand = new DelegateCommand(ClearLog);
            GetMotorStatusCommand = new DelegateCommand(GetMotorStatus);
            AutoAdjustCommand = new DelegateCommand<string>(AutoAdjust);
            ClearMonitorCommand = new DelegateCommand<string>(ClearMonitor);
            AutomaticZeroInitializationCommand = new DelegateCommand(AutomaticZeroInitialization);
            SetMotorLimitEnableCommand = new DelegateCommand<string>(SetMotorLimitEnable);
            GetMotorLimitEnableCommand = new DelegateCommand(GetMotorLimitEnable);
            TestPerformanceCommand = new DelegateCommand(TestPerformance);
            NoPerformanceCommand = new DelegateCommand(NoPerformance);
            SaveToFileCommand = new DelegateCommand<string>(SaveToFile);
            TestSmoothnessDetectionCommand = new DelegateCommand<string>(TestSmoothnessDetection);
            SerilizeCommand = new DelegateCommand(Serilize);
            DeserilizeCommand = new DelegateCommand(Deserilize);
            ShowThreeAzisTestModelViewModelCommand = new DelegateCommand(ShowThreeAzisTestModelViewModel);
            ShutDownDurabilityTestCommand = new DelegateCommand(ShutDownDurabilityTest);
            DurabilityTestgCommand = new DelegateCommand(DurabilityTest);
            DataPathSelectCommand = new DelegateCommand(DataPathSelect);
            AllDeserilizeCommand = new DelegateCommand(AllDeserilize);
            SelectPreviousCommand = new DelegateCommand(SelectPrevious);
            SelectNextCommand = new DelegateCommand(SelectNext);
            SqliteLoadDelegateCommand = new DelegateCommand(SqliteLoad);
            Log = new ObservableCollection<string>();

            _stopwatch = new Stopwatch();

            MotorSpeedplotModel.Legends.Add(new Legend());
            MotorSpeedplotModel.Axes.Add(new DateTimeAxis() { Title = "时间", Position = AxisPosition.Bottom });
            MotorSpeedplotModel.Axes.Add(new LinearAxis() { Title = "速度(脉冲/秒)", Position = AxisPosition.Left });
            MotorplotModel.Legends.Add(new Legend());
            MotorplotModel.Axes.Add(new DateTimeAxis() { Title = "时间", Position = AxisPosition.Bottom });
            MotorplotModel.Axes.Add(new LinearAxis() { Title = "位置(脉冲)", Position = AxisPosition.Left });
            _xSpeedPosLine = new LineSeries() { Title = "x轴", RenderInLegend = true };
            _ySpeedPosLine = new LineSeries() { Title = "y轴", RenderInLegend = true };
            _xPosLine = new LineSeries() { Title = "x轴", RenderInLegend = true };
            _yPosLine = new LineSeries() { Title = "y轴", RenderInLegend = true };
            MotorSpeedplotModel.Series.Add(_xSpeedPosLine);
            MotorSpeedplotModel.Series.Add(_ySpeedPosLine);
            MotorplotModel.Series.Add(_xPosLine);
            MotorplotModel.Series.Add(_yPosLine);


  
            _xPosLine.ItemsSource = MotorModelX.PointList;
            _xPosLine.DataFieldX = "Date";
            _xPosLine.DataFieldY = "Point";

 
            _xSpeedPosLine.ItemsSource = MotorModelX.SpeedList;
            _xSpeedPosLine.DataFieldX = "SpeedDate";
            _xSpeedPosLine.DataFieldY = "Speed";

    
            _yPosLine.ItemsSource = MotorModelY.PointList;
            _yPosLine.DataFieldX = "Date";
            _yPosLine.DataFieldY = "Point";

      
            _ySpeedPosLine.ItemsSource = MotorModelY.SpeedList;
            _ySpeedPosLine.DataFieldX = "SpeedDate";
            _ySpeedPosLine.DataFieldY = "Speed";
        }
        public DelegateCommand ClearLogCommand { get; set; }
        private void ClearLog()
        {
            if (Log != null)
            {
                Log.Clear();
            }
        }
        public DelegateCommand<string> MoveCommand { get; set; }
        private void Move(string direction)
        {


            switch (direction)
            {
                case "up":; Goto(EnumMotorId.MOTOR_2, true); break;
                case "down": Goto(EnumMotorId.MOTOR_2, false); break;
                case "left": Goto(EnumMotorId.MOTOR_1, false); break;
                case "right": Goto(EnumMotorId.MOTOR_1, true); break;
                case "stop": StopAll(); break;
            }
        }
        private void StopAll()
        {
            MotorEntity.SetMotorOperatingStatusCommand(EnumMotorId.MOTOR_1, EnumMotorOperatingState.Stop);
            MotorEntity.SetMotorOperatingStatusCommand(EnumMotorId.MOTOR_2, EnumMotorOperatingState.Stop);
        }
        public DelegateCommand TestPerformanceCommand { get; set; }
        private void TestPerformance()
        {
            _isPerformance = true;
            Task.Run(async () =>
            {
                while (_isPerformance)
                {
                    if (_byteQueue.IsEmpty)
                    {
                        GetMotorStatus();
                    }
                    await Task.Delay(10);

                }
            });

        }
        public DelegateCommand NoPerformanceCommand { get; set; }
        private void NoPerformance()
        {
            _isPerformance = false; ;

        }
        public DelegateCommand<string> TestSmoothnessDetectionCommand { get; set; }
        private async void TestSmoothnessDetection(string parameter)
        {
            if (parameter == "open")
            {

                _work = new BackgroundWorker();
                _work.WorkerSupportsCancellation = true;
                _work.DoWork += Worker_DoWork;
                _work?.RunWorkerAsync();
                if (_testTime == null)
                {
                    _testTime = new Stopwatch();
                }
                _testTime.Start();
                _cts = new CancellationTokenSource();
                try
                {
                    for (int i = 0; i < 60; i++)
                    {

                        bool result1 = await SmoothnessDetection(EnumMotorId.MOTOR_1, _cts.Token);
                        bool result2 = await SmoothnessDetection(EnumMotorId.MOTOR_2, _cts.Token);
                        if (result1 == false || result2 == false)
                        {
                            _dialogHostService.Information("提示", "丝杆顺滑度测试错误");
                            return;
                        }

                        if (_testTime.ElapsedMilliseconds / 1000.0 / 60 > 60)
                        {
                            break;
                        }
                    }
                    _dialogHostService.Information("提示", "丝杆顺滑度测试完毕请选择保存路径");
                    Serilize();
                    _work.CancelAsync();
                    _testTime.Stop();
                    _testTime = null;
                }
                catch (OperationCanceledException)
                {
                    DispathcherInvoke($"丝杆测试退出");
                    StopAll();
                    _work.CancelAsync();
                    if (_testTime != null)
                    {
                        _testTime.Stop();
                        _testTime = null;
                    }

                    return;
                }

            }
            else
            {
                if (_cts != null)
                {
                    _cts.Cancel();
                }
            }


        }
        public DelegateCommand DurabilityTestgCommand { get; set; }
        /// <summary>
        /// 耐久测试
        /// </summary>
        private async void DurabilityTest()
        {
            _isDurabilityTest = true;

            _work = new BackgroundWorker();
            _work.WorkerSupportsCancellation = true;
            _work.DoWork += Worker_DoWork;
            _work?.RunWorkerAsync();

            _cts = new CancellationTokenSource();
            while (_isDurabilityTest)
            {
                try
                {
                    bool result1 = await SmoothnessDetection(EnumMotorId.MOTOR_1, _cts.Token);
                    bool result2 = await SmoothnessDetection(EnumMotorId.MOTOR_2, _cts.Token);
                    var time = MotorModelX.PointList.Last().Date - MotorModelX.PointList[0].Date;
                    var min = time.TotalMinutes;
                    if (min > 30)
                    {
                        MotorModelX.PointList.Clear();
                        MotorModelY.PointList.Clear();

                    }
                }
                catch (OperationCanceledException)
                {
                    DispathcherInvoke($"耐久测试退出");
                    StopAll();
                    _work.CancelAsync();
                    return;
                }



            }
            _work.CancelAsync();

        }
        public DelegateCommand ShutDownDurabilityTestCommand { get; set; }
        private void ShutDownDurabilityTest()
        {
            _isDurabilityTest = false;
            if (_cts != null)
            {
                _cts.Cancel();
            }
        }
        public DelegateCommand TestAllCommand { get; set; }
        private async void TestAll()
        {
            MotorTestMessages.Clear();
            SetMotorInit(EnumMotorId.MOTOR_1);
            SetMotorInit(EnumMotorId.MOTOR_2);

            Goto(EnumMotorId.MOTOR_2, false);
            Goto(EnumMotorId.MOTOR_1, false);
            Thread.Sleep(4000);
            StopAll();
            var result = await TestMotorMove();
            if (result != true)
            {
                DispathcherInvoke($"电机移动测试失败，测试退出");
                _dialogHostService.Information("提示", "请先修复电机移动问题");

                return;
            }

            result = await EncoderDirectionTest();
            if (result != true)
            {
                _dialogHostService.Information("提示", "请先修复编码器反向问题");
                DispathcherInvoke($"电机编码器测试失败，测试退出");

                return;
            }
            _work = new BackgroundWorker();
            _work.WorkerSupportsCancellation = true;
            _work.DoWork += Worker_DoWork;
            _work?.RunWorkerAsync();

            result = await LimitSwitchTest(EnumMotorId.MOTOR_1);
            if (result != true)
            {
                DispathcherInvoke($"x轴电机限位测试失败，测试退出");
                _dialogHostService.Information("提示", "请先修复限位反向问题");
                _work.CancelAsync();
                _work = null;
                return;
            }
            result = await LimitSwitchTest(EnumMotorId.MOTOR_2);
            if (result != true)
            {
                DispathcherInvoke($"y轴电机限位测试失败，测试退出");
                _dialogHostService.Information("提示", "请先修复限位反向问题");
                _work.CancelAsync();
                _work = null;
                return;
            }
            result = await FullTravelTest(EnumMotorId.MOTOR_1);

            result = await LimitPositioningAccuracyDetection(EnumMotorId.MOTOR_1);

            result = await FullTravelTest(EnumMotorId.MOTOR_2);

            result = await LimitPositioningAccuracyDetection(EnumMotorId.MOTOR_2);



            //await SmoothnessDetection();

            _work.CancelAsync();
            _work = null;
            Serilize();

        }
        /// <summary>
        /// 电机位置移动检测
        /// </summary>
        private async Task<bool> TestMotorMove()
        {
            if (NetUdpService.IsOpen == false && SerialPortService.IsOpen == false)
            {
                return false;
            }
            MotorTestMessage TestMessage = new MotorTestMessage();
            MotorTestMessage TestMessage2 = new MotorTestMessage();

            await Task.Run(() =>
            {

                if (MotorModelX.PointList.Count <= 0 || MotorModelX.PointList.Count <= 0)
                {
                    DispathcherInvoke($"没有读取打电机位置，测试退出");
                    return;
                }
                var xposStart = MotorModelX.PointList[MotorModelX.PointList.Count - 1].Point;
                var yposStart = MotorModelY.PointList[MotorModelY.PointList.Count - 1].Point;
                Goto(EnumMotorId.MOTOR_1, true);
                Goto(EnumMotorId.MOTOR_2, true);
                Thread.Sleep(3000);
                StopAll();
                Thread.Sleep(3000);
                var xposEnd = MotorModelX.PointList[MotorModelX.PointList.Count - 1].Point;
                var yposEnd = MotorModelY.PointList[MotorModelY.PointList.Count - 1].Point;
                if (Math.Abs(yposEnd - yposStart) <= 50)
                {
                    TestMessage.TestProject = "Y轴电机控制测试";
                    TestMessage.TestResult = "不合格";
                    TestMessage.TestValue = "不正常";
                    TestMessage.StandardValue = "正常";
                    TestMessage.Description = "电机响应不正常";
                    DispathcherInvoke($"电机测试yEnd:{yposEnd},yStart:{yposStart} 行程:{yposEnd - yposStart}");
                }
                else
                {
                    TestMessage.TestProject = "Y轴电机控制测试";
                    TestMessage.TestResult = "合格";
                    TestMessage.TestValue = "正常";
                    TestMessage.StandardValue = "正常";
                    TestMessage.Description = "电机响应正常";
                    DispathcherInvoke($"电机测试yEnd:{yposEnd},yStart:{yposStart} 行程:{yposEnd - yposStart}");
                }
                if (Math.Abs(xposEnd - xposStart) <= 50)
                {

                    TestMessage2.TestProject = "X轴电机控制测试";
                    TestMessage2.TestResult = "不合格";
                    TestMessage2.TestValue = "不正常";
                    TestMessage2.StandardValue = "正常";
                    TestMessage2.Description = "电机响应不正常";
                    DispathcherInvoke($"电机测试xEnd:{xposEnd},xStart:{xposStart} 行程:{xposEnd - xposStart}");
                }
                else
                {
                    TestMessage2.TestProject = "X轴电机控制测试";
                    TestMessage2.TestResult = "合格";
                    TestMessage2.TestValue = "正常";
                    TestMessage2.StandardValue = "正常";
                    TestMessage2.Description = "电机响应正常";
                    DispathcherInvoke($"电机测试xEnd:{xposEnd},xStart:{xposStart} 行程:{xposEnd - xposStart}");
                }
            });
            MotorTestMessages.Add(TestMessage);
            MotorTestMessages.Add(TestMessage2);
            if (TestMessage.TestResult == "合格" && TestMessage2.TestResult == "合格")
            {
                return true;
            }
            else
            {
                return false;
            }

        }



        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_getMotorStateTimer.Enabled)
            {
                _getMotorStateTimer.Stop();
            }

            while (_work != null && _work.CancellationPending != true)
            {
                if (_byteQueue.IsEmpty)
                {
                    GetMotorStatus();
                }
                Thread.Sleep(10);


            }
            if (!_getMotorStateTimer.Enabled)
            {
                _getMotorStateTimer.Start();
            }
            e.Cancel = true;

        }


        /// <summary>
        /// 编码器测试
        /// </summary>
        private async Task<bool> EncoderDirectionTest()
        {
            if (NetUdpService.IsOpen == false && SerialPortService.IsOpen == false)
            {
                return false;
            }
            MotorTestMessage TestMessage = new MotorTestMessage();
            MotorTestMessage TestMessage2 = new MotorTestMessage();
            SetMotorInit(EnumMotorId.MOTOR_1);
            SetMotorInit(EnumMotorId.MOTOR_2);


            await Task.Run(() =>
            {
                var xposStart = MotorModelX.PointList[MotorModelX.PointList.Count - 1].Point;
                var yposStart = MotorModelY.PointList[MotorModelY.PointList.Count - 1].Point;
                Goto(EnumMotorId.MOTOR_1, true);
                Goto(EnumMotorId.MOTOR_2, true);
                Thread.Sleep(2000);
                StopAll();
                Thread.Sleep(2000);
                var xposEnd = MotorModelX.PointList[MotorModelX.PointList.Count - 1].Point;
                var yposEnd = MotorModelY.PointList[MotorModelY.PointList.Count - 1].Point;
                if (yposEnd - yposStart < 50)
                {
                    TestMessage.TestProject = "Y轴编码器测试";
                    TestMessage.TestResult = "不合格";
                    TestMessage.TestValue = "不正常";
                    TestMessage.StandardValue = "正常";
                    TestMessage.Description = "编码器反向";
                    DispathcherInvoke($"电机测试yEnd:{yposEnd},yStart:{yposStart} 行程:{yposEnd - yposStart}");
                }
                else
                {
                    TestMessage.TestProject = "Y轴编码器测试";
                    TestMessage.TestResult = "合格";
                    TestMessage.TestValue = "正常";
                    TestMessage.StandardValue = "正常";
                    TestMessage.Description = "编码器正常";
                    DispathcherInvoke($"电机测试yEnd:{yposEnd},yStart:{yposStart} 行程:{yposEnd - yposStart}");
                }
                if (xposEnd - xposStart < 50)
                {
                    TestMessage2.TestProject = "X轴编码器测试";
                    TestMessage2.TestResult = "不合格";
                    TestMessage2.TestValue = "不正常";
                    TestMessage2.StandardValue = "正常";
                    TestMessage2.Description = "编码器反向";
                    DispathcherInvoke($"电机测试xEnd:{xposEnd},xStart:{xposStart} 行程:{xposEnd - xposStart}");
                }
                else
                {
                    TestMessage2.TestProject = "X轴编码器测试";
                    TestMessage2.TestResult = "合格";
                    TestMessage2.TestValue = "正常";
                    TestMessage2.StandardValue = "正常";
                    TestMessage2.Description = "编码器正常";
                    DispathcherInvoke($"电机测试xEnd:{xposEnd},xStart:{xposStart} 行程:{xposEnd - xposStart}");
                }
            });
            MotorTestMessages.Add(TestMessage);
            MotorTestMessages.Add(TestMessage2);
            if (TestMessage.TestResult == "合格" && TestMessage2.TestResult == "合格")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 限位测试
        /// </summary>
        private async Task<bool> LimitSwitchTest(EnumMotorId enumMotorId)
        {
            if (NetUdpService.IsOpen == false && SerialPortService.IsOpen == false)
            {
                return false;
            }
            MotorTestMessage TestMessage = new MotorTestMessage();
            MotorTestMessage TestMessage2 = new MotorTestMessage();
            if (enumMotorId == EnumMotorId.MOTOR_1)
            {
                TestMessage.TestProject = "X轴正限位测试";
                TestMessage2.TestProject = "X轴负限位测试";
            }
            else
            {
                TestMessage.TestProject = "Y轴限位测试";
                TestMessage2.TestProject = "Y轴负限位测试";
            }

            List<PlotViewPointMessage> pos;
            if (enumMotorId == EnumMotorId.MOTOR_1)
            {
                pos = MotorModelX.PointList;
            }
            else
            {
                pos = MotorModelY.PointList;
            }
            var posStart = pos[pos.Count - 1].Point;
            var posEnd = pos[pos.Count - 1].Point;
            await Task.Run(async () =>
            {

                Goto(enumMotorId, true);
                Thread.Sleep(5000);
                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();
                    //单开线程去测试堵转和限位
                    Task.Run(() => { MotorStallDetection(enumMotorId); });
                    string result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(60));
                    posEnd = pos[pos.Count - 1].Point;
                    if (result == "stall")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "电机堵转，请检查";
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                    }
                    else if (result == "PhyForwardLimited")
                    {
                        TestMessage.TestResult = "合格";
                        TestMessage.TestValue = "正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "正限位正常";
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                    }
                    else if (result == "PhyBackwardLimited")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "正限位反向，请检查";
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                        MotorEntity.SetMotorOperatingStatusCommand(enumMotorId, EnumMotorOperatingState.Stop);
                    }
                    else
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "正限位测试未知错误";
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                    }
                }
                catch (Exception ex)
                {
                    TestMessage.TestResult = "不合格";
                    TestMessage.TestValue = "不正常";
                    TestMessage.StandardValue = "正常";
                    TestMessage.Description = $"正限位测试触发异常{ex}";
                    DispathcherInvoke($"电机测试正限位测试引发异常:{ex}");
                }
            });
            posStart = posEnd;
            await Task.Run(async () =>
            {
                Goto(enumMotorId, false);
                Thread.Sleep(5000);
                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();
                    Task.Run(() => { MotorStallDetection(enumMotorId); });
                    string result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(60));
                    posEnd = pos[pos.Count - 1].Point;
                    if (result == "stall")
                    {
                        TestMessage2.TestResult = "不合格";
                        TestMessage2.TestValue = "不正常";
                        TestMessage2.StandardValue = "正常";
                        TestMessage2.Description = "电机堵转，请检查";
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                    }
                    else if (result == "PhyForwardLimited")
                    {
                        TestMessage2.TestResult = "不合格";
                        TestMessage2.TestValue = "不正常";
                        TestMessage2.StandardValue = "正常";
                        TestMessage2.Description = "负限位反向";
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                        MotorEntity.SetMotorOperatingStatusCommand(enumMotorId, EnumMotorOperatingState.Stop);
                    }
                    else if (result == "PhyBackwardLimited")
                    {
                        TestMessage2.TestResult = "合格";
                        TestMessage2.TestValue = "正常";
                        TestMessage2.StandardValue = "正常";
                        TestMessage2.Description = "负限位正常";
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                    }
                    else
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "负限位测试未知错误";

                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                    }
                }
                catch (Exception ex)
                {
                    TestMessage.TestResult = "不合格";
                    TestMessage.TestValue = "不正常";
                    TestMessage.StandardValue = "正常";
                    TestMessage.Description = "负限位未知错误";
                    DispathcherInvoke($"电机测试负限位测试引发异常:{ex}");
                }
            });
            MotorTestMessages.Add(TestMessage);
            MotorTestMessages.Add(TestMessage2);
            if (TestMessage.TestResult == "合格" && TestMessage2.TestResult == "合格")
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 满行程测试
        /// </summary>
        private async Task<bool> FullTravelTest(EnumMotorId enumMotorId)
        {
            if (NetUdpService.IsOpen == false && SerialPortService.IsOpen == false)
            {
                return false;
            }
            (int positiveLimitPosition, int negativeLimitPositionendPos) posMessage = (-1, 1);
            if (_limtedPos == null)
            {
                _limtedPos = new int[4];
            }
            MotorTestMessage TestMessage = new MotorTestMessage();
            MotorTestMessage TestMessage2 = new MotorTestMessage();
            MotorModel motorModel;
            if (enumMotorId == EnumMotorId.MOTOR_1)
            {
                TestMessage.TestProject = "X轴满行程测试";
                motorModel = MotorModelX;
            }
            else
            {
                TestMessage.TestProject = "Y轴满行程测试";
                motorModel = MotorModelY;
            }
            string result = "x";
            await Task.Run(async () =>
            {
                //单开线程去测试堵转和限位
                _limitedtcs1 = new TaskCompletionSource<string>();
                Goto(enumMotorId, true);
                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();
                    Thread.Sleep(5000);
                    Task.Run(() => { MotorStallDetection(enumMotorId); });
                    result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(50));
                    if (result == "stall")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "电机堵转，请检查";

                    }
                    else if (result == "PhyForwardLimited")
                    {

                        posMessage.positiveLimitPosition = motorModel.MotorParams.Pos;
                    }
                    else if (result == "PhyBackwardLimited")
                    {

                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "正限位反向";
                        MotorEntity.SetMotorOperatingStatusCommand(enumMotorId, EnumMotorOperatingState.Stop);
                    }
                    else
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = result;

                    }
                }
                catch (Exception ex)
                {
                    TestMessage.TestResult = "不合格";
                    TestMessage.TestValue = "不正常";
                    TestMessage.StandardValue = "正常";
                    TestMessage.Description = $"检测时间超时:{ex}";
                }
            });
            if (result != "PhyForwardLimited")
            {
                MotorTestMessages.Add(TestMessage);
                return false;
            }

            await Task.Run(async () =>
            {
                //单开线程去测试堵转和限位
                Goto(enumMotorId, false);
                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();
                    Thread.Sleep(5000);
                    Task.Run(() => { MotorStallDetection(enumMotorId); });
                    string result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(50));
                    if (result == "stall")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "电机堵转，请检查";
                    }
                    else if (result == "PhyForwardLimited")
                    {
                        TestMessage.Description = "负限位反向";
                        MotorEntity.SetMotorOperatingStatusCommand(enumMotorId, EnumMotorOperatingState.Stop);
                    }
                    else if (result == "PhyBackwardLimited")
                    {

                        posMessage.negativeLimitPositionendPos = motorModel.MotorParams.Pos;
                        int fullStrokeOfMotor = posMessage.positiveLimitPosition - posMessage.negativeLimitPositionendPos;
                        if ((fullStrokeOfMotor < 170000 && fullStrokeOfMotor > 160000)
                        || (fullStrokeOfMotor < 130000 && fullStrokeOfMotor > 120000)
                        || (fullStrokeOfMotor < 240000 && fullStrokeOfMotor > 210000))
                        {
                            TestMessage.TestResult = "合格";
                            TestMessage.TestValue = "正常";
                            TestMessage.StandardValue = "正常";
                            _limtedPos[0] = posMessage.positiveLimitPosition;
                            _limtedPos[1] = posMessage.negativeLimitPositionendPos;
                            TestMessage.Description = $"电机行程正常,正限位位置：{posMessage.positiveLimitPosition} 负限位位置：{posMessage.negativeLimitPositionendPos} 行程为：{posMessage.positiveLimitPosition - posMessage.negativeLimitPositionendPos}脉冲";
                        }
                        else
                        {
                            TestMessage.TestResult = "不合格";
                            TestMessage.TestValue = "不正常";
                            TestMessage.StandardValue = "正常";
                            TestMessage.Description = $"电机行程异常:{posMessage.positiveLimitPosition - posMessage.negativeLimitPositionendPos}脉冲";
                        }
                    }
                }
                catch (Exception ex)
                {
                    TestMessage.TestResult = "不合格";
                    TestMessage.TestValue = "不正常";
                    TestMessage.StandardValue = "正常";
                    TestMessage.Description = $"检测时间超时:{ex}";
                }

            });
            MotorTestMessages.Add(TestMessage);
            if (TestMessage.TestResult == "合格")
            {
                return true;
            }
            else
            {
                return false;
            }


        }
        /// <summary>
        /// 定位精度检测
        /// </summary>
        private async Task<bool> LimitPositioningAccuracyDetection(EnumMotorId enumMotorId)
        {
            if (NetUdpService.IsOpen == false && SerialPortService.IsOpen == false)
            {
                return false;
            }
            (int positiveLimitPosition, int negativeLimitPositionendPos) posMessage = (-1, 1);
            MotorTestMessage TestMessage = new MotorTestMessage();
            MotorTestMessage TestMessage2 = new MotorTestMessage();
            MotorModel motorModel;
            if (enumMotorId == EnumMotorId.MOTOR_1)
            {
                TestMessage.TestProject = "X轴定位精度测试";
                motorModel = MotorModelX;
            }
            else
            {
                TestMessage.TestProject = "Y轴定位精度测试";
                motorModel = MotorModelY;
            }
            string result = "x";
            await Task.Run(async () =>
            {
                //单开线程去测试堵转和限位
                _limitedtcs1 = new TaskCompletionSource<string>();
                Goto(enumMotorId, true);
                Thread.Sleep(5000);

                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();

                    Task.Run(() => { MotorStallDetection(enumMotorId); });
                    result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(50));
                    if (result == "stall")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "电机堵转，请检查";

                    }
                    else if (result == "PhyForwardLimited")
                    {

                        posMessage.positiveLimitPosition = motorModel.MotorParams.Pos;
                    }
                    else if (result == "PhyBackwardLimited")
                    {

                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "正限位反向";
                        MotorEntity.SetMotorOperatingStatusCommand(enumMotorId, EnumMotorOperatingState.Stop);
                    }
                    else
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = result;

                    }
                }
                catch (Exception ex)
                {
                    TestMessage.TestResult = "不合格";
                    TestMessage.TestValue = "不正常";
                    TestMessage.StandardValue = "正常";
                    TestMessage.Description = $"检测时间超时:{ex}";
                }
            });
            if (result != "PhyForwardLimited")
            {
                MotorTestMessages.Add(TestMessage);
                return false;
            }

            await Task.Run(async () =>
            {
                //单开线程去测试堵转和限位
                Goto(enumMotorId, false);
                Thread.Sleep(5000);
                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();
                    Task.Run(() => { MotorStallDetection(enumMotorId); });
                    string result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(50));
                    if (result == "stall")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "电机堵转，请检查";
                    }
                    else if (result == "PhyForwardLimited")
                    {
                        TestMessage.Description = "负限位反向";
                        MotorEntity.SetMotorOperatingStatusCommand(enumMotorId, EnumMotorOperatingState.Stop);

                    }
                    else if (result == "PhyBackwardLimited")
                    {

                        posMessage.negativeLimitPositionendPos = motorModel.MotorParams.Pos;
                        int fullStrokeOfMotor = posMessage.positiveLimitPosition - posMessage.negativeLimitPositionendPos;
                        if ((fullStrokeOfMotor < 170000 && fullStrokeOfMotor > 160000)
                        || (fullStrokeOfMotor < 130000 && fullStrokeOfMotor > 120000)
                         || (fullStrokeOfMotor < 240000 && fullStrokeOfMotor > 210000))
                        {
                            if (_limtedPos == null)
                            {
                                TestMessage.TestResult = "不合格";
                                TestMessage.TestValue = "不正常";
                                TestMessage.StandardValue = "正常";
                                TestMessage.Description = $"满行程测试错误，电机精度无法计算";
                                return;
                            }
                            else
                            {
                                _limtedPos[2] = posMessage.positiveLimitPosition;
                                _limtedPos[3] = posMessage.negativeLimitPositionendPos;
                                if (Math.Abs(_limtedPos[2] - _limtedPos[0]) < 100 && Math.Abs(_limtedPos[3] - _limtedPos[1]) < 100)
                                {
                                    TestMessage.TestResult = "合格";
                                    TestMessage.TestValue = "正常";
                                    TestMessage.StandardValue = "正常";
                                    TestMessage.Description = $"正限位位置为{posMessage.positiveLimitPosition}，负限位位置是{posMessage.negativeLimitPositionendPos} 正限位误差为:{Math.Abs(_limtedPos[2] - _limtedPos[0])},负限位误差为{Math.Abs(_limtedPos[3] - _limtedPos[1])}";
                                }
                                else
                                {
                                    TestMessage.TestResult = "不合格";
                                    TestMessage.TestValue = "不正常";
                                    TestMessage.StandardValue = "正常";
                                    TestMessage.Description = $"正限位位置为{posMessage.positiveLimitPosition}，负限位位置是{posMessage.negativeLimitPositionendPos} 正限位误差为:{Math.Abs(_limtedPos[2] - _limtedPos[0])},负限位误差为{Math.Abs(_limtedPos[3] - _limtedPos[1])}";
                                }

                                return;
                            }

                        }
                        else
                        {
                            TestMessage.TestResult = "不合格";
                            TestMessage.TestValue = "不正常";
                            TestMessage.StandardValue = "正常";
                            TestMessage.Description = $"电机行程异常:{posMessage.positiveLimitPosition - posMessage.negativeLimitPositionendPos}脉冲";
                        }
                    }
                }
                catch (Exception ex)
                {
                    TestMessage.TestResult = "不合格";
                    TestMessage.TestValue = "不正常";
                    TestMessage.StandardValue = "正常";
                    TestMessage.Description = $"检测时间超时:{ex}";
                }

            });
            MotorTestMessages.Add(TestMessage);
            if (TestMessage.TestResult == "合格")
            {
                return true;
            }
            else
            {
                return false;
            }


        }
        /// <summary>
        /// 丝杆顺滑度检测
        /// </summary>
        /// 先让电机跑到右限位的位置，记录为初始位置 
        /// 再跑到左限位，记录中间点数
        /// 再跑到右限位，记录中间点数 计算x轴的丝杆顺滑度

        private async Task<bool> SmoothnessDetection(EnumMotorId enumMotorId, CancellationToken cancellationToken = default)
        {
            int startIndex = 0, endIndex = 0;
            if (NetUdpService.IsOpen == false && SerialPortService.IsOpen == false)
            {
                return false;
            }
            (int positiveLimitPosition, int negativeLimitPositionendPos) posMessage = (-1, 1);
            MotorTestMessage TestMessage = new MotorTestMessage();
            MotorTestMessage TestMessage2 = new MotorTestMessage();
            MotorModel motorModel;
            if (enumMotorId == EnumMotorId.MOTOR_1)
            {
                TestMessage.TestProject = "X轴丝杆顺滑度检测";
                motorModel = MotorModelX;
            }
            else
            {
                TestMessage.TestProject = "Y轴丝杆顺滑度测试";
                motorModel = MotorModelY;
             
            }
            string result = "";
            await Task.Run(async () =>
            {
                //单开线程去测试堵转和限位
                _limitedtcs1 = new TaskCompletionSource<string>();
                GotoInSpeedMode(enumMotorId, 10000);
                // Goto(enumMotorId,true);
                await Task.Delay(5000, cancellationToken);

                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();

                    Task.Run(() => MotorStallDetection(enumMotorId), cancellationToken);
                    result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(50), cancellationToken);
                    if (result == "stall")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "电机堵转，请检查";
                        DispathcherInvoke($"电机测试丝杆测试堵转");

                    }
                    else if (result == "PhyForwardLimited" || result == "SPLimted")
                    {



                    }
                    else if (result == "PhyBackwardLimited" || result == "SNLimted")
                    {

                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "正限位反向";
                        MotorEntity.SetMotorOperatingStatusCommand(enumMotorId, EnumMotorOperatingState.Stop);
                        DispathcherInvoke($"电机测试丝杆正限位反向{result}");
                    }
                    else
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = result;
                        DispathcherInvoke($"电机测试丝杆异常错误");
                    }
                }
                catch (Exception ex)
                {
                    TestMessage.TestResult = "不合格";
                    TestMessage.TestValue = "不正常";
                    TestMessage.StandardValue = "正常";
                    TestMessage.Description = $"请检测网络连接";
                    DispathcherInvoke($"电机测试丝杆异常错误{ex}");
                }
            }, cancellationToken);
            if (!(result == "PhyForwardLimited" || result == "SPLimted"))
            {

                return false;
            }

            await Task.Run(async () =>
            {
                //单开线程去测试堵转和限位
                GotoInSpeedMode(enumMotorId, -10000);
                //Goto(enumMotorId, false);
                startIndex = motorModel.SpeedList.Count;
                Thread.Sleep(5000);
                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();
                    Task.Run(() => MotorStallDetection(enumMotorId), cancellationToken);
                    string result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(50), cancellationToken);
                    if (result == "stall")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "电机堵转，请检查";
                        DispathcherInvoke($"电机测试丝杆堵转");
                    }
                    else if (result == "PhyForwardLimited" || result == "SPLimted")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "负限位反向";
                        MotorEntity.SetMotorOperatingStatusCommand(enumMotorId, EnumMotorOperatingState.Stop);
                        DispathcherInvoke($"电机测试负限位反向{result}");
                    }
                    else if (result == "PhyBackwardLimited" || result == "SNLimted")
                    {
                        //正常的情况下，需要记录前面运行过程中的点数
                        endIndex = motorModel.SpeedList.Count;

                    }
                }
                catch (Exception ex)
                {
                    TestMessage.TestResult = "不合格";
                    TestMessage.TestValue = "不正常";
                    TestMessage.StandardValue = "正常";
                    TestMessage.Description = $"检测时间超时:{ex}";
                    DispathcherInvoke($"电机测试错误{ex}");
                }

            }, cancellationToken);
            if (result == "PhyBackwardLimited" || result == "SNLimted")
            {
                TestMessage.Description = $"从正方向到负方向的位置丝杆顺滑度错误";

            }
            if (cancellationToken.IsCancellationRequested)
            {
                MotorEntity.SetMotorOperatingStatusCommand(enumMotorId, EnumMotorOperatingState.Stop);
                cancellationToken.ThrowIfCancellationRequested();
            }

            return true;

        }
        /// <summary>
        /// 丝杆顺滑度检测
        /// </summary>
        private bool SmoothnessTest(int start, int end, List<PlotViewSpeedMessage> speedList)
        {/*
            double[] data = new double[end - start];
            if (speedList.Count < end )
            {
                return false;
            }
            if (end - start < 10)
            {
                return false;
            }
            int startIndex = 0,endIndex = 0;
            //找头
            for (int i = 0; i< data.Length;i++)
            {
                data[i] = speedList[i + start].Speed;
                if (i > 1)
                {
                    if (data[i] < data[i - 1])
                    {
                        startIndex = i-1;
                    }
                }
            }
            //找尾
            for (int j = data.Length - 2; j > 0; j--)
            {
                if (data[j] < data[j + 1])
                {
                    endIndex = j + 1;
                }
            }
            double[] uniformSpeedInterval = new double[endIndex - startIndex + 1];

            //总体标准差
            double popStdDev = data.PopulationStandardDeviation();
            Std = popStdDev;

            */
            return true;
        }
        /// <summary>
        /// 电机堵转和限位检测
        /// </summary>
        private async void MotorStallDetection(EnumMotorId enumMotorId)
        {
            int count = 0;
            int Movecount = 0;
            countNumber++;
            MotorModel motorModel;
            List<PlotViewPointMessage> pos; ;
            if (enumMotorId == EnumMotorId.MOTOR_1)
            {
                motorModel = MotorModelX;
                pos = MotorModelX.PointList;
            }
            else
            {
                motorModel = MotorModelY;
                pos = MotorModelY.PointList;
            }
            if (pos == null)
            {
                return;
            }
            var fistNowTime = DateTime.Now;

            while (true)
            {

                if (motorModel.MotorParams.LimitedState == EnumMotorLimitedState.PhyForwardLimited)
                {
                    _limitedtcs1.TrySetResult("PhyForwardLimited");
                    DispathcherInvoke($" {countNumber}:{enumMotorId}到达硬件正向限位，停止");
                    NLog.LogManager.GetCurrentClassLogger().Debug($"{countNumber}:{enumMotorId}到达硬件正向限位，停止");
                    return;
                }
                if (motorModel.MotorParams.LimitedState == EnumMotorLimitedState.PhyBackwardLimited)
                {
                    _limitedtcs1.TrySetResult("PhyBackwardLimited");
                    DispathcherInvoke($"{countNumber}:{enumMotorId}到达硬件负向限位，停止");
                    NLog.LogManager.GetCurrentClassLogger().Trace($"{countNumber}:{enumMotorId}到达硬件负向限位，停止");
                    return;
                }
                if (motorModel.MotorParams.SPLimted == true)
                {
                    _limitedtcs1.TrySetResult("SPLimted");
                    DispathcherInvoke($"{countNumber}:{enumMotorId}到达软件正限位，停止");
                    NLog.LogManager.GetCurrentClassLogger().Trace($"{countNumber}:{enumMotorId}到达软件正限位，停止");
                    return;
                }
                if (motorModel.MotorParams.SNLimted == true)
                {
                    _limitedtcs1.TrySetResult("SNLimted");
                    DispathcherInvoke($"{countNumber}:{enumMotorId}到达软件负向限位，停止");
                    NLog.LogManager.GetCurrentClassLogger().Trace($"{countNumber}:{enumMotorId}到达软件负向限位，停止");
                    return;
                }

                //if (motorModel.MotorParams.MoveState != EnumMotorMoveState.MotorMove)
                //{
                //    _runStateUpdate = new TaskCompletionSource<string>();
                //    try
                //    {
                //        string result = await _runStateUpdate.Task.WaitAsync(TimeSpan.FromSeconds(10));
                //        if (result != "move")
                //        {
                //            DispathcherInvoke($"{countNumber}电机10s未运行");
                //            NLog.LogManager.GetCurrentClassLogger().Info($"{countNumber}电机10s未运行");
                //        }
                //        _runStateUpdate = null;
                //    }
                //    catch (Exception ex)
                //    {
                //        DispathcherInvoke($"{ex}等待电机移动，超时");
                //        NLog.LogManager.GetCurrentClassLogger().Info($"{ex}等待电机移动，超时");
                //    }

                //}
                List<PlotViewPointMessage> list;

                lock (_lockobj)
                {
                    if (pos.Count > 2)
                    {
                        list = pos.ToList();
                    }
                    else
                    {
                        DispathcherInvoke($" ，堵转测试退出");
                        NLog.LogManager.GetCurrentClassLogger().Trace($"参数清空，堵转测试退出");
                        return;
                    }
                }


                var posAndTimeLast = list.Last();
                var posAndTimeSecondToLast = list[list.Count - 2];
                var nowTime = DateTime.Now;


                if (pos.Count > 2)
                {

                    if (Math.Abs(posAndTimeLast.Point - posAndTimeSecondToLast.Point) < 5 && posAndTimeLast.MotorMoveState == EnumMotorMoveState.MotorMove)
                    {
                        MotorEntity.SetMotorOperatingStatusCommand(enumMotorId, EnumMotorOperatingState.Stop);
                        _limitedtcs1.TrySetResult("stall");
                        DispathcherInvoke($"{countNumber}:{enumMotorId}起始:{posAndTimeSecondToLast.Date.ToString("yyy-MM-dd HH:mm:ss.fffffff")}:{posAndTimeSecondToLast.Point}终止:{posAndTimeLast.Date.ToString("yyy-MM-dd HH:mm:ss.fffffff")}:{posAndTimeLast.Point}，. 电机堵转，退出");
                        NLog.LogManager.GetCurrentClassLogger().Error($"{countNumber}:{enumMotorId}起始:{posAndTimeSecondToLast.Date.ToString("yyy-MM-dd HH:mm:ss.fffffff")}:{posAndTimeSecondToLast.Point}终止:{posAndTimeLast.Date.ToString("yyy-MM-dd HH:mm:ss.fffffff")}:{posAndTimeLast.Point}，. 电机堵转，退出");
                        DispathcherInvoke($"{list.Count - 3}:{list[list.Count - 3].Date.ToString("yyy-MM-dd HH:mm:ss.fffffff")},{list[list.Count - 3].Point},{list[list.Count - 3].MotorMoveState}");
                        DispathcherInvoke($"{list.Count - 2}:{list[list.Count - 2].Date.ToString("yyy-MM-dd HH:mm:ss.fffffff")},{list[list.Count - 2].Point},{list[list.Count - 3].MotorMoveState}");
                        DispathcherInvoke($"{list.Count - 1}:{list[list.Count - 1].Date.ToString("yyy-MM-dd HH:mm:ss.fffffff")},{list[list.Count - 1].Point},{list[list.Count - 3].MotorMoveState}");
                        NLog.LogManager.GetCurrentClassLogger().Error($"{list.Count - 3}:{list[list.Count - 3].Date.ToString("yyy-MM-dd HH:mm:ss.fffffff")},{list[list.Count - 3].Point},{list[list.Count - 3].MotorMoveState}");
                        NLog.LogManager.GetCurrentClassLogger().Error($"{list.Count - 2}:{list[list.Count - 2].Date.ToString("yyy-MM-dd HH:mm:ss.fffffff")},{list[list.Count - 2].Point},{list[list.Count - 3].MotorMoveState}");
                        NLog.LogManager.GetCurrentClassLogger().Error($"{list.Count - 1}:{list[list.Count - 1].Date.ToString("yyy-MM-dd HH:mm:ss.fffffff")},{list[list.Count - 1].Point},{list[list.Count - 3].MotorMoveState}");
                        return;
                    }
                }

                int waitChangeCount = 0;
                while (list.Count == pos.Count)
                {
                    int timeCount = 1;
                    var time = DateTime.Now;
                    if ((time - nowTime).Milliseconds > 3000 * timeCount)
                    {
                        DispathcherInvoke($"当前时间{time.ToString("HH:mm:ss.fff")},上一次添加点的时间{pos.Last().Date.ToString("HH: mm:ss.fff")}:通讯异常，{timeCount * 3}秒内没有回报");
                        NLog.LogManager.GetCurrentClassLogger().Error($"当前时间{time.ToString("HH:mm:ss.fff")},上一帧率时间{pos.Last().Date.ToString("HH: mm:ss.fff")}:通讯异常，{timeCount * 3}内没有回报出");
                        timeCount++;
                    }

                    Thread.Sleep(50);
                }

                count++;

                var differTime = nowTime - fistNowTime;
                if (differTime.Seconds > 70)
                {
                    return;
                }


            }
            DispathcherInvoke($"测试堵转功能退出");
            NLog.LogManager.GetCurrentClassLogger().Error($"测试堵转功能退出");
        }




        public DelegateCommand AutomaticZeroInitializationCommand { get; set; }
        private async void AutomaticZeroInitialization()
        {
            var resultx = await FindLinted(EnumMotorId.MOTOR_1);
            if (resultx == (0, 0))
            {
                return;
            }
            var resulty = await FindLinted(EnumMotorId.MOTOR_2);
            if (resulty == (0, 0))
            {
                return;
            }
            int x = (resultx.Item1 - resultx.Item2) / 2 + resultx.Item2;
            int y = (resulty.Item1 - resulty.Item2) / 2 + resulty.Item2;
            Goto(EnumMotorId.MOTOR_1, x);
            Goto(EnumMotorId.MOTOR_2, y);
            Thread.Sleep(5000);
            await Task.Run(() =>
            {
                int i = 0;
                while (true)
                {
                    if (MotorModelX.MotorParams.MoveState == EnumMotorMoveState.MotorStop && MotorModelY.MotorParams.MoveState == EnumMotorMoveState.MotorStop)
                    {
                        MotorEntity.SetMotorZeroCommand(EnumMotorId.MOTOR_1);
                        MotorEntity.SetMotorZeroCommand(EnumMotorId.MOTOR_2);
                        DispathcherInvoke($"{EnumMotorId.MOTOR_1} :设置电机零点");
                        DispathcherInvoke($"{EnumMotorId.MOTOR_2} :设置电机零点");
                        return;
                    }
                    i++;
                    if (i > 50)
                    {
                        return;
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }
            });
        }

        /// <summary>
        /// 自动初始化零点
        /// </summary>
        private async Task<(int, int)> FindLinted(EnumMotorId enumMotorId)
        {
            (int x, int y) limtedPos = (0, 0);
            if (NetUdpService.IsOpen == false && SerialPortService.IsOpen == false)
            {
                return (0, 0);
            }
            MotorTestMessage TestMessage = new MotorTestMessage();
            MotorTestMessage TestMessage2 = new MotorTestMessage();
            if (enumMotorId == EnumMotorId.MOTOR_1)
            {
                TestMessage.TestProject = "X轴初始化零点";

            }
            else
            {
                TestMessage.TestProject = "Y轴初始化零点";

            }

            List<PlotViewPointMessage> pos;
            if (enumMotorId == EnumMotorId.MOTOR_1)
            {
                pos = MotorModelX.PointList;
            }
            else
            {
                pos = MotorModelY.PointList;
            }
            int posStart = pos[pos.Count - 1].Point;
            int posEnd = pos[pos.Count - 1].Point;
            await Task.Run(async () =>
            {

                Goto(enumMotorId, true);
                Thread.Sleep(5000);
                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();
                    //单开线程去测试堵转和限位
                    Task.Run(() => { MotorStallDetection(enumMotorId); });
                    string result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(60));
                    posEnd = pos[pos.Count - 1].Point;
                    if (result == "stall")
                    {
                        _dialogHostService.Information("提示", "电机堵转请检查");
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                        NLog.LogManager.GetCurrentClassLogger().Error($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                    }
                    else if (result == "PhyForwardLimited")
                    {


                        limtedPos.x = pos[pos.Count - 1].Point;
                        DispathcherInvoke($"电机测试正限位位置:{pos[pos.Count - 1]}");
                    }
                    else if (result == "PhyBackwardLimited")
                    {
                        _dialogHostService.Information("提示", "正限位反向请检查");
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                        NLog.LogManager.GetCurrentClassLogger().Error($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                        MotorEntity.SetMotorOperatingStatusCommand(enumMotorId, EnumMotorOperatingState.Stop);
                    }
                    else
                    {
                        _dialogHostService.Information("提示", "错误");
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                    }
                }
                catch (Exception ex)
                {
                    _dialogHostService.Information("提示", $"错误{ex}");
                }
            });
            posStart = posEnd;
            await Task.Run(async () =>
            {
                Goto(enumMotorId, false);
                Thread.Sleep(1000);
                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();
                    Task.Run(() => { MotorStallDetection(enumMotorId); });
                    string result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(60));
                    posEnd = pos[pos.Count - 1].Point;
                    if (result == "stall")
                    {
                        _dialogHostService.Information("提示", "电机堵转请检查");
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                    }
                    else if (result == "PhyForwardLimited")
                    {
                        _dialogHostService.Information("提示", "负限位反向请检查");
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                        MotorEntity.SetMotorOperatingStatusCommand(enumMotorId, EnumMotorOperatingState.Stop);
                    }
                    else if (result == "PhyBackwardLimited")
                    {

                        DispathcherInvoke($"电机测试负限位位置:{pos[pos.Count - 1]}");
                        limtedPos.y = pos[pos.Count - 1].Point;
                    }
                    else
                    {
                        _dialogHostService.Information("提示", "错误");

                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                    }
                }
                catch (Exception ex)
                {
                    _dialogHostService.Information("提示", "错误" + ex);
                    DispathcherInvoke($"电机测试负限位测试引发异常:{ex}");
                }
            });
            return limtedPos;
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
                    lock (_lockobj)
                    {
                        MotorModelX.PointList.Clear();
                        MotorModelY.PointList.Clear();
                    }

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
                    MotorModelX.SpeedList.Clear();
                    MotorModelY.SpeedList.Clear();
                }
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
                   
                }));
            }
            else
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    exporter.ExportToFile(MotorSpeedplotModel, $"{path}\\参数曲线_{timeTip}.png");
                  
                }));
            }

        }
        private void RegisterTimer()
        {
            _getMotorStateTimer = new System.Timers.Timer(1000);
            _getMotorStateTimer.AutoReset = true;
            _getMotorStateTimer.Elapsed += QueryMotorStatusTimerElapsed;
        }

        /// <summary>
        /// 定时器事件，问询电机状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QueryMotorStatusTimerElapsed(object sender, ElapsedEventArgs e)
        {
            GetMotorStatus();

        }
        public DelegateCommand GetMotorStatusCommand { get; set; }
        private void GetMotorStatus()
        {
            MotorEntity.GetMotorStatusCommand(EnumMotorId.MOTOR_1);
            MotorEntity.GetMotorStatusCommand(EnumMotorId.MOTOR_2);
        }



        /// <summary>
        /// 设置电机为闭环位置模式和使能
        /// </summary>
        /// <param name="MotorId">电机编号</param>
        private void SetMotorInit(EnumMotorId channel)
        {
            MotorModel motorModel = null;
            if (channel == EnumMotorId.MOTOR_1)
            {
                motorModel = MotorModelX;
            }
            else
            {
                motorModel = MotorModelY;
            }
            if (motorModel.MotorParams.CtrType != EnumMotorCtrType.CloseLoopPosCtr)
            {

                MotorEntity.SetMotorControlModeCommand(channel, EnumMotorCtrType.CloseLoopPosCtr);
                MotorEntity.SetMotorEnableCommand(channel, EnumMotorEnable.Enable);
            }
            if (motorModel.MotorParams.Enable != true)
            {
                MotorEntity.SetMotorEnableCommand(channel, EnumMotorEnable.Enable);
            }

        }
        /// <summary>
        /// 设置电机为开环速度模式和使能
        /// </summary>
        /// <param name="MotorId">电机编号</param>
        private void SetMotorSpeedInit(EnumMotorId channel)
        {
            if (channel == EnumMotorId.MOTOR_1)
            {
                if (MotorModelX.MotorParams.CtrType != EnumMotorCtrType.OpenLoopSpeedCtr)
                {
                    MotorEntity.SetMotorControlModeCommand(channel, EnumMotorCtrType.OpenLoopSpeedCtr);
                    MotorEntity.SetMotorEnableCommand(channel, EnumMotorEnable.Enable);
                }
                if (!MotorModelX.MotorParams.Enable)
                {
                   
                    MotorEntity.SetMotorEnableCommand(channel, EnumMotorEnable.Enable);
                }
            }
            else
            {
                if (MotorModelY.MotorParams.CtrType != EnumMotorCtrType.OpenLoopSpeedCtr)
                {
                    MotorEntity.SetMotorControlModeCommand(channel, EnumMotorCtrType.OpenLoopSpeedCtr);
                    MotorEntity.SetMotorEnableCommand(channel, EnumMotorEnable.Enable);
                }
                if (!MotorModelY.MotorParams.Enable)
                {
                    MotorEntity.SetMotorEnableCommand(channel, EnumMotorEnable.Enable);
                }
            }

        }
        /// <summary>
        /// 通过速度模式移动
        /// </summary>
        /// <param name="enumMotorId"></param>
        /// <param name="Speed"></param>
        private void GotoInSpeedMode(EnumMotorId enumMotorId, int Speed)
        {

            SetMotorSpeedInit(enumMotorId);
            MotorEntity.SetMotorGoToCommand(enumMotorId, EnumMotorUnit.Pulse, Speed);
        }


        /// <summary>
        /// 通过位置模式移动
        /// </summary>
        /// <param name="enumMotorId"></param>
        /// <param name="pos"></param>
        private void Goto(EnumMotorId enumMotorId, bool direction)
        {
            MotorModel motor;
            if (enumMotorId == EnumMotorId.MOTOR_1)
            {
                motor = MotorModelX;
            }
            else
            {
                motor = MotorModelY;
            }
            SetMotorInit(enumMotorId);
            if (direction)
            {
                MotorEntity.SetMotorGoToCommand(enumMotorId, EnumMotorUnit.Pulse, motor.MotorParams.Pos + 2000000);
            }
            else
            {
                MotorEntity.SetMotorGoToCommand(enumMotorId, EnumMotorUnit.Pulse, motor.MotorParams.Pos - 2000000);
            }


        }
        private void Goto(EnumMotorId enumMotorId, int pos)
        {
            MotorModel motor;
            if (enumMotorId == EnumMotorId.MOTOR_1)
            {
                motor = MotorModelX;
            }
            else
            {
                motor = MotorModelY;
            }
            SetMotorInit(enumMotorId);
            MotorEntity.SetMotorGoToCommand(enumMotorId, EnumMotorUnit.Pulse, pos);
        }
        public DelegateCommand<string> SetMotorLimitEnableCommand { get; set; }
        /// <summary>
        /// 设置软限位
        /// </summary>
        private void SetMotorLimitEnable(string enable)
        {
            byte limitEnable;
            if (enable == "true")
            {
                limitEnable = 0b11111100;
            }
            else
            {
                limitEnable = 0b00011100;
            }
            MotorEntity.SetMotorLimitEnableCommand(EnumMotorId.MOTOR_1, limitEnable);
            MotorEntity.SetMotorLimitEnableCommand(EnumMotorId.MOTOR_2, limitEnable);
        }

        public DelegateCommand GetMotorLimitEnableCommand { get; set; }
        /// <summary>
        /// 获取限位掩码状态
        /// </summary>
        private void GetMotorLimitEnable()
        {
            MotorEntity.GetMotorLimitEnableCommand(EnumMotorId.MOTOR_1);
            MotorEntity.GetMotorLimitEnableCommand(EnumMotorId.MOTOR_2);
        }



        public async Task SendDataThread()
        {
            while (NetUdpService.IsOpen)
            {
                var sendTime = DateTime.Now;

                if (_importantByteQueue != null && _importantByteQueue.Count > 0)
                {
                    _importantByteQueue.TryDequeue(out var cmd);
                    NetUdpService.SendMsg(cmd);
                }
                else
                {
                    if (_byteQueue != null && _byteQueue.Count > 0)
                    {
                        _byteQueue.TryDequeue(out var cmd);
                        NetUdpService.SendMsg(cmd);
                    }
                }
                _waitingReply = new TaskCompletionSource<string>();
                try
                {
                    string result = await _waitingReply.Task.WaitAsync(TimeSpan.FromMilliseconds(300));
                    if (result == "Ready")
                    {
                        Thread.Sleep(10);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    if (_isDurabilityTest)
                    {
                        NLog.LogManager.GetCurrentClassLogger().Error($"等待时间超过300ms ,上一次发送时间为{sendTime.ToString("HH:mm:ss.fff")}");
                    }

                }
            }

            while (SerialPortService.IsOpen)
            {

                if (_importantByteQueue != null && _importantByteQueue.Count > 0)
                {
                    _importantByteQueue.TryDequeue(out var cmd);
                    SerialPortService.SendMsg(cmd);
                    NLog.LogManager.GetCurrentClassLogger().Trace($"串口发送控制指令:{cmd}");
                }
                else
                {
                    if (_byteQueue != null && _byteQueue.Count > 0)
                    {
                        _byteQueue.TryDequeue(out var cmd);
                        SerialPortService.SendMsg(cmd);
                        NLog.LogManager.GetCurrentClassLogger().Trace($"串口发送查询指令:{cmd}");
                    }
                }
                _waitingReply = new TaskCompletionSource<string>();
                try
                {
                    string result = await _waitingReply.Task.WaitAsync(TimeSpan.FromMilliseconds(300));
                    if (result == "Ready")
                    {
                        Thread.Sleep(20);
                        continue;
                    }
                }
                catch (Exception ex)
                {

                }
            }



        }
        /// <summary>
        /// 线程执行的方法
        /// </summary>
        public void ThreadProc()
        {
            Task.Run(async () =>
            {
                try
                {
                    if (NetUdpService.IsOpen || SerialPortService.IsOpen)
                    {

                        // 执行发送逻辑
                        await SendDataThread();
                    }

                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Error($"电机测试发送线程异常{ex}");
                }
                finally
                {
                    NLog.LogManager.GetCurrentClassLogger().Error($"电机测试发送线程退出");
                }
            });

        }
        private void CheckAndRestart()
        {
            if (NetUdpService.IsOpen || SerialPortService.IsOpen)
            {
                ThreadProc();
            }
        }
        public event EventHandler<byte[]> MyCustomEvent;

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

                var data = e.DataSource;
                switch (e.CmdType)
                {
                    case EnumSelfMotorCmdType.CMD_SET_RS://设置点击运行/停止命令
                        NLog.LogManager.GetCurrentClassLogger().Info($"(回报提示)设置运行/停止命令成功");
                        break;
                    case EnumSelfMotorCmdType.CMD_SET_SLIM://设置电机使能掩码
                        NLog.LogManager.GetCurrentClassLogger().Info($"(回报提示)设置是能掩码成功");
                        break;
                    case EnumSelfMotorCmdType.CMD_MOT_GOTO://设置电机绝对运行
                        NLog.LogManager.GetCurrentClassLogger().Info($"(回报提示)设置电机移动成功");
                        break;
                    case EnumSelfMotorCmdType.CMD_SET_MCTL://设置电机的闭环控制状态
                        NLog.LogManager.GetCurrentClassLogger().Info("(回报提示)设置点击闭环控制状态成功");
                        break;

                    case EnumSelfMotorCmdType.CMD_GET_POS:
                        var chananl = data[0];
                        var unit = data[1];
                        var pos = BitConverter.ToInt32(data, 2);
                        var ransformationCoefficient = BitConverter.ToSingle(data, 6);
                        NLog.LogManager.GetCurrentClassLogger().Trace($"获取电机的位置成功{chananl}:{pos}");
                        break;
                    case EnumSelfMotorCmdType.CMD_GET_STATUS:
                        NLog.LogManager.GetCurrentClassLogger().Trace($"获取电机状态成功");
                        ParserMotorStatus(data);
                        break;
                    case EnumSelfMotorCmdType.CMD_GET_SLIM:
                        ParserMotorLimtedStatus(data);
                        break;
                }
                _waitingReply.TrySetResult("Ready");
            }
            else
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"e为null");
            }
        }

        private void ParserMotorStatus(byte[] data)
        {


            byte channel = data[0];//电机通道
            byte motorType = data[1];//电机类型
            byte AxisType = data[2];//轴类型
            byte controlType = data[3];//控制模式
            byte unit = data[4];//参数单位类型
            byte statusMask = data[5];//电机状态掩码 R/H 使能 SN软限位    SZ软限位 SP软限位 R/S运行和停止  N是硬件限位 z硬件限位 p 硬件限位
            byte softLimitEnableMask = data[6];//软限位使能掩码
            sbyte MotorRunningDirection = (sbyte)data[7];//电机运行方向
            float unitConversionFactor = BitConverter.ToSingle(data, 8);//单位换算比例
            Int32 pulseCoordinate = BitConverter.ToInt32(data, 12);//当前的脉冲坐标
            Int32 pulseSpeed = BitConverter.ToInt32(data, 16);//当前的脉冲速度

            EnumMotorId channelId = (EnumMotorId)channel;
            //电机状态掩码分析
            bool[] statusMaskBits = new bool[8];

            for (int i = 0; i < 8; i++)
            {
                bool bit = (statusMask & (1 << i)) != 0;
                statusMaskBits[7 - i] = bit;
            }
            MotorModel motorModel = null;
            if (channelId == EnumMotorId.MOTOR_1)
            {
                motorModel = MotorModelX;
              
            }
            else if (channelId == EnumMotorId.MOTOR_2)
            {
                motorModel = MotorModelY;
              
            }
            else
            {

                return;
            }
            motorModel.MotorParams.MotorType = (EnumMotorType)motorType;
            motorModel.MotorParams.CtrType = (EnumMotorCtrType)controlType;
            motorModel.MotorParams.Unit = (EnumMotorUnit)unit;
            motorModel.MotorParams.Enable = statusMaskBits[0];
            motorModel.MotorParams.MoveState = statusMaskBits[4] == false ? EnumMotorMoveState.MotorStop : EnumMotorMoveState.MotorMove;
            //if (motorModel.MotorParams.MoveState == EnumMotorMoveState.MotorMove)
            //{
            //    if (_runStateUpdate != null)
            //    {
            //        _runStateUpdate.SetResult("move");
            //    }

            //}

            var x = statusMaskBits[5];
            motorModel.MotorParams.LimitedState = statusMaskBits[5] == true ? EnumMotorLimitedState.PhyBackwardLimited : (statusMaskBits[7] == true ? EnumMotorLimitedState.PhyForwardLimited : EnumMotorLimitedState.None);
            motorModel.MotorParams.MoveDirection = (EmumMotorMoveDirection)MotorRunningDirection;
            motorModel.MotorParams.SubRatio = unitConversionFactor;
            motorModel.MotorParams.Pos = pulseCoordinate;
            AddPoint( motorModel, channelId);
            //增加限位位置
            if (motorModel.MotorParams.LimitedState == EnumMotorLimitedState.PhyForwardLimited)
            {
                motorModel.MotorParams.PositiveLimitPosition = motorModel.MotorParams.Pos;
            }
            if (motorModel.MotorParams.LimitedState == EnumMotorLimitedState.PhyBackwardLimited)
            {
                motorModel.MotorParams.NegativeLimitPosition = motorModel.MotorParams.Pos;
            }
            NLog.LogManager.GetCurrentClassLogger().Trace($"获取电机的状态成功:通道:{channel},电机类型:{motorModel.MotorParams.MotorType},控制类型:{motorModel.MotorParams.CtrType},单位:{motorModel.MotorParams.Unit},使能状态:{motorModel.MotorParams.Enable}" +
                $",电机移动状态{motorModel.MotorParams.MoveState},电机限位状态:{motorModel.MotorParams.LimitedState},电机移动方向{motorModel.MotorParams.MoveDirection},电机位置{motorModel.MotorParams.Pos}");
        }

        private void ParserMotorLimtedStatus(byte[] data)
        {

            byte channel = data[0];//电机通道
            byte binaryMask = data[1];//限位掩码


            EnumMotorId channelId = (EnumMotorId)channel;
            //电机限位掩码分析
            bool[] limtedMaskBits = new bool[8];

            for (int i = 0; i < 8; i++)
            {
                bool bit = (binaryMask & (1 << i)) != 0;
                limtedMaskBits[7 - i] = bit;
            }

            MotorModel motorModel = null;
            if (channelId == EnumMotorId.MOTOR_1)
            {
                motorModel = MotorModelX;
            }
            else if (channelId == EnumMotorId.MOTOR_2)
            {
                motorModel = MotorModelY;
            }
            else
            {

                return;
            }
            if (limtedMaskBits[0] == true)
            {
                motorModel.MotorParams.SNLimted = true;
            }
            else
            {
                motorModel.MotorParams.SNLimted = false;
            }
            if (limtedMaskBits[1] == true)
            {
                motorModel.MotorParams.SPLimted = true;
            }
            else
            {
                motorModel.MotorParams.SPLimted = false;
            }
            motorModel = null;

        }
        private void DispathcherInvoke(string log)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                string time = DateTime.Now.ToString("HH:mm:ss.fff") + ":   ";
                Log.Add(time + log);
            }));

        }
        private  async void AddPoint(MotorModel motorModel, EnumMotorId channelId)
        {

            if (motorModel.PointList.Count >= 2)
            {
                var data = DateTime.Now;
                var timeDifference = (data - motorModel.PointList[motorModel.PointList.Count - 2].Date).TotalMilliseconds;
                var speed = (motorModel.MotorParams.Pos - motorModel.PointList[motorModel.PointList.Count - 2].Point) / (timeDifference * 1.0) * 1000;
                PlotViewPointMessage pointView= new PlotViewPointMessage() { Date = data, Point = motorModel.MotorParams.Pos, MotorMoveState = motorModel.MotorParams.MoveState, MotorModelAxis = motorModel.MotorModelAxis };
                PlotViewSpeedMessage speedView = new PlotViewSpeedMessage() { SpeedDate = motorModel.PointList[motorModel.PointList.Count - 1].Date, Speed = speed, MotorModelAxis = motorModel.MotorModelAxis };
                
                lock (_lockobj)
                {
                    motorModel.PointList.Add(pointView);
                    motorModel.SpeedList.Add(speedView);
                }
                MotorSpeedplotModel.InvalidatePlot(true);
                MotorplotModel.InvalidatePlot(true);
                motorModel.MotorParams.Speed = speed;
                motorModel.PointListBuffer.Add(pointView);
                motorModel.SpeedListBuffer.Add(speedView);
                if (motorModel.PointListBuffer.Count == 100)
                {
                    var pt = motorModel.PointListBuffer.ToList();
                    var sp = motorModel.SpeedListBuffer.ToList();
                    motorModel.PointListBuffer.Clear();
                    motorModel.SpeedListBuffer.Clear();
                    await Task.Run(async () =>
                    {
                        var x = channelId;
                        using (var db = new TwoAxisDbContextBase ())
                        {

                            await SpliteOperate.AddPlotViewPointListMessagesSimpleAsync(pt, db);
                            await SpliteOperate.AddPlotViewSpeedListMessagesSimpleAsync(sp, db);
                            TotalSize = await SpliteOperate.GetPlotViewPointMessageCountAsync(db);
                        }
                      
                    });
                   
                }

            }
            else
            {
                var data = DateTime.Now;
                PlotViewPointMessage pointView = new PlotViewPointMessage() { Date = data, Point = motorModel.MotorParams.Pos, MotorModelAxis = motorModel.MotorModelAxis , MotorMoveState  = motorModel .MotorParams.MoveState};
                motorModel.PointList.Add(pointView);
                MotorplotModel.InvalidatePlot(true);
                motorModel.PointListBuffer.Add(pointView);
 
            }
        }

        public DelegateCommand SerilizeCommand { get; set; }
        private void Serilize()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
            {
                Task.Run(() =>
                {

                    var path = dialog.SelectedPath;
                    SaveBase(path);
                });
            }
        }
        private void SaveBase(string path)
        {
            SaveToFile(path, "speed");
            SaveToFile(path, "point");
            string xPointjson = JsonConvert.SerializeObject(MotorModelX.PointList.ToList());
            string yPointjson = JsonConvert.SerializeObject(MotorModelY.PointList.ToList());
            string xSpeedjson = JsonConvert.SerializeObject(MotorModelX.SpeedList.ToList());
            string ySpeedjson = JsonConvert.SerializeObject(MotorModelY.SpeedList.ToList());
            string testMessgae = JsonConvert.SerializeObject(MotorTestMessages.ToList());
            // 获取当前时间并格式化为文件名安全的字符串

            string xPointjsonfilePathfileName = $"xPoint.json";
            string xPointjsonfilePath = Path.Combine(path, xPointjsonfilePathfileName); // 组合完整路径

            string yPointjsonfilePathfileName = $"yPoint.json";
            string yPointjsonfilePath = Path.Combine(path, yPointjsonfilePathfileName); // 组合完整路径

            string xSpeedjsonfilePathfileName = $"xSpeed.json";
            string xSpeedjsonfilePath = Path.Combine(path, xSpeedjsonfilePathfileName); // 组合完整路径

            string ySpeedjsonfilePathfileName = $"ySpeed.json";
            string ySpeedjsonfilePath = Path.Combine(path, ySpeedjsonfilePathfileName); // 组合完整路径

            string testMessgaefilePathfileName = $"testMessage.json";
            string testMessgaejsonfilePath = Path.Combine(path, testMessgaefilePathfileName); // 组合完整路径
                                                                                              // 写入JSON数据
            File.WriteAllText(xPointjsonfilePath, xPointjson);
            File.WriteAllText(yPointjsonfilePath, yPointjson);
            File.WriteAllText(xSpeedjsonfilePath, xSpeedjson);
            File.WriteAllText(ySpeedjsonfilePath, ySpeedjson);
            File.WriteAllText(testMessgaejsonfilePath, testMessgae);
        }


        public DelegateCommand ShowThreeAzisTestModelViewModelCommand { get; set; }
        private void ShowThreeAzisTestModelViewModel()
        {
            var threeAxisTestView = _containerProvider.Resolve<ThreeAxisTestModelWindowsView>();
            threeAxisTestView.Show();
        }
        public DelegateCommand DataPathSelectCommand { get; set; }
        /// <summary>
        /// 数据路径保存
        /// </summary>
        private void DataPathSelect()
        {

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            _dataPath = dialog.SelectedPath;
        }
        private void AutoSaveData()
        {

            if (_dataPath == null || !Directory.Exists(_dataPath))
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                _dataPath = Path.Combine(baseDirectory, "Data");
                Directory.CreateDirectory(_dataPath);
            }
            var path = Path.Combine(_dataPath, DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"));
            Directory.CreateDirectory(path);

            SaveBase(path);
            lock (_lockobj)
            {
                MotorModelX.SpeedList.Clear();
                MotorModelY.SpeedList.Clear();
                MotorModelY.PointList.Clear();
                MotorModelY.PointList.Clear();
            }


            MotorSpeedplotModel.InvalidatePlot(true);
            MotorplotModel.InvalidatePlot(true);

        }
        public DelegateCommand DeserilizeCommand { get; set; }
        private void Deserilize()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            var path = dialog.SelectedPath;
            ReadMotorMessage(path);

        }
        public DelegateCommand AllDeserilizeCommand { get; set; }
        private void AllDeserilize()
        {

            FolderBrowserDialog dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            try
            {
                _directSubDirs = Directory.GetDirectories(dialog.SelectedPath);
                if (_directSubDirs.Count() <= 0)
                {
                    return;
                }
                _selectLoadDataPath = dialog.SelectedPath;

                Task.Run(() =>
                {
                    _selectIndex = 0;
                    var path = Path.Combine(_selectLoadDataPath, _directSubDirs[_selectIndex]);
                    ReadMotorMessage(path);


                });
            }
            catch (Exception ex)
            {

            }
        }
        private void ReadMotorMessage(string path)
        {
            string xPointjsonfilePathfileName = $"xPoint.json";
            string xPointjsonfilePath = Path.Combine(path, xPointjsonfilePathfileName); // 组合完整路径

            string yPointjsonfilePathfileName = $"yPoint.json";
            string yPointjsonfilePath = Path.Combine(path, yPointjsonfilePathfileName); // 组合完整路径

            string xSpeedjsonfilePathfileName = $"xSpeed.json";
            string xSpeedjsonfilePath = Path.Combine(path, xSpeedjsonfilePathfileName); // 组合完整路径

            string ySpeedjsonfilePathfileName = $"ySpeed.json";
            string ySpeedjsonfilePath = Path.Combine(path, ySpeedjsonfilePathfileName); // 组合完整路径

            string testMessgaefilePathfileName = $"testMessage.json";
            string testMessgaejsonfilePath = Path.Combine(path, testMessgaefilePathfileName); // 组合完整路径
            if (File.Exists(xPointjsonfilePath))
            {
                MotorModelX.PointList = JsonConvert.DeserializeObject<List<PlotViewPointMessage>>(File.ReadAllText(xPointjsonfilePath));
            }
            if (File.Exists(yPointjsonfilePath))
            {
                MotorModelY.PointList = JsonConvert.DeserializeObject<List<PlotViewPointMessage>>(File.ReadAllText(yPointjsonfilePath));
            }
            if (File.Exists(xSpeedjsonfilePath))
            {
                MotorModelX.SpeedList = JsonConvert.DeserializeObject<List<PlotViewSpeedMessage>>(File.ReadAllText(xSpeedjsonfilePath));
            }
            if (File.Exists(ySpeedjsonfilePath))
            {
                MotorModelY.SpeedList = JsonConvert.DeserializeObject<List<PlotViewSpeedMessage>>(File.ReadAllText(ySpeedjsonfilePath));
            }
            if (File.Exists(testMessgaejsonfilePath))
            {
                MotorTestMessages = JsonConvert.DeserializeObject<ObservableCollection<MotorTestMessage>>(File.ReadAllText(testMessgaejsonfilePath));
            }

            _xSpeedPosLine.ItemsSource = MotorModelX.SpeedList;
            _ySpeedPosLine.ItemsSource = MotorModelY.SpeedList;
            _xSpeedPosLine.DataFieldX = "SpeedDate";
            _xSpeedPosLine.DataFieldY = "Speed"; 
            _ySpeedPosLine.DataFieldX = "SpeedDate";
            _ySpeedPosLine.DataFieldY = "Speed";
            _xPosLine.ItemsSource = MotorModelX.PointList;
            _yPosLine.ItemsSource = MotorModelY.PointList;
            _xPosLine.DataFieldX = "Date";
            _xPosLine.DataFieldY = "Point";
            _yPosLine.DataFieldX = "Date";
            _yPosLine.DataFieldY = "Point";


            MotorSpeedplotModel.InvalidatePlot(true);
            MotorplotModel.InvalidatePlot(true);
        }

        public DelegateCommand SelectPreviousCommand { get; set; }
        /// <summary>
        /// 加载的文件的上一个
        /// </summary>
        private async void SelectPrevious()
        {
            using (var db =new TwoAxisDbContextBase())
            {
                TotalSize = await SpliteOperate.GetPlotViewPointMessageCountAsync(db);
            }
               
            if (headIndex == 0)
            {
                if (TotalSize <= LoadSize)
                {
                    headIndex = 0;
                }
                else
                {
                    headIndex = TotalSize - LoadSize;
                }
            }
            else
            {
                if (headIndex <= LoadSize)
                {
                    headIndex = 0;
                }
                else
                {
                    headIndex = headIndex - LoadSize;
                }
               

            }
           
        }
        public DelegateCommand SelectNextCommand { get; set; }
        /// <summary>
        /// 选择加载文件夹的下一个
        /// </summary>
        private async void SelectNext()
        {
            using (var db = new TwoAxisDbContextBase())
            {
                TotalSize = await SpliteOperate.GetPlotViewPointMessageCountAsync(db);
            }
            if (headIndex + LoadSize >= TotalSize)
            {

            }
            else 
            {
                if (headIndex + LoadSize + LoadSize > TotalSize)
                {
                    headIndex = TotalSize - LoadSize;
                }
                else
                {
                    headIndex += LoadSize;
                }
               
            }
          
            SqliteLoad();
        }
        public DelegateCommand SqliteLoadDelegateCommand { get; set; }
        /// <summary>
        /// 从数据库中加载数据
        /// </summary>
        private async void SqliteLoad()
        {
            using (var db = new TwoAxisDbContextBase())
            {
                TotalSize = await SpliteOperate.GetPlotViewPointMessageCountAsync(db);
                var pointNumber = await SpliteOperate.GetPlotViewPointMessageCountAsync(db);
                var point = await SpliteOperate.GetPlotViewPointMessagesAsync(db,headIndex, LoadSize > pointNumber ? pointNumber : LoadSize);
                var speedNumber = await SpliteOperate.GetPlotViewSpeedMessageCountAsync(db);
                var speed = await SpliteOperate.GetPlotViewSpeedMessagesAsync(db, LoadSize > speedNumber ? speedNumber : LoadSize);
                var xPointList = point.Where(m => m.MotorModelAxis == EnumMotorModel.MOTOR_x).ToList();
                var yPointList = point.Where(m => m.MotorModelAxis == EnumMotorModel.MOTOR_y).ToList();

                var xSpeedList = speed.Where(m => m.MotorModelAxis == EnumMotorModel.MOTOR_x).ToList();
                var ySpeedList = speed.Where(m => m.MotorModelAxis == EnumMotorModel.MOTOR_y).ToList();

                MotorModelX.PointList = xPointList;
                _xPosLine.ItemsSource = MotorModelX.PointList;
                _xPosLine.DataFieldX = "Date";
                _xPosLine.DataFieldY = "Point";

                MotorModelX.SpeedList = xSpeedList;
                _xSpeedPosLine.ItemsSource = MotorModelX.SpeedList;
                _xSpeedPosLine.DataFieldX = "SpeedDate";
                _xSpeedPosLine.DataFieldY = "Speed";
                ;
                MotorModelY.PointList = yPointList;
                _yPosLine.ItemsSource = MotorModelY.PointList;
                _yPosLine.DataFieldX = "Date";
                _yPosLine.DataFieldY = "Point";

                MotorModelY.SpeedList = ySpeedList;
                _ySpeedPosLine.ItemsSource = MotorModelY.SpeedList;
                _ySpeedPosLine.DataFieldX = "SpeedDate";
                _ySpeedPosLine.DataFieldY = "Speed";

                MotorSpeedplotModel.InvalidatePlot(true);
                MotorplotModel.InvalidatePlot(true);
            }
               

            
        }
       
    }
   

   
}

