using Microsoft.VisualBasic;
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using UtilityTools.Core.Dialog;
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

        public System.Timers.Timer _getMotorStateTimer;
        private System.Timers.Timer _getMotorPosTimer;
        private System.Timers.Timer _StallDetectionTimer;
        private TaskCompletionSource<string> _limitedtcs1;
        private TaskCompletionSource<string> _limitedtcs2;
        private TaskCompletionSource<string> _stallDetectiotcs;
        private List<int> timePosStallDetectionList;
        private Stopwatch _stopwatch;
        private long elapsedTime;
        LineSeries _xPosLine;
        LineSeries _yPosLine;
        LineSeries _xSpeedLine;
        LineSeries _ySpeedPosLine;

        private TimeSpan startTime = TimeSpan.Zero;
        private TimeSpan currentTime = TimeSpan.Zero;
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
            MotorTestMessages = new ObservableCollection<MotorTestMessage>();
            MotorTypeMessage = new ObservableCollection<MotorTypeMessage>
            {
                { new MotorTypeMessage() { Name = "Zem15C" ,MinStrokeRange = 120000,MaxStrokeRange=130000} },
                { new MotorTypeMessage() { Name = "Zem18" ,MinStrokeRange = 120000,MaxStrokeRange=130000} },
                { new MotorTypeMessage() { Name = "Zem20" ,MinStrokeRange = 120000,MaxStrokeRange=130000} }
            }; 
            _TestMessage = new MotorTestMessage();
            XPos = new List<int>();
            YPos = new List<int>();
            XSpeeds = new List<double>();
            YSpeeds = new List<double>();
            timePosStallDetectionList = new List<int>();
            MotorTestMessages.Add(_TestMessage);

            TestAllCommand = new DelegateCommand(TestAll);
            MoveCommand = new DelegateCommand<string>(Move);
            ClearLogCommand = new DelegateCommand(ClearLog);
            GetMotorStatusCommand = new DelegateCommand(GetMotorStatus);
            AutoAdjustCommand = new DelegateCommand<string> (AutoAdjust);
            ClearMonitorCommand = new DelegateCommand<string> (ClearMonitor);
            AutomaticZeroInitializationCommand = new DelegateCommand (AutomaticZeroInitialization);
            Log = new ObservableCollection<string>();
            MyCustomEvent += ParserMotorStatus;
            _stopwatch = new Stopwatch();

            MotorplotModel.Legends.Add(new Legend());
            MotorplotModel.Axes.Add(new TimeSpanAxis() { Title = "时间", Position = AxisPosition.Bottom });
            MotorplotModel.Axes.Add(new LinearAxis() { Title = "位置(脉冲)", Position = AxisPosition.Left });
            _xPosLine = new LineSeries() { Title = "x轴", RenderInLegend = true };
            _yPosLine = new LineSeries() { Title = "y轴", RenderInLegend = true };
            MotorplotModel.Series.Add(_xPosLine);
            MotorplotModel.Series.Add(_yPosLine);

            MotorSpeedplotModel.Legends.Add(new Legend());
            MotorSpeedplotModel.Axes.Add(new TimeSpanAxis() { Title = "时间", Position = AxisPosition.Bottom });
            MotorSpeedplotModel.Axes.Add(new LinearAxis() { Title = "速度(脉冲/秒)", Position = AxisPosition.Left });
            _xSpeedLine = new LineSeries() { Title = "x轴", RenderInLegend = true };
            _ySpeedPosLine = new LineSeries() { Title = "y轴", RenderInLegend = true };
            MotorSpeedplotModel.Series.Add(_xSpeedLine);
            MotorSpeedplotModel.Series.Add(_ySpeedPosLine);
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
                case "up":; Goto(EnumMotorId.MOTOR_2, 1000000); break;
                case "down": Goto(EnumMotorId.MOTOR_2, -1000000); break;
                case "left": Goto(EnumMotorId.MOTOR_1, -1000000); break;
                case "right": Goto(EnumMotorId.MOTOR_1, 1000000); break;
                case "stop":
                    {
                        var xstopcmd = SelfMotorProtocol.SetMotorOperatingStatus(EnumMotorId.MOTOR_1, EnumMotorOperatingState.Stop);
                        var ystopcmd = SelfMotorProtocol.SetMotorOperatingStatus(EnumMotorId.MOTOR_2, EnumMotorOperatingState.Stop);
                        SendData(xstopcmd);
                        SendData(ystopcmd);
                    } break;
            }
        }
        private void Goto(EnumMotorId enumMotorId, int pos)
        {
            SetMotorInit(enumMotorId);
            var gotocmd = SelfMotorProtocol.SetMotorGoTo(enumMotorId, EnumMotorUnit.Pulse, pos);
            SendData(gotocmd);
        }
        public DelegateCommand TestAllCommand { get; set; }
        private async void TestAll()
        {
            MotorTestMessages.Clear();
            SetMotorInit(EnumMotorId.MOTOR_1);
            SetMotorInit(EnumMotorId.MOTOR_2);
            /*
            var result = await TestMotorMove();
            if (result != true)
            {
                DispathcherInvoke($"电机移动测试失败，测试退出");
                return;
            }*/
            
            await EncoderDirectionTest();
            await LimitSwitchTest(EnumMotorId.MOTOR_1);
            await LimitSwitchTest(EnumMotorId.MOTOR_2);
            await FullTravelTest(EnumMotorId.MOTOR_1);
            await FullTravelTest(EnumMotorId.MOTOR_2);
            //await SmoothnessDetection();
            

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
                if (XPos.Count <= 0 || YPos.Count <= 0)
                {
                    DispathcherInvoke($"没有读取打电机位置，测试退出");
                    return;
                }
                var xposStart = XPos[XPos.Count - 1];
                var yposStart = YPos[YPos.Count - 1];
                var xgotocmd = SelfMotorProtocol.SetMotorGoTo(EnumMotorId.MOTOR_1, EnumMotorUnit.Pulse, 1000000);
                var ygotocmd = SelfMotorProtocol.SetMotorGoTo(EnumMotorId.MOTOR_2, EnumMotorUnit.Pulse, 1000000);
                SendData(xgotocmd);
                SendData(ygotocmd);
                Thread.Sleep(1000);
                var xStopcmd = SelfMotorProtocol.SetMotorOperatingStatus(EnumMotorId.MOTOR_1, EnumMotorOperatingState.Stop);
                var yStopcmd = SelfMotorProtocol.SetMotorOperatingStatus(EnumMotorId.MOTOR_2, EnumMotorOperatingState.Stop);
                SendData(xStopcmd);
                SendData(yStopcmd);
                Thread.Sleep(2000);
                var xposEnd = XPos[XPos.Count - 1];
                var yposEnd = YPos[YPos.Count - 1];
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
                    TestMessage2.TestProject = "Y轴电机控制测试";
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
                var xposStart = XPos[XPos.Count - 1];
                var yposStart = YPos[YPos.Count - 1];
                var xgotocmd = SelfMotorProtocol.SetMotorGoTo(EnumMotorId.MOTOR_1, EnumMotorUnit.Pulse, 100000);
                var ygotocmd = SelfMotorProtocol.SetMotorGoTo(EnumMotorId.MOTOR_2, EnumMotorUnit.Pulse, 100000);
                SendData(xgotocmd);
                SendData(ygotocmd);
                Thread.Sleep(2000);
                var xStopcmd = SelfMotorProtocol.SetMotorOperatingStatus(EnumMotorId.MOTOR_1, EnumMotorOperatingState.Stop);
                var yStopcmd = SelfMotorProtocol.SetMotorOperatingStatus(EnumMotorId.MOTOR_2, EnumMotorOperatingState.Stop);
                SendData(xStopcmd);
                SendData(yStopcmd);
                Thread.Sleep(2000);
                var xposEnd = XPos[XPos.Count - 1];
                var yposEnd = YPos[YPos.Count - 1];
                if (yposEnd - yposStart < 200)
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
                if (xposEnd - xposStart < 200)
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
            
            List<int> pos;
            if (enumMotorId == EnumMotorId.MOTOR_1)
            {
                pos = XPos;
            }
            else
            {
                pos = YPos;
            }
            var posStart = pos[pos.Count - 1];
            var posEnd = pos[pos.Count - 1];
            await Task.Run(async () =>
            {
               
                SetMotorInit(enumMotorId);
                var gotocmd = SelfMotorProtocol.SetMotorGoTo(enumMotorId, EnumMotorUnit.Pulse, 1000000);
                SendData(gotocmd);
                Thread.Sleep(5000);
                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();
                    //单开线程去测试堵转和限位
                    Task.Run(() => { MotorStallDetection(enumMotorId); });
                    string result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(60));
                    posEnd = pos[pos.Count - 1];
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
                        SendData(stopcmd);
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
                SendData(gotocmd);
                Thread.Sleep(5000);
                

                
                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();
                    Task.Run(() => { MotorStallDetection(enumMotorId); });
                    string result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(60));
                    posEnd = pos[pos.Count - 1];
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
                        SendData(stopcmd);
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
                SendData(gotocmd);
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
                        SendData(stopcmd);
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
                SendData(gotocmd);
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
                        SendData(stopcmd);

                    }
                    else if (result == "PhyBackwardLimited")
                    {
                       
                        posMessage.negativeLimitPositionendPos = motorModel.MotorParams.Pos;
                        if (posMessage.positiveLimitPosition - posMessage.negativeLimitPositionendPos < 170000 && posMessage.positiveLimitPosition - posMessage.negativeLimitPositionendPos > 150000)
                        {
                            TestMessage.TestResult = "合格";
                            TestMessage.TestValue = "正常";
                            TestMessage.StandardValue = "正常";
                            TestMessage.Description = $"电机行程正常:{posMessage.positiveLimitPosition - posMessage.negativeLimitPositionendPos}脉冲";
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
        private async Task SmoothnessDetection()
        {
            
        }
        /// <summary>
        /// 电机堵转和限位检测
        /// </summary>
        private void MotorStallDetection(EnumMotorId enumMotorId)
        {
            MotorModel motorModel;
            List<int> pos;
            if (enumMotorId == EnumMotorId.MOTOR_1)
            {
                motorModel = MotorModelX;
                pos = XPos;
            }
            else
            {
                motorModel = MotorModelY;
                pos = YPos;
            }
            if (timePosStallDetectionList == null)
            {
                timePosStallDetectionList = new List<int>();
            }
            else
            {
                timePosStallDetectionList.Clear();
            }
            for (int i = 0; i < 60; i++)
            {
                timePosStallDetectionList.Add(motorModel.MotorParams.Pos);
                
                if (i > 4)
                {
                    if (Math.Abs(timePosStallDetectionList[timePosStallDetectionList.Count - 1] - timePosStallDetectionList[timePosStallDetectionList.Count - 4]) < 200)
                    {
                        var Stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
                        SendData(Stopcmd);
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
                Thread.Sleep(1000);
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
                        SendData(xZeroCmd);
                        SendData(yZeroCmd);
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
                TestMessage.TestProject = "X轴正限位测试";
                TestMessage2.TestProject = "X轴负限位测试";
            }
            else
            {
                TestMessage.TestProject = "Y轴限位测试";
                TestMessage2.TestProject = "Y轴负限位测试";
            }

            List<int> pos;
            if (enumMotorId == EnumMotorId.MOTOR_1)
            {
                pos = XPos;
            }
            else
            {
                pos = YPos;
            }
            var posStart = pos[pos.Count - 1];
            var posEnd = pos[pos.Count - 1];
            await Task.Run(async () =>
            {

                SetMotorInit(enumMotorId);
                var gotocmd = SelfMotorProtocol.SetMotorGoTo(enumMotorId, EnumMotorUnit.Pulse, 1000000);
                SendData(gotocmd);
                Thread.Sleep(5000);
                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();
                    //单开线程去测试堵转和限位
                    Task.Run(() => { MotorStallDetection(enumMotorId); });
                    string result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(60));
                    posEnd = pos[pos.Count - 1];
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
                        
                        limtedPos.x = pos[pos.Count - 1];
                        DispathcherInvoke($"电机测试正限位位置:{pos[pos.Count - 1]}");
                    }
                    else if (result == "PhyBackwardLimited")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "正限位反向，请检查";
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                        var stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
                        SendData(stopcmd);
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
                var gotocmd = SelfMotorProtocol.SetMotorGoTo(enumMotorId, EnumMotorUnit.Pulse, -1000000);
                SendData(gotocmd);
                Thread.Sleep(5000);



                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();
                    Task.Run(() => { MotorStallDetection(enumMotorId); });
                    string result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(60));
                    posEnd = pos[pos.Count - 1];
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
                        SendData(stopcmd);
                    }
                    else if (result == "PhyBackwardLimited")
                    {
                        TestMessage2.TestResult = "合格";
                        TestMessage2.TestValue = "正常";
                        TestMessage2.StandardValue = "正常";
                        TestMessage2.Description = "负限位正常";
                        DispathcherInvoke($"电机测试负限位位置:{pos[pos.Count - 1]}");
                        limtedPos.y = pos[pos.Count - 1];
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
                            SaveSeriesToFile(line, $"{path}\\参数{index}_{timeTip}.txt");
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
            SendData(getMotor1Statuscmd);
            var getMotor2Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_2);
            SendData(getMotor2Statuscmd);
        }

        private void MotorEnable()
        {
            var getMotor1Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_1);
            SendData(getMotor1Statuscmd);
        }

        /// <summary>
        /// 设置电机为闭环模式和使能
        /// </summary>
        /// <param name="MotorId">电机编号</param>
        private void SetMotorInit(EnumMotorId channel)
        {
            var setCtModeCmd = SelfMotorProtocol.SetMotorControlMode(channel, EnumMotorCtrType.CloseLoopPosCtr);
            SendData(setCtModeCmd);
            var setMotorEnableCmd = SelfMotorProtocol.SetMotorEnable(channel, EnumMotorEnable.Enable);
            SendData(setMotorEnableCmd);
            
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
        public void SendDataThread()
        {
            Task.Run(() => 
            {
               
                while (NetUdpService.IsOpen)
                {
                    if (_byteQueue != null && _byteQueue.Count > 0)
                    {

                        NetUdpService.SendMsg(_byteQueue.Dequeue());

                    }
                    Thread.Sleep(200);
                }
               /*
                while (SerialPortService.IsOpen)
                {
                    if (_byteQueue != null && _byteQueue.Count > 0)
                    {

                        SerialPortService.SendMsg(_byteQueue.Dequeue());

                    }
                    Thread.Sleep(300);
                }*/
                
               
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
                    
                }
            }
        }

        private void ParserMotorStatus(object? sender, byte[] data)
        {
            if (!_stopwatch.IsRunning)
            {
                _stopwatch.Start();
            }
            var time = _stopwatch.ElapsedMilliseconds;
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
            LineSeries tempLineSeries;
            LineSeries tempSpeedLineSeries;
            MotorModel motorModel = null;
            List<double> speed = null;
            List<int> pos = null;
            if (channelId == EnumMotorId.MOTOR_1)
            {
                motorModel = MotorModelX;
                speed = XSpeeds;
                pos = XPos;
                tempLineSeries = _xPosLine;
                tempSpeedLineSeries = _xSpeedLine;
            }
            else if (channelId == EnumMotorId.MOTOR_2)
            {
                motorModel = MotorModelY;
                speed = YSpeeds;
                pos = YPos;
                tempLineSeries = _yPosLine;
                tempSpeedLineSeries = _ySpeedPosLine;
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
            pos.Add(motorModel.MotorParams.Pos);
            //位置加点
            AddPoint(tempLineSeries, motorModel.MotorParams.Pos, MotorplotModel);
            //计算速度
            if (pos.Count > 2 && elapsedTime > 0)
            {
                var Difference = (time - elapsedTime);
                var spd = ((pos[pos.Count - 1] - pos[pos.Count - 2]) / Difference) * 1000;
                speed.Add(spd);
                motorModel.MotorParams.Speed = spd;
                //给速度加点
                AddPoint(tempSpeedLineSeries, spd, MotorSpeedplotModel);


            }
            elapsedTime = time;
            motorModel = null;
            speed = null;
            pos = null;
            tempLineSeries = null;
            tempSpeedLineSeries = null;
        }
        private void DispathcherInvoke(string log)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => 
            {
                Log.Add(log);
            }));
           
        }
        private void AddPoint(LineSeries series, double yvalue,PlotModel model)
        {
            //将TimeSpan转化为double(总秒速)
            double xValue = _stopwatch.ElapsedMilliseconds / 1000.0;

            //ui线程中添加数据点
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                series.Points.Add(new DataPoint(xValue, yvalue));
                model.InvalidatePlot(true);
            }));



        }
    }
}

