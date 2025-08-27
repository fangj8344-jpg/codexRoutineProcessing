using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Model;
using UtilityTools.Modules.NewMotor5Controller.Entity;
using UtilityTools.Modules.NewMotor5Controller.Protocol;

namespace UtilityTools.Modules.NewMotor5Controller.Model
{
   

    /// <summary>
    /// 电机基础配置参数
    /// </summary>
    public class MotorBaseParam : BindableBase
    {
        private EnumMotorType _type = EnumMotorType.StepperMotor;
        /// <summary>
        /// 电机类型
        /// </summary>
        public EnumMotorType Type
        {
            get { return _type; }
            set { _type = value; RaisePropertyChanged(); }
        }

        private EnumMotorAxisType _axisType = EnumMotorAxisType.Displacement;
        /// <summary>
        /// 电机轴类型
        /// </summary>
        public EnumMotorAxisType AxisType
        {
            get { return _axisType; }
            set { _axisType = value; RaisePropertyChanged(); }
        }

        private EnumMotorControlMode _controlMode = EnumMotorControlMode.CloseLoopPos;
        /// <summary>
        /// 电机控制模式
        /// </summary>
        public EnumMotorControlMode ControlMode
        {
            get { return _controlMode; }
            set { _controlMode = value; RaisePropertyChanged(); }
        }

        private EnumMotorUnitType _unitType = EnumMotorUnitType.Pulse;
        /// <summary>
        /// 单位类型
        /// </summary>
        public EnumMotorUnitType UnitType
        {
            get { return _unitType; }
            set { _unitType = value; RaisePropertyChanged(); }
        }

        private byte _limitMask = 0x00;
        /// <summary>
        /// 限位掩码
        /// </summary>
        public byte LimitMask
        {
            get { return _limitMask; }
            set
            {
                _limitMask = value;
                RaisePropertyChanged();
            }
        }

        private float _coef = float.NaN;
        /// <summary>
        /// 单位换算系数, 脉冲数：实际物理距离
        /// </summary>
        public float Coef
        {
            get { return _coef; }
            set { _coef = value; RaisePropertyChanged(); }
        }

        private int _pulsePos = 0;
        /// <summary>
        /// 脉冲位置
        /// </summary>
        public int PulsePos
        {
            get { return _pulsePos; }
            set
            {
                _pulsePos = value;
                RaisePropertyChanged();

            }
        }

        private float _realUnitPos = float.NaN;
        /// <summary>
        /// 实际单位下的坐标位置
        /// </summary>
        public float RealUnitPos
        {
            get { return _realUnitPos; }
            set
            {
                _realUnitPos = value;
                RaisePropertyChanged();
            }
        }

        private int _pulseSpeed = 0;
        /// <summary>
        /// 实际脉冲速度
        /// </summary>
        public int PulseSpeed
        {
            get { return _pulseSpeed; }
            set
            {
                _pulseSpeed = value;
                RaisePropertyChanged();

                if (Coef != 0 && !float.IsNaN(Coef))
                {
                    RealUnitSpeed = value / Coef;
                }
            }
        }

        private float _RealUnitSpeed = float.NaN;
        /// <summary>
        /// 实际单位下的速度
        /// </summary>
        public float RealUnitSpeed
        {
            get { return _RealUnitSpeed; }
            set { _RealUnitSpeed = value; RaisePropertyChanged(); }
        }

    }

    public class MotorGetParam : MotorBaseParam
    {
        private byte _stateMask = 0;
        /// <summary>
        /// 状态掩码
        /// R/H SN SZ SP R/S N Z P
        /// </summary>
        public byte StateMask
        {
            get { return _stateMask; }
            set
            {
                _stateMask = value;
                RaisePropertyChanged();

            }
        }

        private EnumMotorDirection _motorDir;
        /// <summary>
        /// 电机的运行方向
        /// </summary>
        public EnumMotorDirection MotorDir
        {
            get { return _motorDir; }
            set { _motorDir = value; RaisePropertyChanged(); }
        }

    }

    public class MotorSetParam : MotorBaseParam
    {

    }

