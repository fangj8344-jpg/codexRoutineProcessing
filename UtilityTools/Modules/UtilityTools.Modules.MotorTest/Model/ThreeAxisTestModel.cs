using CsvHelper;
using MathNet.Numerics;
using Newtonsoft.Json;
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
using UtilityTools.Core.Dialog;
using UtilityTools.Modules.MotorTest.Entity;
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
        private object _lockobj = new object();
        private IContainerProvider _containerProvider;
        private readonly IDialogHostService _dialogHostService;
        private SelfMotorParser _parser;
        private TaskCompletionSource<string> _waitingReply;

        // 这是一个异步陷阱，专门等 Parser 告诉它电机 Ready 了
        private TaskCompletionSource<bool> _motorReadyTcs;

        private BackgroundWorker _work;
        private bool _isTest = false;
        private EnumMotorInquiry _testMotorId;
        private bool _isSpeedMode = false;
        private int _queryInterval = 500;
        private bool _isQueryInterval = true;
        private Double _progressValue;
        private FiveAxisDbContextBase _fiveAxisDbContextBase;
       
        private FiveAxisDbContextBase _twoAxisDbContextBase;
        private string _version = "3.3.0";
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
        private IAsynRWService _serialPortService;
        /// <summary>
        /// 串口异步通信服务
        /// </summary>
        public IAsynRWService SerialPortService
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
        private void Init()
        {
            ClearMonitorCommand = new DelegateCommand<string>(ClearMonitor);
            TestMotorTogetherCommand = new DelegateCommand<string>(TestMotorTogether);
            IndependentMotortestCommand = new DelegateCommand<string>(IndependentMotortest);
            AutoAdjustCommand = new DelegateCommand<string>(AutoAdjust);
            SaveToFileCommand = new DelegateCommand<string>(SaveToFile);
            IndependentMotorDurabilityTestCommand = new DelegateCommand(IndependentMotorDurabilityTest);
            ShotDownCommand = new DelegateCommand(ShotDown);

            SQLiteTestCommand = new DelegateCommand(SQLiteTest);
            TestPerformanceCommand = new DelegateCommand(TestPerformance);
            CloseTestPerformanceCommand = new DelegateCommand(CloseTestPerformance);
            SaveDataFileCommand = new DelegateCommand(SaveDataFile);
            ReadDataFileCommand = new DelegateCommand(ReadDataFile);
            CloseSlimitedCommand = new DelegateCommand(CloseSlimited);
            SqliteLoadCommand = new DelegateCommand(SqliteLoad);
            ByteQueue = new ConcurrentQueue<byte[]>();
            ImportantByteQueue = new ConcurrentQueue<byte[]>();
            MotorEntity = new MotorEntity(ImportantByteQueue, ByteQueue);
            MotorplotModel = new PlotModel();
            MotorplotModel.Legends.Add(new Legend());
            MotorSpeedplotModel = new PlotModel();
            MotorSpeedplotModel.Legends.Add(new Legend());
            MotorplotModel.Axes.Add(new DateTimeAxis() { Title = "时间", Position = AxisPosition.Bottom });
            MotorplotModel.Axes.Add(new LinearAxis() { Title = "位置(脉冲)", Position = AxisPosition.Left });
            MotorSpeedplotModel.Axes.Add(new DateTimeAxis() { Title = "时间", Position = AxisPosition.Bottom });
            MotorSpeedplotModel.Axes.Add(new LinearAxis() { Title = "速度(脉冲/秒)", Position = AxisPosition.Left });
            
            MotorTypeModel = new MotorTypeModel(this, _containerProvider);
     
        }

       
        public DelegateCommand SQLiteTestCommand { get; set; }
        int plontPoint = 0;
 

        private void SQLiteTest()
        {
            //using var context = new AppDbContext();
            ////确保数据库已经建立(如果不存在会自动创建)
            //context.Database.EnsureCreated();
            ////新增数据
            //context.XPlotViewPointMessages.Add(new PlotViewPointMessage()  { Date = new DateTime(), Point = plontPoint++ } );

            //context.SaveChanges();
            //var List = context.XPlotViewPointMessages.ToList();
            //foreach (var message in List) 
            //{
            //    Console.WriteLine(message.Date);
            //}


        }
        public DelegateCommand TestPerformanceCommand { get; set; }
        private void TestPerformance()
        {
            _queryInterval = 10;
        }
        /// <summary>
        /// 问询状态
        /// </summary>
        public void QueryStatusTask()
        {
            _isQueryInterval = true;
            try
            {
                Task.Run(() =>
                {
                    while (_isQueryInterval)
                    {
                        if (_byteQueue.IsEmpty)
                        {
                            for (int i = 0; i < Motors.Count; i++)
                            {
                                _motorEntity.GetMotorStatusCommand(Motors[i].EnumMotorId);
                            }
                        }
                        Thread.Sleep(_queryInterval);
                    }
                });
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex);
            }
        }
        public void CloseQueryStatusTask()
        {
            _isQueryInterval = false;
        }
        public DelegateCommand CloseTestPerformanceCommand { get; set; }
        private void CloseTestPerformance()
        {
            _queryInterval = 500;
        }

        public DelegateCommand<string> TestMotorTogetherCommand { get; set; }
        /// <summary>
        /// 每个轴基础测试一起测试(并行测试)
        /// </summary>
        private  void TestMotorTogether(string testModel)
        {
            try
            {
                CancellationToken = new CancellationTokenSource(); 
                   TestPerformance();
                   if (Motors != null && Motors.Count > 0)
                   {
                       for (int i = 0; i < Motors.Count; i++)
                       {
                           int index = i;
                           switch (testModel)
                           {
                               case "BaseTest":
                                   Task.Run(() =>
                                {
                                    Motors[index].BaseTest(CancellationToken.Token);
                                }, CancellationToken.Token);
                                   break;
                               case "TestSmoothnessDetection":
                                   Task.Run(() =>
                                   {
                                       Motors[index].TestSmoothnessDetection(CancellationToken.Token);
                                   }, CancellationToken.Token); break;
                               case "DurabilityTest":
                                   Task.Run(() =>
                                   {
                                       Motors[index].DurabilityTest(CancellationToken.Token);
                                   }, CancellationToken.Token); break;
                            case "TotalJourneyGeneralMotorTestDetectionion":
                                Task.Run(() =>
                                {
                                    Motors[index].TotalJourneyGeneralMotorTestDetectionion(CancellationToken.Token);
                                }, CancellationToken.Token); break;
                            case "PositioningAccuracyGeneralMotorTestDetectionion":
                                Task.Run(() =>
                                {
                                    Motors[index].PositioningAccuracyGeneralMotorTestDetectionion(CancellationToken.Token);
                                }, CancellationToken.Token); break;

                        }
                       }
                   } 
            }
            catch (Exception OperationCanceledException) 
            {

                NLog.LogManager.GetCurrentClassLogger().Error($"并行测试异常{OperationCanceledException}");
                for (int j = 0; j < Motors.Count; j++)
                {
                    Motors[j].StopMotor();
                }
            }
         
        }
        public DelegateCommand<string> IndependentMotortestCommand { get; set; }
        /// <summary>
        /// 一个轴测试完后下一个轴测试
        /// </summary>
        private async void IndependentMotortest(string testModel)
        {
            TestPerformance();
            try
            {
                CancellationToken = new CancellationTokenSource();
                if (Motors != null && Motors.Count > 0)
                {
                    for (int i = 0; i < Motors.Count; i++)
                    {
                        await Task.Run(async () =>
                        {
                            
                                switch (testModel)
                                {
                                    case "BaseTest": await Motors[i].BaseTest(CancellationToken.Token); break;
                                    case "TestSmoothnessDetection": await Motors[i].TestSmoothnessDetection(CancellationToken.Token); break;
                                    case "TotalJourneyGeneralMotorTestDetectionion": await Motors[i].TotalJourneyGeneralMotorTestDetectionion(CancellationToken.Token); break;
                                case "PositioningAccuracyGeneralMotorTestDetectionion": await Motors[i].PositioningAccuracyGeneralMotorTestDetectionion(CancellationToken.Token); break;
                            }
                        }, CancellationToken.Token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                for (int j = 0; j < Motors.Count; j++)
                {
                    int index = j;
                    Motors[index].StopMotor();
                }
            }
            CloseTestPerformance();
        }
        public DelegateCommand ShotDownCommand { get; set; }
        private void ShotDown()
        {
            if (CancellationToken != null)
            {
                CancellationToken.Cancel();
            }
        }
        public DelegateCommand IndependentMotorDurabilityTestCommand { get; set; }
        /// <summary>
        /// 异步耐久测试
        /// </summary> 
        private async void IndependentMotorDurabilityTest()
        {
            CancellationToken = new CancellationTokenSource();
            while (!CancellationToken.IsCancellationRequested)
            {
                
                if (Motors != null && Motors.Count > 0)
                {
                    
                    for (int i = 0; i < Motors.Count; i++)
                    {
                        int index = i;
                        try
                        {
                            await Task.Run(async () =>
                            {
                                await Motors[index].SmoothnessGeneralMotorTestDetectionion(CancellationToken.Token);
                            }, CancellationToken.Token);
                            var time = Motors[index].MotorModel.PointList.Last().Date -  Motors[i].MotorModel.PointList[0].Date;
                            var min = time.TotalMinutes;
                            if (min > 30)
                            {
                                Motors[index].MotorModel.PointList.Clear();
                                Motors[index].MotorModel.SpeedList.Clear();
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            for (int j = 0; j < Motors.Count; j++)
                            {
                                Motors[j].StopMotor();
                            }
                        }
                    }
                }
            }
        }


        public DelegateCommand<string> ClearMonitorCommand { get; set; }
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
            else if (parameter == "speed")
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    exporter.ExportToFile(MotorSpeedplotModel, $"{path}\\参数曲线_{timeTip}.png");
                  
                }));
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
        public DelegateCommand SqliteLoadCommand { get; set; }
        /// <summary>
        /// 数据库加载
        /// </summary>
        private async void SqliteLoad()
        {
            int pointNumber, speedNumber;
            ObservableCollection<PlotViewPointMessage> point;
            ObservableCollection<PlotViewSpeedMessage> speed;
            if (MotorTypeModel.EnumMotorAxisType == EnumMotorAxisType.TwoAxisMotor)
            {
                using (var db = new TwoAxisDbContextBase())
                {
                    TotalSize = await SpliteOperate.GetPlotViewPointMessageCountAsync(db);
                    pointNumber = await SpliteOperate.GetPlotViewPointMessageCountAsync(db);
                    point = new ObservableCollection<PlotViewPointMessage>(await SpliteOperate.GetPlotViewPointMessagesAsync(db, HeadIndex, LoadSize > pointNumber ? pointNumber : LoadSize));
                    speedNumber = await SpliteOperate.GetPlotViewSpeedMessageCountAsync(db);
                    speed = new ObservableCollection<PlotViewSpeedMessage>(await SpliteOperate.GetPlotViewSpeedMessagesAsync(db, HeadIndex, LoadSize > speedNumber ? speedNumber : LoadSize)) ;
                } 
            }
            else
            {
                using (var db = new FiveAxisDbContextBase())
                {
                    TotalSize = await SpliteOperate.GetPlotViewPointMessageCountAsync(db);
                    pointNumber = await SpliteOperate.GetPlotViewPointMessageCountAsync(db);
                    point = new ObservableCollection<PlotViewPointMessage>(await SpliteOperate.GetPlotViewPointMessagesAsync(db, HeadIndex, LoadSize > pointNumber ? pointNumber : LoadSize)); 
                    speedNumber = await SpliteOperate.GetPlotViewSpeedMessageCountAsync(db);
                    speed = new ObservableCollection<PlotViewSpeedMessage>(await SpliteOperate.GetPlotViewSpeedMessagesAsync(db, HeadIndex, LoadSize > speedNumber ? speedNumber : LoadSize)); 
                }  
            }
            ObservableCollection<PlotViewPointMessage> xPointList = new ObservableCollection<PlotViewPointMessage>(point.Where(m => m.MotorModelAxis == EnumMotorModel.MOTOR_x).ToList());
            ObservableCollection<PlotViewPointMessage> yPointList = new ObservableCollection<PlotViewPointMessage>(point.Where(m => m.MotorModelAxis == EnumMotorModel.MOTOR_y).ToList());
            ObservableCollection<PlotViewPointMessage> zPointList = new ObservableCollection<PlotViewPointMessage>(point.Where(m => m.MotorModelAxis == EnumMotorModel.MOTOR_z).ToList());
            ObservableCollection<PlotViewPointMessage> tPointList = new ObservableCollection<PlotViewPointMessage>(point.Where(m => m.MotorModelAxis == EnumMotorModel.MOTOR_t).ToList());
            ObservableCollection<PlotViewPointMessage> rPointList = new ObservableCollection<PlotViewPointMessage>(point.Where(m => m.MotorModelAxis == EnumMotorModel.MOTOR_r).ToList());

            ObservableCollection<PlotViewSpeedMessage> xSpeedList = new ObservableCollection<PlotViewSpeedMessage>(speed.Where(m => m.MotorModelAxis == EnumMotorModel.MOTOR_x).ToList());
            ObservableCollection<PlotViewSpeedMessage> ySpeedList = new ObservableCollection<PlotViewSpeedMessage>(speed.Where(m => m.MotorModelAxis == EnumMotorModel.MOTOR_y).ToList());
            ObservableCollection<PlotViewSpeedMessage> zSpeedList = new ObservableCollection<PlotViewSpeedMessage>(speed.Where(m => m.MotorModelAxis == EnumMotorModel.MOTOR_z).ToList());
            ObservableCollection<PlotViewSpeedMessage> tSpeedList = new ObservableCollection<PlotViewSpeedMessage>(speed.Where(m => m.MotorModelAxis == EnumMotorModel.MOTOR_t).ToList());
            ObservableCollection<PlotViewSpeedMessage> rSpeedList = new ObservableCollection<PlotViewSpeedMessage>(speed.Where(m => m.MotorModelAxis == EnumMotorModel.MOTOR_r).ToList()); 
            List<ObservableCollection<PlotViewPointMessage>> pointList = new List<ObservableCollection<PlotViewPointMessage>>();
            pointList.Add(xPointList);
            pointList.Add(yPointList);
            pointList.Add(zPointList);
            pointList.Add(tPointList);
            pointList.Add(rPointList);
            List<ObservableCollection<PlotViewSpeedMessage>> speedList = new List<ObservableCollection<PlotViewSpeedMessage>>();
            speedList.Add(xSpeedList);
            speedList.Add(ySpeedList);
            speedList.Add(zSpeedList);
            speedList.Add(tSpeedList);
            speedList.Add(rSpeedList);
            if (Motors != null && Motors.Count > 0)
            {
                  
                for (int i = 0; i < Motors.Count; i++)  
                {
                
                    Motors[i].MotorModel.PointList = pointList[i];
                    Motors[i].PosLine.ItemsSource = Motors[i].MotorModel.PointList;
                    Motors[i].PosLine.DataFieldX = "Date";
                    Motors[i].PosLine.DataFieldY = "Point";

                    Motors[i].MotorModel.SpeedList = speedList[i];
                    Motors[i].SpeedLine.ItemsSource = Motors[i].MotorModel.SpeedList;
                    Motors[i].SpeedLine.DataFieldX = "SpeedDate";
                    Motors[i].SpeedLine.DataFieldY = "Speed";
                }
            }
            MotorSpeedplotModel.InvalidatePlot(true);
        }
    }
}
