using MathNet.Numerics.RootFinding;
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using UtilityTools.Core.Dialog;
using UtilityTools.Modules.Motor5Controller.Model;
using UtilityTools.Modules.MotorTest.Entity;
using UtilityTools.Modules.MotorTest.Protocol;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.Services.Services;

namespace UtilityTools.Modules.MotorTest.Model
{
    public class FiveAxisModel:BindableBase
    {

        #region ------------Constructor------------
      
        public FiveAxisModel(IContainerProvider containerProvider,EnumMotorId Id, EnumMotorModel enumMotorModel, ThreeAxisTestModel testModel, string name)
        {
            _containerProvider = containerProvider;
            _dialogHostService = containerProvider.Resolve<IDialogHostService>();
            _enumMotorId = Id;
            _testModel = testModel;
            _name = name;
            MotorModel = new MotorModel(enumMotorModel);
            MotorModel.MotorParams.MotorModelID = enumMotorModel;
            Init();
        }
        #endregion
      
        private readonly string _name;
        ThreeAxisTestModel _testModel;
        private IContainerProvider _containerProvider;
        private readonly IDialogHostService _dialogHostService;

        private SelfMotorParser _parser;

        private Stopwatch _testTime;
       
       
        private TaskCompletionSource<string> _limitedtcs1;
        private TaskCompletionSource<string> _waitingReply;
        private TaskCompletionSource<string> _stallDetectiotcs;
        private List<int> timePosStallDetectionList;
   
        private long elapsedTime;
        private int[] _limtedPos;
       
        private bool _isPerformance;



        //public PlotModel SpeedPlotModel;
        //public PlotModel PiontPlotModel;
        public event EventHandler<byte[]> AddCmdEvent;
        public event EventHandler<byte[]> AddImportantCmdEvent;
        private double _std;
        private string _dataPath;
        public double Std
        {
            get { return _std; }
            set { _std = value; RaisePropertyChanged(); }
        }
        private  EnumMotorId _enumMotorId;
        public EnumMotorId EnumMotorId
        {
            get { return _enumMotorId; }
            set { _enumMotorId = value;RaisePropertyChanged(); }
        }

        private MotorTestMessage _TestMessage;