    public class MotorModel : BindableBase
    {
        #region ------------Constructor------------
        public MotorModel()
        {
            InitCommand();
            GetParam.PropertyChanged += GetParam_PropertyChanged;
        }

        public MotorModel(EnumMotorId id, IMotorControl control)
        {
            ID = id;
            Channel = (int)ID;
            _control = control;

            InitCommand();
            GetParam.PropertyChanged += GetParam_PropertyChanged;
        }
        #endregion

        #region ------------Field------------
        private const int StopMaxCount = 3;
        private IMotorControl _control;
        private int _count = 0;
        #endregion

        #region ------------Property------------
        private EnumMotorId _id;
        /// <summary>
        /// 电机的ID，指定电机的实际控制轴
        /// </summary>
        public EnumMotorId ID
        {
            get { return _id; }
            set { _id = value; RaisePropertyChanged(); }
        }

        private int _channel;
        /// <summary>
        /// 通讯通道号
        /// </summary>
        public int Channel
        {
            get { return _channel; }
            set { _channel = value; RaisePropertyChanged(); }
        }

        private bool _polarity;
        /// <summary>
        /// 电机极性
        /// </summary>
        public bool Polarity
        {
            get { return _polarity; }
            set { _polarity = value; RaisePropertyChanged(); }
        }

        private bool _enable = false;
        /// <summary>
        /// 电机是否启用，用于决定是否在项目中使用该电机
        /// </summary>
        public bool Enable
        {
            get { return _enable; }
            set { _enable = value; RaisePropertyChanged(); }
        }

        private bool _active = false;
        /// <summary>
        /// 是否激活响应，未激活状态下不响应任何运动指令，但可以响应设置指令
        /// </summary>
          
        public bool Active
        {
            get { return _active; }
            set { _active = value; RaisePropertyChanged(); }
        }

        private bool _initState;
        /// <summary>
        /// 初始化状态，用于软件判定是否获取当前电机的最新状态
        /// </summary>
        public bool InitState
        {
            get { return _initState; }
            set
            {
                if (value && _initState != value)
                {
                    GetCompensationLength();
                }

                _initState = value;
                RaisePropertyChanged();
            }
        }


        private MotorSetParam _setParam = new MotorSetParam();
        /// <summary>
        /// 电机设置参数
        /// </summary>
        public MotorSetParam SetParam
        {
            get { return _setParam; }
            set { _setParam = value; RaisePropertyChanged(); }
        }

        private MotorGetParam _getParam = new MotorGetParam();
        /// <summary>
        /// 电机读取参数
        /// </summary>
        public MotorGetParam GetParam
        {
            get { return _getParam; }
            set { _getParam = value; RaisePropertyChanged(); }
        }

        private PidModel _pid = new PidModel();
        /// <summary>
        /// 电机PID控制参数
        /// </summary>
        public PidModel Pid
        {
            get { return _pid; }
            set { _pid = value; RaisePropertyChanged(); }
        }

        private int _pidPulseThr = 0;
        /// <summary>
        /// PID脉冲调节阈值
        /// </summary>
        public int PidPulseThr
        {
            get { return _pidPulseThr; }
            set { _pidPulseThr = value; RaisePropertyChanged(); }
        }

        private int _pidMaxCount = 0;
        /// <summary>
        /// PID最大调节次数
        /// </summary>
        public int PidMaxCount
        {
            get { return _pidMaxCount; }
            set { _pidMaxCount = value; RaisePropertyChanged(); }
        }

        private int _microPulseThr = 0;
        /// <summary>
        /// 点动脉冲阈值
        /// </summary>
        public int MicroPulseThr
        {
            get { return _microPulseThr; }
            set { _microPulseThr = value; RaisePropertyChanged(); }
        }

        private int _microPulseMove = 0;
        /// <summary>
        /// 点动脉冲距离
        /// </summary>
        public int MicroPulseMove
        {
            get { return _microPulseMove; }
            set { _microPulseMove = value; RaisePropertyChanged(); }
        }

        private int _softMaxPulsePos = 0;
        /// <summary>
        /// 软限位最大值
        /// </summary>
        public int SoftMaxPulsePos
        {
            get { return _softMaxPulsePos; }
            set { _softMaxPulsePos = value; RaisePropertyChanged(); }
        }

