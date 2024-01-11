using System;
using System.ComponentModel;
using System.Windows.Documents;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Protocol;
using static OpenCvSharp.Stitcher;

namespace UtilityTools.Modules.Motor5Controller.Protocol
{
    /// <summary>
    /// 五轴电机的指令类型
    /// </summary>
    public enum EnumMotor5CmdType
    {
        [Description("电机使能状态")]
        W_MOTOR_ENABLE = 0x0000,

        [Description("设置电机控制方式")]
        W_MOTOR_CTLTYPE = 0x0010,

        [Description("设置电机运动模式")]
        W_MOTOR_RUNMODE = 0x0020,

        [Description("设置PID调节参数")]
        W_MOTOR_PID = 0x0030,

        [Description("获取PID调节参数")]
        R_MOTOR_PID = 0x0031,

        [Description("设置脉冲和实际距离的转换系数")]
        W_MOTOR_SUBRATIO = 0x0040,

        [Description("设置电机控制相关参数，补偿次数、停止阈值")]
        W_MOTOR_CTLPARAMS = 0x0050,

        [Description("读取电机控制相关参数，补偿次数、停止阈值")]
        R_MOTOR_CTLPARAMS = 0x0051,

        [Description("设置电机速度相关参数")]
        W_MOTOR_SPDPARAMS = 0x0060,

        [Description("读取电机速度相关参数")]
        R_MOTOR_SPDPARAMS = 0x0061,

        [Description("设置电机加速度相关参数")]
        W_MOTOR_ACCPARAMS = 0x0070,

        [Description("读取电机加速度相关参数")]
        R_MOTOR_ACCPARAMS = 0x0071,

        [Description("设置电机的运动参数")]
        W_MOTOR_TARPARAMS = 0x0080,

        [Description("设置电机全速运动")]
        W_MOTOR_ALLSPEED = 0x0081,

        [Description("读取电机的运动参数")]
        R_MOTOR_TARPARAMS = 0x0082,

        [Description("设置电机的零点位置")]
        W_MOTOR_ZEROPOS = 0x0083,

        [Description("读取电机的零点位置")]
        R_MOTOR_ZEROPOS = 0x0084,

        [Description("设置软件限位位置")]
        W_MOTOR_SOFTLIMITPOS = 0x0085,

        [Description("获取电机状态")]
        R_MOTOR_STATE = 0x0090,
    }

    /// <summary>
    /// 电机的控制类型
    /// </summary>
    public enum EnumMotorCtrType
    {
        [Description("开环控制")]
        OpenLoopCtr = 0x00,
        [Description("闭环控制")]
        CloseLoopCtr = 0x01,
    }

    /// <summary>
    /// 电机的工作模式
    /// </summary>
    public enum EnumMotorRunMode
    {
        [Description("绝对位置模式")]
        AbsolutePos = 0x01,
        [Description("相对位置模式")]
        RelativePos = 0x02,
        [Description("最大速度模式")]
        MaxSpeed = 0x03,
        [Description("用户速度模式")]
        CustomSpeed = 0x04
    }

    /// <summary>
    /// 电机的运动方向
    /// </summary>
    public enum EmumMotorMoveDirection
    {
        [Description("正向运动")]
        Forward,

        [Description("反向运动")]
        Backward,
    }

    /// <summary>
    /// 电机移动状态
    /// </summary>
    public enum EnumMotorMoveState
    {
        [Description("电机停止")]
        MotorStop,

        [Description("电机移动中")]
        MotorMove,
    }

    /// <summary>
    /// 电机类型
    /// </summary>
    public enum EnumMotorType
    {
        [Description("步进电机")]
        StepperMotorr,
        [Description("直流电机")]
        DcMotor,
    }

    /// <summary>
    /// 编码器方向
    /// </summary>
    public enum EnumEncoderDirecton
    {
        [Description("正向")]
        EncoderForward,

        [Description("反向")]
        EncoderBackward,
    }

    /// <summary>
    /// 电机限位状态
    /// </summary>
    public enum EnumMotorLimitedState
    {
        [Description("物理正向限位")]
        PhyForwardLimited,
        [Description("软件正向限位")]
        SoftForwardLimited,
        [Description("物理反向限位")]
        PhyBackwardLimited,
        [Description("软件反向限位")]
        SoftBackwardLimited,
        [Description("非限位状态")]
        None,
    }

    public enum EnumMotorId
    {
        [Description("主控板电机插槽位1处电机")]
        MOTOR_1 = 0x0001,

        [Description("主控板电机插槽位2处电机(Z轴直流电机)")]
        MOTOR_2 = 0x0002,

        [Description("主控板电机插槽位3处电机")]
        MOTOR_3 = 0x0003,

