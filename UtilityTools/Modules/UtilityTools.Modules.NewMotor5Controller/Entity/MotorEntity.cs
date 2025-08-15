using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using UtilityTools.Core.Model;
using UtilityTools.Core.Protocol;
using UtilityTools.Modules.NewMotor5Controller.Model;
using UtilityTools.Modules.NewMotor5Controller.Protocol;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.Services.Services;

namespace UtilityTools.Modules.NewMotor5Controller.Entity
{
    /// <summary>
    /// 第二版本的电机通用指令
    /// </summary>
    public class MotorEntity : BindableBase, IMotorControl
    {
        #region ------------Constructor------------

        public MotorEntity()
        {
            
            Motors = new ObservableCollection<MotorModel>();

            MotorX = new MotorModel(EnumMotorId.MOTOR_X, this) {  };
            _motorsList.Add(MotorX);

            MotorY = new MotorModel(EnumMotorId.MOTOR_Y, this) {  };
            _motorsList.Add(MotorY);
           
            MotorZ = new MotorModel(EnumMotorId.MOTOR_Z, this) {  };
            _motorsList.Add(MotorZ);
            
            MotorR = new MotorModel(EnumMotorId.MOTOR_R, this) {  };
            _motorsList.Add(MotorR);
           
            MotorT = new MotorModel(EnumMotorId.MOTOR_T, this) { };
            _motorsList.Add(MotorT);
            
            Motors.Add(MotorX);
            Motors.Add(MotorY);
            Motors.Add(MotorZ);
            Motors.Add(MotorR);
            Motors.Add(MotorT);
            MotorX.PropertyChanged += Motor_PropertyChanged;
            MotorY.PropertyChanged += Motor_PropertyChanged;
            MotorZ.PropertyChanged += Motor_PropertyChanged;
            MotorR.PropertyChanged += Motor_PropertyChanged;
            MotorT.PropertyChanged += Motor_PropertyChanged;
            _parser = new NewMotor5ControllerParser();
            _parser.PacketReceivedEvent += DealWithPacketCallback;
            SerialPortService = new SerialPortService();
            SerialPortService.UpdateResponse += SerialPortService_UpdateResponse;
            SerialPortService.IsBinary = true;
            var netUdp = new UdpNetAsyncDevice();
            netUdp.Name = "升级网口";
            netUdp.DeviceInstance.TargetIp = "192.168.1.88";
            netUdp.DeviceInstance.TargetPort = 5003;
            netUdp.DeviceInstance.HostIp = "192.168.1.33";
            netUdp.DeviceInstance.HostPort = 6585;
            netUdp.IsBinary = true;
            NetUdpService = netUdp;
            NetUdpService.UpdateResponse += NetUdpService_UpdateResponse;
        }

      

        #endregion

        #region ------------Field------------
        private List<MotorModel> _motorsList = new List<MotorModel>();
        private System.Timers.Timer? _timer;
        private int _interval = 0;
        private bool _isFirstRequest = true;
        private NewMotor5ControllerParser _parser;
        private MotorProtocol MotorProtocol;
        #endregion

        #region ------------Property------------
       
        private string _hardwareVersion;
        /// <summary>
        /// 硬件版本信息
        /// </summary>
        public string HardwareVersion
        {
            get { return _hardwareVersion; }
            set { _hardwareVersion = value; RaisePropertyChanged(); }
        }

     
        private string _firmwareVersion;
        /// <summary>
        /// 固件版本信息
        /// </summary>
        public string FirmwareVersion
        {
            get { return _firmwareVersion; }
            set { _firmwareVersion = value;RaisePropertyChanged(); }
        }

        private MotorModel _motorX;
        public MotorModel MotorX
        {
            get { return _motorX; }
            set { _motorX = value; RaisePropertyChanged(); }
        }

        private MotorModel _motorY;
        public MotorModel MotorY
        {
            get { return _motorY; }
            set { _motorY = value; RaisePropertyChanged(); }
        }

        private MotorModel _motorZ;
        public MotorModel MotorZ
        {
            get { return _motorZ; }
            set { _motorZ = value; RaisePropertyChanged(); }
        }

