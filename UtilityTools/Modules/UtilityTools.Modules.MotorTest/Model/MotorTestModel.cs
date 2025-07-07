using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using OpenCvSharp.Features2D;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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
            NetUdpService = new UdpNetAsyncDevice();
            NetUdpService.UpdateResponse += NetUdpService_UpdateResponse;
            Init();
            RegisterTimer();
        }
        #endregion
        private IContainerProvider _containerProvider;
        private readonly IDialogHostService _dialogHostService;

        private SelfMotorParser _parser;


        private System.Timers.Timer _getMotorStateTimer;
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

        private ObservableCollection<MotorTestMessage> _motorTestMessages;
        public ObservableCollection<MotorTestMessage> MotorTestMessages
        {
            get { return _motorTestMessages; }
            set { _motorTestMessages = value; RaisePropertyChanged(); }
        }
        private List<int> _xPos;

        public List<int> XPos
        {
            get { return _yPos; }
            set { _yPos = value; RaisePropertyChanged(); }
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
            _TestMessage = new MotorTestMessage();
            timePosStallDetectionList = new List<int>();
            MotorTestMessages.Add(_TestMessage);

            TestAllCommand = new DelegateCommand(TestAll);
            MoveCommand = new DelegateCommand<string>(Move);
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
            var runcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Run);
            SendData(gotocmd);
            SendData(runcmd);
        }
        public DelegateCommand TestAllCommand { get; set; }
        private async void TestAll()
        {
            MotorTestMessages.Clear();
            await TestMotorMove();
            await EncoderDirectionTest();
            await LimitSwitchTest(EnumMotorId.MOTOR_1);
            await LimitSwitchTest(EnumMotorId.MOTOR_2);
            await FullTravelTest(EnumMotorId.MOTOR_1);
            await FullTravelTest(EnumMotorId.MOTOR_2);
            await SmoothnessDetection();

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
            SetMotorInit(EnumMotorId.MOTOR_1);
            SetMotorInit(EnumMotorId.MOTOR_2);
            var xgotocmd = SelfMotorProtocol.SetMotorGoTo(EnumMotorId.MOTOR_1, EnumMotorUnit.Pulse, 1000000);
            var ygotocmd = SelfMotorProtocol.SetMotorGoTo(EnumMotorId.MOTOR_2, EnumMotorUnit.Pulse, 1000000);
            SendData(xgotocmd);
            SendData(ygotocmd);
            var xruncmd = SelfMotorProtocol.SetMotorOperatingStatus(EnumMotorId.MOTOR_1, EnumMotorOperatingState.Run);
            var yruncmd = SelfMotorProtocol.SetMotorOperatingStatus(EnumMotorId.MOTOR_2, EnumMotorOperatingState.Run);
            SendData(xruncmd);
            SendData(yruncmd);
            var xposStart = XPos[XPos.Count - 1];
            var yposStart = YPos[YPos.Count - 1];
            await Task.Run(() =>
            {
                Thread.Sleep(1000);
                var xStopcmd = SelfMotorProtocol.SetMotorOperatingStatus(EnumMotorId.MOTOR_1, EnumMotorOperatingState.Stop);
                var yStopcmd = SelfMotorProtocol.SetMotorOperatingStatus(EnumMotorId.MOTOR_2, EnumMotorOperatingState.Stop);
                SendData(xStopcmd);
                SendData(yStopcmd);
                var xposEnd = XPos[XPos.Count - 1];
                var yposEnd = YPos[YPos.Count - 1];
                if (Math.Abs(yposEnd - yposStart) <= 50)
                {
                    TestMessage.TestProject = "Y轴电机控制测试";
                    TestMessage.TestResult = "不合格";
                    TestMessage.TestValue = "不正常";
                    TestMessage.StandardValue = "正常";
                    TestMessage.Description = "电机响应不正常";

                }
                else
                {
                    TestMessage.TestProject = "Y轴电机控制测试";
                    TestMessage.TestResult = "合格";
                    TestMessage.TestValue = "正常";
                    TestMessage.StandardValue = "正常";
                    TestMessage.Description = "电机响应正常";
                }
                if (Math.Abs(xposEnd - xposStart) <= 50)
                {

                    TestMessage2.TestProject = "X轴电机控制测试";
                    TestMessage2.TestResult = "不合格";
                    TestMessage2.TestValue = "不正常";
                    TestMessage2.StandardValue = "正常";
                    TestMessage2.Description = "电机响应不正常";
                }
                else
                {
                    TestMessage2.TestProject = "Y轴电机控制测试";
                    TestMessage2.TestResult = "合格";
                    TestMessage2.TestValue = "正常";
                    TestMessage2.StandardValue = "正常";
                    TestMessage2.Description = "电机响应正常";
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
            var xgotocmd = SelfMotorProtocol.SetMotorGoTo(EnumMotorId.MOTOR_1, EnumMotorUnit.Pulse, 100000);
            var ygotocmd = SelfMotorProtocol.SetMotorGoTo(EnumMotorId.MOTOR_2, EnumMotorUnit.Pulse, 100000);
            SendData(xgotocmd);
            SendData(ygotocmd);
            var xruncmd = SelfMotorProtocol.SetMotorOperatingStatus(EnumMotorId.MOTOR_1, EnumMotorOperatingState.Run);
            var yruncmd = SelfMotorProtocol.SetMotorOperatingStatus(EnumMotorId.MOTOR_2, EnumMotorOperatingState.Run);
            SendData(xruncmd);
            SendData(yruncmd);
            var xposStart = XPos[XPos.Count - 1];
            var yposStart = YPos[YPos.Count - 1];
            await Task.Run(() =>
            {
                Thread.Sleep(2000);
                var xStopcmd = SelfMotorProtocol.SetMotorOperatingStatus(EnumMotorId.MOTOR_1, EnumMotorOperatingState.Stop);
                var yStopcmd = SelfMotorProtocol.SetMotorOperatingStatus(EnumMotorId.MOTOR_2, EnumMotorOperatingState.Stop);
                SendData(xStopcmd);
                SendData(yStopcmd);
                var xposEnd = XPos[XPos.Count - 1];
                var yposEnd = YPos[YPos.Count - 1];
                if (yposEnd - yposStart < 0)
                {
                    TestMessage.TestProject = "Y轴编码器测试";
                    TestMessage.TestResult = "不合格";
                    TestMessage.TestValue = "不正常";
                    TestMessage.StandardValue = "正常";
                    TestMessage.Description = "编码器反向";
                }
                else
                {
                    TestMessage.TestProject = "Y轴编码器测试";
                    TestMessage.TestResult = "合格";
                    TestMessage.TestValue = "正常";
                    TestMessage.StandardValue = "正常";
                    TestMessage.Description = "编码器正常";
                }
                if (xposEnd - xposStart < 0)
                {
                    TestMessage2.TestProject = "X轴编码器测试";
                    TestMessage2.TestResult = "不合格";
                    TestMessage2.TestValue = "不正常";
                    TestMessage2.StandardValue = "正常";
                    TestMessage2.Description = "编码器反向";
                }
                else
                {
                    TestMessage2.TestProject = "X轴编码器测试";
                    TestMessage2.TestResult = "合格";
                    TestMessage2.TestValue = "正常";
                    TestMessage2.StandardValue = "正常";
                    TestMessage2.Description = "编码器正常";
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
                TestMessage.TestProject = "X轴限位测试";
            }
            else if (enumMotorId == EnumMotorId.MOTOR_2)
            {
                TestMessage.TestProject = "Y轴限位测试";
            }
            await Task.Run(async () =>
            {
                SetMotorInit(enumMotorId);
                var gotocmd = SelfMotorProtocol.SetMotorGoTo(enumMotorId, EnumMotorUnit.Pulse, 1000000);
                SendData(gotocmd);
                var runcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Run);
                SendData(runcmd);
                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();
                    //单开线程去测试堵转和限位
                    Task.Run(() => { MotorStallDetection(enumMotorId); });
                    string result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(60));
                    if (result == "stall")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "电机堵转，请检查";
                    }
                    else if (result == "PhyForwardLimited")
                    {
                        TestMessage.TestResult = "合格";
                        TestMessage.TestValue = "正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "正限位正常";
                    }
                    else if (result == "PhyBackwardLimited")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "正限位反向，请检查";
                        var stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
                        SendData(stopcmd);
                    }
                }
                catch
                {

                }
            });
            await Task.Run(async () =>
            {
                var gotocmd = SelfMotorProtocol.SetMotorGoTo(enumMotorId, EnumMotorUnit.Pulse, -1000000);
                SendData(gotocmd);
                var xruncmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Run);
                SendData(xruncmd);
                Task.Run(() => { MotorStallDetection(enumMotorId); });

                
                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();
                    string result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(60));
                    if (result == "stall")
                    {
                        TestMessage2.TestResult = "不合格";
                        TestMessage2.TestValue = "不正常";
                        TestMessage2.StandardValue = "正常";
                        TestMessage2.Description = "电机堵转，请检查";
                    }
                    else if (result == "PhyForwardLimited")
                    {
                        TestMessage2.TestResult = "不合格";
                        TestMessage2.TestValue = "不正常";
                        TestMessage2.StandardValue = "正常";
                        TestMessage2.Description = "负限位反向";
                        var stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
                        SendData(stopcmd);
                    }
                    else if (result == "PhyBackwardLimited")
                    {
                        TestMessage2.TestResult = "合格";
                        TestMessage2.TestValue = "正常";
                        TestMessage2.StandardValue = "正常";
                        TestMessage2.Description = "负限位正常";
                    }
                }
                catch
                {

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
            else if (enumMotorId == EnumMotorId.MOTOR_2)
            {
                TestMessage.TestProject = "Y轴满行程测试";
                motorModel = MotorModelY;
            }
            else
            {
                TestMessage.TestProject = "***";
                motorModel = new MotorModel();
            }
            await Task.Run(async () =>
            {
                //单开线程去测试堵转和限位
                _limitedtcs1 = new TaskCompletionSource<string>();
                Task.Run(() => { MotorStallDetection(enumMotorId); });
                SetMotorInit(enumMotorId);
                var gotocmd = SelfMotorProtocol.SetMotorGoTo(enumMotorId, EnumMotorUnit.Pulse, 1000000);
                SendData(gotocmd);
                var runcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Run);
                SendData(runcmd);

                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();
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
                }
                catch
                {

                }
            });
            if (posMessage.positiveLimitPosition == -1)
            {
                MotorTestMessages.Add(TestMessage);
                return false;
            }
            await Task.Run(async () =>
            {
                //单开线程去测试堵转和限位
                _limitedtcs1 = new TaskCompletionSource<string>();
                Task.Run(() => { MotorStallDetection(enumMotorId); });
                SetMotorInit(enumMotorId);
                var gotocmd = SelfMotorProtocol.SetMotorGoTo(enumMotorId, EnumMotorUnit.Pulse, -1000000);
                SendData(gotocmd);
                var runcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Run);
                SendData(runcmd);

                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();
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
                    else if (result == "PhyBackwardLimited")
                    {
                        TestMessage.Description = "负限位反向";
                        var stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
                        SendData(stopcmd);
                    }
                }
                catch
                {

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
            if (enumMotorId == EnumMotorId.MOTOR_1)
            {
                for (int i = 0; i < 60; i++)
                {
                    timePosStallDetectionList.Add(XPos[XPos.Count - 1]);
                    if (i >= 2)
                    {
                        if (Math.Abs( timePosStallDetectionList[timePosStallDetectionList.Count - 1] - timePosStallDetectionList[timePosStallDetectionList.Count - 4]) < 200)
                        {
                            var xStopcmd = SelfMotorProtocol.SetMotorOperatingStatus(EnumMotorId.MOTOR_1, EnumMotorOperatingState.Stop);
                            SendData(xStopcmd);
                            _limitedtcs1.SetResult("stall");
                            return;
                        }
                    }
                    if (MotorModelX.MotorParams.LimitedState == EnumMotorLimitedState.PhyForwardLimited)
                    {
                        _limitedtcs1.SetResult("PhyForwardLimited");
                        return;
                    }
                    if (MotorModelX.MotorParams.LimitedState == EnumMotorLimitedState.PhyBackwardLimited)
                    {
                        _limitedtcs1.SetResult("PhyBackwardLimited");
                        return;
                    }
                    Thread.Sleep(1000);
                }
            }
            else if (enumMotorId == EnumMotorId.MOTOR_2)
            {
                for (int i = 0; i < 60; i++)
                {
                    timePosStallDetectionList.Add(YPos[YPos.Count - 1]);
                    if (i >= 2)
                    {
                        if (Math.Abs(timePosStallDetectionList[timePosStallDetectionList.Count - 1] - timePosStallDetectionList[timePosStallDetectionList.Count - 4]) < 200)
                        {
                            var xStopcmd = SelfMotorProtocol.SetMotorOperatingStatus(EnumMotorId.MOTOR_2, EnumMotorOperatingState.Stop);
                            SendData(xStopcmd);
                            _limitedtcs1.SetResult("stall");
                            return;
                        }
                    }
                    if (MotorModelY.MotorParams.LimitedState == EnumMotorLimitedState.PhyForwardLimited)
                    {
                        _limitedtcs1.SetResult("PhyForwardLimited");
                        return;
                    }
                    if (MotorModelY.MotorParams.LimitedState == EnumMotorLimitedState.PhyBackwardLimited)
                    {
                        _limitedtcs1.SetResult("PhyBackwardLimited");
                        return;
                    }
                    Thread.Sleep(1000);
                }
            }
            else
            {
                return;
            }
        }
        private void RegisterTimer()
        {
            _getMotorStateTimer = new System.Timers.Timer(200);
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
            var getMotor1Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_1);
            SendData(getMotor1Statuscmd);
            var getMotor2Statuscmd = SelfMotorProtocol.GetMotorStatus(EnumMotorId.MOTOR_2); 



            SendData(getMotor2Statuscmd);
            var getMotor1Poscmd = SelfMotorProtocol.GetMotorPos(EnumMotorId.MOTOR_1);
            SendData(getMotor1Poscmd);
            var getMotor2Poscmd = SelfMotorProtocol.GetMotorPos(EnumMotorId.MOTOR_2);
            SendData(getMotor2Poscmd);
        }

        /// <summary>
        /// 问询电机位置包含
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QueryMotorPosTimerElapsed()
        {
            


        }

        /// <summary>
        /// 设置电机为闭环模式和使能
        /// </summary>
        /// <param name="MotorId">电机编号</param>
        private void SetMotorInit(EnumMotorId channel)
        {
            var setCtModeCmd = SelfMotorProtocol.SetMotorControlMode(channel, EnumMotorCtrType.OpenLoopPosCtr);
            SendData(setCtModeCmd);
            var setMotorEnableCmd = SelfMotorProtocol.SetMotorEnable(channel, EnumMotorEnable.Enable);
            SendData(setCtModeCmd);
        }
        private void SendData(byte[] cmd)
        {
            if (NetUdpService.IsOpen)
            {
                NetUdpService.SendMsg(cmd);
            }
            if (SerialPortService.IsOpen) 
            {
                SerialPortService.SendMsg(cmd);
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
            byte MotorRunningDirection = data[7];//电机运行方向
            float unitConversionFactor = BitConverter.ToSingle(data,8);//单位换算比例
            Int32 pulseCoordinate =  BitConverter.ToInt32(data,12);//当前的脉冲坐标
            Int32 pulseSpeed = BitConverter.ToInt32(data,16) ;//当前的脉冲速度

            EnumMotorId channelId = (EnumMotorId)channel;
            //电机状态掩码分析
            bool[] statusMaskBits = new bool[8];

            for (int i = 0; i < 8; i++)
            {
                bool bit = (statusMask & (1 << i)) != 0;
                statusMaskBits[i] = bit;
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
            motorModel.MotorParams.LimitedState = statusMaskBits[5] == true ? EnumMotorLimitedState.PhyBackwardLimited : (statusMaskBits[7] == true ? EnumMotorLimitedState.PhyForwardLimited: EnumMotorLimitedState.None);
            motorModel.MotorParams.MoveDirection = (EmumMotorMoveDirection)MotorRunningDirection;
            motorModel.MotorParams.SubRatio = unitConversionFactor;
            motorModel.MotorParams.Pos = pulseCoordinate;
            pos.Add(motorModel.MotorParams.Pos);
            tempLineSeries.Points.Add(new DataPoint(tempLineSeries.Points.Count(), motorModel.MotorParams.Pos));
            //计算速度
            if (pos.Count > 2 && elapsedTime > 0)
            {
                var Difference = (time - elapsedTime) * 1000.0;
                var spd = (pos[pos.Count - 1] - pos[pos.Count - 2]) / Difference;
                speed.Add(spd);
                DateTime currentTime = DateTime.Now;
                tempSpeedLineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(currentTime), spd));
                elapsedTime = time;
            }
        }
    }
}

