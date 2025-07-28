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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Shapes;
using UtilityTools.Core.Dialog;
using UtilityTools.Modules.Motor5Controller.Model;
using UtilityTools.Modules.MotorTest.Protocol;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.Services.Services;

namespace UtilityTools.Modules.MotorTest.Model
{
    public class FiveAxisModel:BindableBase
    {

        #region ------------Constructor------------
        public FiveAxisModel(IContainerProvider containerProvider)
        {
            _containerProvider = containerProvider;
            _dialogHostService = containerProvider.Resolve<IDialogHostService>();
            Init();
        }
        public FiveAxisModel(IContainerProvider containerProvider,EnumMotorId Id, string name)
        {
            _containerProvider = containerProvider;
            _dialogHostService = containerProvider.Resolve<IDialogHostService>();
            enumMotorId = Id;
            _name = name;
            Init();
        }
        #endregion
        private readonly EnumMotorId enumMotorId;
        private readonly string _name;
        private IContainerProvider _containerProvider;
        private readonly IDialogHostService _dialogHostService;

        private SelfMotorParser _parser;

        private Stopwatch _testTime;
        public System.Timers.Timer _getMotorStateTimer;
        private TaskCompletionSource<string> _limitedtcs1;
        private TaskCompletionSource<string> _waitingReply;
        private TaskCompletionSource<string> _stallDetectiotcs;
        private List<int> timePosStallDetectionList;
   
        private long elapsedTime;
        private int[] _limtedPos;
        private BackgroundWorker _work;
        private bool _isPerformance;

       
        

        public event EventHandler<byte[]> AddCmdEvent;
        public event EventHandler<byte[]> AddImportantCmdEvent;
        private double _std;
        public double Std
        {
            get { return _std; }
            set { _std = value; RaisePropertyChanged(); }
        }

        private MotorTestMessage _TestMessage;

        #region ------------Property------------
        private List<PlotViewPointMessage> _plotViewPointMessage;
        /// <summary>
        /// 位置信息列表
        /// </summary>
        public List<PlotViewPointMessage> PlotViewPointMessages
        {
            get { return _plotViewPointMessage;}
            set { _plotViewPointMessage = value; RaisePropertyChanged(); }
        }
        private List<PlotViewSpeedMessage> _plotViewSpeedMessage;
        /// <summary>
        /// 速度信息列表
        /// </summary>
        public List<PlotViewSpeedMessage> plotViewSpeedMessages
        {
            get { return _plotViewSpeedMessage;}
            set { _plotViewSpeedMessage = value;RaisePropertyChanged(); }
        }
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
     
        private Queue<byte[]> _byteQueue;
        public Queue<byte[]> ByteQueue
        {
            get { return _byteQueue; }
            set { _byteQueue = value; RaisePropertyChanged(); }
        }
        private Queue<byte[]> _importantByteQueue;
        public Queue<byte[]> ImportantByteQueue
        {
            get { return _importantByteQueue; }
            set { _importantByteQueue = value; RaisePropertyChanged(); }
        }

        private  LineSeries _posLine;
        /// <summary>
        /// 位置曲线
        /// </summary>
        public LineSeries PosLine
        {
            get { return _posLine; }
            set { _posLine = value; RaisePropertyChanged(); }
        }
        private LineSeries _speedLine;
        /// <summary>
        /// 速度曲线
        /// </summary>
        public LineSeries SpeedLine
        {
            get { return _speedLine; }
            set { _speedLine = value;RaisePropertyChanged(); } 
        }



