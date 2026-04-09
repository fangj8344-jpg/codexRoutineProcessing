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
using Prism.Ioc;
using Prism.Mvvm;
using ScottPlot.Drawing.Colormaps;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Converters;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Interface;
using UtilityTools.Modules.MotorTest.Entity;
using UtilityTools.Modules.MotorTest.Protocol;
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
            Init();
        }
        private object _lockobj = new object();
        private IContainerProvider _containerProvider;
        private readonly IDialogHostService _dialogHostService;
        private SelfMotorParser _parser;
        private TaskCompletionSource<string> _waitingReply;
        private CancellationTokenSource _queryCts;
        private readonly ITestReportService _reportService;

        private BackgroundWorker _work;
        private bool _isTest = false;
        private EnumMotorInquiry _testMotorId;
        private bool _isSpeedMode = false;
        private int _queryInterval = 500;
  
        private Double _progressValue;
        // 统一类型为 MotorDbContext

        private string _version = "4.0.1";
        /// <summary>
        /// 版本号
        /// </summary>
        public string Version
        {
            get { return _version; }
            set { _version = value; RaisePropertyChanged(); }
        }
        private int _maxCount = 30000;
        public int MaxCount
        {
            get { return _maxCount; }
            set { _maxCount = value; RaisePropertyChanged(); }
        }
       
        public Double ProgressValue
        {
            get { return _progressValue; }
            set { _progressValue = value; RaisePropertyChanged(); }
        }
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
        /// 速度模式移动的速度大小
        /// </summary>
        public int MagnitudeOfSpeed = 10000;
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

        public DelegateCommand<string> TestMotorTogetherCommand { get; set; }
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
            ByteQueue = new ConcurrentQueue<byte[]>();
            ImportantByteQueue = new ConcurrentQueue<byte[]>();
            MotorEntity = new MotorEntity(SerialPortService, NetUdpService);
            MotorplotModel = new PlotModel();
            MotorplotModel.Legends.Add(new Legend());
            MotorSpeedplotModel = new PlotModel();
            MotorSpeedplotModel.Legends.Add(new Legend());
            MotorplotModel.Axes.Add(new DateTimeAxis() { Title = "时间", Position = AxisPosition.Bottom });
            MotorplotModel.Axes.Add(new LinearAxis() { Title = "位置(μm)", Position = AxisPosition.Left });
            MotorSpeedplotModel.Axes.Add(new DateTimeAxis() { Title = "时间", Position = AxisPosition.Bottom });
            MotorSpeedplotModel.Axes.Add(new LinearAxis() { Title = "速度(μm/s)", Position = AxisPosition.Left });
            MachineProfile targetProfile = MachineProfile.StandardTwoAxis;
            if (_reportService.MotorKindObj is MachineProfile parsedProfile)
            {
                targetProfile = parsedProfile;
            }
           
            MotorTypeModel = new MotorTypeModel(this, _containerProvider, targetProfile);

            var uiRenderTimer = new System.Windows.Threading.DispatcherTimer();
            uiRenderTimer.Interval = TimeSpan.FromMilliseconds(50); // 50ms 刷新一次，即 20 FPS，极其丝滑且不占 CPU
            uiRenderTimer.Tick += (s, e) =>
            {
                // 统一在这里触发图表重绘
                if (MotorplotModel != null)
                    MotorplotModel.InvalidatePlot(true);

                if (MotorSpeedplotModel != null)
                    MotorSpeedplotModel.InvalidatePlot(true);
            };
            uiRenderTimer.Start(); // 启动定时器

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

            if (Motors != null)
            {
                foreach (var motor in Motors)
                {
                    motor.StopMotor();
                }
            }
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
        private async void Parser_PacketReceivedEvent(object? sender, SelfMotorPacket e)
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
                            await Task.Run(() =>
                            {
                                Motors[i].Parser_PacketReceivedEvent(e);
                            });
                           
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
            XAxis.CloseSlimited();
            YAxis.CloseSlimited();
            ZAxis.CloseSlimited();
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