        private int _softMinPulsePos = 0;
        /// <summary>
        /// 软限位最小值
        /// </summary>
        public int SoftMinPulsePos
        {
            get { return _softMinPulsePos; }
            set { _softMinPulsePos = value; RaisePropertyChanged(); }
        }

        private int _closeLoopMaxPulseSpeed = 0;
        /// <summary>
        /// 闭环控制最大脉冲速度
        /// </summary>
        public int CloseLoopMaxPulseSpeed
        {
            get { return _closeLoopMaxPulseSpeed; }
            set { _closeLoopMaxPulseSpeed = value; RaisePropertyChanged(); }
        }

        private int _closeLoopMinPulseSpeed = 0;
        /// <summary>
        /// 闭环控制最小脉冲速度
        /// </summary>
        public int CloseLoopMinPulseSpeed
        {
            get { return _closeLoopMinPulseSpeed; }
            set { _closeLoopMinPulseSpeed = value; RaisePropertyChanged(); }
        }

        private int _maxAcc = 0;
        /// <summary>
        /// 最大加速度
        /// </summary>
        public int MaxAcc
        {
            get { return _maxAcc; }
            set { _maxAcc = value; RaisePropertyChanged(); }
        }

        private int _maxDec = 0;
        /// <summary>
        /// 最大减速度
        /// </summary>
        public int MaxDec
        {
            get { return _maxDec; }
            set { _maxDec = value; RaisePropertyChanged(); }
        }

        private int _movePulse;
        /// <summary>
        /// 相对移动脉冲
        /// </summary>
        public int MovePulse
        {
            get { return _movePulse; }
            set { _movePulse = value; RaisePropertyChanged(); }
        }

        private float _moveUnit;
        /// <summary>
        /// 相对移动物理单位
        /// </summary>
        public float MoveUnit
        {
            get { return _moveUnit; }
            set { _moveUnit = value; RaisePropertyChanged(); }
        }

        private float _holdDistance = 0.0f;
        /// <summary>
        /// 轴移动需要维持的距离（T轴旋转时维持固定的物理焦距），单位mm
        /// </summary>
        public float HoldDistance
        {
            get { return _holdDistance; }
            set { _holdDistance = value; RaisePropertyChanged(); }
        }

        private float _linkLength;
        /// <summary>
        /// 轴到样品的距离，单位mm
        /// </summary>
        public float LinkLength
        {
            get { return _linkLength; }
            set { _linkLength = value; RaisePropertyChanged(); }
        }

        private float _compensationLength1;
        /// <summary>
        /// 限位补偿距离1，单位mm
        /// </summary>
        public float CompensationLength1
        {
            get { return _compensationLength1; }
            set
            {
                _compensationLength1 = value;
                RaisePropertyChanged();

                CustomPos = SetParam.RealUnitPos / 1000.0f - value;
            }
        }

        private float _compensationLength2;
        /// <summary>
        /// 限位补偿距离2，单位mm
        /// </summary>
        public float CompensationLength2
        {
            get { return _compensationLength2; }
            set { _compensationLength2 = value; RaisePropertyChanged(); }
        }

        private float _relativeCoorDis = 0.0f;
        /// <summary>
        /// 相对坐标
        /// </summary>
        public float RelativeCoorDis
        {
            get { return _relativeCoorDis; }
            set { _relativeCoorDis = value; RaisePropertyChanged(); }
        }

        private float _customPos;
        /// <summary>
        /// 客户坐标参数，这边主要是针对Z轴坐标，单位mm
        /// CustomPos = GetParam
        /// </summary>
        public float CustomPos
        {
            get { return _customPos; }
            set
            {
                _customPos = value;
                RaisePropertyChanged();

                var pos = (value + CompensationLength1) * 1000.0f;
                if (!float.IsNaN(pos))
                    SetParam.RealUnitPos = pos;
            }
        }

        #endregion

        #region ------------Command------------
           
        public DelegateCommand? SetMotorTypeCommand { get; set; }