        private MotorModel _motorModel;
        public MotorModel MotorModel
        {
            get { return _motorModel; }
            set { _motorModel = value; RaisePropertyChanged(); }
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
            RegisterTimer();
            MotorModel = new MotorModel();
            ByteQueue = new Queue<byte[]>();
            ImportantByteQueue = new Queue<byte[]>();
            PosLine = new LineSeries();
            SpeedLine = new LineSeries();

            
            _TestMessage = new MotorTestMessage();

            MyCustomEvent += ParserMotorStatus;
            _speedLine = new LineSeries() { Title = _name, RenderInLegend = true };
            _speedLine.ItemsSource = _plotViewSpeedMessage;
            _speedLine.DataFieldX = "SpeedDate";
            _speedLine.DataFieldY = "Speed";


            _posLine = new LineSeries() { Title = _name, RenderInLegend = true };
            _posLine.ItemsSource = _plotViewPointMessage;
            _posLine.DataFieldX = "Date";
            _posLine.DataFieldY = "Point";
        }
        public void ClearLog()
        {
            if (Log != null)
            {
                Log.Clear();
            }
        }
        public void Move(string direction)
        {

            switch (direction)
            {
                case "p":; Goto(enumMotorId, 5000000); break;
                case "n": Goto(enumMotorId, -5000000); break;
                case "s":
                    {
                        var stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
                        SendImportantData(stopcmd);
                    }
                    break;
            }
        }
        public void TestPerformance()
        {
            _isPerformance = true;
            Task.Run(() =>
            {
                while (_isPerformance)
                {
                    if (_byteQueue.Count == 0)
                    {
                        var getMotorStatuscmd = SelfMotorProtocol.GetMotorStatus(enumMotorId);
                        SendData(getMotorStatuscmd);
                    }
                    Thread.Sleep(10);

                }
            });

        }
        public void NoPerformance()
        {
            _isPerformance = false; ;

        }
        public async void TestSmoothnessDetection()
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
            for (int i = 0; i < 25; i++)
            {
                await SmoothnessDetection(enumMotorId);
                if (_testTime.ElapsedMilliseconds / 1000.0 / 60 > 60)
                {
                    break;
                }

            }
            _work.CancelAsync();
            _testTime.Stop();
            _testTime = null;
        }
        public DelegateCommand TestAllCommand { get; set; }
        public async void TestAll()
        {
            MotorTestMessages.Clear();
            SetMotorInit();
            Goto(enumMotorId, -1000000);
            Thread.Sleep(4000);
            var stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
            SendImportantData(stopcmd);
            _work = new BackgroundWorker();
            _work.WorkerSupportsCancellation = true;
            _work.DoWork += Worker_DoWork;
            _work?.RunWorkerAsync();
            var result = await TestMotorMove();
            if (result != true)
            {
                DispathcherInvoke($"电机移动测试失败，测试退出");
               
                _work.CancelAsync();
                return;
            }

            result = await EncoderDirectionTest();
            if (result != true)
            {
               
                DispathcherInvoke($"电机编码器测试失败，测试退出");
                _work.CancelAsync();
                return;
            }

         
            result = await LimitSwitchTest();
            if (result != true)
            {
                DispathcherInvoke($"y轴电机限位测试失败，测试退出");
               
                _work.CancelAsync();
                return;
            }
            result = await FullTravelTest(enumMotorId);

            result = await LimitPositioningAccuracyDetection();

            //await SmoothnessDetection();

            _work.CancelAsync();
        }
        /// <summary>
        /// 电机位置移动检测
        /// </summary>
        private async Task<bool> TestMotorMove()
        {
           
            MotorTestMessage TestMessage = new MotorTestMessage();

            await Task.Run(() =>
            {
                if (_plotViewPointMessage.Count <= 0 )
                {
                    DispathcherInvoke($"没有读取到电机位置，测试退出");
                    return;
                }
                var posStart = _plotViewPointMessage[_plotViewPointMessage.Count - 1].Point;
                var gotocmd = SelfMotorProtocol.SetMotorGoTo(enumMotorId, EnumMotorUnit.Pulse, 1000000);
                SendImportantData(gotocmd);
                Thread.Sleep(3000);
                var stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
                SendImportantData(stopcmd);
                Thread.Sleep(3000);
                var posEnd = _plotViewPointMessage[_plotViewPointMessage.Count - 1].Point;
                if (Math.Abs(posEnd - posStart) <= 50)
                {
                    TestMessage.TestProject = "电机控制测试";
                    TestMessage.TestResult = "不合格";
                    TestMessage.TestValue = "不正常";
                    TestMessage.StandardValue = "正常";
                    TestMessage.Description = "电机响应不正常";
                }
                else
                {
                    TestMessage.TestProject = "电机控制测试";
                    TestMessage.TestResult = "合格";
                    TestMessage.TestValue = "正常";
                    TestMessage.StandardValue = "正常";
                    TestMessage.Description = "电机响应正常";
                }
               
            });
            MotorTestMessages.Add(TestMessage);
            if (TestMessage.TestResult == "合格" )
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
            
            while (_work != null && _work.CancellationPending != true)
            {
                if (_byteQueue.Count == 0)
                {
                    var getMotorStatuscmd = SelfMotorProtocol.GetMotorStatus(enumMotorId);
                    SendData(getMotorStatuscmd);
            
                }
            }
            e.Cancel = true;
        }


