using OxyPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Protocol;
using static OpenCvSharp.Stitcher;

namespace UtilityTools.Modules.MotorTest.Protocol
{

    /// <summary>
    /// 五轴电机的指令类型
    /// </summary>
    public enum EnumSelfMotorCmdType
    {
        [Description("获取固件信息")]
        Get_Firmware_Information = 0x0004,
        [Description("获取硬件信息")]
        Get_Hardware_Information = 0x0005,
        [Description("设置电机使能状态")]
        CMD_SET_HR = 0x0100,

        [Description("设置电机运行状态")]
        CMD_SET_RS = 0x0101,

        [Description("设置电机绝对运动")]
        CMD_MOT_GOTO = 0x0102,

        [Description("设置电机相对运动")]
        CMD_MOT_MOVE = 0x0103,

        [Description("设置电机零点位置")]
        CMD_SET_ZERO = 0x0104,

        [Description("获取电机当前脉冲坐标（可修改）")]
        CMD_GET_POS = 0x0105,

        [Description("获取电机的状态参数")]
        CMD_GET_STATUS = 0x0106,

        [Description("获取电机的运行速度，单位脉冲")]
        CMD_GET_SPEED = 0x0107,

        [Description("保留项，设置电机的控制回环模式")]
        CMD_SET_LOOP = 0x0210,

        [Description("设计电机的闭环控制模式")]
        CMD_SET_MCTL = 0x0211,
        //保留软件负限位 软件零位 软件正限位 保留硬件负限位 硬件零位 硬件正限位
        [Description("设置电机的限位使能掩码")]
        CMD_SET_SLIM = 0x0212,

        [Description("设置电机PID调节的脉冲阈值")]
        CMD_SET_THQ = 0x0213,

        [Description("设置电机PID调节最大调节次数")]
        CMD_SET_THT = 0x0214,

        [Description("设置电机的点动脉冲阈值")]
        CMD_SET_THMICRO = 0x0215,

        [Description("设置电机的点动脉冲距离")]
        CMD_SET_MICROLEN = 0x0216,

        [Description("设置电机的PID参数")]
        CMD_SET_PID = 0x0217,

        [Description("设置电机软限位最大值")]
        CMD_SET_MAXSPOS = 0x0221,

        [Description("设置电机软限位最小值")]
        CMD_SET_MINSPOS = 0x0222,

        [Description("设置最大开环速度")]
        CMD_SET_MAXCLS = 0X0223,

        [Description("设置最小开环速度")]
        CMD_SET_MINCLS = 0X0224,


        [Description("获取电机当前限位掩码状态")]
        CMD_GET_SLIM = 0x0312,

        [Description("获取电机当前最大软限位脉冲值")]
        CMD_GET_MAXSPOS = 0x0321,
        [Description("获取电机当前最小软限位脉冲值")]
        CMD_GET_MINSPOS = 0x0322,
        [Description("获取电机当前最大速度")]
        CMD_GET_MAXCLS = 0x0323,
        [Description("获取电机闭环最小速度")]
        CMD_GET_MINCLS = 0x0324,
        [Description("设置轴类型（0x00位移轴；0x01旋转轴）")]
        CMD_SET_AXTYPE = 0x0510,

        [Description("设置轴的参数单位类型（0x00表示脉冲，0x01表示um，0x02表示弧度）")]
        CMD_SET_AXUNIT = 0x0512,

        [Description("设置电机的类型，（0x00表示直流电机；0x01表示步进电机）")]
        CMD_SET_MTYPE = 0x0514,

        [Description("设置电机的参数转化系数")]
        CMD_GET_AXCOEF = 0x0516,

        [Description("设置电机补偿时候的臂长")]
        CMD_SET_TLINK = 0x0516,

