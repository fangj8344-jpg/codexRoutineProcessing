using MathNet.Numerics.RootFinding;
using Newtonsoft.Json;
using OpenCvSharp.Aruco;
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using UtilityTools.Core.Dialog;
using UtilityTools.Modules.MotorTest.Entity;
using UtilityTools.Modules.MotorTest.Interface;
using UtilityTools.Modules.MotorTest.Protocol;
using UtilityTools.Modules.MotorTest.SQLite;
using UtilityTools.Modules.MotorTest.TestItems;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.Services.Services;

namespace UtilityTools.Modules.MotorTest.Model
{
    public class FiveAxisModel : BindableBase
    {

        #region ------------Constructor------------

        public FiveAxisModel(IContainerProvider containerProvider, EnumMotorId Id, EnumMotorModel enumMotorModel, ThreeAxisTestModel testModel, string name)
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
        public Dictionary<string, int> keyValuePairs;
        public event EventHandler<byte[]> AddCmdEvent;
        public event EventHandler<byte[]> AddImportantCmdEvent;
        private double _std;
        private string _dataPath;
        public double Std
        {
            get { return _std; }
            set { _std = value; RaisePropertyChanged(); }
        }
        private EnumMotorId _enumMotorId;
        public EnumMotorId EnumMotorId
        {
            get { return _enumMotorId; }
            set { _enumMotorId = value; RaisePropertyChanged(); }
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
        private (int minValue, int maxValue) _fullStrokeRange = (0, 0);
        /// <summary>
        /// 行程
        /// </summary>
        public (int minValue, int maxValue) FullStrokeRange
        {
            get { return _fullStrokeRange; }
            set { _fullStrokeRange = value; RaisePropertyChanged(); }
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

        private LineSeries _posLine;
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
            set { _speedLine = value; RaisePropertyChanged(); }
        }



        private MotorModel _motorModel;
        public MotorModel MotorModel
        {
            get { return _motorModel; }
            set { _motorModel = value; RaisePropertyChanged(); }
        }
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
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
            ReverseMoveCommand = new DelegateCommand<bool?>(ReverseMove);
            ForwardMoveCommand = new DelegateCommand<bool?>(ForwardMove);
            StopMotorCommand = new DelegateCommand(StopMotor);
            ByteQueue = new Queue<byte[]>();
            ImportantByteQueue = new Queue<byte[]>();
            PosLine = new LineSeries();
            SpeedLine = new LineSeries();
            MotorTestMessages = new ObservableCollection<MotorTestMessage>();
            Log = new ObservableCollection<string>();


            _TestMessage = new MotorTestMessage();
            SpeedLine = new LineSeries()
            {
                Title = _name,
                RenderInLegend = true,
                StrokeThickness = 1,
                LineStyle = LineStyle.Solid,
                CanTrackerInterpolatePoints = false,
                MarkerType = MarkerType.None, // 禁用点标记
                MarkerSize = 0,

            };
            PosLine = new LineSeries() 
            {
                Title = _name, 
                RenderInLegend = true,
                StrokeThickness = 1,
                LineStyle = LineStyle.Solid,
                CanTrackerInterpolatePoints = false,
                MarkerType = MarkerType.None, // 禁用点标记
                MarkerSize = 0,

            };


        }
        public void ClearLog()
        {
            if (Log != null)
            {
                Log.Clear();
            }
        }
        public DelegateCommand<bool?> ReverseMoveCommand { get; set; }
        /// <summary>
        /// 反向移动
        /// </summary>
        private void ReverseMove(bool? moveModel)
        {
            if (moveModel == true)
            {
                GotoInSpeedMode(_enumMotorId, false);
            }
            else
            {
                Goto(_enumMotorId, false);
            }
        }
        public DelegateCommand<bool?> ForwardMoveCommand { get; set; }
        /// <summary>
        /// 正向移动
        /// </summary>
        private void ForwardMove(bool? moveModel)
        {
            if (moveModel == true)
            {
                GotoInSpeedMode(_enumMotorId, true);
            }
            else
            {
                Goto(_enumMotorId, true);
            }
        }