        /// <summary>
        /// 编码器测试
        /// </summary>
        private async Task<bool> EncoderDirectionTest()
        {
            
            MotorTestMessage TestMessage = new MotorTestMessage();
            SetMotorInit();
            await Task.Run(() =>
            {
                var posStart = _plotViewPointMessage[_plotViewPointMessage.Count - 1].Point;
                var gotocmd = SelfMotorProtocol.SetMotorGoTo(enumMotorId, EnumMotorUnit.Pulse, 100000);
                SendImportantData(gotocmd);
                Thread.Sleep(2000);
                var stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
                SendImportantData(stopcmd);
                Thread.Sleep(2000);
                var posEnd = _plotViewPointMessage[_plotViewPointMessage.Count - 1].Point;
                if (posEnd - posStart < 100)
                {
                    TestMessage.TestProject = "编码器测试";
                    TestMessage.TestResult = "不合格";
                    TestMessage.TestValue = "不正常";
                    TestMessage.StandardValue = "正常";
                    TestMessage.Description = "编码器反向";
                }
                else
                {
                    TestMessage.TestProject = "编码器测试";
                    TestMessage.TestResult = "合格";
                    TestMessage.TestValue = "正常";
                    TestMessage.StandardValue = "正常";
                    TestMessage.Description = "编码器正常";
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
        /// 限位测试
        /// </summary>
        private async Task<bool> LimitSwitchTest()
        {
          
            MotorTestMessage TestMessage1 = new MotorTestMessage();
            MotorTestMessage TestMessage2 = new MotorTestMessage();

            TestMessage1.TestProject = "正限位测试";;


            TestMessage2.TestProject = "负限位测试";
            
            var posStart = _plotViewPointMessage[_plotViewPointMessage.Count - 1].Point;
            var posEnd = _plotViewPointMessage[_plotViewPointMessage.Count - 1].Point;
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
                    posEnd = _plotViewPointMessage[_plotViewPointMessage.Count - 1].Point;
                    if (result == "stall")
                    {
                        TestMessage1.TestResult = "不合格";
                        TestMessage1.TestValue = "不正常";
                        TestMessage1.StandardValue = "正常";
                        TestMessage1.Description = "电机堵转，请检查";
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                    }
                    else if (result == "PhyForwardLimited")
                    {
                        TestMessage1.TestResult = "合格";
                        TestMessage1.TestValue = "正常";
                        TestMessage1.StandardValue = "正常";
                        TestMessage1.Description = "正限位正常";
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                    }
                    else if (result == "PhyBackwardLimited")
                    {
                        TestMessage1.TestResult = "不合格";
                        TestMessage1.TestValue = "不正常";
                        TestMessage1.StandardValue = "正常";
                        TestMessage1.Description = "正限位反向，请检查";
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                        var stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
                        SendImportantData(stopcmd);
                    }
                    else
                    {
                        TestMessage1.TestResult = "不合格";
                        TestMessage1.TestValue = "不正常";
                        TestMessage1.StandardValue = "正常";
                        TestMessage1.Description = "正限位测试未知错误";
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                    }
                }
                catch (Exception ex)
                {
                    TestMessage1.TestResult = "不合格";
                    TestMessage1.TestValue = "不正常";
                    TestMessage1.StandardValue = "正常";
                    TestMessage1.Description = $"正限位测试触发异常{ex}";
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
                    posEnd = _plotViewPointMessage[_plotViewPointMessage.Count - 1].Point;
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
                        TestMessage2.TestResult = "不合格";
                        TestMessage2.TestValue = "不正常";
                        TestMessage2.StandardValue = "正常";
                        TestMessage2.Description = "负限位测试未知错误";

                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                    }
                }
                catch (Exception ex)
                {
                    TestMessage2.TestResult = "不合格";
                    TestMessage2.TestValue = "不正常";
                    TestMessage2.StandardValue = "正常";
                    TestMessage2.Description = "负限位未知错误";
                    DispathcherInvoke($"电机测试负限位测试引发异常:{ex}");
                }
            });
            MotorTestMessages.Add(TestMessage1);
            MotorTestMessages.Add(TestMessage2);
            if (TestMessage2.TestResult == "合格" && TestMessage2.TestResult == "合格")
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
            
            (int positiveLimitPosition, int negativeLimitPositionendPos) posMessage = (-1, 1);
            if (_limtedPos == null)
            {
                _limtedPos = new int[4];
            }
            MotorTestMessage TestMessage = new MotorTestMessage();
            TestMessage.TestProject = _name + "满行程测试";
            
            string result = "x";
            await Task.Run(async () =>
            {
                //单开线程去测试堵转和限位
                _limitedtcs1 = new TaskCompletionSource<string>();

                SetMotorInit();
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

                        posMessage.positiveLimitPosition = MotorModel.MotorParams.Pos;
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
                SetMotorInit();
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

                        posMessage.negativeLimitPositionendPos = MotorModel.MotorParams.Pos;
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
        private async Task<bool> LimitPositioningAccuracyDetection()
        {
            
            (int positiveLimitPosition, int negativeLimitPositionendPos) posMessage = (-1, 1);
            MotorTestMessage TestMessage = new MotorTestMessage();
            MotorTestMessage TestMessage2 = new MotorTestMessage();
            TestMessage.TestProject = _name  + "定位精度测试";
            string result = "x";
            await Task.Run(async () =>
            {
                //单开线程去测试堵转和限位
                _limitedtcs1 = new TaskCompletionSource<string>();

                SetMotorInit();
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

                        posMessage.positiveLimitPosition = MotorModel.MotorParams.Pos;
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
                SetMotorInit();
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

                        posMessage.negativeLimitPositionendPos = MotorModel.MotorParams.Pos;
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
            (int positiveLimitPosition, int negativeLimitPositionendPos) posMessage = (-1, 1);
            MotorTestMessage TestMessage = new MotorTestMessage();
            string result = "";
            await Task.Run(async () =>
            {
                //单开线程去测试堵转和限位
                _limitedtcs1 = new TaskCompletionSource<string>();
                GotoInSpeedMode(enumMotorId, 10000);
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
                    TestMessage.Description = $"请检测网络连接";
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
                startIndex = _plotViewSpeedMessage.Count;
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
                    }
                    else if (result == "PhyForwardLimited")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "负限位反向";
                        var stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
                        SendImportantData(stopcmd);

                    }
                    else if (result == "PhyBackwardLimited")
                    {
                        //正常的情况下，需要记录前面运行过程中的点数
                        endIndex = _plotViewSpeedMessage.Count;

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
            if (result != "PhyBackwardLimited")
            {
                TestMessage.Description = $"从正方向到负方向的位置丝杆顺滑度错误";

            }
            else
            {
                if (!SmoothnessTest(startIndex, endIndex, _plotViewSpeedMessage))
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
                SetMotorInit();
                var gotocmd = SelfMotorProtocol.SetMotorGoTo(enumMotorId, EnumMotorUnit.Pulse, 10000);
                SendImportantData(gotocmd);
                startIndex = _plotViewSpeedMessage.Count;
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
                    }
                    else if (result == "PhyForwardLimited")
                    {
                        endIndex = _plotViewSpeedMessage.Count;
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

                return false;
            }
            else
            {
                if (!SmoothnessTest(startIndex, endIndex, _plotViewSpeedMessage))
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
                    return false;
                }
            }


        }
        /// <summary>
        /// 丝杆顺滑度检测
        /// </summary>
        private bool SmoothnessTest(int start, int end, List<PlotViewSpeedMessage> speedList)
        {
            return true;
        }
        /// <summary>
        /// 电机堵转和限位检测
        /// </summary>
        private void MotorStallDetection(EnumMotorId enumMotorId)
        {
 

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
                timePosStallDetectionList.Add(MotorModel.MotorParams.Pos);

                if (i > 4)
                {
                    if (Math.Abs(timePosStallDetectionList[timePosStallDetectionList.Count - 1] - timePosStallDetectionList[timePosStallDetectionList.Count - 3]) < 50)
                    {
                        var Stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
                        SendImportantData(Stopcmd);
                        _limitedtcs1.SetResult("stall");
                        DispathcherInvoke($"电机堵转，退出");
                        return;
                    }

                }
                if (MotorModel.MotorParams.LimitedState == EnumMotorLimitedState.PhyForwardLimited)
                {
                    _limitedtcs1.SetResult("PhyForwardLimited");
                    DispathcherInvoke($"发送正向限位，退出");
                    return;
                }
                if (MotorModel.MotorParams.LimitedState == EnumMotorLimitedState.PhyBackwardLimited)
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
        public async void AutomaticZeroInitialization()
        {
            var resultx = await FindLinted(enumMotorId);
            if (resultx == (0, 0))
            {
                return;
            }
         
            int pos = (resultx.Item1 - resultx.Item2) / 2 + resultx.Item2;
            Goto(enumMotorId, pos);
            Thread.Sleep(5000);
            await Task.Run(() => 
            {
                int i = 0;
                while (true)
                {
                    if (MotorModel.MotorParams.MoveState == EnumMotorMoveState.MotorStop)
                    {
                        var zeroCmd = SelfMotorProtocol.SetMotorZero(enumMotorId);
                        SendImportantData(zeroCmd);
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
           
            MotorTestMessage TestMessage = new MotorTestMessage();

            TestMessage.TestProject = _name + "初始化零点";
            int posStart = _plotViewPointMessage[_plotViewPointMessage.Count - 1].Point;
            int posEnd = _plotViewPointMessage[_plotViewPointMessage.Count - 1].Point;
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
                    posEnd = _plotViewPointMessage[_plotViewPointMessage.Count - 1].Point;
                    if (result == "stall")
                    {
                       
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                    }
                    else if (result == "PhyForwardLimited")
                    {


                        limtedPos.x = _plotViewPointMessage[_plotViewPointMessage.Count - 1].Point;
                        DispathcherInvoke($"电机测试正限位位置:{_plotViewPointMessage[_plotViewPointMessage.Count - 1]}");
                    }
                    else if (result == "PhyBackwardLimited")
                    {
                      
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                        var stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
                        SendImportantData(stopcmd);
                    }
                    else
                    {
                       
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                    }
                }
                catch (Exception ex)
                {
                    
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
                    posEnd = _plotViewPointMessage[_plotViewPointMessage.Count - 1].Point;
                    if (result == "stall")
                    {
                      
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                    }
                    else if (result == "PhyForwardLimited")
                    {
                       
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                        var stopcmd = SelfMotorProtocol.SetMotorOperatingStatus(enumMotorId, EnumMotorOperatingState.Stop);
                        SendImportantData(stopcmd);
                    }
                    else if (result == "PhyBackwardLimited")
                    {

                        DispathcherInvoke($"电机测试负限位位置:{_plotViewPointMessage[_plotViewPointMessage.Count - 1]}");
                        limtedPos.y = _plotViewPointMessage[_plotViewPointMessage.Count - 1].Point;
                    }
                    else
                    {
                       

                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                    }
                }
                catch (Exception ex)
                {
                   
                    DispathcherInvoke($"电机测试负限位测试引发异常:{ex}");
                }
            });
            return limtedPos;
        }
      
        /// <summary>
        /// 定时器事件，问询电机状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QueryMotorStatusTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var getMotorStatuscmd = SelfMotorProtocol.GetMotorStatus(enumMotorId);
            SendData(getMotorStatuscmd);
         

        }
        public DelegateCommand GetMotorStatusCommand { get; set; }
        private void GetMotorStatus()
        {
            var getMotorStatuscmd = SelfMotorProtocol.GetMotorStatus(enumMotorId);
            SendImportantData(getMotorStatuscmd);
        }

        private void MotorEnable()
        {
            var getMotorStatuscmd = SelfMotorProtocol.GetMotorStatus(enumMotorId);
            SendImportantData(getMotorStatuscmd);
        }

        /// <summary>
        /// 设置电机为闭环位置模式和使能
        /// </summary>
        /// <param name="MotorId">电机编号</param>
        private void SetMotorInit()
        {

            if (MotorModel.MotorParams.CtrType != EnumMotorCtrType.CloseLoopPosCtr)
            {
                var setCtModeCmd = SelfMotorProtocol.SetMotorControlMode(enumMotorId, EnumMotorCtrType.CloseLoopPosCtr);
                SendImportantData(setCtModeCmd);
            }
            if (MotorModel.MotorParams.Enable != true)
            {
                var setMotorEnableCmd = SelfMotorProtocol.SetMotorEnable(enumMotorId, EnumMotorEnable.Enable);
                SendImportantData(setMotorEnableCmd);
            }

        }
        /// <summary>
        /// 设置电机为开环速度模式和使能
        /// </summary>
        /// <param name="MotorId">电机编号</param>
        private void SetMotorSpeedInit()
        {
            if (MotorModel.MotorParams.CtrType != EnumMotorCtrType.OpenLoopSpeedCtr)
            {
                var setCtModeCmd = SelfMotorProtocol.SetMotorControlMode(enumMotorId, EnumMotorCtrType.OpenLoopSpeedCtr);
                SendImportantData(setCtModeCmd);
            }
            if (MotorModel.MotorParams.Enable != true)
            {
                var setMotorEnableCmd = SelfMotorProtocol.SetMotorEnable(enumMotorId, EnumMotorEnable.Enable);
                SendImportantData(setMotorEnableCmd);
            }
        }
        /// <summary>
        /// 通过速度模式移动
        /// </summary>
        /// <param name="enumMotorId"></param>
        /// <param name="Speed"></param>
        private void GotoInSpeedMode(EnumMotorId enumMotorId, int Speed)
        {

            SetMotorSpeedInit();
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
            SetMotorInit();
            var gotocmd = SelfMotorProtocol.SetMotorGoTo(enumMotorId, EnumMotorUnit.Pulse, pos);
            SendImportantData(gotocmd);
        }

        /// <summary>
        /// 设置软限位
        /// </summary>
        public void SetMotorLimitEnable(string enable)
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
            AddCmdEvent.Invoke(this, cmd);
        }
        private void SendImportantData(byte[] cmd)
        {
            AddImportantCmdEvent.Invoke(this,cmd);
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
        public void Parser_PacketReceivedEvent(object? sender, SelfMotorPacket e)
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
                        var pos = BitConverter.ToInt32(data, 2);
                        var ransformationCoefficient = BitConverter.ToSingle(data, 6);
                        break;
                    case EnumSelfMotorCmdType.CMD_GET_STATUS:
                        MyCustomEvent?.Invoke(this, data);
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
            MotorModel.MotorParams.MotorType = (EnumMotorType)motorType;
            MotorModel.MotorParams.CtrType = (EnumMotorCtrType)controlType;
            MotorModel.MotorParams.Unit = (EnumMotorUnit)unit;
            MotorModel.MotorParams.Enable = statusMaskBits[0];
            MotorModel.MotorParams.MoveState = statusMaskBits[4] == false ? EnumMotorMoveState.MotorStop : EnumMotorMoveState.MotorMove;
            var x = statusMaskBits[5];
            MotorModel.MotorParams.LimitedState = statusMaskBits[5] == true ? EnumMotorLimitedState.PhyBackwardLimited : (statusMaskBits[7] == true ? EnumMotorLimitedState.PhyForwardLimited : EnumMotorLimitedState.None);
            MotorModel.MotorParams.MoveDirection = (EmumMotorMoveDirection)MotorRunningDirection;
            MotorModel.MotorParams.SubRatio = unitConversionFactor;
            MotorModel.MotorParams.Pos = pulseCoordinate;
            AddPoint(_plotViewPointMessage, _plotViewSpeedMessage, MotorModel.MotorParams.Pos, MotorModel);
            //增加限位位置
            if (MotorModel.MotorParams.LimitedState == EnumMotorLimitedState.PhyForwardLimited)
            {
                MotorModel.MotorParams.PositiveLimitPosition = MotorModel.MotorParams.Pos;
            }
            if (MotorModel.MotorParams.LimitedState == EnumMotorLimitedState.PhyBackwardLimited)
            {
                MotorModel.MotorParams.NegativeLimitPosition = MotorModel.MotorParams.Pos;
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
           
            if (limtedMaskBits[0] == true)
            {
                MotorModel.MotorParams.SNLimted = true;
            }
            else
            {
                MotorModel.MotorParams.SNLimted = false;
            }
            if (limtedMaskBits[1] == true)
            {
                MotorModel.MotorParams.SPLimted = true;
            }
            else
            {
                MotorModel.MotorParams.SPLimted = false;
            }

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
                PlotViewPointMessage pointView = new PlotViewPointMessage() { Date = data, Point = point };
                PlotViewSpeedMessage speedView = new PlotViewSpeedMessage() { SpeedDate = pointList[pointList.Count - 1].Date, Speed = speed };
                pointList.Add(pointView);
                speedList.Add(speedView);
                motorModel.MotorParams.Speed = speed;


            }
            else
            {
                var data = DateTime.Now;
                PlotViewPointMessage pointView = new PlotViewPointMessage() { Date = data, Point = point };
                pointList.Add(pointView);
            }
        }

        private void RegisterTimer()
        {
            _getMotorStateTimer = new System.Timers.Timer(1000);
            _getMotorStateTimer.AutoReset = true;
            _getMotorStateTimer.Elapsed += QueryMotorStatusTimerElapsed;
        }

    }

}