        public void SetMotorType()
        {
            _control.SetMotorType(ID, SetParam.Type);
        }

           
        public DelegateCommand? SetMotorAxisTypeCommand { get; set; }

        public void SetMotorAxisType()
        {
            _control.SetMotorAxtype(ID, SetParam.AxisType);
        }

           
        public DelegateCommand? SetMotorControlModeCommand { get; set; }

        public void SetMotorControlMode()
        {
            _control.SetMotorCtlMode(ID, SetParam.ControlMode);
        }

           
        public DelegateCommand? SetMotorUnitTypeCommand { get; set; }

        public void SetMotorUnitType()
        {
            _control.SetMotorAxUnit(ID, SetParam.UnitType);
        }

           
        public DelegateCommand? SetMotorCoefCommand { get; set; }

        public void SetMotorCoef()
        {
            _control.SetMotorAxCoef(ID, SetParam.Coef);
        }

           
        public DelegateCommand? SetMotorLimitMaskCommand { get; set; }

        public void SetMotorLimitMask()
        {
            _control.SetMotorLimMask(ID, SetParam.LimitMask);
        }

           
        public DelegateCommand? SetMotorPulsePosCommand { get; set; }

        public void SetMotorPulsePos()
        {
            if (!Active)
            {
                _control.SetMotorActiveEnable(ID, true);
            }
            GetParam.StateMask |= 0x08;
            _control.GotoMotorWithPulse(ID, SetParam.PulsePos, HoldDistance);
        }

           
        public DelegateCommand? SetMotorUnitPosCommand { get; set; }

        public void SetMotorUnitPos()
        {
            if (!Active)
            {
                _control.SetMotorActiveEnable(ID, true);
            }
            GetParam.StateMask |= 0x08;
            _control.GotoMotorWithDis(ID, SetParam.RealUnitPos, HoldDistance);
        }

           
        public DelegateCommand? SetMotorPulseSpeedCommand { get; set; }

        public void SetMotorPulseSpeed()
        {
            if (!Active)
            {
                _control.SetMotorActiveEnable(ID, true);
            }
            GetParam.StateMask |= 0x08;
            _control.SpeedMotorWithPulse(ID, SetParam.PulseSpeed);
        }

           
        public DelegateCommand? SetMotorUnitSpeedCommand { get; set; }

        public void SetMotorUnitSpeed()
        {
            if (!Active)
            {
                _control.SetMotorActiveEnable(ID, true);
            }
            GetParam.StateMask |= 0x08;
            _control.SpeedMotorWithDis(ID, SetParam.RealUnitSpeed);
        }

           
        public DelegateCommand? SetMotorPulseMoveCommand { get; set; }

        public void SetMotorPulseMove()
        {
            if (!Active)
            {
                _control.SetMotorActiveEnable(ID, true);
            }
            GetParam.StateMask |= 0x08;
            _control.MoveMotorWithPulse(ID, MovePulse);
        }

           
        public DelegateCommand? SetMotorUnitMoveCommand { get; set; }

        public void SetMotorUnitMove()
        {
            if (!Active)
            {
                _control.SetMotorActiveEnable(ID, true);
            }
            GetParam.StateMask |= 0x08;
            _control.MoveMotorWithDis(ID, MoveUnit);
        }

           
        public DelegateCommand? GetMotorPidCommand { get; set; }

        public void GetMotorPid()
        {
            _control.GetMotorPid(ID);
        }

           
        public DelegateCommand? SetMotorPidCommand { get; set; }

        public void SetMotorPid()
        {
            _control.SetMotorPid(ID, Pid);
        }

           
        public DelegateCommand? GetMotorPidThreCommand { get; set; }

        public void GetMotorPidThre()
        {
            _control.GetMotorPidThre(ID);
        }

           
        public DelegateCommand? SetMotorPidThreCommand { get; set; }

        public void SetMotorPidThre()
        {
            _control.SetMotorPidThre(ID, PidPulseThr);
        }

           
        public DelegateCommand? GetMotorPidMaxCountCommand { get; set; }

