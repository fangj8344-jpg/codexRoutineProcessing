using NLog.Fluent;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using UtilityTools.Core.Extension;
using UtilityTools.Core.Model;
using UtilityTools.Modules.Motor5Controller.Protocol;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Modules.Motor5Controller.Model
{
    #region ------------Class------------
    /// <summary>
    /// 电机基本参数
    /// </summary>
    public class MotorBaseParams : BindableBase
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

        private EnumMotorCtrType _ctrType = EnumMotorCtrType.CloseLoopCtr;
        /// <summary>
        /// 控制类型
        /// </summary>
        public EnumMotorCtrType CtrType
        {
            get { return _ctrType; }
            set { _ctrType = value; RaisePropertyChanged(); }
        }

        private EnumMotorRunMode _runMode = EnumMotorRunMode.AbsolutePos;
        /// <summary>
        /// 运行模式
        /// </summary>
        public EnumMotorRunMode RunMode
        {
            get { return _runMode; }
            set { _runMode = value; RaisePropertyChanged(); }
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
    }

    /// <summary>
    /// 电机设置参数
    /// </summary>
    public class MotorSetParams : MotorBaseParams
    {
        #region ------------Property------------
        /// <summary>
        /// 设置电机使能命令
        /// </summary>
        public DelegateCommand SetMotorEnableCommand { get; set; }

        /// <summary>
        /// 设置电机停止命令
        /// </summary>
        public DelegateCommand SetMotorStopCommand { get; set; }

        /// <summary>
        /// 设置电机控制类型命令
        /// </summary>
        public DelegateCommand SetMotorCtlTypeCommand { get; set; }

        /// <summary>
        /// 设置电机运行模式命令
        /// </summary>
        public DelegateCommand SetMotorRunModeCommand { get; set; }

        /// <summary>
        /// 设置电机PID参数命令
        /// </summary>
        public DelegateCommand SetMotorPidCommand { get; set; }

        /// <summary>
        /// 设置电机转换系数命令
        /// </summary>
        public DelegateCommand SetMotorFactorCommand { get; set; }

        /// <summary>
        /// 设置电机控制参数命令
        /// </summary>
        public DelegateCommand SetMotorCtrParamsCommand { get; set; }

        /// <summary>
        /// 设置电机速度参数命令
        /// </summary>
        public DelegateCommand SetMotorSpeedParamsCommand { get; set; }

        /// <summary>
        /// 设置电机加速参数命令
        /// </summary>
        public DelegateCommand SetMotorAccParamsCommand { get; set; }

        /// <summary>
        /// 设置电机位置参数命令
        /// </summary>
        public DelegateCommand SetMotorPosParamsCommand { get; set; }

        private EmumMotorMoveDirection _allSpeedDirection = EmumMotorMoveDirection.Forward;
        /// <summary>
        /// 全速运动方向
        /// </summary>
        public EmumMotorMoveDirection AllSpeedDirection
        {
            get { return _allSpeedDirection; }
            set { _allSpeedDirection = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 设置电机全速运动命令
        /// </summary>
        public DelegateCommand SetMotorAllSpeedCommand { get; set; }

        /// <summary>
        /// 设置电机零点位置指令
        /// </summary>
        public DelegateCommand SetMotorZeroPosCommand { get; set; }

        private EnumMotorMoveType _motorMoveType;
        /// <summary>
        /// 电机移动类型 
        /// </summary>
        public EnumMotorMoveType MotorMoveType
        {
            get { return _motorMoveType; }
            set { _motorMoveType = value; RaisePropertyChanged(); }
        }

        private int _pulseByCycle;
        /// <summary>
        /// 旋转一周的脉冲数
        /// </summary>
        public int PulseByCycle
        {
            get { return _pulseByCycle; }
            set { _pulseByCycle = value; RaisePropertyChanged(); }
        }

        private int _leadDis;
        /// <summary>
        /// 丝杆导程
        /// </summary>
        public int LeadDis
        {
            get { return _leadDis; }
            set { _leadDis = value; RaisePropertyChanged(); }
        }

        private int _minLimitedPos;
        /// <summary>
        /// 最小限位点
        /// </summary>
        public int MinLimitedPos
        {
            get { return _minLimitedPos; }
            set { _minLimitedPos = value; RaisePropertyChanged(); }
        }

        private int _maxLimitedPos;
        /// <summary>
        /// 最大限位点
        /// </summary>
        public int MaxLimitedPos
        {
            get { return _maxLimitedPos; }
            set { _maxLimitedPos = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 设置电机软限位指令
        /// </summary>
        public DelegateCommand SetMotorSoftLimitedCommand { get; set; }
        #endregion
    }

    /// <summary>
    /// 电机读取参数
    /// </summary>
    public class MotorReadParams : MotorBaseParams
    {
        #region ------------Property------------
        /// <summary>
        /// 获取电机PID命令
        /// </summary>
        public DelegateCommand GetMotorPidCommand { get; set; }

        /// <summary>
        /// 获取电机控制参数命令
        /// </summary>
        public DelegateCommand GetMotorCtrParamsComamnd { get; set; }

        private int _curSpeed;
        /// <summary>
        /// 电机当前速度
        /// </summary>
        public int CurSpeed
        {
            get { return _curSpeed; }
            set { _curSpeed = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 获取电机速度参数命令
        /// </summary>
        public DelegateCommand GetMotorSpeedParamsCommand { get; set; }

        /// <summary>
        /// 获取电机加速参数命令
        /// </summary>
        public DelegateCommand GetMotorAccParamsCommand { get; set; }

        private int _curPos;
        /// <summary>
        /// 当前位置
        /// </summary>
        public int CurPos
        {
            get { return _curPos; }
            set { _curPos = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 获取电机位置参数命令
        /// </summary>
        public DelegateCommand GetMotorPosParamsCommand { get; set; }

        /// <summary>
        /// 获取电机零点位置命令
        /// </summary>
        public DelegateCommand GetMotorZeroPosCommand { get; set; }

        private EnumMotorType _motorType = EnumMotorType.StepperMotorr;
        /// <summary>
        /// 电机类型
        /// </summary>
        public EnumMotorType MotorType
        {
            get { return _motorType; }
            set { _motorType = value; RaisePropertyChanged(); }
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

        private int _zeroState;
        /// <summary>
        /// 电机零位状态
        /// </summary>
        public int ZeroState
        {
            get { return _zeroState; }
            set { _zeroState = value; RaisePropertyChanged(); }
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

        /// <summary>
        /// 获取电机状态参数命令
        /// </summary>
        public DelegateCommand GetMotorStateCommand { get; set; }
        #endregion
    }
    #endregion


    public class MotorModel : BindableBase
    {
        #region ------------Constructor------------
        public MotorModel(EnumMotorId id, Func<byte[], bool> func)
        {
            MotorId = id;
            _sendMsgFunc = func;

            SetParams = new MotorSetParams();
            SetParams.SetMotorEnableCommand = new DelegateCommand(SetMotorEnable);
            SetParams.SetMotorStopCommand = new DelegateCommand(SetMotorStop);
            SetParams.SetMotorCtlTypeCommand = new DelegateCommand(SetMotorCtlType);
            SetParams.SetMotorRunModeCommand = new DelegateCommand(SetMotorRunMode);
            SetParams.SetMotorPidCommand = new DelegateCommand(SetMotorPid);
            SetParams.SetMotorFactorCommand = new DelegateCommand(SetMotorFactor);
            SetParams.SetMotorCtrParamsCommand = new DelegateCommand(SetMotorCtrParams);
            SetParams.SetMotorSpeedParamsCommand = new DelegateCommand(SetMotorSpeedParams);
            SetParams.SetMotorAccParamsCommand = new DelegateCommand(SetMotorAccParams);
            SetParams.SetMotorPosParamsCommand = new DelegateCommand(SetMotorPosParams);
            SetParams.SetMotorAllSpeedCommand = new DelegateCommand(SetMotorAllSpeed);
            SetParams.SetMotorZeroPosCommand = new DelegateCommand(SetMotorZeroPos);
            SetParams.SetMotorSoftLimitedCommand = new DelegateCommand(SetMotorSoftLimited);

            ReadParams = new MotorReadParams();
            ReadParams.GetMotorPidCommand = new DelegateCommand(GetMotorPid);
            ReadParams.GetMotorCtrParamsComamnd = new DelegateCommand(GetMotorCtrParams);
            ReadParams.GetMotorSpeedParamsCommand = new DelegateCommand(GetMotorSpeedParams);
            ReadParams.GetMotorAccParamsCommand = new DelegateCommand(GetMotorAccParams);
            ReadParams.GetMotorPosParamsCommand = new DelegateCommand(GetMotorPosParams);
            ReadParams.GetMotorZeroPosCommand = new DelegateCommand(GetMotorZeroPos);
            ReadParams.GetMotorStateCommand = new DelegateCommand(GetMotorState);
        }
        #endregion

        #region ------------Field------------
        private Func<byte[], bool> _sendMsgFunc;
        private System.Timers.Timer _timer;
        #endregion

        #region ------------Property------------
        private EnumMotorId _motorId;
        /// <summary>
        /// 电机ID
        /// </summary>
        public EnumMotorId MotorId
        {
            get { return _motorId; }
            private set { _motorId = value; RaisePropertyChanged(); }
        }

        private bool _isAutoRead;

        public bool IsAutoRead
        {
            get { return _isAutoRead; }
            set 
            { 
                _isAutoRead = value; 
                RaisePropertyChanged();

                if (_timer == null)
                { 
                    _timer = new System.Timers.Timer();
                    _timer.Elapsed += Timer_Elapsed;
                    _timer.AutoReset = true;
                    _timer.Interval = 1000;
                }

                _timer.Enabled = value;
            }
        }

        private MotorSetParams _setParams;

        public MotorSetParams SetParams
        {
            get { return _setParams; }
            set { _setParams = value; RaisePropertyChanged(); }
        }

        private MotorReadParams _readParams;

        public MotorReadParams ReadParams
        {
            get { return _readParams; }
            set { _readParams = value; RaisePropertyChanged(); }
        }

        #endregion

        #region ------------PublicMethod------------

        #region ------------SetMethod------------
        public void SetMotorEnable()
        {
            var msg = Motor5Protocol.SetMotorEnable(MotorId, SetParams.Enable);
            _sendMsgFunc?.Invoke(msg);
        }

        public void SetMotorStop()
        {
            var msg = Motor5Protocol.SetMotorStop(MotorId);
            _sendMsgFunc?.Invoke(msg);
        }

        public void SetMotorCtlType()
        {
            var msg = Motor5Protocol.SetMotorCtrType(MotorId, SetParams.CtrType);
            _sendMsgFunc?.Invoke(msg);
        }

        public void SetMotorRunMode()
        {
            var msg = Motor5Protocol.SetMotorRunMode(MotorId, SetParams.RunMode);
            _sendMsgFunc?.Invoke(msg);
        }

        public void SetMotorPid()
        {
            var msg = Motor5Protocol.SetMotorPid(MotorId, SetParams.Pid.P, SetParams.Pid.I, SetParams.Pid.D);
            _sendMsgFunc?.Invoke(msg);
        }

        public void SetMotorFactor()
        {
            var msg = Motor5Protocol.SetMotorSubRatio(MotorId, SetParams.MotorMoveType, SetParams.PulseByCycle, SetParams.LeadDis);
            _sendMsgFunc?.Invoke(msg);
        }

        public void SetMotorCtrParams()
        {
            var msg = Motor5Protocol.SetMotorCtrParams(MotorId, SetParams.Times, SetParams.Threshold, SetParams.InchThreshold);
            _sendMsgFunc?.Invoke(msg);
        }

        public void SetMotorSpeedParams()
        {
            var msg = Motor5Protocol.SetMotorSpeedParams(MotorId, SetParams.StartSpeed, SetParams.MaxSpeed);
            _sendMsgFunc?.Invoke(msg);
        }

        public void SetMotorAccParams()
        {
            var msg = Motor5Protocol.SetMotorAccParams(MotorId, SetParams.MaxAcc, SetParams.MaxDec);
            _sendMsgFunc?.Invoke(msg);
        }

        public void SetMotorPosParams()
        {
            var msg = Motor5Protocol.SetMotorTargetParam(MotorId, SetParams.TargetValue);
            _sendMsgFunc?.Invoke(msg);
        }

        public void SetMotorAllSpeed()
        {
            var msg = Motor5Protocol.SetMotorAllSpeed(MotorId, SetParams.AllSpeedDirection);
            _sendMsgFunc?.Invoke(msg);
        }

        public void SetMotorZeroPos()
        {
            var msg = Motor5Protocol.SetMotorZeroPos(MotorId, SetParams.OriginPos, SetParams.ZeroPos);
            _sendMsgFunc?.Invoke(msg);
        }

        public void SetMotorSoftLimited()
        {
            var msg = Motor5Protocol.SetMotorSoftLimitPos(MotorId, SetParams.MinLimitedPos, SetParams.MaxLimitedPos);
            _sendMsgFunc?.Invoke(msg);
        }
        #endregion


        #region ------------GetMethod------------
        public void GetMotorPid()
        {
            var msg = Motor5Protocol.GetMotorPid(MotorId);
            _sendMsgFunc?.Invoke(msg);
        }

        public void GetMotorCtrParams()
        {
            var msg = Motor5Protocol.GetMotorCtrParams(MotorId);
            _sendMsgFunc?.Invoke(msg);
        }

        public void GetMotorSpeedParams()
        {
            var msg = Motor5Protocol.GetMotorSpeedParams(MotorId);
            _sendMsgFunc?.Invoke(msg);
        }

        public void GetMotorAccParams()
        {
            var msg = Motor5Protocol.GetMotorAccParams(MotorId);
            _sendMsgFunc?.Invoke(msg);
        }

        public void GetMotorPosParams()
        {
            var msg = Motor5Protocol.GetMotorTargetParam(MotorId);
            _sendMsgFunc?.Invoke(msg);
        }

        public void GetMotorZeroPos()
        {
            var msg = Motor5Protocol.GetMotorZeroPos(MotorId);
            _sendMsgFunc?.Invoke(msg);
        }

        public void GetMotorState()
        {
            var msg = Motor5Protocol.GetMotorStateParams(MotorId);
            _sendMsgFunc?.Invoke(msg);
        }
        #endregion
        #endregion

        #region ------------PrivateMethod------------
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Thread.Sleep(100);
            GetMotorPid();
            Thread.Sleep(100);
            GetMotorCtrParams();
            Thread.Sleep(100);
            GetMotorSpeedParams();
            Thread.Sleep(100);
            GetMotorAccParams();
            Thread.Sleep(100);
            GetMotorPosParams();
            Thread.Sleep(100);
            GetMotorZeroPos();
            Thread.Sleep(100);
            GetMotorState();

        }

        #endregion

        #region ------------StaticMethod------------
        #endregion


    }
}