        [Description("获取轴类型（0x00位移轴0x01是旋转转轴）；")]
        CMD_GET_AXTYPE = 0x0611,

    }
    public enum EnumMotorAxisType
    {
        [Description("两轴电机")]
        TwoAxisMotor = 0X00,
        [Description("三轴电机")]
        ThreeAxisMotor = 0x01,
        [Description("五轴电机")]
        FiveAxisMotor = 0x02,
    }
    /// <summary>
    /// 电机的控制类型
    /// </summary>
    public enum EnumMotorCtrType
    {
        [Description("闭环位置")]
        CloseLoopPosCtr = 0x00,
        [Description("开环位置")]
        OpenLoopPosCtr = 0x01,
        [Description("闭环速度")]
        CloseLoopSpeedCtr = 0x02,
        [Description("开环速度")]
        OpenLoopSpeedCtr = 0x03,
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

        [Description("反向运动")]
        Backward = -1,
        [Description("停止")]
        Stop = 0,
        [Description("正向运动")]
        Forward = 1 ,

    }

    /// <summary>
    /// 电机运动类型
    /// </summary>
    public enum EnumMotorMoveType : ushort
    {
        [Description("位移轴")]
        Move  = 0x00,

        [Description("旋转轴")]
        MoRotationve = 0x01,
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
        
        [Description("直流电机")]
        DcMotor = 0x00 ,
        [Description("步进电机")]
        StepperMotorr = 0x01,
    }

    /// <summary>
    /// 绝对移动的数值单位
    /// </summary>
    public enum EnumMotorUnit
    {
        [Description("脉冲")]
        Pulse = 0x00,
        [Description("微米")]
        Um = 0x01,
        [Description("弧度")]
        Radian = 0x02,
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
    /// 电机的使能状态
    /// </summary>
    public enum EnumMotorEnable
    {
        [Description("失能")]
        DisableEnable = 0x00,

        [Description("使能")]
        Enable = 0x01,
    }
    /// <summary>
    /// 电机的运行状态
    /// </summary>
    public enum EnumMotorOperatingState
    {
        [Description("停止")]
        Stop = 0x00,

        [Description("运行")]
        Run = 0x01,
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
        MOTOR_1 = 0x01,

        [Description("主控板电机插槽位2处电机(Z轴直流电机)")]
        MOTOR_2 = 0x02,

        [Description("主控板电机插槽位3处电机")]
        MOTOR_3 = 0x03,

        [Description("主控板电机插槽位4处电机")]
        MOTOR_4 = 0x04,

        [Description("主控板电机插槽位5处电机")]
        MOTOR_5 = 0x05,

    }
    public enum EnumMotorModel
    {
        [Description("x轴电机")]
        MOTOR_x = 0x01,

        [Description("y轴电机")]
        MOTOR_y = 0x02,

        [Description("t轴电机")]
        MOTOR_z = 0x03,

        [Description("z轴电机")]
        MOTOR_t = 0x04,

        [Description("r轴电机")]
        MOTOR_r = 0x05,
        [Description("其他电机")]
        MOTOR_other = 0x09,

    }
    /// <summary>
    /// 电机问询的枚举
    /// </summary>
    public enum EnumMotorInquiry
    {
        InquiryMotor_x = 0x01,
        InquiryMotor_y = 0x02,
        InquiryMotor_z = 0x03,
        InquiryMotor_t = 0x04,
        InquiryMotor_r = 0x05,
        InquiryMotor_all = 0x06,
        InquiryMotor_null = 0x07
    }
    public class SelfMotorProtocol
    {
        private static byte[] DeviceID = BitConverter.GetBytes((short)0x0300);
        private static byte[] DeviceAddr = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        

        /// <summary>
        /// 指令生成方法
        /// </summary>
        /// <param name="command">指令类型</param>
        /// <param name="motorId">电机编号</param>
        /// <param name="data">指令参数</param>
        /// <returns></returns>
        private static byte[] GetCmd(EnumSelfMotorCmdType command, byte[] data)
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
            var id = DeviceID;
            return ZepGenericProtocol.GetCmd(id, cmd, data);
        }