        public void PosModeMove(string direction)
        {

            switch (direction)
            {
                case "p":; Goto(_enumMotorId, true); break;
                case "n": Goto(_enumMotorId, false); break;
                case "s": StopMotor();
                    break;
            }
        }
        public void SpeedModeMove(string direction)
        {

            switch (direction)
            {
                case "p":; GotoInSpeedMode(_enumMotorId, true); break;
                case "n": GotoInSpeedMode(_enumMotorId, false); break;
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
        public async Task TestSmoothnessDetection(CancellationToken CancellationToken = default)
        {
            var startTime = DateTime.Now;
            for (int i = 0; i < 50; i++)
            {

                bool result1 = await SmoothnessGeneralMotorTestDetectionion(CancellationToken);
                var endTime = DateTime.Now;
                if (result1 == false)
                {
                    DispathcherInvoke($"电机限位测试失败，测试退出");
                    return;
                }
                var diffTime = endTime - startTime;
                if (diffTime.TotalSeconds / 60 > 20)
                {
                    break;
                }
            }
          
        }
        public async Task DurabilityTest(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    bool result1 = await SmoothnessGeneralMotorTestDetectionion(cancellationToken);
                    if (result1 == false)
                    {
                        DispathcherInvoke($"电机异常，请注意");
                    }
                    await Task.Delay(100, cancellationToken);
                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Error($"{Name}耐久测试异常:{ex}");
                    StopMotor();
                    break;
                }

            }
        }
        public DelegateCommand BaseTestCommand { get; set; }
        /*
        public async Task BaseTest(CancellationToken cancellationToken)
        {

            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MotorTestMessages.Clear();
            }));

            SetMotorInit();
            Goto(_enumMotorId, false);
            await Task.Delay(4000, cancellationToken);
            _testModel.MotorEntity.SetMotorOperatingStatusCommand(_enumMotorId, EnumMotorOperatingState.Stop);
            var result = await TestMotorMove(cancellationToken);
            if (result != true)
            {
                DispathcherInvoke($"电机移动测试失败，测试退出");
                return;
            }
            result = await EncoderDirectionTest(cancellationToken);
            if (result != true)
            { 
                DispathcherInvoke($"电机编码器测试失败，测试退出");
                return;
            }
          
            result = await LimitSwitchGeneralMotorTestDetectionion(cancellationToken);
            if (result != true)
            {
                DispathcherInvoke($"电机限位测试失败，测试退出");
                return;
            }
                       var point = await TotalJourneyGeneralMotorTestDetectionion(cancellationToken);
            Goto(_enumMotorId, point);
            await Task.Delay(5000, cancellationToken);
        }
        */