        [Description("主控板电机插槽位4处电机")]
        MOTOR_4 = 0x0004,

        [Description("主控板电机插槽位5处电机")]
        MOTOR_5 = 0x0005,

    }

    public class Motor5Protocol
    {
        /// <summary>
        /// 打开电机使能
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="enable">电机使能</param>
        /// <returns></returns>
        public static byte[] SetMotorEnable(EnumMotorId motorId, bool enable)
        {
            byte[] param = new byte[1];
            param[0] = enable ? (byte)0x01 : (byte)0x00;
            return GetCmd(EnumMotor5CmdType.W_MOTOR_ENABLE, motorId, param);
        }

        /// <summary>
        /// 设置电机的控制类型
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="ctrType">控制类型</param>
        /// <returns></returns>
        public static byte[] SetMotorCtrType(EnumMotorId motorId, EnumMotorCtrType ctrType) 
        {
            byte[] param = new byte[1];
            param[0] = (byte)ctrType;
            return GetCmd(EnumMotor5CmdType.W_MOTOR_CTLTYPE, motorId, param);
        }

        /// <summary>
        /// 设置电机的运行模式
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="mode">运行模式</param>
        /// <returns></returns>
        public static byte[] SetMotorRunMode(EnumMotorId motorId, EnumMotorRunMode mode)
        {
            byte[] param = new byte[1];
            param[0] = (byte)mode;
            return GetCmd(EnumMotor5CmdType.W_MOTOR_RUNMODE, motorId, param);
        }

        /// <summary>
        /// 设置电机的pid调节参数
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="p">p参数</param>
        /// <param name="i">i参数</param>
        /// <param name="d">d参数</param>
        /// <returns></returns>
        public static byte[] SetMotorPid(EnumMotorId motorId, float p, float i, float d)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(p);
            writer.Write(i);
            writer.Write(d);
            return GetCmd(EnumMotor5CmdType.W_MOTOR_PID, motorId, writer.EndWrite());
        }

        /// <summary>
        /// 获取电机PID参数
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public static byte[] GetMotorPid(EnumMotorId motorId) 
        {
            return GetSimpleReadCmd(motorId, EnumMotor5CmdType.R_MOTOR_PID);
        }