        /// <summary>
        /// 获取固件信息
        /// </summary>
        /// <param name="motorId"></param>
        /// <returns></returns>
        public static byte[] GetFirmwareInformation()
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(0x00);
            return GetCmd(EnumSelfMotorCmdType.Get_Firmware_Information, writer.EndWrite());
        }
        /// <summary>
        /// 获取硬件信息
        /// </summary>
        /// <param name="motorId"></param>
        /// <returns></returns>
        public static byte[] GetHardwareInformation()
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write(0x00);
            return GetCmd(EnumSelfMotorCmdType.Get_Hardware_Information, writer.EndWrite());
        }
        /// <summary>
        /// 设置电机是否使能
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="enable">电机使能</param>
        /// <returns></returns>
        public static byte[] SetMotorEnable(EnumMotorId motorId, EnumMotorEnable enable)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write((byte)motorId);
            writer.Write((byte)enable);
            return GetCmd(EnumSelfMotorCmdType.CMD_SET_HR, writer.EndWrite());
        }
        

        /// <summary>
        /// 设置电机运行和停止
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public static byte[] SetMotorOperatingStatus(EnumMotorId motorId, EnumMotorOperatingState operatingStatus)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write((byte)motorId);
            writer.Write((byte)operatingStatus);
            return GetCmd(EnumSelfMotorCmdType.CMD_SET_RS, writer.EndWrite());
        }

     /// <summary>
     /// 设置电机绝对运动
     /// </summary>
     /// <param name="motorId">电机通道</param>
     /// <param name="unit">单位</param>
     /// <param name="distance">绝对移动数值</param>
     /// <returns></returns>
        public static byte[] SetMotorGoTo(EnumMotorId motorId, EnumMotorUnit unit ,float distance)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write((byte)motorId);
            writer.Write((byte)unit);
            writer.Write(distance);
            return GetCmd(EnumSelfMotorCmdType.CMD_MOT_GOTO,writer.EndWrite());
        }
        /// <summary>
        /// 设置电机零点
        /// </summary>
        /// <param name="motorId">电机通道</param>
        /// <returns></returns>
        public static byte[] SetMotorZero(EnumMotorId motorId)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write((byte)motorId);
            return GetCmd(EnumSelfMotorCmdType.CMD_SET_ZERO, writer.EndWrite());
        }

        /// <summary>
        /// 获取电机的位置返回结果的单位和设置值有关系
        /// </summary>
        /// <param name="motorId">电机通道</param>
        /// <returns></returns>
        public static byte[]  GetMotorPos(EnumMotorId motorId)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write((byte)motorId);
            return GetCmd(EnumSelfMotorCmdType.CMD_GET_POS, writer.EndWrite());
        }

        /// <summary>
        /// 获取电机的状态参数
        /// </summary>
        /// <param name="motorId">电机通道</param>
        /// <returns></returns>
        public static byte[] GetMotorStatus(EnumMotorId motorId)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write((byte)motorId);
            return GetCmd(EnumSelfMotorCmdType.CMD_GET_STATUS, writer.EndWrite());
        }

        /// <summary>
        /// 获取电机的运行速度，单位是脉冲/s
        /// </summary>
        /// <param name="motorId">电机通道</param>
        /// <returns></returns>
        public static byte[] GetMotorSpeed(EnumMotorId motorId)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write((byte)motorId);
            return GetCmd(EnumSelfMotorCmdType.CMD_GET_SPEED, writer.EndWrite());
        }

        /// <summary>
        /// 设置电机的闭环控制模式
        /// </summary>
        /// <param name="motorId"></param>
        /// <param name="controlMode"></param>
        /// <returns></returns>
        public static byte[] SetMotorControlMode(EnumMotorId motorId, EnumMotorCtrType controlMode)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write((byte)motorId);
            writer.Write((byte)controlMode);
            return GetCmd(EnumSelfMotorCmdType.CMD_SET_MCTL, writer.EndWrite());
        }

        /// <summary>
        /// 设置电机最小闭环速度
        /// </summary>
        /// <param name="motorId"></param>
        /// <param name="controlMode"></param>
        /// <returns></returns>
        public static byte[] SetMotorControlMinCls(EnumMotorId motorId, Int32 controlMode)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write((byte)motorId);
            writer.Write((byte)controlMode);
            return GetCmd(EnumSelfMotorCmdType.CMD_SET_MINCLS, writer.EndWrite());
        }

        /// <summary>
        /// 设置电机最大闭环速度
        /// </summary>
        /// <param name="motorId"></param>
        /// <param name="controlMode"></param>
        /// <returns></returns>
        public static byte[] SetMotorControlMaxCls(EnumMotorId motorId, Int32 controlMode)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write((byte)motorId);
            writer.Write((byte)controlMode);
            return GetCmd(EnumSelfMotorCmdType.CMD_SET_MAXCLS, writer.EndWrite());
        }

        /// <summary>
        /// 设置软件的限位使能掩码
        /// </summary>
        /// <param name="motorId">电机名称</param>
        /// <param name="controlMode"></param>
        /// <returns></returns>
        public static byte[] SetMotorLimitEnable(EnumMotorId motorId, byte limitEnableMask)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write((byte)motorId);
            writer.Write(limitEnableMask);
            return GetCmd(EnumSelfMotorCmdType.CMD_SET_SLIM, writer.EndWrite());
        }
        /// <summary>
        /// 获取当前限位状态掩码
        /// </summary>
        /// <param name="motorId"></param>
        /// <returns></returns>
        public static byte[] GetMotorLimitEnable(EnumMotorId motorId)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write((byte)motorId);
            return GetCmd(EnumSelfMotorCmdType.CMD_GET_SLIM, writer.EndWrite());
        }
        /// <summary>
        /// 设置轴类型
        /// </summary>
        /// <param name="motorId">电机名称</param>
        /// <param name="moveType"> </param>
        /// <returns></returns>
        public static byte[] SetAxType(EnumMotorId motorId, EnumMotorMoveType moveType)
        {
            ByteWriter writer = new ByteWriter(36);
            writer.Write((byte)motorId);
            writer.Write((byte)moveType);
            return GetCmd(EnumSelfMotorCmdType.CMD_SET_AXTYPE, writer.EndWrite());
        }
        /// <summary>
        /// 获取轴类型
        /// </summary>
        /// <param name="motorId"></param>
        /// <returns></returns>
        public static byte[] GetAxType(EnumMotorId motorId)
        {
            ByteWriter writer = new ByteWriter(36);
            return GetCmd(EnumSelfMotorCmdType.CMD_GET_AXTYPE, writer.EndWrite());
        }


    }

        public class SelfMotorPacket
        {
            public SelfMotorPacket(DataPacket packet)
            {
                this.packet = packet;
            }

            public EnumSelfMotorCmdType CmdType { get { return (EnumSelfMotorCmdType)BitConverter.ToUInt16(this.packet.command); } }
            public EnumMotorId MotorId { get { return (EnumMotorId)BitConverter.ToUInt16(this.packet.id); } }
            public byte[] DataSource { get => packet.data; }
            public byte[] Timestamp { get => packet.timestamp; }
            private DataPacket packet;
        }

        public class SelfMotorParser
        {
            public SelfMotorParser()
            {
                _parser = new ZepGenericProtocolParser();
                _parser.PacketReceivedEvent += GenericPacketReceived;
            }

            private void GenericPacketReceived(object sender, DataPacket packet)
            {
                PacketReceivedEvent(this, new SelfMotorPacket(packet));
            }

            public event EventHandler<SelfMotorPacket> PacketReceivedEvent;


            public void ReceiveBytes(byte[] data)
            {
                _parser.ReceiveBytes(data);
            }


            private ZepGenericProtocolParser _parser;
        }

}