        public void GetMotorPidMaxCount()
        {
            _control.GetMotorPidMaxCount(ID);
        }

           
        public DelegateCommand? SetMotorPidMaxCountCommand { get; set; }

        public void SetMotorPidMaxCount()
        {
            _control.SetMotorPidMaxCount(ID, PidMaxCount);
        }

           
        public DelegateCommand? GetMotorMicroThreCommand { get; set; }

        public void GetMotorMicroThre()
        {
            _control.GetMotorMicroThre(ID);
        }

           
        public DelegateCommand? SetMotorMicroThreCommand { get; set; }

        public void SetMotorMicroThre()
        {
            _control.SetMotorMicroThre(ID, MicroPulseThr);
        }

           
        public DelegateCommand? GetMotorMicroLenCommand { get; set; }

        public void GetMotorMicroLen()
        {
            _control.GetMotorMicroLen(ID);
        }

           
        public DelegateCommand? SetMotorMicroLenCommand { get; set; }

        public void SetMotorMicroLen()
        {
            _control.SetMotorMicroLen(ID, MicroPulseMove);
        }

           
        public DelegateCommand? GetMotorMinPosCommand { get; set; }

        public void GetMotorMinPos()
        {
            _control.GetMotorMinPos(ID);
        }

           
        public DelegateCommand? SetMotorMinPosCommand { get; set; }

        public void SetMotorMinPos()
        {
            _control.SetMotorSLimMin(ID, SoftMinPulsePos);
        }

           
        public DelegateCommand? GetMotorMaxPosCommand { get; set; }

        public void GetMotorMaxPos()
        {
            _control.GetMotorMaxPos(ID);
        }

           
        public DelegateCommand? SetMotorMaxPosCommand { get; set; }

        public void SetMotorMaxPos()
        {
            _control.SetMotorSLimMax(ID, SoftMaxPulsePos);
        }

           
        public DelegateCommand? GetMotorMinSpeedCommand { get; set; }

        public void GetMotorMinSpeed()
        {
            _control.GetMotorMinSpeed(ID);
        }

           
        public DelegateCommand? SetMotorMinSpeedCommand { get; set; }

        public void SetMotorMinSpeed()
        {
            _control.SetMotorMinSpeed(ID, CloseLoopMinPulseSpeed);
        }

           
        public DelegateCommand? GetMotorMaxSpeedCommand { get; set; }

        public void GetMotorMaxSpeed()
        {
            _control.GetMotorMaxSpeed(ID);
        }

           
        public DelegateCommand? SetMotorMaxSpeedCommand { get; set; }

        public void SetMotorMaxSpeed()
        {
            _control.SetMotorMaxSpeed(ID, CloseLoopMaxPulseSpeed);
        }

           
        public DelegateCommand? GetMotorMaxAccCommand { get; set; }

        public void GetMotorMaxAcc()
        {
            _control.GetMotorMaxAcc(ID);
        }

           
        public DelegateCommand? SetMotorMaxAccCommand { get; set; }

        public void SetMotorMaxAcc()
        {
            _control.SetMotorMaxAcc(ID, MaxAcc);
        }

           
        public DelegateCommand? GetMotorMaxDecCommand { get; set; }

        public void GetMotorMaxDec()
        {
            _control.GetMotorMaxDec(ID);
        }

           
        public DelegateCommand? SetMotorMaxDecCommand { get; set; }

        public void SetMotorMaxDec()
        {
            _control.SetMotorMaxDec(ID, MaxDec);
        }

           
        public DelegateCommand? MotorActiveCommand { get; set; }

        public void MotorActive()
        {
            _control.SetMotorActiveEnable(ID, true);
        }

           
        public DelegateCommand? MotorDisactiveCommand { get; set; }

        public void MotorDisactive()
        {
            _control.SetMotorActiveEnable(ID, false);
        }

           
        public DelegateCommand? SetLinkLengthCommand { get; set; }

        public void SetLinkLength()
        {
            _control.SetMotorTLink(ID, LinkLength);
        }

           
        public DelegateCommand? GetLinkLengthCommand { get; set; }

        public void GetLinkLength()
        {
            _control.GetMotorTLink(ID);
        }

           
        public DelegateCommand? SetCompensationLengthCommand { get; set; }

