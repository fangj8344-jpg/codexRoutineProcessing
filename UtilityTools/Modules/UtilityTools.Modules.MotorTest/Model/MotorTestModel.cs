using MathNet.Numerics.Statistics;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp.Features2D;
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Extension;
using UtilityTools.Modules.MotorTest.Protocol;
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
            netUdp.DeviceInstance.TargetPort = 5002;
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
        private Queue<byte[]> _byteQueue;
        private Queue<byte[]> _importantByteQueue;

        public System.Timers.Timer _getMotorStateTimer;
        private System.Timers.Timer _getMotorPosTimer;
        private System.Timers.Timer _StallDetectionTimer;
        private TaskCompletionSource<string> _limitedtcs1;
        private TaskCompletionSource<string> _waitingReply;
        private TaskCompletionSource<string> _stallDetectiotcs;
        private List<int> timePosStallDetectionList;
        private Stopwatch _stopwatch;
        private Stopwatch _testTime;
        private long elapsedTime;
        private int[] _limtedPos;
        private BackgroundWorker _work;
        private bool _isPerformance;

        private List<PlotViewPointMessage> _xPlotViewPointMessage;
        private List<PlotViewPointMessage> _yPlotViewPointMessage;

        private List<PlotViewSpeedMessage> _xPlotViewSpeedMessage;
        private List<PlotViewSpeedMessage> _yPlotViewSpeedMessage;

        LineSeries _xPosLine;
        LineSeries _yPosLine;
        LineSeries _xSpeedPosLine;
        LineSeries _ySpeedPosLine;
        private double _std;
        public double Std
        {
            get {return _std;}
            set { _std = value;RaisePropertyChanged();  }
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
            set { _motorTypeMessage = value;  RaisePropertyChanged(); }
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
            set { _yPointPos = value;RaisePropertyChanged(); }
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
            set { _yPointSpeed = value;RaisePropertyChanged(); }
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

        private void Init()
        {
            MotorModelX = new MotorModel();
            MotorModelY = new MotorModel();
            MotorplotModel = new PlotModel();
            MotorSpeedplotModel = new PlotModel();
            _xPlotViewPointMessage = new List<PlotViewPointMessage>();
            _yPlotViewPointMessage = new List<PlotViewPointMessage>();

            _xPlotViewSpeedMessage = new List<PlotViewSpeedMessage>();
            _yPlotViewSpeedMessage = new List<PlotViewSpeedMessage>();

            MotorTestMessages = new ObservableCollection<MotorTestMessage>();
            MotorTypeMessage = new ObservableCollection<MotorTypeMessage>
            {
                { new MotorTypeMessage() { Name = "Zem15C" ,MinStrokeRange = 120000,MaxStrokeRange=130000} },
                { new MotorTypeMessage() { Name = "Zem18" ,MinStrokeRange = 120000,MaxStrokeRange=130000} },
                { new MotorTypeMessage() { Name = "Zem20" ,MinStrokeRange = 120000,MaxStrokeRange=130000} }
            }; 
            _TestMessage = new MotorTestMessage();
           
            timePosStallDetectionList = new List<int>();
            MotorTestMessages.Add(_TestMessage);

            TestAllCommand = new DelegateCommand(TestAll);
            MoveCommand = new DelegateCommand<string>(Move);
            ClearLogCommand = new DelegateCommand(ClearLog);
            GetMotorStatusCommand = new DelegateCommand(GetMotorStatus);
            AutoAdjustCommand = new DelegateCommand<string> (AutoAdjust);
            ClearMonitorCommand = new DelegateCommand<string> (ClearMonitor);
            AutomaticZeroInitializationCommand = new DelegateCommand (AutomaticZeroInitialization);
            SetMotorLimitEnableCommand = new DelegateCommand<string>(SetMotorLimitEnable);
            GetMotorLimitEnableCommand = new DelegateCommand(GetMotorLimitEnable);
            TestPerformanceCommand = new DelegateCommand(TestPerformance);
            NoPerformanceCommand = new DelegateCommand(NoPerformance);
            SaveToFileCommand = new DelegateCommand<string>(SaveToFile);
            TestSmoothnessDetectionCommand = new DelegateCommand(TestSmoothnessDetection);
            SerilizeCommand = new DelegateCommand(Serilize);
            DeserilizeCommand = new DelegateCommand(Deserilize);
            Log = new ObservableCollection<string>();
            MyCustomEvent += ParserMotorStatus;
            _stopwatch = new Stopwatch();

            MotorSpeedplotModel.Legends.Add(new Legend());
            MotorSpeedplotModel.Axes.Add(new DateTimeAxis() { Title = "时间", Position = AxisPosition.Bottom });
            MotorSpeedplotModel.Axes.Add(new LinearAxis() { Title = "速度(脉冲/秒)", Position = AxisPosition.Left });
            _xSpeedPosLine = new LineSeries() { Title = "x轴", RenderInLegend = true };
            _ySpeedPosLine = new LineSeries() { Title = "y轴", RenderInLegend = true };
            _xSpeedPosLine.ItemsSource = _xPlotViewSpeedMessage;
            _ySpeedPosLine.ItemsSource = _yPlotViewSpeedMessage;
            _xSpeedPosLine.DataFieldX = "SpeedDate";
            _xSpeedPosLine.DataFieldY = "Speed";

            _ySpeedPosLine.DataFieldX = "SpeedDate";
            _ySpeedPosLine.DataFieldY = "Speed";

            MotorSpeedplotModel.Series.Add(_xSpeedPosLine);
            MotorSpeedplotModel.Series.Add(_ySpeedPosLine);

            MotorplotModel.Legends.Add(new Legend());
            MotorplotModel.Axes.Add(new DateTimeAxis() { Title = "时间", Position = AxisPosition.Bottom });
            MotorplotModel.Axes.Add(new LinearAxis() { Title = "位置(脉冲)", Position = AxisPosition.Left });
            _xPosLine = new LineSeries() { Title = "x轴", RenderInLegend = true };
            _yPosLine = new LineSeries() { Title = "y轴", RenderInLegend = true };
            _xPosLine.ItemsSource = _xPlotViewPointMessage;
            _yPosLine.ItemsSource = _yPlotViewPointMessage;
            _xPosLine.DataFieldX = "Date";
            _xPosLine.DataFieldY = "Point";
            _yPosLine.DataFieldX = "Date";
            _yPosLine.DataFieldY = "Point";
      
            MotorplotModel.Series.Add(_xPosLine);
            MotorplotModel.Series.Add(_yPosLine);



          
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
                case "up":; Goto(EnumMotorId.MOTOR_2, 5000000); break;
                case "down": Goto(EnumMotorId.MOTOR_2, -5000000); break;
                case "left": Goto(EnumMotorId.MOTOR_1, -5000000); break;
                case "right": Goto(EnumMotorId.MOTOR_1, 5000000); break;
                case "stop":
                    {
                        var xstopcmd = SelfMotorProtocol.SetMotorOperatingStatus(EnumMotorId.MOTOR_1, EnumMotorOperatingState.Stop);
                        var ystopcmd = SelfMotorProtocol.SetMotorOperatingStatus(EnumMotorId.MOTOR_2, EnumMotorOperatingState.Stop);
                        SendImportantData(xstopcmd);
                        SendImportantData(ystopcmd);
                    } break;
            }
        }
        public DelegateCommand TestPerformanceCommand { get; set; }
        private  void TestPerformance()
        {
            _isPerformance = true;
            Task.Run(() =>
            {
                while (_isPerformance)
                {
                    if (_byteQueue.Count == 0)
                    {
                        var getMotor2Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_2);
                        SendData(getMotor2Statuscmd);
                        var getMotor1Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_1);
                        SendData(getMotor1Statuscmd);
                    }
                    Thread.Sleep(1); 
                    
                }
            });
           
        }
        public DelegateCommand NoPerformanceCommand { get; set; }
        private void NoPerformance()
        {
            _isPerformance = false; ;
            
        }
        public DelegateCommand TestSmoothnessDetectionCommand { get; set; }
        private async void TestSmoothnessDetection()
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
            for (int i = 0; i < 60; i++)
            {

               bool result1 = await SmoothnessDetection(EnumMotorId.MOTOR_1);
               bool result2 = await SmoothnessDetection(EnumMotorId.MOTOR_2);
               if (result1 == false || result2 == false)
               {
                    _dialogHostService.Information("提示", "丝杆顺滑度测试错误");
                    return;
               }
               if(_testTime.ElapsedMilliseconds /1000.0 / 60 > 60)
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
        public DelegateCommand TestAllCommand { get; set; }
        private async void TestAll()
        {
            MotorTestMessages.Clear();
            SetMotorInit(EnumMotorId.MOTOR_1);
            SetMotorInit(EnumMotorId.MOTOR_2);
         
            Goto(EnumMotorId.MOTOR_2, -1000000);
            Goto(EnumMotorId.MOTOR_1, -1000000);
            Thread.Sleep(4000);
            var xstopcmd = SelfMotorProtocol.SetMotorOperatingStatus(EnumMotorId.MOTOR_1, EnumMotorOperatingState.Stop);
            var ystopcmd = SelfMotorProtocol.SetMotorOperatingStatus(EnumMotorId.MOTOR_2, EnumMotorOperatingState.Stop);
            SendImportantData(xstopcmd);
            SendImportantData(ystopcmd);


          

           

        
            var result = await TestMotorMove();
            if (result != true)
            {
                DispathcherInvoke($"电机移动测试失败，测试退出");
                _dialogHostService.Information("提示","请先修复电机移动问题");
           
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
                if (_xPlotViewPointMessage.Count <= 0 || _xPlotViewPointMessage.Count <= 0)
                {
                    DispathcherInvoke($"没有读取打电机位置，测试退出");
                    return;
                }
                var xposStart = _xPlotViewPointMessage[_xPlotViewPointMessage.Count - 1].Point;
                var yposStart = _yPlotViewPointMessage[_yPlotViewPointMessage.Count - 1].Point;
                var xgotocmd = SelfMotorProtocol.SetMotorGoTo(EnumMotorId.MOTOR_1, EnumMotorUnit.Pulse, 1000000);
                var ygotocmd = SelfMotorProtocol.SetMotorGoTo(EnumMotorId.MOTOR_2, EnumMotorUnit.Pulse, 1000000);
                SendImportantData(xgotocmd);
                SendImportantData(ygotocmd);
                Thread.Sleep(3000);
                var xStopcmd = SelfMotorProtocol.SetMotorOperatingStatus(EnumMotorId.MOTOR_1, EnumMotorOperatingState.Stop);
                var yStopcmd = SelfMotorProtocol.SetMotorOperatingStatus(EnumMotorId.MOTOR_2, EnumMotorOperatingState.Stop);
                SendImportantData(xStopcmd);
                SendImportantData(yStopcmd);
                Thread.Sleep(3000);
                var xposEnd = _xPlotViewPointMessage[_xPlotViewPointMessage.Count - 1].Point;
                var yposEnd = _yPlotViewPointMessage[_yPlotViewPointMessage.Count - 1].Point;
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
                if (_byteQueue.Count == 0)
                {
                    var getMotor2Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_2);
                    SendData(getMotor2Statuscmd);
                    var getMotor1Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_1);
                    SendData(getMotor1Statuscmd);
                }
                

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
                var xposStart = _xPlotViewPointMessage[_xPlotViewPointMessage.Count - 1].Point;
                var yposStart = _yPlotViewPointMessage[_yPlotViewPointMessage.Count - 1].Point;
                var xgotocmd = SelfMotorProtocol.SetMotorGoTo(EnumMotorId.MOTOR_1, EnumMotorUnit.Pulse, 100000);
                var ygotocmd = SelfMotorProtocol.SetMotorGoTo(EnumMotorId.MOTOR_2, EnumMotorUnit.Pulse, 100000);
                SendImportantData(xgotocmd);
                SendImportantData(ygotocmd);
                Thread.Sleep(5000);
                var xStopcmd = SelfMotorProtocol.SetMotorOperatingStatus(EnumMotorId.MOTOR_1, EnumMotorOperatingState.Stop);
                var yStopcmd = SelfMotorProtocol.SetMotorOperatingStatus(EnumMotorId.MOTOR_2, EnumMotorOperatingState.Stop);
                SendImportantData(xStopcmd);
                SendImportantData(yStopcmd);
                Thread.Sleep(2000);
                var xposEnd = _xPlotViewPointMessage[_xPlotViewPointMessage.Count - 1].Point;
                var yposEnd = _yPlotViewPointMessage[_yPlotViewPointMessage.Count - 1].Point;
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
                pos = _xPlotViewPointMessage;
            }
            else
            {
                pos = _yPlotViewPointMessage;
            }
            var posStart = pos[pos.Count - 1].Point;
            var posEnd = pos[pos.Count - 1].Point;
            await Task.Run(async () =>
            {
              
                Goto(enumMotorId, 1000000);
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
                        var stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
                        SendImportantData(stopcmd);
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
                catch(Exception ex)
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
                var gotocmd = SelfMotorProtocol.SetMotorGoTo(enumMotorId, EnumMotorUnit.Pulse, -1000000);
                SendImportantData(gotocmd);
                Goto(enumMotorId, -1000000);
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
                        var stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
                        SendImportantData(stopcmd);
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
                catch(Exception ex)
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
                
                SetMotorInit(enumMotorId);
                var gotocmd = SelfMotorProtocol.SetMotorGoTo(enumMotorId, EnumMotorUnit.Pulse, 1000000);
                SendImportantData(gotocmd);
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
                        var stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
                        SendImportantData(stopcmd);
                    }
                    else
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = result;

                    }
                }
                catch(Exception ex)
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
                SetMotorInit(enumMotorId);
                var gotocmd = SelfMotorProtocol.SetMotorGoTo(enumMotorId, EnumMotorUnit.Pulse, -1000000);
                SendImportantData(gotocmd);
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
                        var stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
                        SendImportantData(stopcmd);

                    }
                    else if (result == "PhyBackwardLimited")
                    {
                       
                        posMessage.negativeLimitPositionendPos = motorModel.MotorParams.Pos;
                        int fullStrokeOfMotor = posMessage.positiveLimitPosition - posMessage.negativeLimitPositionendPos;
                        if ((fullStrokeOfMotor  < 170000 && fullStrokeOfMotor > 160000) 
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

                SetMotorInit(enumMotorId);
                var gotocmd = SelfMotorProtocol.SetMotorGoTo(enumMotorId, EnumMotorUnit.Pulse, 1000000);
                SendImportantData(gotocmd);
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
                        var stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
                        SendImportantData(stopcmd);
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
                SetMotorInit(enumMotorId);
                var gotocmd = SelfMotorProtocol.SetMotorGoTo(enumMotorId, EnumMotorUnit.Pulse, -1000000);
                SendImportantData(gotocmd);
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
                        var stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
                        SendImportantData(stopcmd);

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
  
        private async Task<bool> SmoothnessDetection(EnumMotorId enumMotorId)
        {
            int startIndex = 0, endIndex = 0;
            List<PlotViewSpeedMessage> tempSpeedList;
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
                tempSpeedList = _xPlotViewSpeedMessage;
            }
            else
            {
                TestMessage.TestProject = "Y轴丝杆顺滑度测试";
                motorModel = MotorModelY;
                tempSpeedList = _yPlotViewSpeedMessage; 
            }
            string result = "";
            await Task.Run(async () =>
            {
                //单开线程去测试堵转和限位
                _limitedtcs1 = new TaskCompletionSource<string>();
                GotoInSpeedMode(enumMotorId,10000);
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
                        DispathcherInvoke($"电机测试丝杆测试堵转");

                    }
                    else if (result == "PhyForwardLimited")
                    {

                        

                    }
                    else if (result == "PhyBackwardLimited")
                    {

                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "正限位反向";
                        var stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
                        SendImportantData(stopcmd);
                        DispathcherInvoke($"电机测试丝杆正限位反向");
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
            });
            if (result != "PhyForwardLimited")
            {
               
                return false;
            }

            await Task.Run(async () =>
            {
                //单开线程去测试堵转和限位
                GotoInSpeedMode(enumMotorId, -10000);
                startIndex = tempSpeedList.Count;
                Thread.Sleep(5000);
                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();
                    Task.Run(() => { MotorStallDetection(enumMotorId); });
                    string result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(120)); 
                    if (result == "stall")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "电机堵转，请检查";
                        DispathcherInvoke($"电机测试丝杆堵转");
                    }
                    else if (result == "PhyForwardLimited")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "负限位反向";
                        var stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
                        SendImportantData(stopcmd);
                        DispathcherInvoke($"电机测试负限位反向");
                    }
                    else if (result == "PhyBackwardLimited")
                    {
                        //正常的情况下，需要记录前面运行过程中的点数
                        endIndex = tempSpeedList.Count;
                       
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

            });
            if (result != "PhyBackwardLimited")
            {
                TestMessage.Description = $"从正方向到负方向的位置丝杆顺滑度错误";
              
            }
            else
            {
                if (!SmoothnessTest(startIndex, endIndex, tempSpeedList))
                {

                    TestMessage.TestResult = "不合格";
                    TestMessage.TestValue = "不正常";
                    TestMessage.StandardValue = "正常";
                    TestMessage.Description = $"正方向到负方向丝杆顺滑度不达标，请重新安装";
                    return false; 
                }
            }
            await Task.Run(async () =>
            {
                //单开线程去测试堵转和限位
                SetMotorInit(enumMotorId);
                var gotocmd = SelfMotorProtocol.SetMotorGoTo(enumMotorId, EnumMotorUnit.Pulse, 10000);
                SendImportantData(gotocmd);
                startIndex = tempSpeedList.Count;
                Thread.Sleep(5000);
                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();
                    Task.Run(() => { MotorStallDetection(enumMotorId); });
                    string result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(120));
                    if (result == "stall")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "电机堵转，请检查";
                        DispathcherInvoke($"电机丝杆测试堵转");
                    }
                    else if (result == "PhyForwardLimited")
                    {
                        endIndex = tempSpeedList.Count;
                        //正常情况需要计算，从负限位到正限位运行点数的情况
                    }
                    else if (result == "PhyBackwardLimited")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "正限位限位反向";
                        var stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
                        SendImportantData(stopcmd);
                        DispathcherInvoke($"电机丝杆测试正限位反向");
                    }
                }
                catch (Exception ex)
                {
                    TestMessage.TestResult = "不合格";
                    TestMessage.TestValue = "不正常";
                    TestMessage.StandardValue = "正常";
                    TestMessage.Description = $"检测时间超时:{ex}";
                    DispathcherInvoke($"电机丝杆测试错误{ex}");
                }

            });

            if (result != "PhyForwardLimited")
            {
             
                return false;
            }
            else
            {
                if (!SmoothnessTest(startIndex, endIndex, tempSpeedList))
                {

                    TestMessage.TestResult = "不合格";
                    TestMessage.TestValue = "不正常";
                    TestMessage.StandardValue = "正常";
                    TestMessage.Description = $"丝杆顺滑度不达标，请重新安装";
                    return false;
                }
                else
                {
                    TestMessage.TestResult = "请分析速度曲线";
                    TestMessage.TestValue = "请分析速度曲线";
                    TestMessage.StandardValue = "正常";
                    TestMessage.Description = $"请分析速度曲线";
                    return true;
                }
            }


        }
        /// <summary>
        /// 丝杆顺滑度检测
        /// </summary>
        private bool SmoothnessTest(int start,int end,List<PlotViewSpeedMessage> speedList) 
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
        private void MotorStallDetection(EnumMotorId enumMotorId)
        {
            MotorModel motorModel;
            List<PlotViewPointMessage> pos; ;
            if (enumMotorId == EnumMotorId.MOTOR_1)
            {
                motorModel = MotorModelX;
                pos = _xPlotViewPointMessage;
            }
            else
            {
                motorModel = MotorModelY;
                pos = _yPlotViewPointMessage;
            }
            if (timePosStallDetectionList == null)
            {
                timePosStallDetectionList = new List<int>();
            }
            else
            {
                timePosStallDetectionList.Clear();
            }
            for (int i = 0; i < 120; i++)
            {
                timePosStallDetectionList.Add(motorModel.MotorParams.Pos);
                
                if (i > 4)
                {
                    if (Math.Abs(timePosStallDetectionList[timePosStallDetectionList.Count - 1] - timePosStallDetectionList[timePosStallDetectionList.Count - 4]) < 30)
                    {
                        var Stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
                        SendImportantData(Stopcmd);
                        _limitedtcs1.SetResult("stall");
                        DispathcherInvoke($"电机堵转，退出");
                        return;
                    }
                   
                }
                if (motorModel.MotorParams.LimitedState == EnumMotorLimitedState.PhyForwardLimited)
                {
                    _limitedtcs1.SetResult("PhyForwardLimited");
                    DispathcherInvoke($"发送正向限位，退出");
                    return;
                }
                if (motorModel.MotorParams.LimitedState == EnumMotorLimitedState.PhyBackwardLimited)
                {
                    _limitedtcs1.SetResult("PhyBackwardLimited");
                    DispathcherInvoke($"发送负向限位退出");
                    return;
                }
                Thread.Sleep(500);
            }
            DispathcherInvoke($"测试堵转功能退出");

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
                        var xZeroCmd = SelfMotorProtocol.SetMotorZero(EnumMotorId.MOTOR_1);
                        var yZeroCmd = SelfMotorProtocol.SetMotorZero(EnumMotorId.MOTOR_2);
                        SendImportantData(xZeroCmd);
                        SendImportantData(yZeroCmd);
                        return ;
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
        private async Task<(int ,int)> FindLinted(EnumMotorId enumMotorId)
        {
            (int x, int y) limtedPos = (0,0);
            if (NetUdpService.IsOpen == false && SerialPortService.IsOpen == false)
            {
                return (0,0);
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
                pos = _xPlotViewPointMessage;
            }
            else
            {
                pos = _yPlotViewPointMessage;
            }
            int posStart = pos[pos.Count - 1].Point;
            int posEnd = pos[pos.Count - 1].Point;
            await Task.Run(async () =>
            {

                Goto(enumMotorId, 1000000);
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
                        var stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
                        SendImportantData(stopcmd);
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
                Goto(enumMotorId, -1000000);
                Thread.Sleep(5000);
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
                        var stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
                        SendImportantData(stopcmd);
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
                    _dialogHostService.Information("提示", "错误"+ex);
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
                    _xPlotViewPointMessage.Clear();
                    _yPlotViewPointMessage.Clear();
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
                    _xPlotViewSpeedMessage.Clear();
                    _yPlotViewSpeedMessage.Clear();
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

        private void SaveToFile(string path,string parameter)
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
            var getMotor2Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_2);
            SendData(getMotor2Statuscmd);
            var getMotor1Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_1);
            SendData(getMotor1Statuscmd);
         
        }
        public DelegateCommand GetMotorStatusCommand { get; set; }
        private void GetMotorStatus()
        {
            var getMotor1Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_1);
            SendImportantData(getMotor1Statuscmd);
            var getMotor2Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_2);
            SendImportantData(getMotor2Statuscmd);
        }

        private void MotorEnable()
        {
            var getMotor1Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_1);
            SendImportantData(getMotor1Statuscmd);
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
                var setCtModeCmd = SelfMotorProtocol.SetMotorControlMode(channel, EnumMotorCtrType.CloseLoopPosCtr);
                SendImportantData(setCtModeCmd);
            }
            if (motorModel.MotorParams.Enable != true)
            {
                var setMotorEnableCmd = SelfMotorProtocol.SetMotorEnable(channel, EnumMotorEnable.Enable);
                SendImportantData(setMotorEnableCmd);
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
                    var setCtModeCmd = SelfMotorProtocol.SetMotorControlMode(channel, EnumMotorCtrType.OpenLoopSpeedCtr);
                    SendImportantData(setCtModeCmd);
                    var setMotorEnableCmd = SelfMotorProtocol.SetMotorEnable(channel, EnumMotorEnable.Enable);
                    SendImportantData(setMotorEnableCmd);
                }
            }
            else if (channel == EnumMotorId.MOTOR_2)
            {
                if (MotorModelY.MotorParams.CtrType != EnumMotorCtrType.OpenLoopSpeedCtr)
                {
                    var setCtModeCmd = SelfMotorProtocol.SetMotorControlMode(channel, EnumMotorCtrType.OpenLoopSpeedCtr);
                    SendImportantData(setCtModeCmd);
                    var setMotorEnableCmd = SelfMotorProtocol.SetMotorEnable(channel, EnumMotorEnable.Enable);
                    SendImportantData(setMotorEnableCmd);
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
            var gotocmd = SelfMotorProtocol.SetMotorGoTo(enumMotorId, EnumMotorUnit.Pulse, Speed);
            SendImportantData(gotocmd);
        }


        /// <summary>
        /// 通过位置模式移动
        /// </summary>
        /// <param name="enumMotorId"></param>
        /// <param name="pos"></param>
        private void Goto(EnumMotorId enumMotorId, int pos)
        {
            SetMotorInit(enumMotorId);
            var gotocmd = SelfMotorProtocol.SetMotorGoTo(enumMotorId, EnumMotorUnit.Pulse, pos);
            SendImportantData(gotocmd);
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
           
            var cmd1 = SelfMotorProtocol.SetMotorLimitEnable(EnumMotorId.MOTOR_1, limitEnable);
            var cmd2 = SelfMotorProtocol.SetMotorLimitEnable(EnumMotorId.MOTOR_2, limitEnable);
            SendImportantData(cmd1);
            SendImportantData(cmd2);     
        }

        public DelegateCommand GetMotorLimitEnableCommand { get; set; }
        /// <summary>
        /// 获取限位掩码状态
        /// </summary>
        private void GetMotorLimitEnable()
        {
            var cmd1 = SelfMotorProtocol.GetMotorLimitEnable(EnumMotorId.MOTOR_1);
            var cmd2 = SelfMotorProtocol.GetMotorLimitEnable(EnumMotorId.MOTOR_2);
            SendImportantData(cmd1);
            SendImportantData(cmd2);
        }
        private void SendData(byte[] cmd)
        {
            if (NetUdpService.IsOpen)
            {
                if (_byteQueue == null)
                {
                    _byteQueue = new Queue<byte[]>();
                }
                _byteQueue.Enqueue(cmd);
            }
            else if (SerialPortService.IsOpen) 
            {
                if (_byteQueue == null)
                {
                    _byteQueue = new Queue<byte[]>();
                }
                _byteQueue.Enqueue(cmd);
            }
        }
        private void SendImportantData(byte[] cmd)
        {
            if (NetUdpService.IsOpen)
            {
                if (_importantByteQueue == null)
                {
                    _importantByteQueue = new Queue<byte[]>();
                }
                _importantByteQueue.Enqueue(cmd);
            }
            else if (SerialPortService.IsOpen)
            {
                if (_importantByteQueue == null)
                {
                    _importantByteQueue = new Queue<byte[]>();
                }
                _importantByteQueue.Enqueue(cmd);
            }
        }
        public void SendDataThread()
        {
            Task.Run(async () => 
            {
               List<long> times = new List<long>();
                while (NetUdpService.IsOpen)
                {
                    if (_stopwatch == null)
                    {
                        
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
                    //  times.Add(time);
                    // Debug.WriteLine($"发送时间间隔{time}");

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
                    if (_byteQueue != null && _byteQueue.Count > 0)
                    {

                        SerialPortService.SendMsg(_byteQueue.Dequeue());

                    }
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
                _waitingReply.SetResult("Ready");
                var data = e.DataSource;
                switch (e.CmdType)
                {
                    
                    case EnumSelfMotorCmdType.CMD_GET_POS:
                        var chananl = data[0];
                        var unit = data[1];
                        var pos = BitConverter.ToInt32(data,2);
                        var ransformationCoefficient = BitConverter.ToSingle(data,6);
                        break;
                    case EnumSelfMotorCmdType.CMD_GET_STATUS:
                        MyCustomEvent?.Invoke(this,data);
                        break;
                    case EnumSelfMotorCmdType.CMD_GET_SLIM:
                        ParserMotorLimtedStatus(data);
                        break;
                }
            }
        }

        private void ParserMotorStatus(object? sender, byte[] data)
        {

           
            byte channel = data[0];//电机通道
            byte motorType = data[1];//电机类型
            byte AxisType = data[2];//轴类型
            byte controlType = data[3];//控制模式
            byte unit = data[4];//参数单位类型
            byte statusMask = data[5];//电机状态掩码 R/H 使能 SN软限位    SZ软限位 SP软限位 R/S运行和停止  N是硬件限位 z硬件限位 p 硬件限位
            byte softLimitEnableMask = data[6];//软限位使能掩码
            sbyte MotorRunningDirection = (sbyte)data[7];//电机运行方向
            float unitConversionFactor = BitConverter.ToSingle(data,8);//单位换算比例
            Int32 pulseCoordinate =  BitConverter.ToInt32(data,12);//当前的脉冲坐标
            Int32 pulseSpeed = BitConverter.ToInt32(data,16) ;//当前的脉冲速度

            EnumMotorId channelId = (EnumMotorId)channel;
            //电机状态掩码分析
            bool[] statusMaskBits = new bool[8];

            for (int i = 0; i < 8; i++)
            {
                bool bit = (statusMask & (1 << i)) != 0;
                statusMaskBits[7-i] = bit;
            }
            MotorModel motorModel = null;
            List<PlotViewPointMessage> tempPlotPointList;
            List<PlotViewSpeedMessage> tempPlotSpeedList;
            if (channelId == EnumMotorId.MOTOR_1)
            {
                motorModel = MotorModelX;
                tempPlotPointList = _xPlotViewPointMessage;
                tempPlotSpeedList = _xPlotViewSpeedMessage;
            }
            else if (channelId == EnumMotorId.MOTOR_2)
            {
                motorModel = MotorModelY;
                tempPlotPointList = _yPlotViewPointMessage;
                tempPlotSpeedList = _yPlotViewSpeedMessage;
            }
            else
            {
                return;
            }
            motorModel.MotorParams.MotorType = (EnumMotorType)motorType;
            motorModel.MotorParams.CtrType = (EnumMotorCtrType)controlType;
            motorModel.MotorParams.Unit = (EnumMotorUnit)unit;
            motorModel.MotorParams.Enable = statusMaskBits[0];
            motorModel.MotorParams.MoveState = statusMaskBits[4] == false? EnumMotorMoveState.MotorStop: EnumMotorMoveState.MotorMove;
            var x = statusMaskBits[5];
            motorModel.MotorParams.LimitedState = statusMaskBits[5] == true ? EnumMotorLimitedState.PhyBackwardLimited : (statusMaskBits[7] == true ? EnumMotorLimitedState.PhyForwardLimited: EnumMotorLimitedState.None);
            motorModel.MotorParams.MoveDirection = (EmumMotorMoveDirection)MotorRunningDirection;
            motorModel.MotorParams.SubRatio = unitConversionFactor;
            motorModel.MotorParams.Pos = pulseCoordinate;
            AddPoint(tempPlotPointList, tempPlotSpeedList, motorModel.MotorParams.Pos, motorModel);
            //增加限位位置
            if (motorModel.MotorParams.LimitedState == EnumMotorLimitedState.PhyForwardLimited)
            {
                motorModel.MotorParams.PositiveLimitPosition = motorModel.MotorParams.Pos;
            }
            if (motorModel.MotorParams.LimitedState == EnumMotorLimitedState.PhyBackwardLimited)
            {
                motorModel.MotorParams.NegativeLimitPosition = motorModel.MotorParams.Pos;
            }
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
                Log.Add(log);
            }));
           
        }
        private void AddPoint(List<PlotViewPointMessage> pointList, List<PlotViewSpeedMessage> speedList, int point, MotorModel motorModel)
        {

            if (pointList.Count >= 2)
            {
                var data = DateTime.Now;
                var timeDifference = (data - pointList[pointList.Count - 2].Date).TotalMilliseconds;
                var speed = (point - pointList[pointList.Count - 2].Point) / (timeDifference * 1.0) * 1000;
                PlotViewPointMessage pointView = new PlotViewPointMessage() { Date = data, Point = point};
                PlotViewSpeedMessage speedView = new PlotViewSpeedMessage() { SpeedDate = pointList[pointList.Count - 1].Date, Speed = speed };
                pointList.Add(pointView);
                speedList.Add(speedView);
                MotorSpeedplotModel.InvalidatePlot(true);
                MotorplotModel.InvalidatePlot(true);
                motorModel.MotorParams.Speed = speed;


            } 
            else
            {
                var data = DateTime.Now;
                PlotViewPointMessage pointView = new PlotViewPointMessage() { Date = data, Point = point };
                pointList.Add(pointView);
                MotorplotModel.InvalidatePlot(true);
            }
        }

        public DelegateCommand SerilizeCommand { get; set; }
        private  void Serilize()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
            {

                var path = dialog.SelectedPath;
                SaveToFile(path, "speed");
                SaveToFile(path, "point");
                string xPointjson = JsonConvert.SerializeObject(_xPlotViewPointMessage);
                string yPointjson = JsonConvert.SerializeObject(_yPlotViewPointMessage);
                string xSpeedjson = JsonConvert.SerializeObject(_xPlotViewSpeedMessage);
                string ySpeedjson = JsonConvert.SerializeObject(_yPlotViewSpeedMessage);
                string testMessgae = JsonConvert.SerializeObject(MotorTestMessages);   
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
           

        }
        public DelegateCommand DeserilizeCommand { get; set; }
        private void Deserilize()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
            {
                var path = dialog.SelectedPath;
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

                _xPlotViewPointMessage = JsonConvert.DeserializeObject<List<PlotViewPointMessage>>(File.ReadAllText(xPointjsonfilePath));
                _yPlotViewPointMessage = JsonConvert.DeserializeObject<List<PlotViewPointMessage>>(File.ReadAllText(yPointjsonfilePath));
                _xPlotViewSpeedMessage = JsonConvert.DeserializeObject<List<PlotViewSpeedMessage>>(File.ReadAllText(xSpeedjsonfilePath));
                _yPlotViewSpeedMessage = JsonConvert.DeserializeObject<List<PlotViewSpeedMessage>>(File.ReadAllText(ySpeedjsonfilePath));
                MotorTestMessages = JsonConvert.DeserializeObject<ObservableCollection<MotorTestMessage>>(File.ReadAllText(testMessgaejsonfilePath));

                _xSpeedPosLine.ItemsSource = _xPlotViewSpeedMessage;
                _ySpeedPosLine.ItemsSource = _yPlotViewSpeedMessage;
                _xSpeedPosLine.DataFieldX = "SpeedDate";
                _xSpeedPosLine.DataFieldY = "Speed";

                _ySpeedPosLine.DataFieldX = "SpeedDate";
                _ySpeedPosLine.DataFieldY = "Speed";
                _xPosLine.ItemsSource = _xPlotViewPointMessage;
                _yPosLine.ItemsSource = _yPlotViewPointMessage;
                _xPosLine.DataFieldX = "Date";
                _xPosLine.DataFieldY = "Point";
                _yPosLine.DataFieldX = "Date";
                _yPosLine.DataFieldY = "Point";

            
                MotorSpeedplotModel.InvalidatePlot(true);
                MotorplotModel.InvalidatePlot(true);
            }
           
        }
    }

}