        /// <summary>
        /// 基础测试 (全自动化积木组装版 - 包含所有测试项)
        /// </summary>
        public async Task BaseTest(CancellationToken cancellationToken)
        {
            // 1. 清空界面上的旧测试记录
            await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                MotorTestMessages.Clear();
            }));

            DispathcherInvoke($"--- 开始执行 [{Name}] 全套基础测试 ---");

            // ==========================================
            // 积木 1：电机基础移动测试
            // ==========================================
            var moveTest = new MovementTestItem();
            var moveResult = await moveTest.ExecuteAsync(_enumMotorId, MotorModel, _testModel.MotorEntity, cancellationToken);
            UpdateTestResultToUI(moveTest.TestName, "到达指定位置", moveResult);
            if (!moveResult.IsPassed) { DispathcherInvoke($"[{Name}] 移动测试失败，终止。"); return; }

            // ==========================================
            // 积木 2：编码器方向及响应测试
            // ==========================================
            var encoderTest = new EncoderTestItem();
            var encoderResult = await encoderTest.ExecuteAsync(_enumMotorId, MotorModel, _testModel.MotorEntity, cancellationToken);
            UpdateTestResultToUI(encoderTest.TestName, "变化>=100", encoderResult);
            if (!encoderResult.IsPassed) { DispathcherInvoke($"[{Name}] 编码器测试失败，终止。"); return; }

            // ==========================================
            // 积木 3：满行程及限位测试 
            // ==========================================
            DispathcherInvoke($"[{Name}] 正在执行满行程及限位扫描...");
            // 传入当前轴的标准行程范围
            var fullTravelTest = new FullTravelTestItem(FullStrokeRange);
            var fullTravelResult = await fullTravelTest.ExecuteAsync(_enumMotorId, MotorModel, _testModel.MotorEntity, cancellationToken);
            UpdateTestResultToUI(fullTravelTest.TestName, $"[{FullStrokeRange.minValue}-{FullStrokeRange.maxValue}]", fullTravelResult);
            if (!fullTravelResult.IsPassed) { DispathcherInvoke($"[{Name}] 满行程测试失败，终止。"); return; }

            // ==========================================
            // 积木 4：定位精度(重复性)测试
            // ==========================================
            DispathcherInvoke($"[{Name}] 正在执行定位精度(重复性)测试...");
            var accuracyTest = new PositioningAccuracyTestItem();
            var accuracyResult = await accuracyTest.ExecuteAsync(_enumMotorId, MotorModel, _testModel.MotorEntity, cancellationToken);
            UpdateTestResultToUI(accuracyTest.TestName, "偏差<200", accuracyResult);
            if (!accuracyResult.IsPassed) { DispathcherInvoke($"[{Name}] 定位精度测试失败，终止。"); return; }

            // ==========================================
            // 积木 5：分段定位线性测试 (98点测试)
            // ==========================================
            DispathcherInvoke($"[{Name}] 正在执行 98 点线性测试...");
            // 利用底层跑完满行程后，自动记录在 MotorParams 里的真实物理限位作为测试区间
            int minPos = MotorModel.MotorParams.NegativeLimitPosition;
            int maxPos = MotorModel.MotorParams.PositiveLimitPosition;

            // 安全保护：如果限位记录异常，给个默认区间兜底
            if (maxPos <= minPos) { minPos = 0; maxPos = 100000; }

            var linearTest = new LinearStepPrecisionTestItem(minPos, maxPos);
            var linearResult = await linearTest.ExecuteAsync(_enumMotorId, MotorModel, _testModel.MotorEntity, cancellationToken);
            UpdateTestResultToUI(linearTest.TestName, "StdDev<150", linearResult);

            // ==========================================
            // 收尾：回到物理行程中点
            // ==========================================
            int midPoint = minPos + (maxPos - minPos) / 2;
            DispathcherInvoke($"[{Name}] 测试全部完毕，正在回到中点位置: {midPoint}");
            Goto(_enumMotorId, midPoint);
            await Task.Delay(5000, cancellationToken);

            DispathcherInvoke($"--- [{Name}] 所有基础测试已完美通过！ ---");
        }

        /// <summary>
        /// 【UI小助手】负责把底层的 MotorTestResult 安全地刷新到界面的列表中
        /// </summary>
        private void UpdateTestResultToUI(string testProject, string standardValue, MotorTestResult result)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                MotorTestMessages.Add(new MotorTestMessage
                {
                    TestProject = testProject,
                    StandardValue = standardValue,
                    TestResult = result.IsPassed ? "合格" : "不合格",
                    TestValue = result.MeasuredValue ?? "",
                    Description = result.IsPassed ? result.Description : result.ErrorDescription
                });
            }));
        }

        /// <summary>
        /// 电机移动测试优化版本
        /// </summary>
        /// <returns></returns>
        public async Task RunMoveTest(CancellationToken cancellationToken = default)
        {
            var moveTest = new MovementTestItem() { };

            var result = await moveTest.ExecuteAsync(_enumMotorId, MotorModel, _testModel.MotorEntity, cancellationToken);
            App.Current.Dispatcher.Invoke(() =>
            {
                MotorTestMessages.Add(new MotorTestMessage
                {
                    TestProject = moveTest.TestName,
                    TestResult = result.IsPassed ? "合格" : "不合格",
                    TestValue = result.MeasuredValue,
                    Description = result.ErrorDescription
                });
            });
        }
 
      

        /// <summary>
        /// 通用检测
        /// </summary>
        private async Task<List<KeyValuePair<string, int>>> GeneralMotorTestDetection(CancellationToken cancellationToken = default)
        {
            List<KeyValuePair<string, int>> posMessage = new List<KeyValuePair<string, int>>();
            string result;
            //单开线程去测试堵转和限位
            _limitedtcs1 = new TaskCompletionSource<string>();
            DistanceSettingForDifferentAxes(true);
            await Task.Delay(5000, cancellationToken);
            try
            {
                _limitedtcs1 = new TaskCompletionSource<string>();
                Task.Run(() => { MotorStallDetection(_enumMotorId); }, cancellationToken);
                result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(50), cancellationToken);
                posMessage.Add(new KeyValuePair<string, int>(result, MotorModel.MotorParams.Pos));
            }
            catch (Exception ex)
            {
                result = "Exception" + ex.Message;
                posMessage.Clear();
                posMessage.Add(new KeyValuePair<string, int>(result, MotorModel.MotorParams.Pos));
            }

            DistanceSettingForDifferentAxes(false);
            await Task.Delay(5000, cancellationToken);
            try
            {
                _limitedtcs1 = new TaskCompletionSource<string>();
                Task.Run(() => { MotorStallDetection(_enumMotorId); }, cancellationToken);
                result = await _limitedtcs1.Task.WaitAsync(TimeSpan.FromSeconds(50), cancellationToken);
                posMessage.Add(new KeyValuePair<string, int>(result, MotorModel.MotorParams.Pos));

            }
            catch (Exception ex)
            {
                if (posMessage.Count > 1)
                {
                    posMessage.RemoveAt(1);
                }
                posMessage.Add(new KeyValuePair<string, int>(result, MotorModel.MotorParams.Pos));
            }
            return posMessage;
        }

        /// <summary>
        /// 通用正常版本
        /// </summary>
        /// <param name="dircrtion"></param>
        /// <param name="result"></param>
        /// <param name="motorTestMessage"></param>
        private bool LimitPositioningAccuracyDetectionFunction(List<KeyValuePair<string, int>> keyValuePairs, MotorTestMessage motorTestMessage)
        {
            if (keyValuePairs[0].Key.Contains("PhyForwardLimited") || keyValuePairs[0].Key.Contains("SPLimted"))
            {
                if (keyValuePairs[1].Key.Contains("PhyBackwardLimited") || keyValuePairs[1].Key.Contains("SNLimted"))
                {
                    return true;
                }
                else
                {
                    GeneralMotorErrorDetection(keyValuePairs, motorTestMessage);
                    return false;
                }
            }
            GeneralMotorErrorDetection(keyValuePairs, motorTestMessage);
            return false;
        }
        /// <summary>
        /// 通用的错误检测
        /// </summary>
        /// <param name="keyValuePairs"></param>
        /// <param name="motorTestMessage"></param>
        private void GeneralMotorErrorDetection(List<KeyValuePair<string, int>> keyValuePairs, MotorTestMessage motorTestMessage)
        {
            motorTestMessage.TestResult = "不合格";
            motorTestMessage.StandardValue = "合格";
            if (keyValuePairs[0].Key.Contains("stall"))
            {
                motorTestMessage.Description = $"位置:{keyValuePairs[0].Value},堵转";
            }
            else if (keyValuePairs[0].Key.Contains("PhyBackwardLimited") || (keyValuePairs[0].Key.Contains("SNLimted")))
            {
                motorTestMessage.Description = $"位置:{keyValuePairs[0].Value},限位反向";
            }
            else if (keyValuePairs[0].Key.Contains("stop"))
            {
                motorTestMessage.Description = $"位置:{keyValuePairs[0].Value},到达指定位置，检查软限位是否正常";
            }
            else
            {
                motorTestMessage.Description = $"位置:{keyValuePairs[0].Value},触发异常:{keyValuePairs[0].Key}";
            }


            if (keyValuePairs[1].Key.Contains("stall"))
            {
                motorTestMessage.Description += $"\r\n位置:{keyValuePairs[1].Value},堵转";
            }
            else if (keyValuePairs[1].Key.Contains("SPLimted") || keyValuePairs[1].Key.Contains("PhyForwardLimited"))
            {
                motorTestMessage.Description += $"\r\n位置:{keyValuePairs[1].Value},限位反向";
            }
            else if (keyValuePairs[1].Key.Contains("stop"))
            {
                motorTestMessage.Description = $"位置:{keyValuePairs[1].Value},到达指定位置，检查软限位是否正常";
            }
            else
            {
                motorTestMessage.Description = $"位置:{keyValuePairs[1].Value},触发异常:{keyValuePairs[1].Key}";
            }

        }
        /// <summary>
        /// 满行程通用测试
        /// </summary>
        public async Task<int> TotalJourneyGeneralMotorTestDetectionion(CancellationToken cancellationToken = default)
        {
            MotorTestMessage motorTestMessage = new MotorTestMessage();
            motorTestMessage.TestProject = "满行程测试";
            motorTestMessage.StandardValue = "合格";
            var dir = await GeneralMotorTestDetection(cancellationToken);
            int fullStrokeOfMotor = dir[0].Value - dir[1].Value;
            motorTestMessage.TestValue = $"{fullStrokeOfMotor}";
            if (LimitPositioningAccuracyDetectionFunction(dir, motorTestMessage))
            {
                if (fullStrokeOfMotor < FullStrokeRange.maxValue && fullStrokeOfMotor > FullStrokeRange.minValue)
                {
                    motorTestMessage.TestResult = "合格";
                    using (var motorMessageDb = new MotorMessageDbContextBase())
                    {
                        var motorMessage = new MotorMessage() { Name = "null", MotorModelAxis  = MotorModel.MotorModelAxis, TotalDistance = fullStrokeOfMotor, LeftLimitPosition= dir[0].Value , RightLimitPosition = dir[1].Value };
                        await SpliteOperate.AddMotorMessageAsync(motorMessage, motorMessageDb);
                    }     
                }
                else
                {
                    motorTestMessage.TestResult = "不合格";
                } 
            }
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MotorTestMessages.Add(motorTestMessage);

            }));
            return dir[0].Value + fullStrokeOfMotor / 2;
        }
        /// <summary>
        /// 丝杆通用测试
        /// </summary>
        public async Task<bool> SmoothnessGeneralMotorTestDetectionion(CancellationToken cancellationToken = default)
        {
            MotorTestMessage motorTestMessage = new MotorTestMessage() 
            {
                TestProject = "丝杆测试",
                StandardValue = "合格"
            };
    
            var dir = await GeneralMotorTestDetection(cancellationToken);
            if (dir == null || dir.Count < 2)
            {
                motorTestMessage.TestResult = "数据获取失败";
                return false;
            }
            int fullStrokeOfMotor = dir[0].Value - dir[1].Value;
            motorTestMessage.TestValue = $"行程:{fullStrokeOfMotor}";
            using (var motorMessageDb = new MotorMessageDbContextBase())
            {
                var motorMessage = new MotorMessage() { Name = "null", MotorModelAxis = MotorModel.MotorModelAxis, TotalDistance = fullStrokeOfMotor, LeftLimitPosition = dir[0].Value, RightLimitPosition = dir[1].Value };
                await SpliteOperate.AddMotorMessageAsync(motorMessage, motorMessageDb);
            }
            if (LimitPositioningAccuracyDetectionFunction(dir, motorTestMessage))
            {
                if (fullStrokeOfMotor >= FullStrokeRange.maxValue || fullStrokeOfMotor <= FullStrokeRange.minValue)
                {
                    motorTestMessage.TestResult = "不合格";
                }
            }
            else
            {
                motorTestMessage.TestResult = "不合格";
            }
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MotorTestMessages.Add(motorTestMessage);

            }));
            return motorTestMessage.TestResult != "不合格";
        }
        /// <summary>
        /// 定位精度通用测试
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task PositioningAccuracyGeneralMotorTestDetectionion(CancellationToken cancellationToken = default)
        {
            MotorTestMessage motorTestMessage = new MotorTestMessage();
            motorTestMessage.TestProject = "定位精度测试";
            motorTestMessage.StandardValue = "合格";
            var dir = await GeneralMotorTestDetection(cancellationToken);
            var dir2 = await GeneralMotorTestDetection(cancellationToken);
            int fullStrokeOfMotor = dir[0].Value - dir[1].Value;
            int fullStrokeOfMotor2 = dir2[0].Value - dir2[1].Value;
            int leftDiff = Math.Abs(dir2[0].Value - dir[0].Value);
            int rightDiff = Math.Abs(dir2[1].Value - dir[1].Value);
            
            motorTestMessage.TestValue = $"左限位:{leftDiff} ,右限位:{rightDiff}";
            await SpliteOperate.MotorMessageSemaphore.WaitAsync();
            try
            {

                using (var motorMessageDb = new MotorMessageDbContextBase())
                {
                    var motorMessage = new MotorMessage() { Name = "null", MotorModelAxis = MotorModel.MotorModelAxis, TotalDistance = fullStrokeOfMotor, LeftLimitPosition = dir[0].Value, RightLimitPosition = dir[1].Value };
                    var motorMessage2 = new MotorMessage() { Name = "null", MotorModelAxis = MotorModel.MotorModelAxis, TotalDistance = fullStrokeOfMotor2, LeftLimitPosition = dir2[0].Value, RightLimitPosition = dir2[1].Value };
                    await SpliteOperate.AddMotorMessageAsync(motorMessage, motorMessageDb);
                    await SpliteOperate.AddMotorMessageAsync(motorMessage2, motorMessageDb);
                }



            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"MotorMessage 保存错误：{ex}");
            }
            finally 
            {
                SpliteOperate.MotorMessageSemaphore.Release();
            }
            if (LimitPositioningAccuracyDetectionFunction(dir, motorTestMessage))
            {
                if (leftDiff < 200 && rightDiff < 200)
                {
                    motorTestMessage.TestResult = "合格";
                }
                else
                {
                    motorTestMessage.TestResult = "不合格";
                }
            }
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MotorTestMessages.Add(motorTestMessage);

            }));

        }
        /// <summary>
        /// 限位开关通用测试
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> LimitSwitchGeneralMotorTestDetectionion(CancellationToken cancellationToken = default)
        {
            MotorTestMessage motorTestMessage = new MotorTestMessage();
            motorTestMessage.TestProject = "限位开关测试";
            motorTestMessage.StandardValue = "合格";
            var dir = await GeneralMotorTestDetection(cancellationToken);
            int fullStrokeOfMotor = dir[0].Value - dir[1].Value;
            motorTestMessage.TestValue = $"左限位:{dir[0].Value} ,右限位:{dir[1].Value}";
            await SpliteOperate.MotorMessageSemaphore.WaitAsync();
            try
            {
                using (var motorMessageDb = new MotorMessageDbContextBase())
                {
                    var motorMessage = new MotorMessage() { Name = "null", MotorModelAxis = MotorModel.MotorModelAxis, TotalDistance = fullStrokeOfMotor, LeftLimitPosition = dir[0].Value, RightLimitPosition = dir[1].Value };
                    await SpliteOperate.AddMotorMessageAsync(motorMessage, motorMessageDb);
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"MotorMessage 保存错误：{ex}");
            }
            finally
            {
                SpliteOperate.MotorMessageSemaphore.Release();
            }
            if (LimitPositioningAccuracyDetectionFunction(dir, motorTestMessage))
            {
                motorTestMessage.TestResult = "合格";
            }
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MotorTestMessages.Add(motorTestMessage);

            }));
            if (motorTestMessage.TestResult == "合格")
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        
      
        /// <summary>
        /// 电机堵转和限位检测
        /// </summary>
        private async Task MotorStallDetection(EnumMotorId _enumMotorId, CancellationToken cancellationToken = default)
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
                if (cancellationToken.IsCancellationRequested)
                {
                    DispathcherInvoke("收到外部关闭信号，退出堵转检测");
                    return;
                }

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
                await Task.Delay( 1000 , cancellationToken);
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
      
        public DelegateCommand StopMotorCommand { get; set; }
        public void StopMotor()
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
                    GotoInSpeedMode(_enumMotorId, true);
                }
                else
                {
                    GotoInSpeedMode(_enumMotorId, false);
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
        private void GotoInSpeedMode(EnumMotorId _enumMotorId, bool direction)
        {

            SetMotorSpeedInit();
            if (direction)
            {
                _testModel.MotorEntity.SetMotorGoToCommand(_enumMotorId, EnumMotorUnit.Pulse, _testModel.MagnitudeOfSpeed);

            }
            else
            {
                _testModel.MotorEntity.SetMotorGoToCommand(_enumMotorId, EnumMotorUnit.Pulse, -(_testModel.MagnitudeOfSpeed));
            }
           
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
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Log.Add(log);
            }));

        }
        private  void AddPoint( )
        {
             
            if (MotorModel.PointList.Count >= 2)
            {
                var data = DateTime.Now;
                var timeDifference = (data - MotorModel.PointList[MotorModel.PointList.Count - 2].Date).TotalMilliseconds;
                var speed = (MotorModel.MotorParams.Pos - MotorModel.PointList[MotorModel.PointList.Count - 2].Point) / (timeDifference * 1.0) * 1000;
                PlotViewPointMessage pointView = new PlotViewPointMessage() { Date = data, Point = MotorModel.MotorParams.Pos, MotorMoveState = MotorModel.MotorParams.MoveState , MotorModelAxis  = MotorModel.MotorModelAxis};
                PlotViewSpeedMessage speedView = new PlotViewSpeedMessage() { SpeedDate = MotorModel.PointList[MotorModel.PointList.Count - 1].Date, Speed = speed , MotorModelAxis = MotorModel.MotorModelAxis };

                // 【终极防弹衣】：在把数据塞进集合的那一瞬间，锁死图表！
                // 这句话的意思是：我在塞数据的时候，OxyPlot 你给我闭嘴不许画图；
                // 等我塞完了，你再画！
                lock (_testModel.MotorplotModel.SyncRoot)
                {
                    lock (_testModel.MotorSpeedplotModel.SyncRoot)
                    {

                        if (_testModel.MotorSpeedplotModel != null && _testModel.MotorplotModel != null)
                        {
                            while (MotorModel.PointList.Count >= _testModel.MaxCount)
                            {
                                MotorModel.PointList.RemoveAt(0);
                            }
                            while (MotorModel.SpeedList.Count >= _testModel.MaxCount)
                            {
                                MotorModel.SpeedList.RemoveAt(0);
                            }

                            MotorModel.PointList.Add(pointView);
                            MotorModel.SpeedList.Add(speedView);
                            MotorModel.PointListBuffer.Add(pointView);
                            MotorModel.SpeedListBuffer.Add(speedView);
                            MotorModel.MotorParams.Speed = speed;

                            //                 _testModel.MotorplotModel.InvalidatePlot(true);
                            //                 _testModel.MotorSpeedplotModel.InvalidatePlot(true);



                        }
                    }
                }
                if (MotorModel.PointListBuffer.Count == _testModel.SavePointNumber)
                {
                    SqliteSaveDate();
                }
            }
            else
            {
                var data = DateTime.Now;
                PlotViewPointMessage pointView = new PlotViewPointMessage() { Date = data, Point = MotorModel.MotorParams.Pos, MotorMoveState = MotorModel.MotorParams.MoveState , MotorModelAxis = MotorModel.MotorModelAxis };
                MotorModel.PointList.Add(pointView);
                
                if (_testModel.MotorplotModel != null)
                {
 //                   _testModel.MotorplotModel.InvalidatePlot(true);
                }
                
            }
        }
        private async void SqliteSaveDate()
        {
            var speedBuffer = MotorModel.SpeedListBuffer.ToList();
            var pointBuffer = MotorModel.PointListBuffer.ToList();
            var speedCoint = speedBuffer.Count;
            var pointCoint = pointBuffer.Count;
            if (_testModel.MotorTypeModel.EnumMotorAxisType == EnumMotorAxisType.TwoAxisMotor)
            {
                using (var dbContext = new TwoAxisDbContextBase())
                {
                    await SpliteOperate.AddPlotViewSpeedListMessagesSimpleAsync(speedBuffer, dbContext);
                    await SpliteOperate.AddPlotViewPointListMessagesSimpleAsync(pointBuffer, dbContext);
                    _testModel.TotalSize = await SpliteOperate.GetPlotViewPointMessageCountAsync(dbContext);
                }
            }
            else
            {
                using (var dbContext = new FiveAxisDbContextBase())
                {
                    await SpliteOperate.AddPlotViewSpeedListMessagesSimpleAsync(speedBuffer, dbContext);
                    await SpliteOperate.AddPlotViewPointListMessagesSimpleAsync(pointBuffer, dbContext);
                    _testModel.TotalSize = await SpliteOperate.GetPlotViewPointMessageCountAsync(dbContext);
                }
                   
              
            }
            if (MotorModel.SpeedListBuffer != null && MotorModel.SpeedListBuffer.Count > 0)
            {
                if (MotorModel.SpeedListBuffer.Count >= speedCoint)
                {
                    MotorModel.SpeedListBuffer.RemoveRange(0, speedCoint);
                }
                else
                {
                    MotorModel.SpeedListBuffer.RemoveRange(0, MotorModel.SpeedListBuffer.Count);
                }
               
            }
            if (MotorModel.PointListBuffer != null && MotorModel.PointListBuffer.Count > 0)
            {
                if (MotorModel.PointListBuffer.Count >= pointCoint)
                {
                    MotorModel.PointListBuffer.RemoveRange(0, pointCoint);
                }
                else
                {
                    MotorModel.PointListBuffer.RemoveRange(0, MotorModel.PointListBuffer.Count);
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
                MotorModel.PointList = new ObservableCollection<PlotViewPointMessage>(JsonConvert.DeserializeObject<List<PlotViewPointMessage>>(File.ReadAllText(PointjsonfilePath)));  
            }
            if (File.Exists(SpeedjsonfilePath))
            {
                MotorModel.SpeedList = new ObservableCollection<PlotViewSpeedMessage>(JsonConvert.DeserializeObject<List<PlotViewSpeedMessage>>(File.ReadAllText(SpeedjsonfilePath))); 
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
        public void ConfirmTheStandardStroke()
        {
            if (_testModel.MotorTypeModel== null)
            {
                if (MotorModel.MotorModelAxis == EnumMotorModel.MOTOR_x)
                {
                    FullStrokeRange = (120000, 130000);
                }
                else
                {
                    FullStrokeRange = (160000, 170000);
                }
                return;
            }
            if (_testModel.MotorTypeModel.EnumMotorAxisType == Protocol.EnumMotorAxisType.TwoAxisMotor)
            {
                if (_testModel.MotorTypeModel.IsZem18)
                {
                    if (MotorModel.MotorModelAxis == EnumMotorModel.MOTOR_x)
                    {
                        FullStrokeRange = (120000, 130000);
                    }
                    else
                    {
                        FullStrokeRange = (160000, 170000);
                    }
                }
                else
                {
                    if (MotorModel.MotorModelAxis == EnumMotorModel.MOTOR_x)
                    {
                        FullStrokeRange = (235000, 255000);
                    }
                    else
                    {
                        FullStrokeRange = (215000, 225000);
                    }
                }
                
            }
            else
            {
                switch (MotorModel.MotorModelAxis) 
                {
                    case EnumMotorModel.MOTOR_x: FullStrokeRange = (0, 1000000); break;
                    case EnumMotorModel.MOTOR_y: FullStrokeRange = (0, 1000000); break;
                    case EnumMotorModel.MOTOR_z: FullStrokeRange = (0, 1000000); break;
                    case EnumMotorModel.MOTOR_t: FullStrokeRange = (0, 1000000); break;
                    case EnumMotorModel.MOTOR_r: FullStrokeRange = (0, 1000000); break;
                }
            }
        }
        /// <summary>
        /// 【临时测试通道】运行新架构的 98 点线性测试
        /// </summary>
        public async Task RunNewLinearTestAsync(CancellationToken cancellationToken = default)
        {
          

            // 2. 实例化我们的新测试“积木”
            var newTest = new EncoderTestItem();

            // 3. 执行测试！注意这里的参数全是你 FiveAxisModel 里现成的
            var result = await newTest.ExecuteAsync(_enumMotorId, MotorModel, _testModel.MotorEntity, cancellationToken);

            // 4. 将结果推送到你的 UI 上
            await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                MotorTestMessages.Add(new MotorTestMessage
                {
                    TestProject = newTest.TestName,
                    StandardValue = "标准差<150",
                    TestResult = result.IsPassed ? "合格" : "不合格",
                    TestValue = result.MeasuredValue, // 比如 "StdDev: 45.2"
                    Description = result.IsPassed ? result.Description : result.ErrorDescription
                });

                Log.Add($"新架构测试结束 -> 结果: {(result.IsPassed ? "成功" : "失败")} | {result.MeasuredValue}");
            });
        }
    }
   
    
}