        public void SetCompensationLength()
        {
            _control.SetCompensationLength(ID, CompensationLength1, CompensationLength2);
        }

           
        public DelegateCommand? GetCompensationLengthCommand { get; set; }

        public void GetCompensationLength()
        {
            _control.GetCompensationLength(ID);
        }

           
        public DelegateCommand? SetRelativeCoorInfoCommand { get; set; }

        public void SetRelativeCoorInfo()
        {
            _control.SetRelativeCoorInfo(ID, RelativeCoorDis);
            _control.GetRelativeCoorInfo(ID);
        }

           
        public DelegateCommand? GetRelativeCoorInfoCommand { get; set; }

        public void GetRelativeCoorInfo()
        {
            _control.GetRelativeCoorInfo(ID);
        }

           
        public DelegateCommand? SetMotorZeroPosCommand { get; set; }

        public void SetMotorZeroPos()
        {
            _control.SetMotorZero(ID);
        }

           
        public DelegateCommand? SetMotorStopCommand { get; set; }

        public void SetMotorStop()
        {
            _control.StopMotor(ID);
        }

           
        public DelegateCommand? SetMotorOfflineCommand { get; set; }

        public void SetMotorOffline()
        {
            _control.SetMotorActiveEnable(ID, false);
        }

           
        public DelegateCommand? GetMotorStatusCommand { get; set; }

        public void GetMotorStatus()
        {
            _control?.GetMotorStatus(ID);
        }

        private void InitCommand()
        {
            SetMotorTypeCommand = new DelegateCommand(SetMotorType);
            SetMotorAxisTypeCommand = new DelegateCommand(SetMotorAxisType);
            SetMotorControlModeCommand = new DelegateCommand(SetMotorControlMode);
            SetMotorUnitTypeCommand = new DelegateCommand(SetMotorUnitType);
            SetMotorCoefCommand = new DelegateCommand(SetMotorCoef);
            SetMotorLimitMaskCommand = new DelegateCommand(SetMotorLimitMask);
            SetMotorPulsePosCommand = new DelegateCommand(SetMotorPulsePos);
            SetMotorUnitPosCommand = new DelegateCommand(SetMotorUnitPos);
            SetMotorPulseSpeedCommand = new DelegateCommand(SetMotorPulseSpeed);
            SetMotorUnitSpeedCommand = new DelegateCommand(SetMotorUnitSpeed);
            SetMotorPulseMoveCommand = new DelegateCommand(SetMotorPulseMove);
            SetMotorUnitMoveCommand = new DelegateCommand(SetMotorUnitMove);
            GetMotorPidCommand = new DelegateCommand(GetMotorPid);
            SetMotorPidCommand = new DelegateCommand(SetMotorPid);
            GetMotorPidThreCommand = new DelegateCommand(GetMotorPidThre);
            SetMotorPidThreCommand = new DelegateCommand(SetMotorPidThre);
            GetMotorPidMaxCountCommand = new DelegateCommand(GetMotorPidMaxCount);
            SetMotorPidMaxCountCommand = new DelegateCommand(SetMotorPidMaxCount);
            GetMotorMicroThreCommand = new DelegateCommand(GetMotorMicroThre);
            SetMotorMicroThreCommand = new DelegateCommand(SetMotorMicroThre);
            GetMotorMicroLenCommand = new DelegateCommand(GetMotorMicroLen);
            SetMotorMicroLenCommand = new DelegateCommand(SetMotorMicroLen);
            GetMotorMinPosCommand = new DelegateCommand(GetMotorMinPos);
            SetMotorMinPosCommand = new DelegateCommand(SetMotorMinPos);
            GetMotorMaxPosCommand = new DelegateCommand(GetMotorMaxPos);
            SetMotorMaxPosCommand = new DelegateCommand(SetMotorMaxPos);
            GetMotorMinSpeedCommand = new DelegateCommand(GetMotorMinSpeed);
            SetMotorMinSpeedCommand = new DelegateCommand(SetMotorMinSpeed);
            GetMotorMaxSpeedCommand = new DelegateCommand(GetMotorMaxSpeed);
            SetMotorMaxSpeedCommand = new DelegateCommand(SetMotorMaxSpeed);
            GetMotorMaxDecCommand = new DelegateCommand(GetMotorMaxDec);
            SetMotorMaxDecCommand = new DelegateCommand(SetMotorMaxDec);
            GetMotorMaxAccCommand = new DelegateCommand(GetMotorMaxAcc);
            SetMotorMaxAccCommand = new DelegateCommand(SetMotorMaxAcc);
            MotorActiveCommand = new DelegateCommand(MotorActive);
            MotorDisactiveCommand = new DelegateCommand(MotorDisactive);
            SetLinkLengthCommand = new DelegateCommand(SetLinkLength);
            GetLinkLengthCommand = new DelegateCommand(GetLinkLength);
            SetCompensationLengthCommand = new DelegateCommand(SetCompensationLength);
            GetCompensationLengthCommand = new DelegateCommand(GetCompensationLength);
            SetRelativeCoorInfoCommand = new DelegateCommand(SetRelativeCoorInfo);
            GetRelativeCoorInfoCommand = new DelegateCommand(GetRelativeCoorInfo);
            SetMotorZeroPosCommand = new DelegateCommand(SetMotorZeroPos);
            SetMotorStopCommand = new DelegateCommand(SetMotorStop);
            SetMotorOfflineCommand = new DelegateCommand(SetMotorOffline);
            GetMotorStatusCommand = new DelegateCommand(GetMotorStatus);
        }
        #endregion