        private MotorModel _motorR;
        public MotorModel MotorR
        {
            get { return _motorR; }
            set { _motorR = value; RaisePropertyChanged(); }
        }

        private MotorModel _motorT;
        public MotorModel MotorT
        {
            get { return _motorT; }
            set { _motorT = value; RaisePropertyChanged(); }
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
        private IAsynRWService _device;
        /// <summary>
        /// 当前使用的通信
        /// </summary>
        public IAsynRWService Device
        {
            get { return _device; }
            set { _device = value; RaisePropertyChanged(); }
        }
        public bool IsAutoUpdateState { get; } = true;

        private ObservableCollection<MotorModel> _motors = new ObservableCollection<MotorModel>();

        public ObservableCollection<MotorModel> Motors
        {
            get { return _motors; }
            set { _motors = value; RaisePropertyChanged(); }
        }
        #endregion

        #region ------------PublicMethod------------
        private void NetUdpService_UpdateResponse(object? sender, byte[] e)
        {
            _parser.ReceiveBytes(e);
        }

        private void SerialPortService_UpdateResponse(object? sender, byte[] e)
        {
            _parser.ReceiveBytes(e);
        }

        public  void DealWithPacketCallback(object? sender, DataPacket e)
        {
           
            var cmd = BitConverter.ToUInt16(e.command, 0);
            if (Enum.IsDefined(typeof(EnumCommonMotorCmdType), cmd))
            {
                var type = (EnumCommonMotorCmdType)cmd;
                switch (type)
                {
                    case EnumCommonMotorCmdType.CMD_MOT_GOTO:
                        {
                            int id = e.data[0];
                            var model = _motorsList.FirstOrDefault((item) => item.Channel == id);
                            if (model == null)
                                return ;

                            var length = BitConverter.ToSingle(e.data, 1);
                            var offset = BitConverter.ToSingle(e.data, 5);
                          
                        }
                        break;
                    // PID
                    case EnumCommonMotorCmdType.CMD_GET_POS:
                        {
                            int id = e.data[0];
                            var model = _motorsList.FirstOrDefault((item) => item.Channel == id);
                            if (model == null)
                                return ;

                            model.InitState = true;
                            model.GetParam.UnitType = MotorProtocol.GetMotorUnit(e.data[1]);
                            model.GetParam.PulsePos = BitConverter.ToInt32(e.data, 2) * GetMotorPolarity(model.ID);
                            model.GetParam.Coef = BitConverter.ToSingle(e.data, 6);
                            if (model.SetParam.Coef == float.NaN)
                            {
                                model.SetParam.Coef = model.GetParam.Coef;
                            }
                            if (model.SetParam.RealUnitPos == float.NaN)
                            {
                                model.SetParam.PulsePos = model.GetParam.PulsePos;
                            }
                        }
                        break;
                    case EnumCommonMotorCmdType.CMD_GET_STATUS:
                        {
                            int id = e.data[0];
                            var model = _motorsList.FirstOrDefault((item) => item.Channel == id);
                            if (model == null)
                                return ;

                            model.InitState = true;
                            model.GetParam.Type = MotorProtocol.GetMotorType(e.data[1]);
                            model.GetParam.AxisType = MotorProtocol.GetMotorAxisType(e.data[2]);
                            model.GetParam.ControlMode = MotorProtocol.GetMotorControlMode(e.data[3]);
                            model.GetParam.UnitType = MotorProtocol.GetMotorUnit(e.data[4]);
                            model.GetParam.StateMask = e.data[5];
                            model.GetParam.LimitMask = e.data[6];
                            model.GetParam.MotorDir = MotorProtocol.GetMotorDirection(e.data[7]);
                            model.GetParam.Coef = BitConverter.ToSingle((byte[])e.data, 8);
                            if (float.IsNaN(model.SetParam.Coef))
                            {
                                model.SetParam.Coef = model.GetParam.Coef;
                            }
                            model.GetParam.PulsePos = BitConverter.ToInt32((byte[])e.data, 12) * GetMotorPolarity(model.ID);
                            if (float.IsNaN(model.SetParam.RealUnitPos))
                            {
                                model.SetParam.PulsePos = model.GetParam.PulsePos;
                            }
                            model.GetParam.PulseSpeed = BitConverter.ToInt32((byte[])e.data, 16) * GetMotorPolarity(model.ID);
                        }
                        break;
                    case EnumCommonMotorCmdType.CMD_GET_SPEED:
                        {
                            int id = e.data[0];
                            var model = _motorsList.FirstOrDefault((item) => item.Channel == id);
                            if (model == null)
                                return ;

                            model.InitState = true;
                            model.GetParam.UnitType = MotorProtocol.GetMotorUnit(e.data[1]);
                            model.GetParam.PulseSpeed = BitConverter.ToInt32(e.data, 2) * GetMotorPolarity(model.ID); ;
                            model.GetParam.Coef = BitConverter.ToSingle(e.data, 6);
                        }
                        break;
                    case EnumCommonMotorCmdType.CMD_GET_MCTL:
                        {
                            int id = e.data[0];
                            var model = _motorsList.FirstOrDefault((item) => item.Channel == id);
                            if (model == null)
                                return ;

                            model.InitState = true;
                            model.GetParam.ControlMode = MotorProtocol.GetMotorControlMode(e.data[1]);
                        }
                        break;
                    case EnumCommonMotorCmdType.CMD_GET_SLIM:
                        {
                            int id = e.data[0];
                            var model = _motorsList.FirstOrDefault((item) => item.Channel == id);
                            if (model == null)
                                return ;

                            model.InitState = true;
                            model.GetParam.LimitMask = e.data[1];
                        }
                        break;
                    case EnumCommonMotorCmdType.CMD_GET_THQ:
                        {
                            int id = e.data[0];
                            var model = _motorsList.FirstOrDefault((item) => item.Channel == id);
                            if (model == null)
                                return ;

                            model.InitState = true;
                            model.PidPulseThr = BitConverter.ToInt32(e.data, 1);
                        }
                        break;
                    case EnumCommonMotorCmdType.CMD_GET_THT:
                        {
                            int id = e.data[0];
                            var model = _motorsList.FirstOrDefault((item) => item.Channel == id);
                            if (model == null)
                                return ;

                            model.InitState = true;
                            model.PidMaxCount = BitConverter.ToInt32(e.data, 1);
                        }
                        break;
                    case EnumCommonMotorCmdType.CMD_GET_THMICRO:
                        {
                            int id = e.data[0];
                            var model = _motorsList.FirstOrDefault((item) => item.Channel == id);
                            if (model == null)
                                return ;

                            model.InitState = true;
                            model.MicroPulseThr = BitConverter.ToInt32(e.data, 1);
                        }
                        break;
                    case EnumCommonMotorCmdType.CMD_GET_MICROLEN:
                        {
                            int id = e.data[0];
                            var model = _motorsList.FirstOrDefault((item) => item.Channel == id);
                            if (model == null)
                                return ;

                            model.InitState = true;
                            model.MicroPulseMove = BitConverter.ToInt32(e.data, 1);
                        }
                        break;
                    case EnumCommonMotorCmdType.CMD_GET_PID:
                        {
                            int id = e.data[0];
                            var model = _motorsList.FirstOrDefault((item) => item.Channel == id);
                            if (model == null)
                                return ;

                            model.InitState = true;
                            model.Pid.Kp = BitConverter.ToSingle(e.data, 1);
                            model.Pid.Ki = BitConverter.ToSingle(e.data, 5);
                            model.Pid.Kd = BitConverter.ToSingle(e.data, 9);
                        }
                        break;
                    case EnumCommonMotorCmdType.CMD_GET_MAXSPOS:
                        {
                            int id = e.data[0];
                            var model = _motorsList.FirstOrDefault((item) => item.Channel == id);
                            if (model == null)
                                return ;

                            model.InitState = true;
                            model.GetParam.UnitType = MotorProtocol.GetMotorUnit(e.data[1]);
                            model.SoftMaxPulsePos = BitConverter.ToInt32(e.data, 2);
                            model.GetParam.Coef = BitConverter.ToSingle(e.data, 6);
                        }
                        break;
                    case EnumCommonMotorCmdType.CMD_GET_MINSPOS:
                        {
                            int id = e.data[0];
                            var model = _motorsList.FirstOrDefault((item) => item.Channel == id);
                            if (model == null)
                                return ;

                            model.InitState = true;
                            model.GetParam.UnitType = MotorProtocol.GetMotorUnit(e.data[1]);
                            model.SoftMinPulsePos = BitConverter.ToInt32(e.data, 2);
                            model.GetParam.Coef = BitConverter.ToSingle(e.data, 6);
                        }
                        break;
                    case EnumCommonMotorCmdType.CMD_GET_MAXCLS:
                        {
                            int id = e.data[0];
                            var model = _motorsList.FirstOrDefault((item) => item.Channel == id);
                            if (model == null)
                                return ;

                            model.InitState = true;
                            model.GetParam.UnitType = MotorProtocol.GetMotorUnit(e.data[1]);
                            model.CloseLoopMaxPulseSpeed = BitConverter.ToInt32(e.data, 2);
                            model.GetParam.Coef = BitConverter.ToSingle(e.data, 6);
                        }
                        break;
                    case EnumCommonMotorCmdType.CMD_GET_MINCLS:
                        {
                            int id = e.data[0];
                            var model = _motorsList.FirstOrDefault((item) => item.Channel == id);
                            if (model == null)
                                return ;

                            model.InitState = true;
                            model.GetParam.UnitType = MotorProtocol.GetMotorUnit(e.data[1]);
                            model.CloseLoopMinPulseSpeed = BitConverter.ToInt32(e.data, 2);
                            model.GetParam.Coef = BitConverter.ToSingle(e.data, 6);
                        }
                        break;
                    case EnumCommonMotorCmdType.CMD_GET_AXTYPE:
                        {
                            int id = e.data[0];
                            var model = _motorsList.FirstOrDefault((item) => item.Channel == id);
                            if (model == null)
                                return ;

                            model.InitState = true;
                            model.GetParam.AxisType = MotorProtocol.GetMotorAxisType(e.data[1]);
                        }
                        break;
                    case EnumCommonMotorCmdType.CMD_GET_AXUNIT:
                        {
                            int id = e.data[0];
                            var model = _motorsList.FirstOrDefault((item) => item.Channel == id);
                            if (model == null)
                                return ;

                            model.InitState = true;
                            model.GetParam.UnitType = MotorProtocol.GetMotorUnit(e.data[1]);
                        }
                        break;
                    case EnumCommonMotorCmdType.CMD_GET_MTYPE:
                        {
                            int id = e.data[0];
                            var model = _motorsList.FirstOrDefault((item) => item.Channel == id);
                            if (model == null)
                                return ;

                            model.InitState = true;
                            model.GetParam.Type = MotorProtocol.GetMotorType(e.data[1]);
                        }
                        break;
                    case EnumCommonMotorCmdType.CMD_GET_AXCOEF:
                        {
                            int id = e.data[0];
                            var model = _motorsList.FirstOrDefault((item) => item.Channel == id);
                            if (model == null)
                                return ;

                            model.InitState = true;
                            model.GetParam.Coef = BitConverter.ToSingle(e.data, 1);
                        }
                        break;
                    case EnumCommonMotorCmdType.CMD_GET_TLINK:
                        {
                            int id = e.data[0];
                            var model = _motorsList.FirstOrDefault((item) => item.Channel == id);
                            if (model == null)
                                return ;

                            model.InitState = true;
                            model.GetParam.Coef = BitConverter.ToSingle(e.data, 2);
                            var value = BitConverter.ToSingle(e.data, 6);
                            switch (e.data[1])
                            {
                                case 0x00:
                                    model.LinkLength = (float)(model.PulseToDis((int)value) / 1000.0f);
                                    break;
                                default:
                                    model.LinkLength = (float)(value / 1000.0f);
                                    break;
                            }
                        }
                        break;
                    case EnumCommonMotorCmdType.CMD_GET_SAMPLEH:
                        {
                            int id = e.data[0];
                            var model = _motorsList.FirstOrDefault((item) => item.Channel == id);
                            if (model == null)
                                return ;

                            model.InitState = true;
                            model.GetParam.Coef = BitConverter.ToSingle(e.data, 2);
                            var value1 = BitConverter.ToSingle(e.data, 6);
                            var value2 = BitConverter.ToSingle(e.data, 10);
                            switch (e.data[1])
                            {
                                case 0x00:
                                    model.CompensationLength1 = (float)(model.PulseToDis((int)value1) / 1000.0f);
                                    model.CompensationLength2 = (float)(model.PulseToDis((int)value2) / 1000.0f);
                                    break;
                                default:
                                    model.CompensationLength1 = (float)(value1 / 1000.0f);
                                    model.CompensationLength2 = (float)(value2 / 1000.0f);
                                    break;
                            }
                        }
                        break;
                    case EnumCommonMotorCmdType.CMD_GET_PHLEH:
                        {
                            int id = e.data[0];
                            var model = _motorsList.FirstOrDefault((item) => item.Channel == id);
                            if (model == null)
                                return ;

                            model.InitState = true;
                            model.GetParam.Coef = BitConverter.ToSingle(e.data, 2);
                            var value = BitConverter.ToSingle(e.data, 6);
                            switch (e.data[1])
                            {
                                case 0x00:
                                    model.RelativeCoorDis = (float)(model.PulseToDis((int)value) / 1000.0f);
                                    break;
                                default:
                                    model.RelativeCoorDis = (float)(value / 1000.0f);
                                    break;
                            }
                        }
                        break;
                }
            }

            return ;
        }

        public int DisToPulse(EnumMotorId motorId, double dis)
        {
            var targetMotor = Motors.FirstOrDefault((item) => item.ID == motorId);
            if (targetMotor == null)
                return (int)dis;

            return targetMotor.DisToPulse(dis);
        }

        public void GetMotorCtlMode(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"获取通道{motorId}电机的控制模式");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.GetMotorControlModeCommand(channel);
            Device?.SendMsg(packet);
        }

