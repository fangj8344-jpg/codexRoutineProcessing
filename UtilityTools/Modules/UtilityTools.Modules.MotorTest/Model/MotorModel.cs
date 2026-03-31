using CsvHelper.Configuration.Attributes;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Media3D;
using UtilityTools.Modules.MotorTest.Protocol;
using UtilityTools.Modules.MotorTest.SQLite;

namespace UtilityTools.Modules.MotorTest.Model
{
    public class MotorModel:BindableBase
    {
        public MotorModel(EnumMotorModel enumMotorModel)
        {
            MotorModelAxis = enumMotorModel;
            PointListBuffer = new List<PlotViewPointMessage>();
            SpeedListBuffer = new List<PlotViewSpeedMessage>();
            PointList = new ObservableCollection<PlotViewPointMessage>();
            SpeedList = new ObservableCollection<PlotViewSpeedMessage>();
            MotorParams = new MotorParams();

            BindingOperations.EnableCollectionSynchronization(PointList, _pointListLock);
            BindingOperations.EnableCollectionSynchronization(SpeedList, _speedListLock);
        }
        // 专门给跨线程更新准备的锁对象
        private readonly object _pointListLock = new object();
        private readonly object _speedListLock = new object();
        private MotorParams _motorParams;
        public MotorParams MotorParams 
        {
            get { return _motorParams; }
            
            set 
            { 
                _motorParams = value;
               RaisePropertyChanged();
            }
             
        }
       
        private List<PlotViewPointMessage> _pointListBuffer;
        /// <summary>
        /// 保存到数据库的位置缓存区
        /// </summary>
        public List<PlotViewPointMessage> PointListBuffer
        {
            get { return _pointListBuffer; }
            set { _pointListBuffer = value; }
        }
        private List<PlotViewSpeedMessage> _speedListBuffer;
        /// <summary>
        /// 保存到数据库的速度缓存区
        /// </summary>
        public List<PlotViewSpeedMessage> SpeedListBuffer
        {
            get { return _speedListBuffer; }
            set { _speedListBuffer = value; }
        }
        private ObservableCollection<PlotViewPointMessage> _pointList;
        /// <summary>
        /// 位置
        /// </summary>
        public ObservableCollection<PlotViewPointMessage> PointList
        {
            get { return _pointList; }
            set { _pointList = value; RaisePropertyChanged(); }
        }
        private ObservableCollection<PlotViewSpeedMessage> _speedList;
        /// <summary>
        /// 速度
        /// </summary>
        public ObservableCollection<PlotViewSpeedMessage> SpeedList
        {
            get { return _speedList; }
            set { _speedList = value; RaisePropertyChanged(); }
        }
        private EnumMotorModel _motorModelAxis;
        public EnumMotorModel MotorModelAxis
        {
            get { return _motorModelAxis; }
            set { _motorModelAxis = value; }
        }
    } 
    public class MotorParams : BindableBase
    {
        private bool _enable;
        /// <summary>
        /// 电机使能
        /// </summary>
        public bool Enable
        {
            get { return _enable; }
            set { _enable = value; RaisePropertyChanged(); }
        }

        private bool _sPLimted;
        /// <summary>
        /// 软件正限位
        /// </summary>
        public bool SPLimted
        {
            get { return _sPLimted; }
            set { _sPLimted = value; RaisePropertyChanged(); }
        }
        private bool _sNLimted;
        /// <summary>
        /// 软件负限位
        /// </summary>
        public bool SNLimted
        {
            get { return _sNLimted; }
            set { _sNLimted = value;RaisePropertyChanged(); }  
        }
        private EnumMotorCtrType _ctrType = EnumMotorCtrType.CloseLoopPosCtr;
        /// <summary>
        /// 控制类型
        /// </summary>
        public EnumMotorCtrType CtrType
        {
            get { return _ctrType; }
            set { _ctrType = value; RaisePropertyChanged(); }
        }

