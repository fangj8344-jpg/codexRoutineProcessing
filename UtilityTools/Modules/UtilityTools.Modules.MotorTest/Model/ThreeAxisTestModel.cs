using CsvHelper;
using MathNet.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using OxyPlot.Wpf;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Converters;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Interface;
using UtilityTools.Modules.MotorTest.Entity;
using UtilityTools.Modules.MotorTest.Event;
using UtilityTools.Modules.MotorTest.Protocol;
using UtilityTools.Modules.MotorTest.Runners;
using UtilityTools.Modules.MotorTest.Service;
using UtilityTools.Modules.MotorTest.SQLite;
using UtilityTools.Modules.MotorTest.TestItems;
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
            _reportService = containerProvider.Resolve<ITestReportService>();
            _eventAggregator = containerProvider.Resolve<IEventAggregator>();
            Init();
            SubscribeTestProgress();
        }
        private object _lockobj = new object();
        private IContainerProvider _containerProvider;
        private readonly IDialogHostService _dialogHostService;
        private SelfMotorParser _parser;
        private TaskCompletionSource<string> _waitingReply;
        private CancellationTokenSource _queryCts;
        private readonly ITestReportService _reportService;
        private readonly IEventAggregator _eventAggregator;

        private BackgroundWorker _work;
        private bool _isTest = false;
        private EnumMotorInquiry _testMotorId;
        private bool _isSpeedMode = false;
        private int _queryInterval = 500;
        private int _plotDirty = 0;
        private DateTime _lastPlotRefreshAt = DateTime.MinValue;
        private const int PlotRefreshMinIntervalMs = 50;
        private System.Windows.Threading.DispatcherTimer? _uiRenderTimer;
  
        private Double _progressValue;
        private bool _isCurrentTestRunning;
        private string _currentTestDisplay = "等待测试开始";
        private Double _xProgressValue;
        private Double _yProgressValue;
        private string _xCurrentTestDisplay = "X轴等待测试开始";
        private string _yCurrentTestDisplay = "Y轴等待测试开始";
        private string _speedCalcTimeSource = "速度时间源: 本地时间(回退)";
        // 统一类型为 MotorDbContext

        private string _version = "4.0.3";
        /// <summary>
        /// 版本号
        /// </summary>
        public string Version
        {
            get { return _version; }
            set { _version = value; RaisePropertyChanged(); }
        }
        private int _maxCount = 12000;
        public int MaxCount
        {
            get { return _maxCount; }
            set { _maxCount = value; RaisePropertyChanged(); }
        }
        private int _plotDisplayStride = 3;
        /// <summary>
        /// 图表显示抽样步长（每 N 个点显示 1 个）；采集与存库保持全量。
        /// </summary>
        public int PlotDisplayStride
        {
            get { return _plotDisplayStride; }
            set
            {
                if (value <= 0) value = 1;
                _plotDisplayStride = value;
                RaisePropertyChanged();
            }
        }
       
        public Double ProgressValue
        {
            get { return _progressValue; }
            set { _progressValue = value; RaisePropertyChanged(); }
        }
        public bool IsCurrentTestRunning
        {
            get { return _isCurrentTestRunning; }
            set { _isCurrentTestRunning = value; RaisePropertyChanged(); }
        }
        public string CurrentTestDisplay
        {
            get { return _currentTestDisplay; }
            set { _currentTestDisplay = value; RaisePropertyChanged(); }
        }
        public Double XProgressValue
        {
            get { return _xProgressValue; }
            set { _xProgressValue = value; RaisePropertyChanged(); }
        }
        public Double YProgressValue
        {
            get { return _yProgressValue; }
            set { _yProgressValue = value; RaisePropertyChanged(); }
        }
        public string XCurrentTestDisplay
        {
            get { return _xCurrentTestDisplay; }
            set { _xCurrentTestDisplay = value; RaisePropertyChanged(); }
        }
        public string YCurrentTestDisplay
        {
            get { return _yCurrentTestDisplay; }
            set { _yCurrentTestDisplay = value; RaisePropertyChanged(); }
        }
        public string SpeedCalcTimeSource
        {
            get { return _speedCalcTimeSource; }
            set
            {
                if (_speedCalcTimeSource == value) return;
                _speedCalcTimeSource = value;
                RaisePropertyChanged();
            }
        }
        public bool IsSpeedMode
        {
            get { return _isSpeedMode; }
            set
            {
                if (_isSpeedMode == value)
                    return;

                _isSpeedMode = value;
                RaisePropertyChanged();
                ApplyManualModeToAllMotors(_isSpeedMode);
            }
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
        private ObservableCollection<FiveAxisModel> _motors;
        /// <summary>
        /// 电机集合
        /// </summary>
        public ObservableCollection<FiveAxisModel> Motors
        {
            get { return _motors; }
            set { _motors = value; RaisePropertyChanged(); }
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
        private ConcurrentQueue<byte[]> _byteQueue;
        /// <summary>
        /// 普通队列
        /// </summary>
        public ConcurrentQueue<byte[]> ByteQueue
        {
            get { return _byteQueue; }
            set { _byteQueue = value; RaisePropertyChanged(); }
        }
        private ConcurrentQueue<byte[]> _importantByteQueue;
        /// <summary>
        /// 重要队列
        /// </summary>
        public ConcurrentQueue<byte[]> ImportantByteQueue
        {
            get { return _importantByteQueue; }
            set { _importantByteQueue = value; RaisePropertyChanged(); }
        }
        private SerialPortService _serialPortService;
        /// <summary>
        /// 串口异步通信服务
        /// </summary>
        public SerialPortService SerialPortService
        {
            get { return _serialPortService; }
            set { _serialPortService = value; RaisePropertyChanged(); }
        }

        private UdpNetAsyncDevice _netUdpService;
        /// <summary>
        /// 网口异步通信服务
        /// </summary>
        public UdpNetAsyncDevice NetUdpService
        {
            get { return _netUdpService; }
            set { _netUdpService = value; RaisePropertyChanged(); }
        }
        private MotorTypeModel _motorTypemodel;
        /// <summary>
        /// 电机类型
        /// </summary>
        public MotorTypeModel MotorTypeModel
        {
            get { return _motorTypemodel; }
            set { _motorTypemodel = value; RaisePropertyChanged(); }    
        }
        private MotorEntity _motorEntity;
        public MotorEntity MotorEntity
        {
            get { return _motorEntity; }
            set { _motorEntity = value; RaisePropertyChanged(); }
        }
        private int _totalSize;
        /// <summary>
        /// 数据库中的总数
        /// </summary>
        public int TotalSize
        {
            get { return _totalSize; }
            set { _totalSize = value; RaisePropertyChanged(); }
        }
        private int _savePointNumber = 200;
        /// <summary>
        ///每次保存数量
        /// </summary>
        public int SavePointNumber
        {
            get { return _savePointNumber; }
            set { _savePointNumber = value; RaisePropertyChanged(); }
        }
        private int _headIndex = 0;
        /// <summary>
        /// 头索引
        /// </summary>
        public int HeadIndex
        {
            get { return _headIndex; }
            set { _headIndex = value; RaisePropertyChanged(); }
        }
        private int _loadSize = 100000;
        /// <summary>
        /// 加载大小
        /// </summary>
        public int LoadSize 
        {
            get { return _loadSize; }
            set { _loadSize = value; RaisePropertyChanged(); }
        }
        private CancellationTokenSource _cancellationToken;
        /// <summary>
        /// 取消令牌
        /// </summary>
        public CancellationTokenSource CancellationToken
        {
            get { return _cancellationToken; }
            set { _cancellationToken = value; RaisePropertyChanged(); }
        }
        private int _magnitudeOfSpeed = 10000;
        /// <summary>
        /// 速度模式移动速度（单位：um/s）
        /// </summary>
        public int MagnitudeOfSpeed
        {
            get { return _magnitudeOfSpeed; }
            set
            {
                if (value <= 0)
                    value = 1000;
                _magnitudeOfSpeed = value;
                RaisePropertyChanged();
            }
        }
        public DelegateCommand<string> IndependentMotortestCommand { get; set; }
        public DelegateCommand ShotDownCommand { get; set; }
        public DelegateCommand IndependentMotorDurabilityTestCommand { get; set; }
        public DelegateCommand CloseTestPerformanceCommand { get; set; }
        public DelegateCommand TestPerformanceCommand { get; set; }
        public DelegateCommand SQLiteTestCommand { get; set; }
        public DelegateCommand SaveDataFileCommand { get; set; }
        public DelegateCommand ReadDataFileCommand { get; set; }
        public DelegateCommand SqliteLoadCommand { get; set; }
        public DelegateCommand CloseSlimitedCommand { get; set; }
        public DelegateCommand<string> SaveToFileCommand { get; set; }
        public DelegateCommand<string> AutoAdjustCommand { get; set; }
        public DelegateCommand<string> ClearMonitorCommand { get; set; }
        public DelegateCommand RandomRepeatabilityTestCommand { get; set; }
        public DelegateCommand GenerateRandomRepeatabilityReportCommand { get; set; }

        public DelegateCommand<string> TestMotorTogetherCommand { get; set; }
        public ObservableCollection<RandomTargetPointRecord> RandomTargetPoints { get; } = new();
        public ObservableCollection<RandomMoveTripAxisRecord> RandomMoveTripsX { get; } = new();
        public ObservableCollection<RandomMoveTripAxisRecord> RandomMoveTripsY { get; } = new();
        public ObservableCollection<RandomPointStatAxisRecord> RandomPointStatsX { get; } = new();
        public ObservableCollection<RandomPointStatAxisRecord> RandomPointStatsY { get; } = new();
        public ObservableCollection<HistogramBinRecord> RandomDistanceHistogramX { get; } = new();
        public ObservableCollection<HistogramBinRecord> RandomDistanceHistogramY { get; } = new();
        public ObservableCollection<HistogramBinRecord> RandomSpeedHistogramX { get; } = new();
        public ObservableCollection<HistogramBinRecord> RandomSpeedHistogramY { get; } = new();

        private string _randomRepeatabilitySummary = "未执行随机坐标重复精度测试。";
        public string RandomRepeatabilitySummary
        {
            get => _randomRepeatabilitySummary;
            set { _randomRepeatabilitySummary = value; RaisePropertyChanged(); }
        }

        /// <summary>最近一次完成的随机重复精度测试原始结果（用于事后生成报告）。</summary>
        private RandomRepeatabilityTestResult? _lastRandomRepeatabilityXResult;

        private RandomRepeatabilityTestResult? _lastRandomRepeatabilityYResult;

        private void Init()
        {
            ClearMonitorCommand = new DelegateCommand<string>(ClearMonitor);
            TestMotorTogetherCommand = new DelegateCommand<string>(TestMotorTogether);
            IndependentMotortestCommand = new DelegateCommand<string>(IndependentMotortest);
            AutoAdjustCommand = new DelegateCommand<string>(AutoAdjust);
            SaveToFileCommand = new DelegateCommand<string>(SaveToFile);
            IndependentMotorDurabilityTestCommand = new DelegateCommand(IndependentMotorDurabilityTest);
            ShotDownCommand = new DelegateCommand(ShotDown);
            TestPerformanceCommand = new DelegateCommand(TestPerformance);
            CloseTestPerformanceCommand = new DelegateCommand(CloseTestPerformance);
            SaveDataFileCommand = new DelegateCommand(SaveDataFile);
            ReadDataFileCommand = new DelegateCommand(ReadDataFile);
            CloseSlimitedCommand = new DelegateCommand(CloseSlimited);
            SqliteLoadCommand = new DelegateCommand(SqliteLoad);
            RandomRepeatabilityTestCommand = new DelegateCommand(StartRandomRepeatabilityTest);
            GenerateRandomRepeatabilityReportCommand = new DelegateCommand(
                GenerateRandomRepeatabilityReport,
                CanGenerateRandomRepeatabilityReport);
            ByteQueue = new ConcurrentQueue<byte[]>();
            ImportantByteQueue = new ConcurrentQueue<byte[]>();
            MotorEntity = new MotorEntity(SerialPortService, NetUdpService);
            MotorplotModel = new PlotModel();
            MotorplotModel.Legends.Add(new Legend());
            MotorSpeedplotModel = new PlotModel();
            MotorSpeedplotModel.Legends.Add(new Legend());
            MotorplotModel.Axes.Add(new DateTimeAxis()
            {
                Title = "时间",
                Position = AxisPosition.Bottom,
                IsPanEnabled = true,
                IsZoomEnabled = true
            });
            MotorplotModel.Axes.Add(new LinearAxis()
            {
                Title = "位置(μm)",
                Position = AxisPosition.Left,
                IsPanEnabled = true,
                IsZoomEnabled = true
            });
            MotorSpeedplotModel.Axes.Add(new DateTimeAxis()
            {
                Title = "时间",
                Position = AxisPosition.Bottom,
                IsPanEnabled = true,
                IsZoomEnabled = true
            });
            MotorSpeedplotModel.Axes.Add(new LinearAxis()
            {
                Title = "速度(μm/s)",
                Position = AxisPosition.Left,
                IsPanEnabled = true,
                IsZoomEnabled = true
            });
            MachineProfile targetProfile = MachineProfile.StandardTwoAxis;
            if (_reportService.MotorKindObj is MachineProfile parsedProfile)
            {
                targetProfile = parsedProfile;
            }
           
            MotorTypeModel = new MotorTypeModel(this, _containerProvider, targetProfile);
            if (_reportService.DevShortcutXyOnlyAxes && targetProfile == MachineProfile.UniversalFiveAxis)
            {
                ApplyUniversalFiveAxisDevShortcutXyOnlyTrim();
            }

            ApplyManualModeToAllMotors(_isSpeedMode);

            _uiRenderTimer = new System.Windows.Threading.DispatcherTimer();
            _uiRenderTimer.Interval = TimeSpan.FromMilliseconds(50); // 轻量心跳，真正刷新由最小间隔节流控制
            _uiRenderTimer.Tick += (s, e) =>
            {
                // 数据没有变化，不刷新图表
                if (System.Threading.Volatile.Read(ref _plotDirty) == 0)
                {
                    return;
                }

                // 节流：最短刷新间隔 100ms（10 FPS），优先保证长时间运行流畅性。
                var now = DateTime.UtcNow;
                if ((now - _lastPlotRefreshAt).TotalMilliseconds < PlotRefreshMinIntervalMs)
                {
                    return;
                }

                _lastPlotRefreshAt = now;
                System.Threading.Interlocked.Exchange(ref _plotDirty, 0);

                if (MotorplotModel != null)
                    MotorplotModel.InvalidatePlot(true);

                if (MotorSpeedplotModel != null)
                    MotorSpeedplotModel.InvalidatePlot(true);
            };
            _uiRenderTimer.Start(); // 启动定时器

        }

        public void MarkPlotDirty()
        {
            System.Threading.Interlocked.Exchange(ref _plotDirty, 1);
        }

        private void SubscribeTestProgress()
        {
            _eventAggregator.GetEvent<MotorTestResultEvent>().Subscribe(msg =>
            {
                if (msg == null) return;

                string axisName = string.IsNullOrWhiteSpace(msg.AxisName) ? "未知轴" : msg.AxisName;
                string testName = string.IsNullOrWhiteSpace(msg.TestProject) ? "未命名测试" : msg.TestProject;
                bool isXAxis = axisName.Contains("X", StringComparison.OrdinalIgnoreCase);
                bool isYAxis = axisName.Contains("Y", StringComparison.OrdinalIgnoreCase);

                if (msg.ProgressState == MotorTestProgressState.Running)
                {
                    IsCurrentTestRunning = true;
                    ProgressValue = Math.Max(0, Math.Min(95, msg.ProgressPercent));
                    CurrentTestDisplay = $"{axisName} - {testName} 进行中 {ProgressValue:F0}%";
                    if (isXAxis)
                    {
                        XProgressValue = ProgressValue;
                        XCurrentTestDisplay = $"{axisName} - {testName} 进行中 {XProgressValue:F0}%";
                    }
                    if (isYAxis)
                    {
                        YProgressValue = ProgressValue;
                        YCurrentTestDisplay = $"{axisName} - {testName} 进行中 {YProgressValue:F0}%";
                    }
                    return;
                }

                if (msg.ProgressState == MotorTestProgressState.Passed || msg.ProgressState == MotorTestProgressState.Failed)
                {
                    IsCurrentTestRunning = false;
                    ProgressValue = 100;
                    string resultText = msg.ProgressState == MotorTestProgressState.Passed ? "合格" : "不合格";
                    CurrentTestDisplay = $"{axisName} - {testName} 已完成（{resultText}）";
                    if (isXAxis)
                    {
                        XProgressValue = 100;
                        XCurrentTestDisplay = $"{axisName} - {testName} 已完成（{resultText}）";
                    }
                    if (isYAxis)
                    {
                        YProgressValue = 100;
                        YCurrentTestDisplay = $"{axisName} - {testName} 已完成（{resultText}）";
                    }
                }
            }, ThreadOption.UIThread);
        }

        private void ApplyManualModeToAllMotors(bool useSpeedMode)
        {
            if (Motors == null || Motors.Count == 0)
                return;

            foreach (var motor in Motors)
            {
                motor?.EnsureControlModeAndEnable(useSpeedMode);
            }
        }

        internal void UpdateSpeedCalcTimeSource(bool useHardwareTimestamp)
        {
            string sourceText = useHardwareTimestamp
                ? "速度时间源: 下位机时间戳(ms)"
                : "速度时间源: 本地时间(回退)";

            if (System.Windows.Application.Current?.Dispatcher?.CheckAccess() == true)
            {
                SpeedCalcTimeSource = sourceText;
                return;
            }

            System.Windows.Application.Current?.Dispatcher?.BeginInvoke(new Action(() =>
            {
                SpeedCalcTimeSource = sourceText;
            }));
        }


       
        private void TestPerformance()
        {
            _queryInterval = 50;
        }
        /// <summary>
        /// 问询状态
        /// </summary>
        public async Task QueryStatusTask() 
        {
            _queryCts?.Cancel();
            _queryCts = new CancellationTokenSource();
            var token = _queryCts.Token;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if(GetTotalQueueCount() < 5)
                    for (int i = 0; i < Motors.Count; i++)
                    {
                        _motorEntity.GetMotorStatusCommand(Motors[i].EnumMotorId);
                    }
                     
                   await Task.Delay (_queryInterval,token);
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex);
            }
        }
        private int GetTotalQueueCount()
        {
            int udpCount = 0;
            int serialCount = 0;
            if (NetUdpService != null && NetUdpService.IsOpen)
            {
                udpCount = NetUdpService.SendQueueCount;
            }
            if (SerialPortService != null && SerialPortService.IsOpen)
            {
                 serialCount = SerialPortService.SendQueueCount;

            }
            return Math.Max(udpCount, serialCount);
        }
        public void CloseQueryStatusTask()
        {
            if (_queryCts != null)
            {
                _queryCts.Cancel();
                _queryCts.Dispose();
                _queryCts = null;
            }
            
        }
   
        private void CloseTestPerformance()
        {
            _queryInterval = 500;
        }

        private async void TestMotorTogether(string testModel)
        {
            try
            {
                // 1. 初始化取消令牌
                CancellationToken = new CancellationTokenSource();
                TestPerformance(); // 开启高频问询
                _reportService.StartReport();
                if (Motors != null && Motors.Count > 0)
                {
                    // 【核心修复】：在这里显式声明这个“任务篮子”变量
                    List<Task> testTasks = new List<Task>();

                    for (int i = 0; i < Motors.Count; i++)
                    {
                        // 获取当前轴的引用，避免变量不存在报错
                        var currentMotor = Motors[i];
                        var token = CancellationToken.Token;

                        // 2. 将每个轴的任务添加到篮子里
                        switch (testModel)
                        {
                            case "BaseTest":
                                testTasks.Add(currentMotor.BaseTest(token));
                                break;
                            case "TestSmoothnessDetection":
                                testTasks.Add(currentMotor.SmoothnessTest(token));
                                break;
                            case "DurabilityTest":
                                testTasks.Add(currentMotor.DurabilityTest(token));
                                break;
                        }
                    }

                    // 3. 同时等待篮子里所有的任务完成
                    if (testTasks.Count > 0)
                    {
                        await Task.WhenAll(testTasks);
                        _reportService.CompleteReport();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 用户点了停止，正常退出
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"并行测试异常: {ex}");
            }
            finally
            {
                CloseTestPerformance(); // 恢复正常频率
            }
        }


        private async void IndependentMotortest(string testModel)
        {
            // 1. 准备工作（还在 UI 线程，为了重置界面状态）
            TestPerformance();
            ProgressValue = 0;
            CancellationToken = new CancellationTokenSource();
            var token = CancellationToken.Token;

            try
            {
                // 这样整个 for 循环和 switch 判断，都在后台跑，绝对不占 UI 资源
                await Task.Run(async () =>
                {
                    if (Motors != null && Motors.Count > 0)
                    {
                        for (int i = 0; i < Motors.Count; i++)
                        {
                            token.ThrowIfCancellationRequested(); // 随时检查强行中止

                            var currentMotor = Motors[i];

                            // 3. 执行具体的测试逻辑
                            // 这里的 await 会在后台线程异步等待，不会跳回 UI 线程
                            switch (testModel)
                            {
                                case "BaseTest": await currentMotor.BaseTest(token); break;
                                case "TestSmoothnessDetection": await currentMotor.SmoothnessTest(token); break;
                                case "DurabilityTest": await currentMotor.DurabilityTest(token); break;
                            }

                            // 4. 【关键点】：更新进度条这种 UI 操作，必须手动切回 UI 线程
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                ProgressValue = ((i + 1.0) / Motors.Count) * 100;
                            });
                        }
                    }
                }, token);

                // 全部测完，拉满进度
                ProgressValue = 100;
            }
            catch (OperationCanceledException)
            {
                // 捕获到取消，进度清零
                ProgressValue = 0;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"串行测试（后台模式）异常: {ex}");
            }
            finally
            {
                CloseTestPerformance();
            }
        }

        private void ShotDown()
        {
            if (CancellationToken != null)
            {
                CancellationToken.Cancel();
            }

            IsCurrentTestRunning = false;
            ProgressValue = 0;
            CurrentTestDisplay = "测试已手动停止";
            XProgressValue = 0;
            YProgressValue = 0;
            XCurrentTestDisplay = "X轴测试已手动停止";
            YCurrentTestDisplay = "Y轴测试已手动停止";

            if (Motors != null)
            {
                foreach (var motor in Motors)
                {
                    motor.StopMotor();
                }
            }
        }

        private async void StartRandomRepeatabilityTest()
        {
            CancellationToken?.Cancel();
            CancellationToken = new CancellationTokenSource();
            try
            {
                AppendRandomRepeatLogSafe("随机重复精度：测试任务已启动。");
                await RunRandomRepeatabilityTestAsync(CancellationToken.Token);
            }
            catch (OperationCanceledException)
            {
                var msg = "随机坐标重复精度测试已取消（可能点了「关闭测试」或再次点了测试按钮）。";
                RandomRepeatabilitySummary = msg;
                AppendRandomRepeatLogSafe(msg);
                NLog.LogManager.GetCurrentClassLogger().Warn("随机坐标重复精度测试已取消");
            }
            catch (Exception ex)
            {
                var detail = ex is AggregateException agg
                    ? string.Join("；", agg.InnerExceptions.Select(e => e.Message))
                    : ex.Message;
                var summary = $"随机坐标重复精度测试异常：{detail}";
                RandomRepeatabilitySummary = summary;
                AppendRandomRepeatLogSafe($"{summary}\n{ex}");
                NLog.LogManager.GetCurrentClassLogger().Error(ex, "随机坐标重复精度测试异常");
                TryRestoreXyMotorsAfterRandomFailure();
                ShowRandomRepeatabilityErrorDialog(detail);
            }
        }

        /// <summary>
        /// 满行程/堵转流程可能下发「停止运行」；随机段若不再发「运行」部分驱动器会拒收 GOTO，表现为轴全不动。此处统一恢复闭环+使能+运行。
        /// </summary>
        private void TryRestoreXyMotorsAfterRandomFailure()
        {
            try
            {
                var xAxis = Motors?.FirstOrDefault(m => m.Name.Contains("X", StringComparison.OrdinalIgnoreCase));
                var yAxis = Motors?.FirstOrDefault(m => m.Name.Contains("Y", StringComparison.OrdinalIgnoreCase));
                if (xAxis == null || yAxis == null) return;
                foreach (var axis in new[] { xAxis, yAxis })
                {
                    MotorEntity.SetMotorControlModeCommand(axis.EnumMotorId, EnumMotorCtrType.CloseLoopPosCtr);
                    MotorEntity.SetMotorEnableCommand(axis.EnumMotorId, EnumMotorEnable.Enable);
                    MotorEntity.SetMotorOperatingStatusCommand(axis.EnumMotorId, EnumMotorOperatingState.Run);
                }

                AppendRandomRepeatLog(xAxis, "测试异常结束：已恢复闭环位置、使能与运行。");
                AppendRandomRepeatLog(yAxis, "测试异常结束：已恢复闭环位置、使能与运行。");
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Warn(ex, "随机重复精度：恢复 X/Y 运动状态失败");
            }
        }

        private void AppendRandomRepeatLogSafe(string message)
        {
            var xAxis = Motors?.FirstOrDefault(m => m.Name.Contains("X", StringComparison.OrdinalIgnoreCase));
            var yAxis = Motors?.FirstOrDefault(m => m.Name.Contains("Y", StringComparison.OrdinalIgnoreCase));
            if (xAxis != null) AppendRandomRepeatLog(xAxis, message);
            if (yAxis != null) AppendRandomRepeatLog(yAxis, message);
        }

        private static void ShowRandomRepeatabilityErrorDialog(string detail)
        {
            var app = System.Windows.Application.Current;
            if (app?.Dispatcher == null) return;
            var text = $"随机坐标重复精度测试中断。\n\n原因：{detail}\n\n请查看「随机重复精度」页顶部摘要与「实时日志」。";
            app.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    System.Windows.MessageBox.Show(app.MainWindow, text, "随机重复精度", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
                catch
                {
                    System.Windows.MessageBox.Show(text, "随机重复精度", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
            }));
        }

        /// <summary>
        /// 随机坐标重复精度测试：
        /// 1) 以 XY 中心为原点生成高斯分布点（5mm 密度级别）
        /// 2) 点间随机跳转，每点至少 10 次
        /// 3) 输出点表、行程表、重复精度统计和距离/速度直方图
        /// </summary>
        private void AppendRandomRepeatLog(FiveAxisModel axis, string message)
        {
            var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
            _eventAggregator.GetEvent<MotorLogEvent>().Publish($"{axis.Name}|{line}");
        }

        /// <summary>
        /// 随机坐标跳转：单轴 MoveState 两段判停（先离开 MotorStop，再回到 MotorStop）。超时返回 false，不抛异常；取消仍向上抛出。
        /// </summary>
        private static async Task<bool> TryRandomAxisLeaveThenStopAsync(
            FiveAxisModel axis,
            CancellationToken token,
            int leaveStopTimeoutSeconds,
            int stopTimeoutSeconds,
            int pollDelayMs)
        {
            static bool IsMotorStop(FiveAxisModel a) =>
                a.MotorModel.MotorParams.MoveState == EnumMotorMoveState.MotorStop;

            try
            {
                var startWait = DateTime.UtcNow;
                while (IsMotorStop(axis))
                {
                    token.ThrowIfCancellationRequested();
                    if ((DateTime.UtcNow - startWait).TotalSeconds > leaveStopTimeoutSeconds)
                        return false;
                    await Task.Delay(pollDelayMs, token).ConfigureAwait(false);
                }

                startWait = DateTime.UtcNow;
                while (!IsMotorStop(axis))
                {
                    token.ThrowIfCancellationRequested();
                    if ((DateTime.UtcNow - startWait).TotalSeconds > stopTimeoutSeconds)
                        return false;
                    await Task.Delay(pollDelayMs, token).ConfigureAwait(false);
                }

                return true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 随机坐标重复精度测试
        /// 
        /// 测试概述：
        /// 1) 以 XY 中心为原点生成高斯分布点（5mm 密度级别）
        /// 2) 点间随机跳转，每点至少 10 次
        /// 3) 输出点表、行程表、重复精度统计和距离/速度直方图
        /// 
        /// 实现方式：
        /// - 使用规范化的 RandomRepeatabilityTestItem 类执行测试
        /// - 该类封装了完整的测试逻辑，包括满行程检测、随机点生成、跳转测试等
        /// - 测试结果通过 RandomRepeatabilityTestResult 返回
        /// 
        /// 测试流程：
        /// 1. 查找 X/Y 轴电机
        /// 2. 清空之前的测试数据
        /// 3. 创建 RandomRepeatabilityTestItem 实例
        /// 4. 调用 ExecuteAsync 执行测试
        /// 5. 将测试结果复制到 UI 集合（ObservableCollection）
        /// 6. 更新测试摘要信息
        /// 
        /// 异常处理：
        /// - OperationCanceledException：测试被取消，更新 UI 并返回
        /// - 其他异常：捕获并显示错误信息，不中断程序
        /// 
        /// UI 更新：
        /// - 所有测试数据都通过 ObservableCollection 绑定到 UI
        /// - 测试摘要显示在 RandomRepeatabilitySummary 属性中
        /// - 测试日志通过 AppendRandomRepeatLog 记录
        /// </summary>
        /// <summary>
        /// 随机坐标重复精度测试
        /// 
        /// 测试概述：
        /// 1) 以 XY 中心为原点生成高斯分布点（5mm 密度级别）
        /// 2) 点间随机跳转，每点至少 10 次
        /// 3) 输出点表、行程表、重复精度统计和距离/速度直方图
        /// 
        /// 实现方式：
        /// - 使用规范化的 RandomRepeatabilityTestItem 类执行测试
        /// - 该类封装了完整的测试逻辑，包括满行程检测、随机点生成、跳转测试等
        /// - 测试结果通过 RandomRepeatabilityTestResult 返回
        /// 
        /// 测试流程：
        /// 1. 查找 X/Y 轴电机
        /// 2. 清空之前的测试数据
        /// 3. 创建 RandomRepeatabilityTestItem 实例
        /// 4. 分别调用 X 轴和 Y 轴测试（单轴测试）
        /// 5. 将测试结果复制到 UI 集合（ObservableCollection）
        /// 6. 更新测试摘要信息
        /// 
        /// 异常处理：
        /// - OperationCanceledException：测试被取消，更新 UI 并返回
        /// - 其他异常：捕获并显示错误信息，不中断程序
        /// 
        /// UI 更新：
        /// - 所有测试数据都通过 ObservableCollection 绑定到 UI
        /// - 测试摘要显示在 RandomRepeatabilitySummary 属性中
        /// - 测试日志通过 AppendRandomRepeatLog 记录
        /// </summary>
        /// <param name="token">取消令牌</param>
        private async Task RunRandomRepeatabilityTestAsync(CancellationToken token)
        {
            var xAxis = Motors?.FirstOrDefault(m => m.Name.Contains("X", StringComparison.OrdinalIgnoreCase));
            var yAxis = Motors?.FirstOrDefault(m => m.Name.Contains("Y", StringComparison.OrdinalIgnoreCase));
            if (xAxis == null || yAxis == null)
                throw new InvalidOperationException("未找到 X/Y 轴，无法执行随机坐标重复精度测试。");

            AppendRandomRepeatLog(xAxis,
                $"随机重复精度：准备开始。轴={xAxis.Name}({xAxis.EnumMotorId})，当前位置={xAxis.MotorModel.MotorParams.PosUm:F3}μm");
            AppendRandomRepeatLog(yAxis,
                $"随机重复精度：准备开始。轴={yAxis.Name}({yAxis.EnumMotorId})，当前位置={yAxis.MotorModel.MotorParams.PosUm:F3}μm");

            _lastRandomRepeatabilityXResult = null;
            _lastRandomRepeatabilityYResult = null;
            GenerateRandomRepeatabilityReportCommand.RaiseCanExecuteChanged();

            // 清空之前的数据
            RandomTargetPoints.Clear();
            RandomMoveTripsX.Clear();
            RandomMoveTripsY.Clear();
            RandomPointStatsX.Clear();
            RandomPointStatsY.Clear();
            RandomDistanceHistogramX.Clear();
            RandomDistanceHistogramY.Clear();
            RandomSpeedHistogramX.Clear();
            RandomSpeedHistogramY.Clear();

            AppendRandomRepeatLog(xAxis, "随机重复精度：任务已启动（本轴独立执行）。");
            AppendRandomRepeatLog(yAxis, "随机重复精度：任务已启动（本轴独立执行）。");

            var xRunner = new MotorWorkflowRunner(
                _reportService,
                _eventAggregator,
                MotorEntity,
                xAxis.EnumMotorId,
                xAxis.MotorModel,
                xAxis.FullStrokeRange,
                xAxis.Name);
            var yRunner = new MotorWorkflowRunner(
                _reportService,
                _eventAggregator,
                MotorEntity,
                yAxis.EnumMotorId,
                yAxis.MotorModel,
                yAxis.FullStrokeRange,
                yAxis.Name);

            var xTask = xRunner.RunRandomRepeatabilityTestAsync(
                progressReporter: (completed, total, elapsed, eta) =>
                    AppendRandomRepeatProgressLog(xAxis, completed, total, elapsed, eta),
                token);
            var yTask = yRunner.RunRandomRepeatabilityTestAsync(
                progressReporter: (completed, total, elapsed, eta) =>
                    AppendRandomRepeatProgressLog(yAxis, completed, total, elapsed, eta),
                token);
            AppendRandomRepeatLog(xAxis, "随机重复精度：任务已提交，等待完成。");
            AppendRandomRepeatLog(yAxis, "随机重复精度：任务已提交，等待完成。");

            UtilityTools.Modules.MotorTest.Model.RandomRepeatabilityTestResult xResult = null;
            UtilityTools.Modules.MotorTest.Model.RandomRepeatabilityTestResult yResult = null;

            try
            {
                await Task.WhenAll(xTask, yTask).ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
                RandomRepeatabilitySummary = "测试已取消";
                AppendRandomRepeatLog(xAxis, "随机重复精度测试已取消。");
                AppendRandomRepeatLog(yAxis, "随机重复精度测试已取消。");
                return;
            }
            catch (Exception ex)
            {
                AppendRandomRepeatLog(xAxis, $"并行等待异常: {ex.Message}");
                AppendRandomRepeatLog(yAxis, $"并行等待异常: {ex.Message}");
            }

            if (xTask.Status == TaskStatus.RanToCompletion)
                xResult = (UtilityTools.Modules.MotorTest.Model.RandomRepeatabilityTestResult)xTask.Result;
            else if (xTask.IsFaulted)
                AppendRandomRepeatLog(xAxis, $"任务失败: {xTask.Exception?.GetBaseException()?.Message ?? "未知错误"}");
            else if (xTask.IsCanceled)
                AppendRandomRepeatLog(xAxis, "任务已取消。");

            if (yTask.Status == TaskStatus.RanToCompletion)
                yResult = (UtilityTools.Modules.MotorTest.Model.RandomRepeatabilityTestResult)yTask.Result;
            else if (yTask.IsFaulted)
                AppendRandomRepeatLog(yAxis, $"任务失败: {yTask.Exception?.GetBaseException()?.Message ?? "未知错误"}");
            else if (yTask.IsCanceled)
                AppendRandomRepeatLog(yAxis, "任务已取消。");

            if (xResult != null)
            {
                AppendRandomRepeatLog(xAxis,
                    $"结果：完成={xResult.IsTestCompleted}，通过={xResult.IsPassed}，标准差={xResult.AvgStdX:F3}μm，点数={xResult.TargetPoints.Count}，行程记录={xResult.MoveTripsX.Count}，错误={xResult.ErrorDescription ?? "无"}");
            }
            if (yResult != null)
            {
                AppendRandomRepeatLog(yAxis,
                    $"结果：完成={yResult.IsTestCompleted}，通过={yResult.IsPassed}，标准差={yResult.AvgStdX:F3}μm，点数={yResult.TargetPoints.Count}，行程记录={yResult.MoveTripsX.Count}，错误={yResult.ErrorDescription ?? "无"}");
            }

            // 更新界面数据 - X轴
            if (xResult != null)
            {
                foreach (var point in xResult.TargetPoints)
                    RandomTargetPoints.Add(point);

                foreach (var trip in xResult.MoveTripsX)
                    RandomMoveTripsX.Add(trip);

                foreach (var stat in xResult.PointStatsX)
                    RandomPointStatsX.Add(stat);

                foreach (var bin in xResult.DistanceHistogramX)
                    RandomDistanceHistogramX.Add(bin);

                foreach (var bin in xResult.SpeedHistogramX)
                    RandomSpeedHistogramX.Add(bin);

                AppendRandomRepeatLog(xAxis,
                    $"数据写入界面：目标点={xResult.TargetPoints.Count}，统计点={xResult.PointStatsX.Count}，行程={xResult.MoveTripsX.Count}，距离分箱={xResult.DistanceHistogramX.Count}，速度分箱={xResult.SpeedHistogramX.Count}");
            }

            // 更新界面数据 - Y轴
            if (yResult != null)
            {
                foreach (var point in yResult.TargetPoints)
                    RandomTargetPoints.Add(point);

                foreach (var trip in yResult.MoveTripsX)
                    RandomMoveTripsY.Add(trip);

                foreach (var stat in yResult.PointStatsX)
                    RandomPointStatsY.Add(stat);

                foreach (var bin in yResult.DistanceHistogramX)
                    RandomDistanceHistogramY.Add(bin);

                foreach (var bin in yResult.SpeedHistogramX)
                    RandomSpeedHistogramY.Add(bin);

                AppendRandomRepeatLog(yAxis,
                    $"数据写入界面：目标点={yResult.TargetPoints.Count}，统计点={yResult.PointStatsX.Count}，行程={yResult.MoveTripsX.Count}，距离分箱={yResult.DistanceHistogramX.Count}，速度分箱={yResult.SpeedHistogramX.Count}");
            }

            // 更新测试摘要（Y 轴单轴结果的标准差仍记在 AvgStdX）
            RandomRepeatabilitySummary =
                $"完成（并行）：X轴标准差={xResult?.AvgStdX:F3} μm，Y轴标准差={yResult?.AvgStdX:F3} μm\n" +
                $"可点击「生成随机重复精度报告」导出到程序目录：{GetRandomRepeatabilityReportDirectory()}（Word 与同次导出的 CSV）。";

            _lastRandomRepeatabilityXResult = xResult;
            _lastRandomRepeatabilityYResult = yResult;
            GenerateRandomRepeatabilityReportCommand.RaiseCanExecuteChanged();

            AppendRandomRepeatLog(xAxis, "随机重复精度测试全部结束。");
            AppendRandomRepeatLog(yAxis, "随机重复精度测试全部结束。");
        }

        private void AppendRandomRepeatProgressLog(
            FiveAxisModel axis,
            int completed,
            int total,
            TimeSpan elapsed,
            TimeSpan eta)
        {
            int remaining = Math.Max(0, total - completed);
            string elapsedText = elapsed.TotalHours >= 1 ? elapsed.ToString(@"hh\:mm\:ss") : elapsed.ToString(@"mm\:ss");
            string etaText = completed == 0 ? "--:--" : (eta.TotalHours >= 1 ? eta.ToString(@"hh\:mm\:ss") : eta.ToString(@"mm\:ss"));
            double percent = total <= 0 ? 0 : (completed * 100.0 / total);

            AppendRandomRepeatLog(
                axis,
                $"进度：{completed}/{total}（剩余{remaining}）| {percent:F1}% | 已用{elapsedText} | 预计剩余{etaText}");
        }

        private bool CanGenerateRandomRepeatabilityReport()
        {
            return _lastRandomRepeatabilityXResult != null || _lastRandomRepeatabilityYResult != null;
        }

        private void GenerateRandomRepeatabilityReport()
        {
            var reportPath = BuildRandomRepeatabilityWordReport(_lastRandomRepeatabilityXResult, _lastRandomRepeatabilityYResult);
            if (!string.IsNullOrWhiteSpace(reportPath))
            {
                AppendRandomRepeatLogSafe($"随机重复精度报告已生成：{reportPath}");
                RandomRepeatabilitySummary += $"\n最近导出：{reportPath}";
                NotifyRandomRepeatabilityExportFinished(reportPath, success: true);
            }
            else
            {
                AppendRandomRepeatLogSafe("随机重复精度报告生成失败：请查看日志（NLog Error）。");
                NotifyRandomRepeatabilityExportFinished(null, success: false);
            }
        }

        private static void NotifyRandomRepeatabilityExportFinished(string? reportPath, bool success)
        {
            var app = System.Windows.Application.Current;
            if (app?.Dispatcher == null)
                return;
            app.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (success && !string.IsNullOrEmpty(reportPath))
                    {
                        System.Windows.MessageBox.Show(
                            app.MainWindow,
                            $"导出成功。\n\n{reportPath}\n\nWord 含摘要、曲线图与完整表格；另附同目录 CSV（UTF-8 BOM，可用 Excel 打开）。",
                            "随机重复精度报告",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Information);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(
                            app.MainWindow,
                            "导出失败，请查看日志（NLog）。",
                            "随机重复精度报告",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Warning);
                    }
                }
                catch
                {
                    if (success && !string.IsNullOrEmpty(reportPath))
                    {
                        System.Windows.MessageBox.Show(
                            $"导出成功。\n\n{reportPath}",
                            "随机重复精度报告",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Information);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(
                            "导出失败，请查看日志（NLog）。",
                            "随机重复精度报告",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Warning);
                    }
                }
            }));
        }

        /// <summary>
        /// 随机重复精度报告输出目录：<c>程序目录\报告\随机重复精度</c>。
        /// </summary>
        private static string GetRandomRepeatabilityReportDirectory()
        {
            string dir = Path.Combine(AppContext.BaseDirectory, "报告", "随机重复精度");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
        }

        /// <summary>
        /// 导出随机重复精度报告：Word 使用 DocumentFormat.OpenXml（免费）；明细同时写入 CSV。
        /// </summary>
        private string BuildRandomRepeatabilityWordReport(
            RandomRepeatabilityTestResult? xResult,
            RandomRepeatabilityTestResult? yResult)
        {
            try
            {
                if (xResult == null && yResult == null)
                {
                    NLog.LogManager.GetCurrentClassLogger().Warn("无随机重复精度缓存数据，跳过报告导出。");
                    return string.Empty;
                }

                string reportDir = GetRandomRepeatabilityReportDirectory();

                string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                IReadOnlyList<string> csvRelative = ExportRandomRepeatabilityDetailCsvs(reportDir, stamp, xResult, yResult);

                string? positionPng = ExportPlotToPng(reportDir, "random_position_curve", MotorplotModel);
                string? speedPng = ExportPlotToPng(reportDir, "random_speed_curve", MotorSpeedplotModel);

                string? histXDistPng = ExportHistogramToPng(reportDir, $"{stamp}_hist_x_distance", "X轴距离分箱", xResult?.DistanceHistogramX, "分箱区间 (μm)");
                string? histYDistPng = ExportHistogramToPng(reportDir, $"{stamp}_hist_y_distance", "Y轴距离分箱", yResult?.DistanceHistogramX, "分箱区间 (μm)");
                string? histXSpdPng = ExportHistogramToPng(reportDir, $"{stamp}_hist_x_speed", "X轴速度分箱", xResult?.SpeedHistogramX, "分箱区间 (μm/s)");
                string? histYSpdPng = ExportHistogramToPng(reportDir, $"{stamp}_hist_y_speed", "Y轴速度分箱", yResult?.SpeedHistogramX, "分箱区间 (μm/s)");

                string reportPath = Path.Combine(reportDir, $"随机重复精度报告_{stamp}.docx");
                RandomRepeatabilityOpenXmlReport.Save(
                    reportPath,
                    xResult,
                    yResult,
                    positionPng,
                    speedPng,
                    histXDistPng,
                    histYDistPng,
                    histXSpdPng,
                    histYSpdPng,
                    csvRelative);
                return reportPath;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex, "生成随机重复精度 Word 报告失败");
                return string.Empty;
            }
        }

        private static IReadOnlyList<string> ExportRandomRepeatabilityDetailCsvs(
            string reportDir,
            string stamp,
            RandomRepeatabilityTestResult? xResult,
            RandomRepeatabilityTestResult? yResult)
        {
            var names = new List<string>();
            var utf8Bom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

            void WriteCsv<T>(string fileSuffix, IEnumerable<T> rows)
            {
                string path = Path.Combine(reportDir, $"{stamp}_{fileSuffix}");
                using var writer = new StreamWriter(path, false, utf8Bom);
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                csv.WriteRecords(rows);
                names.Add($"{stamp}_{fileSuffix}");
            }

            try
            {
                var targets = xResult?.TargetPoints ?? yResult?.TargetPoints;
                if (targets != null && targets.Count > 0)
                    WriteCsv("TargetPoints.csv", targets);

                if (xResult != null)
                {
                    WriteCsv("X_PointStats.csv", xResult.PointStatsX);
                    WriteCsv("X_MoveTrips.csv", xResult.MoveTripsX);
                    WriteCsv("X_DistanceHistogram.csv", xResult.DistanceHistogramX);
                    WriteCsv("X_SpeedHistogram.csv", xResult.SpeedHistogramX);
                }

                if (yResult != null)
                {
                    WriteCsv("Y_PointStats.csv", yResult.PointStatsX);
                    WriteCsv("Y_MoveTrips.csv", yResult.MoveTripsX);
                    WriteCsv("Y_DistanceHistogram.csv", yResult.DistanceHistogramX);
                    WriteCsv("Y_SpeedHistogram.csv", yResult.SpeedHistogramX);
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex, "导出随机重复精度 CSV 失败");
            }

            return names;
        }

        private string ExportPlotToPng(string outputDir, string prefix, PlotModel? model)
        {
            if (model == null)
                return string.Empty;

            try
            {
                string imagePath = Path.Combine(outputDir, $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
                {
                    var exporter = new PngExporter { Width = 1280, Height = 720 };
                    exporter.ExportToFile(model, imagePath);
                });
                return imagePath;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Warn(ex, $"导出图像失败: {prefix}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 将直方图分箱数据用 OxyPlot 导出为柱状图 PNG（与位置/速度曲线相同的导出尺寸，便于插入 Word）。
        /// </summary>
        private static string ExportHistogramToPng(
            string outputDir,
            string filePrefix,
            string plotTitle,
            IEnumerable<HistogramBinRecord>? bins,
            string binAxisTitle)
        {
            var list = bins?.ToList();
            if (list == null || list.Count == 0)
                return string.Empty;

            try
            {
                string imagePath = Path.Combine(outputDir, $"{filePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
                {
                    var model = new PlotModel { Title = plotTitle };
                    var categoryAxis = new CategoryAxis
                    {
                        Position = AxisPosition.Bottom,
                        Title = binAxisTitle,
                        Angle = 45,
                        GapWidth = 0.35
                    };
                    var valueAxis = new LinearAxis
                    {
                        Position = AxisPosition.Left,
                        Title = "计数",
                        MinimumPadding = 0,
                        AbsoluteMinimum = 0
                    };
                    model.Axes.Add(categoryAxis);
                    model.Axes.Add(valueAxis);

                    var series = new BarSeries
                    {
                        FillColor = OxyColor.FromArgb(220, 66, 165, 245),
                        StrokeColor = OxyColors.DarkBlue,
                        StrokeThickness = 1
                    };
                    for (int i = 0; i < list.Count; i++)
                    {
                        var b = list[i];
                        categoryAxis.Labels.Add($"{b.MinValue:F1}–{b.MaxValue:F1}");
                        series.Items.Add(new BarItem(b.Count));
                    }

                    model.Series.Add(series);
                    var exporter = new PngExporter { Width = 1280, Height = 720 };
                    exporter.ExportToFile(model, imagePath);
                });
                return imagePath;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Warn(ex, $"导出直方图失败: {filePrefix}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 测量轴满行程（保留方法以兼容其他测试）
        /// </summary>
        private async Task<(double minUm, double maxUm)> MeasureAxisFullTravelUmAsync(FiveAxisModel axis, CancellationToken token)
        {
            var testItem = new FullTravelTestItem(axis.FullStrokeRange);
            var result = await testItem.ExecuteAsync(axis.EnumMotorId, axis.MotorModel, MotorEntity, token);
            if (result is not TravelTestResult travel)
                throw new InvalidOperationException($"{axis.Name} 满行程检测返回结果异常。");
            // 无论是否超出标准，都优先采用实测最大/最小位置
            double minUm = Math.Min(travel.RealMinPosUm, travel.RealMaxPosUm);
            double maxUm = Math.Max(travel.RealMinPosUm, travel.RealMaxPosUm);
            if (maxUm - minUm <= 0)
                throw new InvalidOperationException($"{axis.Name} 满行程检测结果无效。");
            return (minUm, maxUm);
        }

        /// <summary>
        /// 随机跳转前：闭环位置、使能、运行（保留方法以兼容其他测试）
        /// </summary>
        private async Task PrepareXyAxesForRandomJumpsAsync(FiveAxisModel xAxis, FiveAxisModel yAxis, CancellationToken token)
        {
            foreach (var axis in new[] { xAxis, yAxis })
            {
                MotorEntity.SetMotorControlModeCommand(axis.EnumMotorId, EnumMotorCtrType.CloseLoopPosCtr);
                MotorEntity.SetMotorEnableCommand(axis.EnumMotorId, EnumMotorEnable.Enable);
            }

            await Task.Delay(120, token);
            foreach (var axis in new[] { xAxis, yAxis })
                MotorEntity.SetMotorOperatingStatusCommand(axis.EnumMotorId, EnumMotorOperatingState.Run);
            await Task.Delay(80, token);
            AppendRandomRepeatLog(xAxis, "已置于闭环位置、使能并已发「运行」，开始下发随机 GOTO。");
            AppendRandomRepeatLog(yAxis, "已置于闭环位置、使能并已发「运行」，开始下发随机 GOTO。");
        }



        /// <summary>
        /// 异步耐久测试
        /// </summary> 
        /// 
        private async void IndependentMotorDurabilityTest()
        {
            // 1. 取消之前的任务并重新创建令牌
            CancellationToken?.Cancel();
            CancellationToken = new CancellationTokenSource();
            var token = CancellationToken.Token;

            try
            {
                // 开启狂暴模式（高频问询状态）
                TestPerformance();

                // 2. 准备所有电机的测试任务（并行起跑）
                var testTasks = new List<Task>();
                foreach (var motor in Motors)
                {
                    // 注意：这里不加 await，直接把任务丢进列表
                    testTasks.Add(RunSingleMotorDurabilityWithCleanup(motor, token));
                }

                // 3. 同时等待所有电机测试结束（或被取消）
                await Task.WhenAll(testTasks);
            }
            catch (OperationCanceledException)
            {
                // 正常取消，不需要处理
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex, "多轴耐久测试发生整体异常");
            }
            finally
            {
                // 4. 收尾：停止所有电机，恢复正常问询频率
                if (Motors != null)
                {
                    foreach (var motor in Motors) motor.StopMotor();
                }
                CloseTestPerformance();
            }
        }

        /// <summary>
        /// 辅助方法：包装单轴的耐久测试，并带有自动清理曲线功能
        /// </summary>
        private async Task RunSingleMotorDurabilityWithCleanup(FiveAxisModel motor, CancellationToken ct)
        {
            // 启动一个后台定时检查，每隔 1 分钟检查一次是否需要清理 30 分钟前的曲线数据
            _ = Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromMinutes(1), ct); // 每分钟查一次

                    var pointList = motor.MotorModel.PointList;
                    if (pointList != null && pointList.Count > 0)
                    {
                        var timeSpan = pointList.Last().Date - pointList[0].Date;
                        if (timeSpan.TotalMinutes > 30)
                        {
                            // 使用 UI 线程安全清理
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                motor.MotorModel.PointList.Clear();
                                motor.MotorModel.SpeedList.Clear();
                            });
                        }
                    }
                }
            }, ct);

            // 调用咱们重构后的轴耐久测试（内部由 Runner 执行）
            await motor.DurabilityTest(ct);
        }

        /// <summary>
        /// 清除监控数据
        /// </summary>
        private void ClearMonitor(string parameter)
        {
            if (parameter == "point" && MotorplotModel.Series.Count > 0)
            {

                foreach (var series in MotorplotModel.Series)
                {
                    var line = series as LineSeries;
                    if (line != null)
                    {
                        line.Points.Clear();
                    }

                    if (Motors != null && Motors.Count > 0)
                    {
                        for (int i = 0; i < Motors.Count; i++)
                        {
                           
                            lock (_lockobj)
                            {
                                Motors[i].MotorModel.PointList.Clear();
                            }

                            MotorplotModel.InvalidatePlot(true);

                        }
                    }
                }
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

                    if (Motors != null && Motors.Count > 0)
                    {
                        for (int i = 0; i < Motors.Count; i++)
                        {
                        
                            Motors[i].MotorModel.SpeedList.Clear();
                        }
                    }
                }
                MotorSpeedplotModel.InvalidatePlot(true);
            }

        }

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
                var channel = (EnumMotorId)(data[0]);//电机通道
              
                if (Motors != null && Motors.Count > 0)
                {
                    for (int i = 0; i < Motors.Count; i++)
                    {
                        if (Motors[i].EnumMotorId == channel)
                        {
                            // 高频状态包直接处理，避免每包 Task.Run 造成额外线程调度开销。
                            Motors[i].Parser_PacketReceivedEvent(e);
                            break;
                        }
                    }
                }
                if (_waitingReply != null && !_waitingReply.Task.IsCompleted)
                {
                    _waitingReply.SetResult("Ready");
                }
                 
            }
        }
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
            else if (parameter == "speed")
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    exporter.ExportToFile(MotorSpeedplotModel, $"{path}\\参数曲线_{timeTip}.png");
                  
                }));
            }

        }
        /// <summary>
        /// 关闭x，y，z的软限位
        /// </summary>
        private void CloseSlimited()
        {
            XAxis?.CloseSlimited();
            YAxis?.CloseSlimited();
            ZAxis?.CloseSlimited();
        }

        /// <summary>
        /// 无扫码快捷键进入全动五轴时：从集合与曲线中移除 Z/T/R，仅保留 X/Y。
        /// </summary>
        private void ApplyUniversalFiveAxisDevShortcutXyOnlyTrim()
        {
            if (Motors == null || Motors.Count == 0)
                return;

            var toRemove = Motors
                .Where(m =>
                    m.MotorModel.MotorParams.MotorModelID != EnumMotorModel.MOTOR_x
                    && m.MotorModel.MotorParams.MotorModelID != EnumMotorModel.MOTOR_y)
                .ToList();

            foreach (var m in toRemove)
            {
                if (m.PosLine != null)
                    MotorplotModel.Series.Remove(m.PosLine);
                if (m.SpeedLine != null)
                    MotorSpeedplotModel.Series.Remove(m.SpeedLine);
                Motors.Remove(m);
            }

            MotorplotModel?.InvalidatePlot(true);
            MotorSpeedplotModel?.InvalidatePlot(true);
        }

        /// <summary>
        /// 数据库加载
        /// </summary>
        private async void SqliteLoad()
        {
            // 1. 确保 MotorTypeModel 不为空，然后使用改名后的 SelectedAxisType
            // 这里的 ?. 和 ?? 是为了防止初始化顺序导致的空引用
            var axisType = MotorTypeModel?.SelectedAxisType ?? Protocol.EnumMotorAxisType.TwoAxisMotor;

            // 2. 调用服务加载数据
            await MotorDataService.LoadFromSqliteAsync(
                Motors,
                axisType,
                HeadIndex,
                LoadSize);

            // 3. 读完通知图表刷新
            MotorplotModel?.InvalidatePlot(true);
            MotorSpeedplotModel?.InvalidatePlot(true);
        }
        // 2. 保存历史数据文件命令
        private void SaveDataFile()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;

            var path = dialog.SelectedPath;
            Task.Run(() =>
            {
                foreach (var motor in Motors)
                    MotorDataService.SaveToJson(path, motor);
            });
        }
        // 3. 读取历史数据文件命令
        private void ReadDataFile()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;

            var path = dialog.SelectedPath;
            Task.Run(() =>
            {
                foreach (var motor in Motors)
                    MotorDataService.LoadFromJson(path, motor);

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    MotorplotModel?.InvalidatePlot(true);
                    MotorSpeedplotModel?.InvalidatePlot(true);
                });
            });
        }
    }

}
