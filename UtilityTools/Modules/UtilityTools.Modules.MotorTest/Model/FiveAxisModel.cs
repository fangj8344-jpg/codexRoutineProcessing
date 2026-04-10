using MathNet.Numerics.RootFinding;
using Newtonsoft.Json;
using OpenCvSharp.Aruco;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using OxyPlot.Wpf;
using Prism.Commands;
using Prism.Events;
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
using UtilityTools.Core.Interface;
using UtilityTools.Modules.MotorTest.Entity;
using UtilityTools.Modules.MotorTest.Event;
using UtilityTools.Modules.MotorTest.Interface;
using UtilityTools.Modules.MotorTest.Protocol;
using UtilityTools.Modules.MotorTest.Runners;
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
            _eventAggregator = _containerProvider.Resolve<IEventAggregator>();
            testReportService = _containerProvider.Resolve<ITestReportService>();
            _enumMotorId = Id;
            _testModel = testModel;
            _name = name;
            MotorModel = new MotorModel(enumMotorModel);
            MotorModel.MotorParams.MotorModelID = enumMotorModel;
            SubscribeToEvents();
            Init();
        }

      
        #endregion

        ThreeAxisTestModel _testModel;
        private readonly IEventAggregator _eventAggregator;
        private ITestReportService testReportService;
        private static readonly Stopwatch _globalTimer = Stopwatch.StartNew();

        private IContainerProvider _containerProvider;
        private readonly IDialogHostService _dialogHostService;
        private readonly object _lockobj = new object();
        private bool _isPerformance;
        private DateTime? _baseSystemTime = null;
        private uint _baseHardwareTimestamp = 0;
       // private Queue<double> _speedFilter = new Queue<double>();

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


        #region ------------Property------------
     
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
            SpeedLine = new LineSeries()
            {
                Title = _name,
                RenderInLegend = true,
                StrokeThickness = 1,
                LineStyle = LineStyle.Solid,
                CanTrackerInterpolatePoints = false,
                MarkerType = MarkerType.None,
                MarkerSize = 0,
         
                MinimumSegmentLength = 2,
                // 直连数据，拒绝反射！
                Mapping = item =>
                {
                    var msg = (PlotViewSpeedMessage)item;
                    return new DataPoint(DateTimeAxis.ToDouble(msg.SpeedDate), msg.SpeedUm);
                }
            };
            PosLine = new LineSeries()
            {
                Title = _name,
                RenderInLegend = true,
                StrokeThickness = 1,
                LineStyle = LineStyle.Solid,
                CanTrackerInterpolatePoints = false,
                MarkerType = MarkerType.None,
                MarkerSize = 0,
                // 
                MinimumSegmentLength = 2,
                // 直连数据，拒绝反射！
                Mapping = item =>
                {
                    var msg = (PlotViewPointMessage)item;
                    return new DataPoint(DateTimeAxis.ToDouble(msg.Date), msg.PointUm);
                }
            };


        }
        
        public void ClearLog()
        {
            if (Log != null)
            {
                Log.Clear();
            }
        }
        private void SubscribeToEvents()
        {

            // 订阅文本日志
            _eventAggregator.GetEvent<MotorLogEvent>().Subscribe(log =>
            {
                // 🚨 过滤逻辑：按 '|' 拆分，如果前半部分是我的名字，我才收下
                var parts = log.Split(new[] { '|' }, 2);
                if (parts.Length == 2 && parts[0] == Name)
                {
                    Log.Add(parts[1]); // 只存入真正的内容部分
                }
            }, ThreadOption.UIThread);

            // 订阅测试成绩单
            _eventAggregator.GetEvent<MotorTestResultEvent>().Subscribe(msg =>
            {
                // 🚨 过滤逻辑：如果成绩单上的名字是我的，我才签收
                if (msg.AxisName == Name)
                {
                    var existing = MotorTestMessages.FirstOrDefault(x => x.TestProject == msg.TestProject);
                    if (existing == null)
                    {
                        MotorTestMessages.Add(msg);
                    }
                    else
                    {
                        existing.AxisName = msg.AxisName;
                        existing.ProgressState = msg.ProgressState;
                        existing.TestResult = msg.TestResult;
                        existing.TestValue = msg.TestValue;
                        existing.StandardValue = msg.StandardValue;
                        existing.Description = msg.Description;
                    }
                }
            }, ThreadOption.UIThread);
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
            Task.Run(async () =>
            {
                while (_isPerformance)
                {
                    if (_byteQueue.Count == 0)
                    {
                        _testModel.MotorEntity.GetMotorStatusCommand(_enumMotorId);
                    }
                    await Task.Delay(10);

                }
            });

        }
        public void NoPerformance()
        {
            _isPerformance = false; ;

        }

        public async Task DurabilityTest(CancellationToken cancellationToken)
        {
            var runner = new MotorWorkflowRunner(testReportService,_eventAggregator, _testModel.MotorEntity, _enumMotorId, MotorModel, FullStrokeRange, Name);
            await runner.RunDurabilityTestAsync(cancellationToken);
        }
       
       
        /// <summary>
        /// 基础测试 (全自动化积木组装版)
        /// </summary>
        public async Task BaseTest(CancellationToken cancellationToken)
        {
            MotorTestMessages.Clear(); // 清理旧成绩单
            var runner = new MotorWorkflowRunner(testReportService,_eventAggregator, _testModel.MotorEntity, _enumMotorId, MotorModel, FullStrokeRange, Name);
            await runner.RunBaseTestAsync(cancellationToken);
        }
        // --- 下面是新补齐的“招数” ---

        /// <summary>
        /// 丝杆顺滑度测试（30分钟）
        /// </summary>
        public async Task SmoothnessTest(CancellationToken ct)
        {
            var runner = new MotorWorkflowRunner(testReportService,_eventAggregator, _testModel.MotorEntity, _enumMotorId, MotorModel, FullStrokeRange, Name);
            // 这里调用 Runner 里对应的图纸（如果你还没写，咱们下一步在 Runner 里补）
            await runner.RunLeadScrewTestAsync(ct);
        }

   

        /// <summary>
     
        /// <summary>
        /// 取消软限位
        /// </summary>

        public void CloseSlimited()
        {
            byte limitEnable = 0b00011100;
            _testModel.MotorEntity.SetMotorLimitEnableCommand(_enumMotorId, limitEnable);
        }

   
        public async void AutomaticZeroInitialization()
        {
            // 1.调度器
            var runner = new MotorWorkflowRunner(testReportService,_eventAggregator, _testModel.MotorEntity, _enumMotorId, MotorModel, FullStrokeRange, Name);

            // 2. 执行图纸 D：自动回零
            // (注意：这里如果你的按钮不带取消功能，可以传 CancellationToken.None)
            await runner.RunAutomaticZeroInitializationAsync(CancellationToken.None);
        }
      
      
        public DelegateCommand StopMotorCommand { get; set; }
        public void StopMotor()
        {
            _testModel.MotorEntity.SetMotorOperatingStatusCommand(_enumMotorId, EnumMotorOperatingState.Stop);
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
                        ParserMotorStatus(data,e);

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

        private void ParserMotorStatus( byte[] data, SelfMotorPacket e)
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
            AddPoint(e);
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
        // 🚨 请在类开头声明一个私有队列，用来存最近的 3 个原始点
        private Queue<PlotViewPointMessage> _calcQueue = new Queue<PlotViewPointMessage>();

        private void AddPoint(SelfMotorPacket e)
        {
            // 1. 解析当前包的原始数据
            DateTime nowTime = DateTime.Now;
            var data = e.DataSource;
            if (data == null || data.Length < 16) return;
            int currentPos = BitConverter.ToInt32(data, 12);

            // 2. 封装成临时的点对象（此时速度还不知道，设为 0）
            var currentPoint = new PlotViewPointMessage()
            {
                Date = nowTime,
                Point = currentPos,
                PointUm = MotorModel.MotorParams.PosUm,

                MotorMoveState = MotorModel.MotorParams.MoveState,
                MotorModelAxis = MotorModel.MotorModelAxis
            };

            // 3. 塞进计算队列
            _calcQueue.Enqueue(currentPoint);

            // 🚨 核心：只有当队列攒够 3 个点时，才计算中间那个点（第 2 包）的速度
            if (_calcQueue.Count == 3)
            {
                // 拿出这三个点
                var p1 = _calcQueue.ElementAt(0); // 第一包
                var p2 = _calcQueue.ElementAt(1); // 第二包 (我们要补齐速度的对象)
                var p3 = _calcQueue.ElementAt(2); // 第三包

                // 4. ：用 1 包和 3 包算 2 包的速度
                double deltaMs = (p3.Date - p1.Date).TotalMilliseconds;
                double speedForP2 = 0;

                if (deltaMs > 0)
                {
                    // 速度 = (第三包位置 - 第一包位置) / (第三包时间 - 第一包时间)
                    speedForP2 = (p3.Point - p1.Point) / (deltaMs / 1000.0);
                }

                // 更新界面显示的瞬时速度
                MotorModel.MotorParams.Speed = speedForP2;

                // 5. 封装最终的 SpeedView (关联的是第二包的时间)
                var speedView = new PlotViewSpeedMessage()
                {
                    SpeedDate = p2.Date, // 对应中间那一包的时间
                    Speed = speedForP2,
                    SpeedUm = MotorModel.MotorParams.SpeedUm,
                    MotorModelAxis = MotorModel.MotorModelAxis
                };

                // 6. 【UI 安全层】：更新界面
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    while (MotorModel.PointList.Count >= _testModel.MaxCount)
                        MotorModel.PointList.RemoveAt(0);
                    while (MotorModel.SpeedList.Count >= _testModel.MaxCount)
                        MotorModel.SpeedList.RemoveAt(0);

                    // 注意：这里 Add 的是 p2，因为 p2 现在的速度才算出来
                    MotorModel.PointList.Add(p2);
                    MotorModel.SpeedList.Add(speedView);
                }));

                // 7. 【数据缓冲层】：加锁存库
                lock (_lockobj)
                {
                    MotorModel.PointListBuffer.Add(p2);
                    MotorModel.SpeedListBuffer.Add(speedView);
                }

                // 8. 弹出最老的一包，为下一轮计算做准备
                _calcQueue.Dequeue();

                // 9. 存库检查
                if (MotorModel.PointListBuffer.Count >= _testModel.SavePointNumber)
                {
                    SqliteSaveDate();
                }
            }
        }

 
        private async void SqliteSaveDate()
        {
            List<PlotViewSpeedMessage> speedSnapshot;
            List<PlotViewPointMessage> pointSnapshot;

            // 🚨 核心逻辑：在锁里面，一次性把“快照”拿出来，并立刻把“母体”清空！
            // 这样，不管存库花多久，母体怎么加新数据，都跟这次存库的数据没关系了。
            lock (_lockobj)
            {
                if (MotorModel.SpeedListBuffer.Count == 0 && MotorModel.PointListBuffer.Count == 0) return;

                // 1. 拍照存副本
                speedSnapshot = MotorModel.SpeedListBuffer.ToList();
                pointSnapshot = MotorModel.PointListBuffer.ToList();

                // 2. 🚨 立刻清空母体！不要用 RemoveRange，直接 Clear！
                // 这样就绝对不存在“数量对不上”的问题了
                MotorModel.SpeedListBuffer.Clear();
                MotorModel.PointListBuffer.Clear();
            }

            // 3. 此时锁已经释放了，UDP 线程可以继续往空的 Buffer 里塞新数据
            // 我们拿着刚才拿出来的 snapshot 慢慢存库，互不干扰
            string dbFileName = _testModel.MotorTypeModel.CurrentDbName;

            try
            {
                using (var dbContext = new MotorDbContext(dbFileName))
                {
                    // 存的是刚才锁内抓取的 snapshot
                    await SpliteOperate.AddPlotViewSpeedListMessagesSimpleAsync(speedSnapshot, dbContext);
                    await SpliteOperate.AddPlotViewPointListMessagesSimpleAsync(pointSnapshot, dbContext);

                    _testModel.TotalSize = await SpliteOperate.GetPlotViewPointMessageCountAsync(dbContext);
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex, "后台存库失败");
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
            if (_testModel.MotorSpeedplotModel != null && _testModel.MotorplotModel != null)
            {
                _testModel.MotorSpeedplotModel.InvalidatePlot(true);
                _testModel.MotorplotModel.InvalidatePlot(true);
            }

     

        }
        public void ConfirmTheStandardStroke()
        {
            // 彻底清空！什么都不用写了！
            // 为什么？因为 FullStrokeRange 已经由外面的大管家 (MotorTypeModel)
            // 在创建这个轴的时候，查字典并精准塞进来了。

            // 如果你后期需要根据量程在图表 (MotorplotModel) 上画红线、绿线，
            // 直接在这里拿 FullStrokeRange.minValue 和 .maxValue 画就行了，绝不需要做任何 if 判断。
        }
    }    
}