        private EnumMotorType _motorType;
        public EnumMotorType MotorType
        {
            get { return _motorType; }
            set { _motorType = value; RaisePropertyChanged(); }
        }
        private EnumMotorUnit _unit;
        public EnumMotorUnit Unit
        {
            get { return _unit; }
            set { _unit = value; RaisePropertyChanged(); }
        }


        private PIDModel _pid = new PIDModel();
        /// <summary>
        /// PID参数
        /// </summary>
        public PIDModel Pid
        {
            get { return _pid; }
            set { _pid = value; RaisePropertyChanged(); }
        }
        private Int32 _pos;
        /// <summary>
        /// 位置
        /// </summary>
        public Int32 Pos
        {
            get { return _pos; }
            set { _pos = value; RaisePropertyChanged(); }
        }
        private double _speed;
        /// <summary>
        /// 速度
        /// </summary>
        public double Speed
        {
            get { return _speed; }
            set { _speed = value; RaisePropertyChanged(); }
        }
        private float _subRatio;
        /// <summary>
        /// 物理与脉冲的换算系数
        /// </summary>
        public float SubRatio
        {
            get { return _subRatio; }
            set { _subRatio = value; RaisePropertyChanged(); }
        }

        private int _times;
        /// <summary>
        /// 控制补偿次数
        /// </summary>
        public int Times
        {
            get { return _times; }
            set { _times = value; RaisePropertyChanged(); }
        }

        private int _threshold;
        /// <summary>
        /// 停止阈值
        /// </summary>
        public int Threshold
        {
            get { return _threshold; }
            set { _threshold = value; RaisePropertyChanged(); }
        }

        private int _inchThreshold;
        /// <summary>
        /// 点动阈值
        /// </summary>
        public int InchThreshold
        {
            get { return _inchThreshold; }
            set { _inchThreshold = value; RaisePropertyChanged(); }
        }


        private int _startSpeed;
        /// <summary>
        /// 起始速度
        /// </summary>
        public int StartSpeed
        {
            get { return _startSpeed; }
            set { _startSpeed = value; RaisePropertyChanged(); }
        }

        private int _maxSpeed;
        /// <summary>
        /// 最大速度
        /// </summary>
        public int MaxSpeed
        {
            get { return _maxSpeed; }
            set { _maxSpeed = value; RaisePropertyChanged(); }
        }

        private int _maxAcc;
        /// <summary>
        /// 最大加速度
        /// </summary>
        public int MaxAcc
        {
            get { return _maxAcc; }
            set { _maxAcc = value; RaisePropertyChanged(); }
        }

        private int _maxDec;
        /// <summary>
        /// 最大减速度
        /// </summary>
        public int MaxDec
        {
            get { return _maxDec; }
            set { _maxDec = value; RaisePropertyChanged(); }
        }

        private int _targetValue;
        /// <summary>
        /// 目标值
        /// </summary>
        public int TargetValue
        {
            get { return _targetValue; }
            set { _targetValue = value; RaisePropertyChanged(); }
        }

        private int _originPos;
        /// <summary>
        /// 原点位置
        /// </summary>
        public int OriginPos
        {
            get { return _originPos; }
            set { _originPos = value; RaisePropertyChanged(); }
        }

        private int _zeroPos;
        /// <summary>
        /// 零点位置
        /// </summary>
        public int ZeroPos
        {
            get { return _zeroPos; }
            set { _zeroPos = value; RaisePropertyChanged(); }
        }

        private EnumMotorMoveType _moveType;
        /// <summary>
        /// 电机轴的类型
        /// </summary>
        public EnumMotorMoveType MoveType
        {
            get { return _moveType; }
            set { _moveType = value; RaisePropertyChanged(); }
        }
        private EnumMotorModel _motorModelId;
        /// <summary>
        /// 电机的类型x，y，z，t，r
        /// </summary>
        public EnumMotorModel MotorModelID
        {
            get { return _motorModelId; }
            set { _motorModelId = value; RaisePropertyChanged(); }
        }


        private EnumMotorMoveState _moveState = EnumMotorMoveState.MotorStop;
        /// <summary>
        /// 电机移动状态
        /// </summary>
        public EnumMotorMoveState MoveState
        {
            get { return _moveState; }
            set { _moveState = value; RaisePropertyChanged(); }
        }