        #region ------------PublicMethod------------

        /// <summary>
        /// 物理距离转换成脉冲
        /// </summary>
        /// <param name="dis">实际物理数值</param>
        /// <returns></returns>
        public int DisToPulse(double dis)
        {
            if (GetParam.Coef == float.NaN)
            {
                return (int)dis;
            }

            return (int)(dis * GetParam.Coef);
        }

        /// <summary>
        /// 脉冲数转距离，距离单位μm
        /// </summary>
        /// <param name="pulse">脉冲数</param>
        /// <returns></returns>
        public double PulseToDis(int pulse)
        {
            if (GetParam.Coef == float.NaN)
            {
                return pulse;
            }

            return pulse / GetParam.Coef;
        }

        /// <summary>
        /// 设置电机脉冲位置
        /// </summary>
        /// <param name="pulse">电机脉冲位置</param>
        public void SetMotorGetPulsePos(int pulse)
        {
            if (_control != null && !_control.IsAutoUpdateState)
            {
                if (GetParam.PulsePos - pulse < 5)
                {
                    _count++;
                    if (_count > StopMaxCount)
                    {
                        GetParam.StateMask = (byte)(GetParam.StateMask | 0x80);
                    }
                }
                else
                {
                    _count = 0;
                    GetParam.StateMask = (byte)(GetParam.StateMask & 0x7F);
                }
            }

            GetParam.PulsePos = pulse * (Polarity ? 1 : -1);
            if (SetParam.RealUnitPos == float.NaN)
            {
                SetParam.PulsePos = GetParam.PulsePos;
            }
        }
        #endregion

        #region ------------PrivateMethod------------
        private void GetParam_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var getParam = sender as MotorGetParam;
            if (getParam != null)
            {

                switch (e.PropertyName)
                {
                    case "StateMask":
                        this.Active = (getParam.StateMask & 128) != 0;
                        break;
                    case "PulsePos":
                        {
                            if (getParam.Coef != 0 && !float.IsNaN(getParam.Coef))
                            {
                                getParam.RealUnitPos = (getParam.PulsePos / getParam.Coef);
                            }
                        }
                        break;
                    case "RealUnitPos":
                        if (float.IsNaN(SetParam.RealUnitPos) || Math.Abs(getParam.RealUnitPos - SetParam.RealUnitPos) > 1.0)
                        {
                            SetParam.RealUnitPos = getParam.RealUnitPos;
                        }
                        if (!float.IsNaN(CompensationLength1))
                            CustomPos = getParam.RealUnitPos / 1000.0f - CompensationLength1;
                        break;
                }
            }
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