        public void GetMotorMaxSpeed(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"获取通道{motorId}电机最大速度");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.GetMotorMaxSpeedCommand(channel);
            Device?.SendMsg(packet);
        }

        public void GetMotorMicroLen(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"获取通道{motorId}电机点动距离");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.GetMotorMicroLenCommand(channel);
            Device?.SendMsg(packet);
        }

        public void GetMotorMicroThre(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"获取通道{motorId}电机点动阈值");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.GetMotorMicroThrCountCommand(channel);
            Device?.SendMsg(packet);
        }

        public void GetMotorMinSpeed(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"获取通道{motorId}电机最小速度");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.GetMotorMinSpeedCommand(channel);
            Device?.SendMsg(packet);
        }

        public void GetMotorPid(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"获取通道{motorId}电机的PID参数");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.GetMotorPidCommand(channel);
            Device?.SendMsg(packet);
        }

        public void GetMotorPidThre(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"获取通道{motorId}电机的PID调节阈值");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.GetMotorPidThrCommand(channel);
            Device?.SendMsg(packet);
        }

        public void GetMotorPidMaxCount(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"获取通道{motorId}电机的PID最大调节次数");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.GetMotorPidMaxCountCommand(channel);
            Device?.SendMsg(packet);
        }

        public void GetMotorPos(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"获取通道{motorId}电机的位置");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.GetMotorPosCommand(channel);
            Device?.SendMsg(packet);
        }

        public void GetMotorMaxPos(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"获取通道{motorId}电机的最大限位");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.GetMotorMaxPosCommand(channel);
            Device?.SendMsg(packet);
        }

        public void GetMotorMinPos(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"获取通道{motorId}电机的最小限位");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.GetMotorMinPosCommand(channel);
            Device?.SendMsg(packet);
        }

        public void GetMotorLimMask(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"获取通道{motorId}电机的限位掩码");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.GetMotorLimMaskCommand(channel);
            Device?.SendMsg(packet);
        }

        public void GetMotorSpeed(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"获取通道{motorId}电机的速度");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.GetMotorSpeedCommand(channel);
            Device?.SendMsg(packet);
        }

        public void GetMotorStatus(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"获取通道{motorId}电机的状态");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.GetMotorStatusCommand(channel);
            Device?.SendMsg(packet);
        }

        public void GotoMotorWithDis(EnumMotorId motorId, double dis, float obValue = 0.0f)
        {
            var pulse = DisToPulse(motorId, dis);
            GotoMotorWithPulse(motorId, pulse, obValue);
        }

        public void GotoMotorWithPulse(EnumMotorId motorId, int pulse, float obValue = 0.0f)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"绝对移动通道{motorId}电机到{pulse}");
            var motor = _motorsList.FirstOrDefault(item => item.ID == motorId);
            if (motor == null)
                return;

            CheckMotorControlModel(motorId, EnumMotorControlMode.CloseLoopPos);
            CheckMotorActive(motorId);
            SetMotorActiveEnable(motorId, true);
            var packet = MotorProtocol.SetMotorGotoCommand((byte)motor.Channel, EnumMotorUnitType.Pulse, pulse * GetMotorPolarity(motorId), obValue);
            Device?.SendMsg(packet);
        }

        public void MoveMotorWithDis(EnumMotorId motorId, double dis)
        {
            var pulse = DisToPulse(motorId, dis);
            MoveMotorWithPulse(motorId, pulse);
        }

        public void MoveMotorWithPulse(EnumMotorId motorId, int pulse)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"相对移动通道{motorId}电机{pulse}");
            var motor = _motorsList.FirstOrDefault(item => item.ID == motorId);
            if (motor == null)
                return;

            CheckMotorControlModel(motorId, EnumMotorControlMode.CloseLoopPos);
            CheckMotorActive(motorId);
            var packet = MotorProtocol.SetMotorMoveCommand((byte)motor.Channel, EnumMotorUnitType.Pulse, pulse * GetMotorPolarity(motorId));
            Device?.SendMsg(packet);
        }

        public double PulseToDis(EnumMotorId motorId, int pulse)
        {
            var targetMotor = Motors.First((item) => item.ID == motorId);
            if (targetMotor == null)
                return pulse;

            return targetMotor.PulseToDis(pulse);
        }

        public void SetMotorActiveEnable(EnumMotorId motorId, bool active)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置通道{motorId}电机的激活状态{active}");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.SetMotorActiveCommand(channel, active);
            Device?.SendMsg(packet);
        }

        public void SetMotorAxCoef(EnumMotorId motorId, float coef)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置通道{motorId}电机的转换系数");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.SetMotorAxisCoefCommand(channel, coef);
            Device?.SendMsg(packet);
        }

        public void SetMotorAxtype(EnumMotorId motorId, EnumMotorAxisType type)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置通道{motorId}电机的轴类型{(type)}");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.SetMotorAxisTypeCommand(channel, type);
            Device?.SendMsg(packet);
        }

        public void SetMotorAxUnit(EnumMotorId motorId, EnumMotorUnitType unit)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置通道{motorId}电机的单位类型{(unit)}");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.SetMotorAxisUnitCommand(channel, unit);
            Device?.SendMsg(packet);
        }

        public void SetMotorCtlMode(EnumMotorId motorId, EnumMotorControlMode mode)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置通道{motorId}电机的控制模式{(mode)}");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.SetMotorLoopCommand(channel, mode);
            Device?.SendMsg(packet);
        }

        public void SetMotorMaxSpeed(EnumMotorId motorId, int maxSpeed)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置通道{motorId}电机的最大速度");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.SetMotorMaxSpeedCommand(channel, EnumMotorUnitType.Pulse, maxSpeed);
            Device?.SendMsg(packet);
        }

        public void SetMotorMicroLen(EnumMotorId motorId, int microLen)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置通道{motorId}电机的点动距离{microLen}");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.SetMotorMicroLenCommand(channel, EnumMotorUnitType.Pulse, microLen);
            Device?.SendMsg(packet);
        }

        public void SetMotorMicroThre(EnumMotorId motorId, int threshold)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置通道{motorId}电机的点动阈值{threshold}");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.SetMotorMicroThrCommand(channel, EnumMotorUnitType.Pulse, threshold);
            Device?.SendMsg(packet);
        }

        public void SetMotorMinSpeed(EnumMotorId motorId, int minSpeed)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置通道{motorId}电机的最小速度{minSpeed}");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.SetMotorMinSpeedCommand(channel, EnumMotorUnitType.Pulse, minSpeed);
            Device?.SendMsg(packet);
        }

        public void SetMotorPid(EnumMotorId motorId, PidModel pid)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置通道{motorId}电机的PID参数({pid.Kp},{pid.Ki},{pid.Kd})");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.SetMotorPidCommand(channel, pid);
            Device?.SendMsg(packet);
        }

        public void SetMotorPidThre(EnumMotorId motorId, int pidThre)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置通道{motorId}电机的PID调节阈值{pidThre}");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.SetMotorPidThrCommand(channel, EnumMotorUnitType.Pulse, pidThre);
            Device?.SendMsg(packet);
        }

        public void SetMotorPidMaxCount(EnumMotorId motorId, int pidTimes)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置通道{motorId}电机的Pid最大调节次数{pidTimes}");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.SetMotorPidMaxCountCommand(channel, EnumMotorUnitType.Pulse, pidTimes);
            Device?.SendMsg(packet);
        }

        public void SetMotorSLimMax(EnumMotorId motorId, int sLimMax)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置通道{motorId}电机的最大软限位{sLimMax}");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.SetMotorMaxLimCommand(channel, EnumMotorUnitType.Pulse, sLimMax);
            Device?.SendMsg(packet);
        }

        public void SetMotorSLimMin(EnumMotorId motorId, int sLimMin)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置通道{motorId}电机的最小软限位{sLimMin}");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.SetMotorMinLimCommand(channel, EnumMotorUnitType.Pulse, sLimMin);
            Device?.SendMsg(packet);
        }

        public void SetMotorLimMask(EnumMotorId motorId, byte softLim)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置通道{motorId}电机的软限位掩码{softLim}");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.SetMotorLimtMaskCommand(channel, softLim);
            Device?.SendMsg(packet);
        }

        public void SetMotorType(EnumMotorId motorId, EnumMotorType type)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置通道{motorId}电机的电机类型{(type)}");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.SetMotorTypeCommand(channel, type);
            Device?.SendMsg(packet);
        }

        public void SetMotorZero(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置通道{motorId}电机零点");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.SetMotorZeroCommand(channel);
            Device?.SendMsg(packet);
        }

        public void SpeedMotorWithDis(EnumMotorId motorId, double dis)
        {
            var pulse = DisToPulse(motorId, dis);
            SpeedMotorWithPulse(motorId, pulse);
        }

        public void SpeedMotorWithPulse(EnumMotorId motorId, int pulse)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"速度移动通道{motorId}电机{pulse}");
            var motor = _motorsList.FirstOrDefault(item => item.ID == motorId);
            if (motor == null)
                return;

            CheckMotorControlModel(motorId, EnumMotorControlMode.OpenLoopSpeed);
            CheckMotorActive(motorId);

            var packet = MotorProtocol.SetMotorGotoCommand((byte)motor.Channel, EnumMotorUnitType.Pulse, pulse * GetMotorPolarity(motorId));
            Device?.SendMsg(packet);
        }

        public void StartRequest(int interval)
        {
            if (interval == _interval && _timer != null && _timer.Enabled == true)
                return;
            _interval = interval;
            if (_timer != null)
            {
                _timer.Enabled = false;
                _timer.Elapsed -= Timer_Elapsed;
                _timer = null;
            }
            _timer = new System.Timers.Timer();
            _timer.AutoReset = true;
            _timer.Interval = interval;
            _timer.Elapsed += Timer_Elapsed;
            _timer.Enabled = true;
        }

        public void StopMotor(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置通道{motorId}电机停止");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.SetMotorRunCommand(channel, false);
            Device?.SendMsg(packet);
        }

        public void StopRequest()
        {
            if (_timer != null)
            {
                _timer.Enabled = false;
                _timer.Elapsed -= Timer_Elapsed;
                _timer = null;
            }
        }

        public void GetMotorMaxAcc(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"获取通道{motorId}电机最大加速度");
        }

        public void SetMotorMaxAcc(EnumMotorId motorId, int maxAcc)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置通道{motorId}电机最大加速度{maxAcc}");
        }

        public void GetMotorMaxDec(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"获取通道{motorId}电机最大减速度");
        }

        public void SetMotorMaxDec(EnumMotorId motorId, int maxDec)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置通道{motorId}电机最大减速度{maxDec}");
        }

        public void SetMotorTLink(EnumMotorId motorId, float length)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置通道{motorId}补偿臂长");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.SetMotorTLinkCommand(channel, length);
            Device?.SendMsg(packet);
        }

        public void GetMotorTLink(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"获取通道{motorId}补偿臂长");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.GetMotorTLinkCommand(channel);
            Device?.SendMsg(packet);
        }

        public void SetCompensationLength(EnumMotorId motorId, float nailH, float heigh)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置通道{motorId}样品钉");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.SetSampleHeightCommand(channel, nailH, heigh);
            Device?.SendMsg(packet);
        }

        public void GetCompensationLength(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置通道{motorId}补偿臂长");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.GetSampleHeightCommand(channel);
            Device?.SendMsg(packet);
        }

        public void SetRelativeCoorInfo(EnumMotorId motorId, float length)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"设置通道{motorId}相对坐标参数");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.SetMotorPhlehCommand(channel, length);
            Device?.SendMsg(packet);
        }

        public void GetRelativeCoorInfo(EnumMotorId motorId)
        {
            NLog.LogManager.GetCurrentClassLogger().Info($"获取通道{motorId}相对坐标参数");
            byte channel = GetMotorChannel(motorId);
            var packet = MotorProtocol.GetMotorPhlehCommand(channel);
            Device?.SendMsg(packet);
        }
        #endregion

        #region ------------PrivateMethod------------
        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            foreach (var motor in _motorsList)
            {
                if (_isFirstRequest)
                {
                    motor.GetRelativeCoorInfo();
                }

                if (motor.Enable)
                {
                    GetMotorStatus(motor.ID);
                    Thread.Sleep(_interval / Motors.Count);
                }
            }

            _isFirstRequest = false;
        }

        private void Motor_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var timerEnable = _timer != null && _timer.Enabled;
            var motor = sender as MotorModel;
            if (motor != null)
            {
                switch (e.PropertyName)
                {
                    case "Enable":
                        {
                            if (motor.Enable)
                            {
                                if (!Motors.Contains(motor))
                                {
                                    if (timerEnable)
                                    {
                                        StopRequest();
                                    }
                                    Motors.Add(motor);
                                    if (timerEnable)
                                    {
                                        StartRequest(_interval);
                                    }
                                }
                            }
                            else
                            {
                                if (Motors.Contains(motor))
                                {
                                    if (timerEnable)
                                    {
                                        StopRequest();
                                    }
                                    Motors.Remove(motor);
                                    if (timerEnable)
                                    {
                                        StartRequest(_interval);
                                    }
                                }
                            }
                           
                        }
                        break;
                    case "Channel":
                        {
                            
                        }
                        break;
                    case "Polarity":
                        {
                           
                        }
                        break;
                }
            }
        }


        private void CheckMotorControlModel(EnumMotorId motorId, EnumMotorControlMode mode)
        {
            var motor = _motorsList.FirstOrDefault(item => item.ID == motorId);
            if (motor == null)
                return;

            if (motor.GetParam.ControlMode != mode)
            {
                SetMotorCtlMode(motorId, mode);
                motor.GetParam.StateMask &= 0x7F;
            }
        }

        private void CheckMotorActive(EnumMotorId motorId)
        {
            var motor = _motorsList.FirstOrDefault(item => item.ID == motorId);
            if (motor == null)
                return;

            if ((motor.GetParam.StateMask & 0x80) == 0)
            {
                SetMotorActiveEnable(motorId, true);
            }
        }

        private byte GetMotorChannel(EnumMotorId motorId)
        {
            var motor = _motorsList.FirstOrDefault(item => item.ID == motorId);
            if (motor == null)
                return 0;

            return (byte)motor.Channel;
        }

        private int GetMotorPolarity(EnumMotorId motorId)
        {
            var motor = _motorsList.FirstOrDefault(item => item.ID == motorId);
            if (motor == null)
                return 1;

            return motor.Polarity ? 1 : -1;
        }

       

        #endregion

        #region ------------StaticMethod------------
        #endregion

    }
}