        private EnumMotorLimitedState _limitedState = EnumMotorLimitedState.None;
        /// <summary>
        /// 电机限位状态
        /// </summary>
        public EnumMotorLimitedState LimitedState
        {
            get { return _limitedState; }
            set { _limitedState = value; RaisePropertyChanged(); }
        }

        private EmumMotorMoveDirection _moveDirection = EmumMotorMoveDirection.Forward;
        /// <summary>
        /// 电机移动方向
        /// </summary>
        public EmumMotorMoveDirection MoveDirection
        {
            get { return _moveDirection; }
            set { _moveDirection = value; RaisePropertyChanged(); }
        }

        private EnumEncoderDirecton _encoderDirection = EnumEncoderDirecton.EncoderForward;
        /// <summary>
        /// 电机编码器方向
        /// </summary>
        public EnumEncoderDirecton EncoderDirection
        {
            get { return _encoderDirection; }
            set { _encoderDirection = value; RaisePropertyChanged(); }
        }
        private int _positiveLimitPosition;
        /// <summary>
        /// 正限位位置 单位脉冲
        /// </summary>
        public int PositiveLimitPosition
        {
            get { return _positiveLimitPosition; }
            set { _positiveLimitPosition = value;RaisePropertyChanged(); }
        }

        private int _negativeLimitPosition;
        /// <summary>
        /// 负限位位置，单位脉冲
        /// </summary>
        public int NegativeLimitPosition
        {
            get { return _negativeLimitPosition; }
            set { _negativeLimitPosition = value; RaisePropertyChanged(); }
        }
        private int _timer = 0;
       
    }
    public class MotorTypeMessage:BindableBase
    {
        private string _name;
        /// <summary>
        /// 电机名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(); }
        }

        private int _minStrokeRange;
        /// <summary>
        /// 最小行程
        /// </summary>
        public int MinStrokeRange
        {
            get { return _minStrokeRange; }
            set { _minStrokeRange = value; RaisePropertyChanged(); }
        }

        private int _maxStrokeRange;
        /// <summary>
        /// 最大行程范围
        /// </summary>
        public int MaxStrokeRange
        {
            get { return _maxStrokeRange; }
            set { _maxStrokeRange = value; RaisePropertyChanged(); }
        }
       

    }
    public class PlotViewPointMessage 
    {

        public int Id { get; set; }
        /// <summary>
        /// 电机轴的类型
        /// </summary>
        public EnumMotorModel MotorModelAxis { get; set; }
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Date { get; set; }
       
        /// <summary>
        /// 位置
        /// </summary>
        public int Point { get; set; }
        /// <summary>
        /// 电机此时的状态
        /// </summary>
        public EnumMotorMoveState MotorMoveState { get; set; }

        /// <summary>
        /// 硬件时间
        /// </summary>
        public uint HardwareTime { get; set; }

    }
    public class PlotViewSpeedMessage
    {

        public int Id { get; set; }

        public EnumMotorModel MotorModelAxis { get; set; }
        /// <summary>
        /// 速度的时间
        /// </summary>
        public DateTime SpeedDate { get; set; }
      
        /// <summary>
        /// 速度
        /// </summary>
        public double Speed { get; set; }
      
    }
    public class MotorMessage
    {
        public int Id { get; set; }
        /// <summary>
        /// 电机名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 电机轴
        /// </summary>
        public EnumMotorModel MotorModelAxis { get; set; }
        /// <summary>
        /// 总行程
        /// </summary>
        public int TotalDistance { get; set; }
        /// <summary>
        /// 左限位位置
        /// </summary>
        public int LeftLimitPosition { get; set; }
        /// <summary>
        /// 右限位位置
        /// </summary>
        public int RightLimitPosition { get; set; }
    }
    public class MotorPlotMessage
    {
        [Key]
        public int Id { get; set; }
        public PlotViewSpeedMessage PlotViewSpeedMessage { get; set; }
        public PlotViewPointMessage PlotViewPointMessage { get; set; }


    }

}