        /// <summary>
        /// 设置电机的换算比
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="ratio">换算比，单位为 pluse/μm</param>
        /// <returns></returns>
        public static byte[] SetMotorSubRatio(EnumMotorId motorId, float ratio)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(ratio);
            return GetCmd(EnumMotor5CmdType.W_MOTOR_SUBRATIO, motorId, writer.EndWrite());
        }

        /// <summary>
        /// 设置电机的控制参数
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="times">最大补偿次数</param>
        /// <param name="threshold">停止阈值，单位为pluse</param>
        /// <returns></returns>
        public static byte[] SetMotorCtrParams(EnumMotorId motorId, int times, int threshold) 
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(times);
            writer.Write(threshold);
            return GetCmd(EnumMotor5CmdType.W_MOTOR_CTLPARAMS, motorId, writer.EndWrite());
        }

        /// <summary>
        /// 获取电机的控制参数
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public static byte[] GetMotorCtrParams(EnumMotorId motorId) 
        {
            return GetSimpleReadCmd(motorId, EnumMotor5CmdType.R_MOTOR_CTLPARAMS);
        }

        /// <summary>
        /// 设置电机的速度参数
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="startSpeed">起始速度</param>
        /// <param name="maxSpeed">最大速度</param>
        /// <returns></returns>
        public static byte[] SetMotorSpeedParams(EnumMotorId motorId, int startSpeed, int maxSpeed)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(startSpeed);
            writer.Write(maxSpeed);
            return GetCmd(EnumMotor5CmdType.W_MOTOR_SPDPARAMS, motorId, writer.EndWrite());
        }

        /// <summary>
        /// 获取电机的速度参数
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public static byte[] GetMotorSpeedParams(EnumMotorId motorId)
        {
            return GetSimpleReadCmd(motorId, EnumMotor5CmdType.R_MOTOR_SPDPARAMS);
        }

        /// <summary>
        /// 设置电机的加速度参数
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="maxAcc">起始速度</param>
        /// <param name="maxDec">最大速度</param>
        /// <returns></returns>
        public static byte[] SetMotorAccParams(EnumMotorId motorId, int maxAcc, int maxDec)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(maxAcc);
            writer.Write(maxDec);
            return GetCmd(EnumMotor5CmdType.W_MOTOR_ACCPARAMS, motorId, writer.EndWrite());
        }

        /// <summary>
        /// 获取电机的加速度参数
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public static byte[] GetMotorAccParams(EnumMotorId motorId)
        {
            return GetSimpleReadCmd(motorId, EnumMotor5CmdType.R_MOTOR_ACCPARAMS);
        }

        /// <summary>
        /// 设置运动目标参数
        /// </summary>
        /// <param name="motor">电机编号</param>
        /// <param name="value">目标值</param>
        public static byte[] SetMotorTargetParam(EnumMotorId motorId, int value)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(value);
            return GetCmd(EnumMotor5CmdType.W_MOTOR_TARPARAMS, motorId, writer.EndWrite());
        }

        /// <summary>
        /// 设置电机全速运动
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="dir">运动方向</param>
        /// <returns></returns>
        public static byte[] SetMotorAllSpeed(EnumMotorId motorId, EmumMotorMoveDirection dir)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write((byte)dir);
            return GetCmd(EnumMotor5CmdType.W_MOTOR_ALLSPEED, motorId, writer.EndWrite());
        }

        /// <summary>
        /// 获取运动目标参数
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public static byte[] GetMotorTargetParam(EnumMotorId motorId)
        {
            return GetSimpleReadCmd(motorId, EnumMotor5CmdType.R_MOTOR_TARPARAMS);
        }

        /// <summary>
        /// 设置电机零点参数
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="origin">原点位置</param>
        /// <param name="zero">零点位置</param>
        /// <returns></returns>
        public static byte[] SetMotorZeroPos(EnumMotorId motorId, int origin, int zero) 
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(origin);
            writer.Write(zero);
            return GetCmd(EnumMotor5CmdType.W_MOTOR_ZEROPOS, motorId, writer.EndWrite());
        }

        /// <summary>
        /// 获取电机零点参数
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public static byte[] GetMotorZeroPos(EnumMotorId motorId)
        {
            return GetSimpleReadCmd(motorId, EnumMotor5CmdType.R_MOTOR_ZEROPOS);
        }

        /// <summary>
        /// 设置电机软限位位置
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="min">最小脉冲位置</param>
        /// <param name="max">最大脉冲位置</param>
        /// <returns></returns>
        public static byte[] SetMotorSoftLimitPos(EnumMotorId motorId, int min, int max)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(min);
            writer.Write(max);
            return GetCmd(EnumMotor5CmdType.W_MOTOR_SOFTLIMITPOS, motorId, writer.EndWrite());
        }

        /// <summary>
        /// 获取电机的状态参数
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public static byte[] GetMotorStateParams(EnumMotorId motorId) 
        {
            return GetSimpleReadCmd(motorId, EnumMotor5CmdType.R_MOTOR_STATE);
        }

        /// <summary>
        /// 通用无参指令生成方法
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="command">指令类型</param>
        /// <returns></returns>
        private static byte[] GetSimpleReadCmd(EnumMotorId motorId, EnumMotor5CmdType command)
        {
            return GetCmd(command, motorId, Array.Empty<byte>());
        }

        /// <summary>
        /// 指令生成方法
        /// </summary>
        /// <param name="command">指令类型</param>
        /// <param name="motorId">电机编号</param>
        /// <param name="data">指令参数</param>
        /// <returns></returns>
        private static byte[] GetCmd(EnumMotor5CmdType command, EnumMotorId motorId, byte[] data)
        {
            // 五轴电机的data段固定36字节，不足用零填充
            if (data.Length < 36)
            {
                var tmp = new byte[36];
                Array.Clear(tmp, 0, tmp.Length);

                Buffer.BlockCopy(data, 0, tmp, 0, data.Length);
                data = tmp;
            }

            var cmd = BitConverter.GetBytes((ushort)command);
            var id = BitConverter.GetBytes((ushort)motorId);
            return ZepGenericProtocol.GetCmd(id, cmd, data);
        }
    }

    public class Motor5DataPacket
    {
        public Motor5DataPacket(DataPacket packet)
        {
            this.packet = packet;
        }

        public EnumMotor5CmdType CmdType { get { return (EnumMotor5CmdType)BitConverter.ToUInt16(this.packet.command); } }
        public EnumMotorId MotorId { get { return (EnumMotorId)BitConverter.ToUInt16(this.packet.id); } }
        public byte[] DataSource { get => packet.data; }

        private DataPacket packet;
    }

    public class Motor5ProtocolParser
    {
        public Motor5ProtocolParser()
        {
            _parser = new ZepGenericProtocolParser();
            _parser.PacketReceivedEvent += GenericPacketReceived;
        }

        private void GenericPacketReceived(object sender, DataPacket packet)
        {
            PacketReceivedEvent(this, new Motor5DataPacket(packet));
        }

        public event EventHandler<Motor5DataPacket> PacketReceivedEvent;


        public void ReceiveBytes(byte[] data)
        {
            _parser.ReceiveBytes(data);
        }


        private ZepGenericProtocolParser _parser;
    }
}