        #region ------------Property------------
        //private List<PlotViewPointMessage> _plotViewPointMessage;
        ///// <summary>
        ///// 位置信息列表
        ///// </summary>
        //public List<PlotViewPointMessage> PlotViewPointMessages
        //{
        //    get { return _plotViewPointMessage;}
        //    set { _plotViewPointMessage = value; RaisePropertyChanged(); }
        //}
        //private List<PlotViewSpeedMessage> _plotViewSpeedMessage;
        ///// <summary>
        ///// 速度信息列表
        ///// </summary>
        //public List<PlotViewSpeedMessage> PlotViewSpeedMessages
        //{
        //    get { return _plotViewSpeedMessage;}
        //    set { _plotViewSpeedMessage = value;RaisePropertyChanged(); }
        //}
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
            ByteQueue = new Queue<byte[]>();
            ImportantByteQueue = new Queue<byte[]>();
            PosLine = new LineSeries();
            SpeedLine = new LineSeries();
            MotorTestMessages = new ObservableCollection<MotorTestMessage>();
            Log = new ObservableCollection<string>();

           
            _TestMessage = new MotorTestMessage();
            SpeedLine = new LineSeries() { Title = _name, RenderInLegend = true };
            PosLine = new LineSeries() { Title = _name, RenderInLegend = true };
       
            
        }
        public void ClearLog()
        {
            if (Log != null)
            {
                Log.Clear();
            }
        }
        public void PosModeMove(string direction)
        {

            switch (direction)
            {
                case "p":; Goto(_enumMotorId, true); break;
                case "n": Goto(_enumMotorId, false); break;
                case "s":StopMotor();
                    break;
            }
        }
        public void SpeedModeMove(string direction)
        {

            switch (direction)
            {
                case "p":; GotoInSpeedMode(_enumMotorId, 5000000); break;
                case "n": GotoInSpeedMode(_enumMotorId, -5000000); break;
                case "s": StopMotor();
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
                        _testModel.MotorEntity.GetMotorStatusCommand(_enumMotorId);
                    }
                    Thread.Sleep(10);

                }
            });

        }
        public void NoPerformance()
        {
            _isPerformance = false; ;

        }
        public async Task TestSmoothnessDetection()
        {

            if (_testTime == null)
            {
                _testTime = new Stopwatch();
            }
            _testTime.Start();
            for (int i = 0; i < 50; i++)
            {

                bool result1 = await SmoothnessDetection(_enumMotorId);
                if (result1 == false)
                {
                    DispathcherInvoke($"电机限位测试失败，测试退出");
                    return;
                }
                if (_testTime.ElapsedMilliseconds / 1000.0 / 60 > 20)
                {
                    break;
                }
            }
            _testTime.Stop();
            _testTime = null;
           
        }
        public DelegateCommand BaseTestCommand { get; set; }
        public async Task BaseTest()
        {
           
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MotorTestMessages.Clear();
            }));
           
            SetMotorInit();
            Goto(_enumMotorId, false);
            Thread.Sleep(4000);
            _testModel.MotorEntity.SetMotorOperatingStatusCommand(_enumMotorId, EnumMotorOperatingState.Stop);
            var result = await TestMotorMove();
            if (result != true)
            {
                DispathcherInvoke($"电机移动测试失败，测试退出");
               
                
                return;
            }

            result = await EncoderDirectionTest();
            if (result != true)
            {
               
                DispathcherInvoke($"电机编码器测试失败，测试退出");
                
                return;
            }
            else
            {
                result = await LimitSwitchTest();
                if (result != true)
                {
                    DispathcherInvoke($"电机限位测试失败，测试退出");
                    return;
                }
            }
            
            var point = await FullTravelTest(_enumMotorId);
          
            result = await LimitPositioningAccuracyDetection();
            Goto(_enumMotorId, point);
            Thread.Sleep(5000);
            //await SmoothnessDetection();


        }
        /// <summary>
        /// 电机位置移动检测
        /// </summary>
        private async Task<bool> TestMotorMove()
        {
           
            MotorTestMessage TestMessage = new MotorTestMessage();

            await Task.Run(() =>
            {
                if (MotorModel.PointList.Count <= 0 )
                {
                    DispathcherInvoke($"没有读取到电机位置，测试退出");
                    return;
                }
                var posStart = MotorModel.PointList[MotorModel.PointList.Count - 1].Point;
                Goto(_enumMotorId, true);
                Thread.Sleep(3000);
                StopMotor();
                Thread.Sleep(3000);
                var posEnd = MotorModel.PointList[MotorModel.PointList.Count - 1].Point;
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
                DispathcherInvoke($"电机移动测试测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
            });
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MotorTestMessages.Add(TestMessage);
            }));
           
            if (TestMessage.TestResult == "合格" )
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
            
            MotorTestMessage TestMessage = new MotorTestMessage();
            SetMotorInit();
            await Task.Run(() =>
            {
                var posStart = MotorModel.PointList[MotorModel.PointList.Count - 1].Point;
                Goto(_enumMotorId,  true);
                Thread.Sleep(2000);
                StopMotor();
                Thread.Sleep(2000);
                var posEnd = MotorModel.PointList[MotorModel.PointList.Count - 1].Point;
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
                DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
            });
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MotorTestMessages.Add(TestMessage);
            }));
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
            
            var posStart = MotorModel.PointList[MotorModel.PointList.Count - 1].Point;
            var posEnd = MotorModel.PointList[MotorModel.PointList.Count - 1].Point;
            await Task.Run(async () =>
            {

                DistanceSettingForDifferentAxes(true);
                Thread.Sleep(5000);
                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();
                    //单开线程去测试堵转和限位
                    Task.Run(() => { MotorStallDetection(_enumMotorId); });
                    string result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(60));
                    posEnd = MotorModel.PointList[MotorModel.PointList.Count - 1].Point;
                    if (result == "stall")
                    {
                        TestMessage1.TestResult = "不合格";
                        TestMessage1.TestValue = "不正常";
                        TestMessage1.StandardValue = "正常";
                        TestMessage1.Description = "电机堵转，请检查";
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                    }
                    else if (result == "PhyForwardLimited"|| result == "SPLimted")
                    {
                        TestMessage1.TestResult = "合格";
                        TestMessage1.TestValue = "正常";
                        TestMessage1.StandardValue = "正常";
                        TestMessage1.Description = $"触发{result},正限位正常";
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                    }
                    else if (result == "PhyBackwardLimited" || result == "SNLimted")
                    {
                        TestMessage1.TestResult = "不合格";
                        TestMessage1.TestValue = "不正常";
                        TestMessage1.StandardValue = "正常";
                        TestMessage1.Description = $"触发{result}正限位反向，请检查";
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                        StopMotor();
                    }
                    else if (result == "stop")
                    {
                        TestMessage1.TestResult = "不合格";
                        TestMessage1.TestValue = "不正常";
                        TestMessage1.StandardValue = "正常";
                        TestMessage1.Description = $"触发{result},电机到达指定位置，脉冲过大，脉冲不合理";
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                     
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
                DispathcherInvoke($"电机正限位测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
            });
            posStart = posEnd;
            await Task.Run(async () =>
            {
                DistanceSettingForDifferentAxes(false);
                Thread.Sleep(5000);
                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();
                    Task.Run(() => { MotorStallDetection(_enumMotorId); });
                    string result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(60));
                    posEnd = MotorModel.PointList[MotorModel.PointList.Count - 1].Point;
                    if (result == "stall")
                    {
                        TestMessage2.TestResult = "不合格";
                        TestMessage2.TestValue = "不正常";
                        TestMessage2.StandardValue = "正常";
                        TestMessage2.Description = "电机堵转，请检查";
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                    }
                    else if (result == "PhyForwardLimited" || result == "SPLimted")
                    {
                        TestMessage2.TestResult = "不合格";
                        TestMessage2.TestValue = "不正常";
                        TestMessage2.StandardValue = "正常";
                        TestMessage2.Description = $"触发{result},负限位反向";
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                        StopMotor();
                    }
                    else if (result == "PhyBackwardLimited" || result == "SNLimted")
                    {
                        TestMessage2.TestResult = "合格";
                        TestMessage2.TestValue = "正常";
                        TestMessage2.StandardValue = "正常";
                        TestMessage2.Description = $"触发负限位{result}负限位正常";
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                    }
                    else if (result == "stop")
                    {
                        TestMessage1.TestResult = "不合格";
                        TestMessage1.TestValue = "不正常";
                        TestMessage1.StandardValue = "正常";
                        TestMessage1.Description = $"触发{result},电机到达指定位置，脉冲过大，脉冲不合理";
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
                DispathcherInvoke($"电机负限位测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
            });
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MotorTestMessages.Add(TestMessage1);
                MotorTestMessages.Add(TestMessage2);
            }));
           
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
        private async Task<int> FullTravelTest(EnumMotorId _enumMotorId)
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
                DistanceSettingForDifferentAxes(true);
                Thread.Sleep(3000);
                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();

                    Task.Run(() => { MotorStallDetection(_enumMotorId); });
                    result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(50));
                    if (result == "stall")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "电机堵转，请检查";

                    }
                    else if (result == "PhyForwardLimited" || result == "SPLimted")
                    {

                        posMessage.positiveLimitPosition = MotorModel.MotorParams.Pos;
                    }
                    else if (result == "PhyBackwardLimited" || result == "SNLimted")
                    {

                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = $"触发{result},正限位反向";
                        StopMotor();
                    }
                    else if (result == "stop")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = $"触发{result},电机到达指定位置，脉冲过大，脉冲不合理";
                        DispathcherInvoke($"stop");

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
            if (!(result == "PhyForwardLimited" || result == "SPLimted"))
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    MotorTestMessages.Add(TestMessage);
               
                }));
               
            }

            await Task.Run(async () =>
            {
                //单开线程去测试堵转和限位
                DistanceSettingForDifferentAxes(false);
                Thread.Sleep(3000);
                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();
                    Task.Run(() => { MotorStallDetection(_enumMotorId); });
                    string result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(50));
                    if (result == "stall")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "电机堵转，请检查";
                    }
                    else if (result == "PhyForwardLimited" || result == "SPLimted")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = $"触发{result},负限位反向";
                        StopMotor();

                    }
                    else if (result == "PhyBackwardLimited" || result == "SNLimted")
                    {

                        posMessage.negativeLimitPositionendPos = MotorModel.MotorParams.Pos;
                        int fullStrokeOfMotor = posMessage.positiveLimitPosition - posMessage.negativeLimitPositionendPos;
                        if ((fullStrokeOfMotor < 170000 && fullStrokeOfMotor > 160000)
                        || (fullStrokeOfMotor < 130000 && fullStrokeOfMotor > 120000)
                        || (fullStrokeOfMotor < 240000 && fullStrokeOfMotor > 210000)
                         || (fullStrokeOfMotor < 900000 && fullStrokeOfMotor > 700000)
                         || (fullStrokeOfMotor < 500000 && fullStrokeOfMotor > 350000)
                         || (fullStrokeOfMotor < 5000 && fullStrokeOfMotor > 3000))
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
                    else if (result == "stop")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = $"电机到达，指定位置，脉冲过大，请检查编码器是否正常";
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
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MotorTestMessages.Add(TestMessage);
            }));
           
         
             return (posMessage.positiveLimitPosition - posMessage.negativeLimitPositionendPos)/2 + posMessage.negativeLimitPositionendPos;

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

                DistanceSettingForDifferentAxes(true);
                Thread.Sleep(5000);

                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();

                    Task.Run(() => { MotorStallDetection(_enumMotorId); });
                    result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(50));
                    switch (result)
                    {
                        case "stall":
                            TestMessage.TestResult = "不合格";
                            TestMessage.TestValue = "不正常";
                            TestMessage.StandardValue = "正常";
                            TestMessage.Description = "电机堵转，请检查"; break;
                        case "PhyForwardLimited":
                            posMessage.positiveLimitPosition = MotorModel.MotorParams.Pos; break;
                        case "SPLimted":
                            posMessage.positiveLimitPosition = MotorModel.MotorParams.Pos; break;
                        case "PhyBackwardLimited":
                            TestMessage.TestResult = "不合格";
                            TestMessage.TestValue = "不正常";
                            TestMessage.StandardValue = "正常";
                            TestMessage.Description = $"触发{result},正限位反向";
                            StopMotor(); break;
                        case "SNLimted":
                            TestMessage.TestResult = "不合格";
                            TestMessage.TestValue = "不正常";
                            TestMessage.StandardValue = "正常";
                            TestMessage.Description = $"触发{result},正限位反向";
                            StopMotor(); break;
                        case "stop":
                            TestMessage.TestResult = "不合格";
                            TestMessage.TestValue = "不正常";
                            TestMessage.StandardValue = "正常";
                            TestMessage.Description = $"电机到达指定位置，脉冲过大，检查编码器是否正常"; break;
                        default:
                            TestMessage.TestResult = "不合格";
                            TestMessage.TestValue = "不正常";
                            TestMessage.StandardValue = "正常";
                            TestMessage.Description = result;break;
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
            if (!(result == "PhyForwardLimited" || result == "SPLimted"))
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    MotorTestMessages.Add(TestMessage);

                }));
                return false;
            }

            await Task.Run(async () =>
            {
                //单开线程去测试堵转和限位
                DistanceSettingForDifferentAxes(false);
                Thread.Sleep(5000);
                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();
                    Task.Run(() => { MotorStallDetection(_enumMotorId); });
                    string result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(50));

                   
                    if (result == "stall")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "电机堵转，请检查";
                    }
                    else if (result == "PhyForwardLimited" || result == "SPLimted")
                    {
                        TestMessage.Description = $"触发{result},负限位反向";
                        StopMotor();

                    }
                    else if (result == "PhyBackwardLimited" || result == "SNLimted")
                    {

                        posMessage.negativeLimitPositionendPos = MotorModel.MotorParams.Pos;
                        int fullStrokeOfMotor = posMessage.positiveLimitPosition - posMessage.negativeLimitPositionendPos;
                        if ((fullStrokeOfMotor < 170000 && fullStrokeOfMotor > 160000)
                        || (fullStrokeOfMotor < 130000 && fullStrokeOfMotor > 120000)
                        || (fullStrokeOfMotor < 240000 && fullStrokeOfMotor > 210000)
                        || (fullStrokeOfMotor < 900000 && fullStrokeOfMotor > 700000)
                        || (fullStrokeOfMotor < 450000 && fullStrokeOfMotor > 350000)
                        || (fullStrokeOfMotor < 5000 && fullStrokeOfMotor > 3000))
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
                                if (Math.Abs(_limtedPos[2] - _limtedPos[0]) < 200 && Math.Abs(_limtedPos[3] - _limtedPos[1]) < 200)
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
                    else if (result == "stop")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = $"电机到达指定位置，脉冲过大，检查编码器是否正常"; 
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
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MotorTestMessages.Add(TestMessage);

            }));
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

        private async Task<bool> SmoothnessDetection(EnumMotorId _enumMotorId)
        {
            int startIndex = 0, endIndex = 0;
            (int positiveLimitPosition, int negativeLimitPositionendPos) posMessage = (-1, 1);
            MotorTestMessage TestMessage = new MotorTestMessage();
            string result = "";
            await Task.Run(async () =>
            {
                //单开线程去测试堵转和限位
                _limitedtcs1 = new TaskCompletionSource<string>();
                GotoInSpeedMode(_enumMotorId, 10000);
                Thread.Sleep(5000);

                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();

                    Task.Run(() => { MotorStallDetection(_enumMotorId); });
                    result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(50));
                    if (result == "stall")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "电机堵转，请检查";

                    }
                    else if (result == "PhyForwardLimited" || result == "SPLimted")
                    {



                    }
                    else if (result == "PhyBackwardLimited" || result == "SNLimted")
                    {

                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = $"触发{result}正限位反向";
                        StopMotor();
                    }
                    else if (result == "stop")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = $"触发{result}，脉冲过大异常";
                    }
                    else
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = $"触发{result} ,电机到达指点位置脉冲过大，异常";

                    }
                }
                catch (Exception ex)
                {
                    TestMessage.TestResult = "不合格";
                    TestMessage.TestValue = "不正常";
                    TestMessage.StandardValue = "正常";
                    TestMessage.Description = $"异常{ex}";
                }
            });
            if (!(result == "PhyForwardLimited" || result == "SPLimted" ))
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    MotorTestMessages.Add(TestMessage);

                }));
                return false;
            }

            await Task.Run(async () =>
            {
                //单开线程去测试堵转和限位
                GotoInSpeedMode(_enumMotorId, -10000);
                startIndex = MotorModel.SpeedList.Count;
                Thread.Sleep(5000);
                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();
                    Task.Run(() => { MotorStallDetection(_enumMotorId); });
                    result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(120));
                    if (result == "stall")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "电机堵转，请检查";
                    }
                    else if (result == "PhyForwardLimited"|| result == "SPLimted")
                    {
                        TestMessage.TestResult = "不合格";
                        TestMessage.TestValue = "不正常";
                        TestMessage.StandardValue = "正常";
                        TestMessage.Description = "负限位反向";
                        StopMotor();

                    }
                    else if (result == "PhyBackwardLimited" || result == "SNLimted")
                    {
                        //正常的情况下，需要记录前面运行过程中的点数
                        endIndex = MotorModel.SpeedList.Count;

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
            if (!(result == "PhyBackwardLimited" || result == "SNLimted"))
            {
                TestMessage.Description = $" 触发{result}从正方向到负方向的位置丝杆顺滑度错误";
                return false;

            }
          
            if (!SmoothnessTest(startIndex, endIndex, MotorModel.SpeedList))
            {

                TestMessage.TestResult = "不合格";
                TestMessage.TestValue = "不正常";
                TestMessage.StandardValue = "正常";
                TestMessage.Description = $"触发{result}正方向到负方向丝杆顺滑度不达标，请重新安装";
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    MotorTestMessages.Add(TestMessage);

                }));
                return false;
            }
            

            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MotorTestMessages.Add(TestMessage);

            }));
            return true;

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
        private void MotorStallDetection(EnumMotorId _enumMotorId)
        {
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
                timePosStallDetectionList.Add(MotorModel.MotorParams.Pos);

                if (i > 4)
                {
                    if (Math.Abs(timePosStallDetectionList[timePosStallDetectionList.Count - 1] - timePosStallDetectionList[timePosStallDetectionList.Count - 4]) < 50)
                    {
                        StopMotor();
                        _limitedtcs1.SetResult("stall");
                        DispathcherInvoke($"{timePosStallDetectionList[timePosStallDetectionList.Count - 1]}-{timePosStallDetectionList[timePosStallDetectionList.Count - 3]} = {timePosStallDetectionList[timePosStallDetectionList.Count - 1]- timePosStallDetectionList[timePosStallDetectionList.Count - 3]}电机堵转，退出");
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
                if (MotorModel.MotorParams.SNLimted)
                {
                    _limitedtcs1.SetResult("SNLimted");
                    DispathcherInvoke($"发送软件负向限位退出");
                    return;
                }
                if (MotorModel.MotorParams.SPLimted)
                {
                    _limitedtcs1.SetResult("SPLimted");
                    DispathcherInvoke($"发送软件正向限位退出");
                    return;
                }
                if (MotorModel.MotorParams.MoveState == EnumMotorMoveState.MotorStop)
                {
                    _limitedtcs1.SetResult("stop");
                    DispathcherInvoke($"发送电机到位停止退出");
                    return;
                }
                Thread.Sleep(1000);
            }
            DispathcherInvoke($"测试堵转功能退出");

        }

        /// <summary>
        /// 电机堵转和限位检测
        /// </summary>
        private void RotateMotorStallDetection(EnumMotorId _enumMotorId)
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
                        StopMotor();
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
                if (MotorModel.MotorParams.SNLimted)
                {
                    _limitedtcs1.SetResult("SNLimted");
                    DispathcherInvoke($"发送软件负向限位退出");
                    return;
                }
                if (MotorModel.MotorParams.SPLimted)
                {
                    _limitedtcs1.SetResult("SPLimted");
                    DispathcherInvoke($"发送软件正向限位退出");
                    return;
                }
                if (MotorModel.MotorParams.MoveState == EnumMotorMoveState.MotorStop)
                {
                    _limitedtcs1.SetResult("stop");
                    DispathcherInvoke($"发送电机到位停止退出");
                    return;
                }
                Thread.Sleep(500);
            }
            DispathcherInvoke($"测试堵转功能退出");

        }
        
        /// <summary>
        /// 取消软限位
        /// </summary>
        
        public void CloseSlimited()
        {
            byte limitEnable = 0b00011100;
            _testModel.MotorEntity.SetMotorLimitEnableCommand(_enumMotorId, limitEnable);
        }

        public DelegateCommand AutomaticZeroInitializationCommand { get; set; }
        public async void AutomaticZeroInitialization()
        {
            var resultx = await FindLinted(_enumMotorId);
            if (resultx == (0, 0))
            {
                return;
            }
         
            int pos = (resultx.Item1 - resultx.Item2) / 2 + resultx.Item2;
            Goto(_enumMotorId, pos);
            Thread.Sleep(5000);
            await Task.Run(() => 
            {
                int i = 0;
                while (true)
                {
                    if (MotorModel.MotorParams.MoveState == EnumMotorMoveState.MotorStop)
                    {
                        _testModel.MotorEntity.SetMotorZeroCommand(_enumMotorId);
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
        private async Task<(int, int)> FindLinted(EnumMotorId _enumMotorId)
        {
            (int x, int y) limtedPos = (0, 0);
           
            MotorTestMessage TestMessage = new MotorTestMessage();

            TestMessage.TestProject = _name + "初始化零点";
            int posStart = MotorModel.PointList[MotorModel.PointList.Count - 1].Point;
            int posEnd = MotorModel.PointList[MotorModel.PointList.Count - 1].Point;
            await Task.Run(async () =>
            {

                Goto(_enumMotorId, true);
                Thread.Sleep(5000);
                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();
                    //单开线程去测试堵转和限位
                    Task.Run(() => { MotorStallDetection(_enumMotorId); });
                    string result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(60));
                    posEnd = MotorModel.PointList[MotorModel.PointList.Count - 1].Point;
                    if (result == "stall")
                    {
                       
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                    }
                    else if (result == "PhyForwardLimited")
                    {


                        limtedPos.x = MotorModel.PointList[MotorModel.PointList.Count - 1].Point;
                        DispathcherInvoke($"电机测试正限位位置:{MotorModel.PointList[MotorModel.PointList.Count - 1]}");
                    }
                    else if (result == "PhyBackwardLimited")
                    {
                      
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                        StopMotor();
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
                Goto(_enumMotorId, false);
                Thread.Sleep(5000);
                try
                {
                    _limitedtcs1 = new TaskCompletionSource<string>();
                    Task.Run(() => { MotorStallDetection(_enumMotorId); });
                    string result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(60));
                    posEnd = MotorModel.PointList[MotorModel.PointList.Count - 1].Point;
                    if (result == "stall")
                    {
                      
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                    }
                    else if (result == "PhyForwardLimited")
                    {
                       
                        DispathcherInvoke($"电机测试End:{posEnd},Start:{posStart} 行程:{posEnd - posStart}");
                        StopMotor();
                    }
                    else if (result == "PhyBackwardLimited")
                    {

                        DispathcherInvoke($"电机测试负限位位置:{MotorModel.PointList[MotorModel.PointList.Count - 1]}");
                        limtedPos.y = MotorModel.PointList[MotorModel.PointList.Count - 1].Point;
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
        private void AddMessage(MotorTestMessage message,string TestResult, string TestValue, string StandardValue, string Descriptiondes)
        {
            message.TestResult = TestResult;
            message.TestValue = TestValue;
            message.StandardValue = StandardValue;
            message.Description = Descriptiondes;
        }
        private void StopMotor()
        {
            _testModel.MotorEntity.SetMotorOperatingStatusCommand(_enumMotorId, EnumMotorOperatingState.Stop);
        }
        private void DistanceSettingForDifferentAxes(bool direction)
        {
            if (MotorModel.MotorParams.MotorModelID == EnumMotorModel.MOTOR_t)
            {
                if (direction)
                {
                    Goto(_enumMotorId, (int)(MotorModel.MotorParams.SubRatio * 95));
                }
                else
                {
                    Goto(_enumMotorId, -(int)(MotorModel.MotorParams.SubRatio * 15));
                }
            }
            else if ((MotorModel.MotorParams.MotorModelID == EnumMotorModel.MOTOR_r))
            {
                if (direction)
                {
                    Goto(_enumMotorId, (int)(MotorModel.MotorParams.SubRatio * 360));
                }
                else
                {
                    Goto(_enumMotorId, -(int)(MotorModel.MotorParams.SubRatio * 360));
                }
            }
            else 
            {
                if (direction)
                {
                    Goto(_enumMotorId, true);
                }
                else
                {
                    Goto(_enumMotorId, false);
                }
            }
          
        }


        public DelegateCommand GetMotorStatusCommand { get; set; }
        /// <summary>
        /// 获取电机状态
        /// </summary>
        private void GetMotorStatus()
        {
            _testModel.MotorEntity.GetMotorStatusCommand(_enumMotorId);
        }

       /// <summary>
       /// 初始化移动轴或者旋转轴
       /// </summary>
        private void InitAxType()
        {
            if (MotorModel.MotorParams.MotorModelID == EnumMotorModel.MOTOR_t || MotorModel.MotorParams.MotorModelID == EnumMotorModel.MOTOR_r)
            {
                if (MotorModel.MotorParams.MoveType != EnumMotorMoveType.MoRotationve)
                {
                    _testModel.MotorEntity.SetAxTypeCommand(_enumMotorId, EnumMotorMoveType.MoRotationve);
                }
            }
            else
            {
                if (MotorModel.MotorParams.MoveType != EnumMotorMoveType.Move)
                {
                    _testModel.MotorEntity.SetAxTypeCommand(_enumMotorId, EnumMotorMoveType.Move);
                }
            }
        }
        /// <summary>
        /// 设置电机为闭环位置模式和使能
        /// </summary>
        /// <param name="MotorId">电机编号</param>
        private void SetMotorInit()
        {

            InitAxType();
            if (MotorModel.MotorParams.CtrType != EnumMotorCtrType.CloseLoopPosCtr)
            {
                _testModel.MotorEntity.SetMotorControlModeCommand(_enumMotorId, EnumMotorCtrType.CloseLoopPosCtr);
                _testModel.MotorEntity.SetMotorEnableCommand(_enumMotorId, EnumMotorEnable.Enable);
            }
            if (MotorModel.MotorParams.Enable != true)
            {
                _testModel.MotorEntity.SetMotorEnableCommand(_enumMotorId, EnumMotorEnable.Enable);
            }

        }
        /// <summary>
        /// 设置电机为开环速度模式和使能
        /// </summary>
        /// <param name="MotorId">电机编号</param>
        private void SetMotorSpeedInit()
        {
            InitAxType();
            if (MotorModel.MotorParams.CtrType != EnumMotorCtrType.OpenLoopSpeedCtr)
            {
                _testModel.MotorEntity.SetMotorControlModeCommand(_enumMotorId, EnumMotorCtrType.OpenLoopSpeedCtr);
                _testModel.MotorEntity.SetMotorEnableCommand(_enumMotorId, EnumMotorEnable.Enable);
            }
            if (MotorModel.MotorParams.Enable != true)
            {
                var setMotorEnableCmd = SelfMotorProtocol.SetMotorEnable(_enumMotorId, EnumMotorEnable.Enable);
                _testModel.MotorEntity.SetMotorEnableCommand(_enumMotorId, EnumMotorEnable.Enable);

            }
            
        }
        /// <summary>
        /// 通过速度模式移动
        /// </summary>
        /// <param name="_enumMotorId"></param>
        /// <param name="Speed"></param>
        private void GotoInSpeedMode(EnumMotorId _enumMotorId, int Speed)
        {

            SetMotorSpeedInit();
            _testModel.MotorEntity.SetMotorGoToCommand(_enumMotorId, EnumMotorUnit.Pulse, Speed);
        }

        /// <summary>
        /// 获取除了查询电机状态以外的所有的信息
        /// </summary>
        private void GetAllStatuses()
        {
            _testModel.MotorEntity.GetAxTypeCommand(_enumMotorId);
        }
        /// <summary>
        /// 通过位置模式移动
        /// </summary>
        /// <param name="_enumMotorId"></param>
        /// <param name="pos"></param>
        private void Goto(EnumMotorId _enumMotorId, bool direction)
        {
            SetMotorInit();
            if (direction)
            {
                _testModel.MotorEntity.SetMotorGoToCommand(_enumMotorId, EnumMotorUnit.Pulse, MotorModel.MotorParams.Pos + 1000000);
            }
            else
            {
                _testModel.MotorEntity.SetMotorGoToCommand(_enumMotorId, EnumMotorUnit.Pulse, MotorModel.MotorParams.Pos - 1000000);
            }
           
            
        }
        private void Goto(EnumMotorId _enumMotorId, int pos)
        {
            SetMotorInit();
          
            _testModel.MotorEntity.SetMotorGoToCommand(_enumMotorId, EnumMotorUnit.Pulse, pos);
            
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
            _testModel.MotorEntity.SetMotorLimitEnableCommand(_enumMotorId, limitEnable);

        }

        
        /// <summary>
        /// 获取限位掩码状态
        /// </summary>
        public void GetMotorLimitEnable()
        {
            _testModel.MotorEntity.GetMotorLimitEnableCommand(_enumMotorId);
        }
       
       
        public event EventHandler<byte[]> MyCustomEvent;

        
        
        public void Parser_PacketReceivedEvent( SelfMotorPacket e)
        {
            if (e != null)
            {
              
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
                        ParserMotorStatus(data);

                        break;
                    case EnumSelfMotorCmdType.CMD_GET_SLIM:
                        ParserMotorLimtedStatus(data); 
                        break;
                    case EnumSelfMotorCmdType.CMD_SET_AXUNIT:
                        GetAllStatuses();
                        break;
                    case EnumSelfMotorCmdType.CMD_GET_AXTYPE:
                        MotorModel.MotorParams.MoveType = (EnumMotorMoveType)(e.DataSource[1]);
                        break;
                }
            }
        }

        private void ParserMotorStatus( byte[] data)
        {
            byte channel = data[0];//电机通道
            byte motorType = data[1];//电机类型
            byte AxisType = data[2];//轴类型
            byte controlType = data[3];//控制模式
            byte unit = data[4];//参数单位类型
            byte statusMask = data[5];//电机状态掩码 R/H 使能 SN软限位    SZ软限位 SP软限位 R/S运行和停止  N是硬件限位 z硬件限位 p 硬件限//e2
            byte softLimitEnableMask = data[6];//软限位使能掩码/d
            sbyte MotorRunningDirection = (sbyte)data[7];//电机运行方向
            float unitConversionFactor = BitConverter.ToSingle(data, 8);//单位换算比例
            Int32 pulseCoordinate = BitConverter.ToInt32(data, 12);//当前的脉冲坐标
            Int32 pulseSpeed = BitConverter.ToInt32(data, 16);//当前的脉冲速度

            EnumMotorId channelId = (EnumMotorId)channel;
            //电机状态掩码分析
            bool[] statusMaskBits = new bool[8];
            bool[] statusEnableBits = new bool[8];
            for (int i = 0; i < 8; i++) 
            {
                bool bit = (statusMask & (1 << i)) != 0;
                statusMaskBits[7 - i] = bit;
            }
            for (int i = 0; i < 8; i++)
            {
                bool bit = (softLimitEnableMask & (1 << i)) != 0;
                statusEnableBits[7-i] = bit;
            }
            MotorModel.MotorParams.MotorType = (EnumMotorType)motorType;
            MotorModel.MotorParams.CtrType = (EnumMotorCtrType)controlType;
            MotorModel.MotorParams.Unit = (EnumMotorUnit)unit;
            MotorModel.MotorParams.Enable = statusMaskBits[0];
            MotorModel.MotorParams.MoveState = statusMaskBits[4] == false ? EnumMotorMoveState.MotorStop : EnumMotorMoveState.MotorMove;
            var x = statusMaskBits[5];
            

            MotorModel.MotorParams.LimitedState = statusMaskBits[5] == true ? EnumMotorLimitedState.PhyBackwardLimited : (statusMaskBits[7] == true ? EnumMotorLimitedState.PhyForwardLimited : EnumMotorLimitedState.None);

            if (statusEnableBits[1])
            {
                MotorModel.MotorParams.SNLimted = statusMaskBits[1];
            }
            if (statusEnableBits[3])
            {
                MotorModel.MotorParams.SPLimted = statusMaskBits[3];
            }
            MotorModel.MotorParams.MoveDirection = (EmumMotorMoveDirection)MotorRunningDirection;
            MotorModel.MotorParams.SubRatio = unitConversionFactor;
            MotorModel.MotorParams.Pos = pulseCoordinate;
            AddPoint();
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
        /// <summary>
        ///查限位时候的回报解析
        /// </summary>
        /// <param name="data"></param>
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
        private void AddPoint( )
        {
             
            if (MotorModel.PointList.Count >= 2)
            {
                var data = DateTime.Now;
                var timeDifference = (data - MotorModel.PointList[MotorModel.PointList.Count - 2].Date).TotalMilliseconds;
                var speed = (MotorModel.MotorParams.Pos - MotorModel.PointList[MotorModel.PointList.Count - 2].Point) / (timeDifference * 1.0) * 1000;
                PlotViewPointMessage pointView = new PlotViewPointMessage() { Date = data, Point = MotorModel.MotorParams.Pos, MotorMoveState = MotorModel.MotorParams.MoveState , MotorModelAxis  = MotorModel.MotorModelAxis};
                PlotViewSpeedMessage speedView = new PlotViewSpeedMessage() { SpeedDate = MotorModel.PointList[MotorModel.PointList.Count - 1].Date, Speed = speed , MotorModelAxis = MotorModel.MotorModelAxis };
                if (MotorModel.MotorModelAxis == EnumMotorModel.MOTOR_x)
                {
                    Console.WriteLine();    
                }
                if (MotorModel.MotorModelAxis == EnumMotorModel.MOTOR_y)
                {
                    Console.WriteLine();
                }
                if (EnumMotorId == EnumMotorId.MOTOR_1)
                {
                    Console.WriteLine();
                }
                if (EnumMotorId == EnumMotorId.MOTOR_2)
                {
                    Console.WriteLine();
                }

                MotorModel.PointList.Add(pointView);
                MotorModel.SpeedList.Add(speedView);
                MotorModel.PointListBuffer.Add(pointView);
                MotorModel.SpeedListBuffer.Add(speedView);
                MotorModel.MotorParams.Speed = speed;
                if (MotorModel.PointListBuffer.Count == _testModel.SavePointNumber)
                {
                    SqliteSaveDate();
                }
                if (_testModel.MotorSpeedplotModel != null && _testModel.MotorplotModel != null)
                {
                   
                   
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        _testModel.MotorplotModel.InvalidatePlot(true);
                        _testModel.MotorSpeedplotModel.InvalidatePlot(true);
                    });
                   
                }
            }
            else
            {
                var data = DateTime.Now;
                PlotViewPointMessage pointView = new PlotViewPointMessage() { Date = data, Point = MotorModel.MotorParams.Pos, MotorMoveState = MotorModel.MotorParams.MoveState };
                MotorModel.PointList.Add(pointView);
                
                if (_testModel.MotorplotModel != null)
                {
                    _testModel.MotorplotModel.InvalidatePlot(true);
                }
                
            }
        }
        private void SqliteSaveDate()
        {
            var speedBuffer = MotorModel.SpeedListBuffer.ToList();
            var pointBuffer = MotorModel.PointListBuffer.ToList();
            var speedCoint = speedBuffer.Count;
            var pointCoint = pointBuffer.Count;
            if (_testModel.MotorTypeModel.EnumMotorAxisType == EnumMotorAxisType.TwoAxisMotor)
            {
                _testModel.TwoMotorSpliteOperat.AddPlotViewSpeedListMessagesSimpleAsync(speedBuffer);
                _testModel.TwoMotorSpliteOperat.AddPlotViewPointListMessagesSimpleAsync(pointBuffer);
            }
            else
            {
                _testModel.FiveMotorSpliteOperat.AddPlotViewSpeedListMessagesSimpleAsync(speedBuffer);
                _testModel.FiveMotorSpliteOperat.AddPlotViewPointListMessagesSimpleAsync(pointBuffer);
            }
            
            MotorModel.SpeedListBuffer.RemoveRange(0, speedCoint);
            MotorModel.PointListBuffer.RemoveRange(0, pointCoint);
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
            if (string.IsNullOrEmpty(_dataPath))
            {
                return;
            }
            else
            {
                if(!Directory.Exists(_dataPath))
                {
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    _dataPath = Path.Combine(baseDirectory,"Data");
                    Directory.CreateDirectory(_dataPath);
                }
                var path = Path.Combine(_dataPath, DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"));
                Directory.CreateDirectory(path);
                Serilize(path);

                MotorModel.PointList.Clear();
                MotorModel.SpeedList.Clear();
                if (_testModel.MotorSpeedplotModel != null && _testModel.MotorplotModel != null)
                {
                    _testModel.MotorSpeedplotModel.InvalidatePlot(true);
                    _testModel.MotorplotModel.InvalidatePlot(true);
                }

            }
        }
        public void Serilize(string path)
        {
            string Pointjson = JsonConvert.SerializeObject(MotorModel.PointList);
            string Speedjson = JsonConvert.SerializeObject(MotorModel.SpeedList);
            string testMessgae = JsonConvert.SerializeObject(MotorTestMessages);
            // 获取当前时间并格式化为文件名安全的字符串

            string PointjsonfilePathfileName = $"{MotorModel.MotorParams.MotorModelID}Point.json";
            string PointjsonfilePath = System.IO.Path.Combine(path, PointjsonfilePathfileName); // 组合完整路径

            string SpeedjsonfilePathfileName = $"{MotorModel.MotorParams.MotorModelID}Speed.json";
            string SpeedjsonfilePath = Path.Combine(path, SpeedjsonfilePathfileName); // 组合完整路径

            string testMessgaefilePathfileName = $"{MotorModel.MotorParams.MotorModelID}testMessage.json";
            string testMessgaejsonfilePath = Path.Combine(path, testMessgaefilePathfileName); // 组合完整路径
                                                                                                // 写入JSON数据
            File.WriteAllText(PointjsonfilePath, Pointjson);
            File.WriteAllText(SpeedjsonfilePath, Speedjson);
            File.WriteAllText(testMessgaejsonfilePath, testMessgae);
          
        }
        public void Deserilize(string path)
        {
            string PointjsonfilePathfileName = $"{MotorModel.MotorParams.MotorModelID}Point.json";
            string PointjsonfilePath = Path.Combine(path, PointjsonfilePathfileName); // 组合完整路径
            string SpeedjsonfilePathfileName = $"{MotorModel.MotorParams.MotorModelID}Speed.json";
            string SpeedjsonfilePath = Path.Combine(path, SpeedjsonfilePathfileName); // 组合完整路径

            string testMessgaefilePathfileName = $"{MotorModel.MotorParams.MotorModelID}testMessage.json";
            string testMessgaejsonfilePath = Path.Combine(path, testMessgaefilePathfileName); // 组合完整路径
            if (File.Exists(PointjsonfilePath))
            {
                MotorModel.PointList = JsonConvert.DeserializeObject<List<PlotViewPointMessage>>(File.ReadAllText(PointjsonfilePath));
            }
            if (File.Exists(SpeedjsonfilePath))
            {
                MotorModel.SpeedList = JsonConvert.DeserializeObject<List<PlotViewSpeedMessage>>(File.ReadAllText(SpeedjsonfilePath));
            }
            if (File.Exists(testMessgaejsonfilePath))
            {
                MotorTestMessages = JsonConvert.DeserializeObject<ObservableCollection<MotorTestMessage>>(File.ReadAllText(testMessgaejsonfilePath));
            }
            _speedLine.ItemsSource = MotorModel.SpeedList;
            _posLine.ItemsSource = MotorModel.PointList;
            _speedLine.DataFieldX = "SpeedDate";
            _speedLine.DataFieldY = "Speed";
            _posLine.DataFieldX = "Date";
            _posLine.DataFieldY = "Point";
            if (_testModel.MotorSpeedplotModel != null && _testModel.MotorplotModel != null)
            {
                _testModel.MotorSpeedplotModel.InvalidatePlot(true);
                _testModel.MotorplotModel.InvalidatePlot(true);
            }

     

        }
    }
}
