using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Model;
using UtilityTools.Core.Protocol;

namespace UtilityTools.Modules.NewMotor5Controller.Protocol
{

    /// <summary>
    /// 电机编号枚举
    /// </summary>
    public enum EnumMotorId
    {
        [Description("X轴")]
        MOTOR_X,
        [Description("Y轴")]
        MOTOR_Y,
        [Description("Z轴")]
        MOTOR_Z,
        [Description("T轴")]
        MOTOR_T,
        [Description("R轴")]
        MOTOR_R,
    }

    /// <summary>
    /// 电机轴类型
    /// </summary>
    public enum EnumMotorAxisType
    {
        [Description("位移轴")]
        Displacement,
        [Description("旋转轴")]
        Rotation,
    }

    /// <summary>
    /// 电机类型
    /// </summary>
    public enum EnumMotorType
    {
        [Description("直流电机")]
        DCMotor,
        [Description("步进电机")]
        StepperMotor,
    }

    /// <summary>
    /// 电机控制模式
    /// </summary>
    public enum EnumMotorControlMode
    {
        [Description("闭环位置")]
        CloseLoopPos,
        [Description("开环位置")]
        OpenLoopPos,
        [Description("闭环速度")]
        CloseLoopSpeed,
        [Description("开环速度")]
        OpenLoopSpeed,
    }

    /// <summary>
    /// 电机单位类型
    /// </summary>
    public enum EnumMotorUnitType
    {
        [Description("脉冲个数")]
        Pulse,
        [Description("距离单位μm")]
        Distance,
        [Description("角度单位弧度")]
        Angle,
    }

    /// <summary>
    /// 电机运动方向
    /// </summary>
    public enum EnumMotorDirection
    {
        [Description("正向")]
        Forward,
        [Description("反向")]
        Backward,
        [Description("静止")]
        Stop
    }

    /// <summary>
    /// 电机运行模式
    /// </summary>
    public enum EnumMotorWorkModes
    {
        [Description("位置模式")]
        PosWorkMode = 0,
        [Description("速度模式")]
        SpeedWorkMode = 1,
        [Description("未知模式")]
        None = 2,
    }
    /// <summary>
    /// 通用电机的指令类型
    /// </summary>
    public enum EnumCommonMotorCmdType : ushort
    {
        [Description("设置电机使能/失能")]
        CMD_SET_HR = 0x0100,

        [Description("设置电机运行/停止")]
        CMD_SET_RS = 0x0101,

        [Description("设置电机绝对运动")]
        CMD_MOT_GOTO = 0x0102,

        [Description("设置电机相对运动")]
        CMD_MOT_MOVE = 0x0103,

        [Description("设置电机零点位置")]
        CMD_SET_ZERO = 0x0104,

        [Description("获取电机当前的脉冲坐标")]
        CMD_GET_POS = 0x0105,

        [Description("获取电机的状态参数")]
        CMD_GET_STATUS = 0x0106,

        [Description("获取电机的运行速度，单位：脉冲/s")]
        CMD_GET_SPEED = 0x0107,

        [Description("保留项，设置电机的控制回环模式")]
        CMD_SET_LOOP = 0x0210,

        [Description("设置电机的闭环控制模式")]
        CMD_SET_MCTL = 0x0211,

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

        [Description("设置电机闭环最大速度")]
        CMD_SET_MAXCLS = 0x0223,

        [Description("设置电机闭环最小速度")]
        CMD_SET_MINCLS = 0x0224,

        [Description("保留项，获取电机的回环控制模式")]
        CMD_GET_LOOP = 0x0310,

        [Description("获取电机当前的控制模式")]
        CMD_GET_MCTL = 0x0311,

        [Description("获取电机当前限位掩码状态")]
        CMD_GET_SLIM = 0x0312,

        [Description("获取电机当前PID调节脉冲阈值")]
        CMD_GET_THQ = 0x0313,

        [Description("获取电机当前PID调节次数最大值")]
        CMD_GET_THT = 0x0314,

        [Description("获取电机点动阈值脉冲")]
        CMD_GET_THMICRO = 0x0315,

        [Description("获取电机点动脉冲值")]
        CMD_GET_MICROLEN = 0x0316,

        [Description("获取电机当前PID参数")]
        CMD_GET_PID = 0x0317,

        [Description("获取电机当前最大软限位脉冲值")]
        CMD_GET_MAXSPOS = 0x0321,

        [Description("获取电机当前最小软限位脉冲值")]
        CMD_GET_MINSPOS = 0x0322,

        [Description("获取电机当前最大速度")]
        CMD_GET_MAXCLS = 0x0323,

        [Description("获取电机闭环最小速度")]
        CMD_GET_MINCLS = 0x0324,

        [Description("设置轴类型")]
        CMD_SET_AXTYPE = 0x0510,

        [Description("设置轴的参数单位类型")]
        CMD_SET_AXUNIT = 0x0512,

        [Description("设置电机的类型")]
        CMD_SET_MTYPE = 0x0514,

        [Description("设置电机的参数转换系数")]
        CMD_SET_AXCOEF = 0x0516,

        [Description("获取轴类型")]
        CMD_GET_AXTYPE = 0x0611,

        [Description("获取轴的参数单位类型")]
        CMD_GET_AXUNIT = 0x0613,

        [Description("获取电机的类型")]
        CMD_GET_MTYPE = 0x0615,

        [Description("获取电机的参数转换系数")]
        CMD_GET_AXCOEF = 0x0617,

        [Description("设置电机补偿时候的臂长")]
        CMD_SET_TLINK = 0x0618,

        [Description("获取电机补偿时候的臂长")]
        CMD_GET_TLINK = 0x0619,

        [Description("设置样品钉高度和样品高度")]
        CMD_SET_SAMPLEH = 0x061A,

        [Description("获取样品钉高度和样品高度")]
        CMD_GET_SAMPLEH = 0x061B,

        [Description("设置转换坐标位置")]
        CMD_SET_PHLEH = 0x061C,

        [Description("获取转换坐标位置")]
        CMD_GET_PHLEH = 0x061D,
    }
    public enum ENUM_ZEPGEN_TYPE : ushort
    {
        // System Code
        CMD_SYS_GETHWV = 0x0004,
        CMD_SYS_GETFMV = 0x0005,
        CMD_SYS_BROADCASE = 0x0006,
    }
    public class MotorProtocol
    {
        public MotorProtocol()
        {
            
        }
        public byte[] DeviceID = new byte[] { 0xFF, 0xFF };
        public byte[] DeviceAddr = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        /// <summary>
        /// 指令生成方法
        /// </summary>
        /// <param name="command">指令类型</param>
        /// <param name="motorId">电机编号</param>
        /// <param name="data">指令参数</param>
        /// <returns></returns>
        private byte[] GetCmd(EnumCommonMotorCmdType command, byte[] data)
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
            return ZepGenericProtocol.GetCmd(DeviceID, cmd, data) ;
        }
        public static byte[] GetCmd(byte[] id, byte[] command, byte[] data)
        {
            var packet = new DataPacket(command, data);
            packet.id = id;
            return packet.GetBytes();
        }

        public static byte[] GetCmd(byte[] addr, byte[] id, byte[] command, byte[] data)
        {
            var packet = new DataPacket(command, data);
            packet.addr = addr;
            packet.id = id;
            return packet.GetBytes();
        }

        /// <summary>
        /// 获取电机类型信息对应的发送值
        /// </summary>
        /// <param name="type">电机运动单位信息</param>
        /// <returns></returns>
        public byte GetMotorTypeInfo(EnumMotorType type)
        {
            switch (type)
            {
                case EnumMotorType.StepperMotor:
                    return (byte)0x01;
                default:
                    return (byte)0x00;
            }
        }

        /// <summary>
        /// 将数值信息换成对应的电机类型
        /// </summary>
        /// <param name="motorId">数值信息</param>
        /// <returns></returns>
        public EnumMotorType GetMotorType(byte value)
        {
            switch (value)
            {
                case 0x01:
                    return EnumMotorType.StepperMotor;
                default:
                    return EnumMotorType.DCMotor;
            }
        }

        /// <summary>
        /// 获取电机运动单位信息对应的发送值
        /// </summary>
        /// <param name="type">电机运动单位信息</param>
        /// <returns></returns>
        public byte GetMotorUnitInfo(EnumMotorUnitType type)
        {
            switch (type)
            {
                case EnumMotorUnitType.Distance:
                    return (byte)0x01;
                case EnumMotorUnitType.Angle:
                    return (byte)0x02;
                default:
                    return (byte)0x00;
            }
        }

        /// <summary>
        /// 将数值信息换成对应的电机运动单位
        /// </summary>
        /// <param name="motorId">数值信息</param>
        /// <returns></returns>
        public EnumMotorUnitType GetMotorUnit(byte value)
        {
            switch (value)
            {
                case 0x01:
                    return EnumMotorUnitType.Distance;
                case 0x02:
                    return EnumMotorUnitType.Angle;
                default:
                    return EnumMotorUnitType.Pulse;
            }
        }

        /// <summary>
        /// 获取电机闭环模式对应的发送值
        /// </summary>
        /// <param name="mode">电机闭环模式</param>
        /// <returns></returns>
        public byte GetMotorControlModeInfo(EnumMotorControlMode mode)
        {
            switch (mode)
            {
                case EnumMotorControlMode.OpenLoopPos:
                    return (byte)0x01;
                case EnumMotorControlMode.CloseLoopSpeed:
                    return (byte)0x02;
                case EnumMotorControlMode.OpenLoopSpeed:
                    return (byte)0x03;
                default:
                    return (byte)0x00;
            }
        }

        /// <summary>
        /// 将数值信息换成对应的电机闭环模式
        /// </summary>
        /// <param name="motorId">数值信息</param>
        /// <returns></returns>
        public EnumMotorControlMode GetMotorControlMode(byte value)
        {
            switch (value)
            {
                case 0x01:
                    return EnumMotorControlMode.OpenLoopPos;
                case 0x02:
                    return EnumMotorControlMode.CloseLoopSpeed;
                case 0x03:
                    return EnumMotorControlMode.OpenLoopSpeed;
                default:
                    return EnumMotorControlMode.CloseLoopPos;
            }
        }

        /// <summary>
        /// 获取电机轴类型对应的发送数值
        /// </summary>
        /// <param name="type">电机轴类型</param>
        /// <returns></returns>
        public byte GetMotorAxisTypeInfo(EnumMotorAxisType type)
        {
            switch (type)
            {
                case EnumMotorAxisType.Rotation:
                    return (byte)0x01;
                default:
                    return (byte)0x00;
            }
        }

        /// <summary>
        /// 将数值信息换成对应的电机轴类型
        /// </summary>
        /// <param name="motorId">数值信息</param>
        /// <returns></returns>
        public EnumMotorAxisType GetMotorAxisType(byte value)
        {
            switch (value)
            {
                case 0x01:
                    return EnumMotorAxisType.Rotation;
                default:
                    return EnumMotorAxisType.Displacement;
            }
        }

        /// <summary>
        /// 将数值信息换成对应的电机的运行方向
        /// </summary>
        /// <param name="motorId">数值信息</param>
        /// <returns></returns>
        public EnumMotorDirection GetMotorDirection(byte value)
        {
            switch (value)
            {
                case 0xFF:
                    return EnumMotorDirection.Backward;
                case 0x01:
                    return EnumMotorDirection.Forward;
                default:
                    return EnumMotorDirection.Stop;
            }
        }

        /// <summary>
        /// 设置电机激活使能
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="enable">电机使能</param>
        /// <returns></returns>
        public byte[] SetMotorActiveCommand(byte channel, bool enable)
        {
            byte[] param = new byte[2];
            param[0] = channel;
            param[1] = enable ? (byte)0x01 : (byte)0x00;
            return GetCmd(EnumCommonMotorCmdType.CMD_SET_HR, param);
        }

        /// <summary>
        /// 设置电机运行状态
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="enable">电机运行状态，true表示运行；false表示停止</param>
        /// <returns></returns>
        public byte[] SetMotorRunCommand(byte channel, bool enable)
        {
            byte[] param = new byte[2];
            param[0] = channel;
            param[1] = enable ? (byte)0x01 : (byte)0x00;
            return GetCmd(EnumCommonMotorCmdType.CMD_SET_RS, param);
        }

        /// <summary>
        /// 设置电机绝对运动
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="type">电机运动单位类型</param>
        /// <param name="type">电机运动量</param>
        /// <returns></returns>
        public byte[] SetMotorGotoCommand(byte channel, EnumMotorUnitType type, float value, float obValue = 0.0f)
        {
            byte[] param = new byte[10];
            param[0] = channel;
            param[1] = GetMotorUnitInfo(type);
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, param, 2, bytes.Length);
            bytes = BitConverter.GetBytes(obValue);
            Array.Copy(bytes, 0, param, 6, bytes.Length);
            return GetCmd(EnumCommonMotorCmdType.CMD_MOT_GOTO, param);
        }

        /// <summary>
        /// 设置电机相对运动
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="type">电机运动单位类型</param>
        /// <param name="type">电机运动量</param>
        /// <returns></returns>
        public byte[] SetMotorMoveCommand(byte channel, EnumMotorUnitType type, float value)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            param[1] = GetMotorUnitInfo(type);
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, param, 2, bytes.Length);
            return GetCmd(EnumCommonMotorCmdType.CMD_MOT_MOVE, param);
        }

        /// <summary>
        /// 设置电机零点位置
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public byte[] SetMotorZeroCommand(byte channel)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            return GetCmd(EnumCommonMotorCmdType.CMD_SET_ZERO, param);
        }

        /// <summary>
        /// 获取电机当前的坐标
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public byte[] GetMotorPosCommand(byte channel)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            return GetCmd(EnumCommonMotorCmdType.CMD_GET_POS, param);
        }

        /// <summary>
        /// 获取电机的状态参数
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public byte[] GetMotorStatusCommand(byte channel)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            return GetCmd(EnumCommonMotorCmdType.CMD_GET_STATUS, param);
        }

        /// <summary>
        /// 获取电机的速度
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public byte[] GetMotorSpeedCommand(byte channel)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            return GetCmd(EnumCommonMotorCmdType.CMD_GET_SPEED, param);
        }

        /// <summary>
        /// 设置电机的闭环控制模式
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="mode">闭环控制模式</param>
        /// <returns></returns>
        public byte[] SetMotorLoopCommand(byte channel, EnumMotorControlMode mode)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            param[1] = GetMotorControlModeInfo(mode);
            return GetCmd(EnumCommonMotorCmdType.CMD_SET_MCTL, param);
        }

        /// <summary>
        /// 设置电机的限位使能掩码
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="mask">限位使能掩码</param>
        /// <returns></returns>
        public byte[] SetMotorLimtMaskCommand(byte channel, byte mask)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            param[1] = mask;
            return GetCmd(EnumCommonMotorCmdType.CMD_SET_SLIM, param);
        }

        /// <summary>
        /// 设置电机PID调节的脉冲阈值
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="type">单位类型</param>
        /// <param name="value">脉冲阈值</param>
        /// <returns></returns>
        public byte[] SetMotorPidThrCommand(byte channel, EnumMotorUnitType type, int value)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            param[1] = GetMotorUnitInfo(type);
            var bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, param, 2, bytes.Length);
            return GetCmd(EnumCommonMotorCmdType.CMD_SET_THQ, param);
        }

        /// <summary>
        /// 设置电机PID调节的最大次数
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="type">单位类型</param>
        /// <param name="value">最大次数</param>
        /// <returns></returns>
        public byte[] SetMotorPidMaxCountCommand(byte channel, EnumMotorUnitType type, int value)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            param[1] = GetMotorUnitInfo(type);
            var bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, param, 2, bytes.Length);
            return GetCmd(EnumCommonMotorCmdType.CMD_SET_THT, param);
        }

        /// <summary>
        /// 设置电机的点动脉冲阈值
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="type">单位类型</param>
        /// <param name="value">脉冲阈值</param>
        /// <returns></returns>
        public byte[] SetMotorMicroThrCommand(byte channel, EnumMotorUnitType type, int value)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            param[1] = GetMotorUnitInfo(type);
            var bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, param, 2, bytes.Length);
            return GetCmd(EnumCommonMotorCmdType.CMD_SET_THMICRO, param);
        }

        /// <summary>
        /// 设置电机的点动脉冲距离
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="type">单位类型</param>
        /// <param name="value">脉冲距离</param>
        /// <returns></returns>
        public byte[] SetMotorMicroLenCommand(byte channel, EnumMotorUnitType type, int value)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            param[1] = GetMotorUnitInfo(type);
            var bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, param, 2, bytes.Length);
            return GetCmd(EnumCommonMotorCmdType.CMD_SET_MICROLEN, param);
        }

        /// <summary>
        /// 设置电机的PID参数
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="pid">PID参数</param>
        /// <returns></returns>
        public byte[] SetMotorPidCommand(byte channel, PidModel pid)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            var bytes = BitConverter.GetBytes(pid.Kp);
            Array.Copy(bytes, 0, param, 1, bytes.Length);
            bytes = BitConverter.GetBytes(pid.Ki);
            Array.Copy(bytes, 0, param, 5, bytes.Length);
            bytes = BitConverter.GetBytes(pid.Kd);
            Array.Copy(bytes, 0, param, 9, bytes.Length);
            return GetCmd(EnumCommonMotorCmdType.CMD_SET_PID, param);
        }

        /// <summary>
        /// 设置电机的最大软限位
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="type">单位类型</param>
        /// <param name="value">最大软限位</param>
        /// <returns></returns>
        public byte[] SetMotorMaxLimCommand(byte channel, EnumMotorUnitType type, int value)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            param[1] = GetMotorUnitInfo(type);
            var bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, param, 2, bytes.Length);
            return GetCmd(EnumCommonMotorCmdType.CMD_SET_MAXSPOS, param);
        }

        /// <summary>
        /// 设置电机的最小软限位
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="type">单位类型</param>
        /// <param name="value">最小软限位</param>
        /// <returns></returns>
        public byte[] SetMotorMinLimCommand(byte channel, EnumMotorUnitType type, int value)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            param[1] = GetMotorUnitInfo(type);
            var bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, param, 2, bytes.Length);
            return GetCmd(EnumCommonMotorCmdType.CMD_SET_MINSPOS, param);
        }


        /// <summary>
        /// 设置电机闭环最大速度
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="type">单位类型</param>
        /// <param name="value">最大速度</param>
        /// <returns></returns>
        public byte[] SetMotorMaxSpeedCommand(byte channel, EnumMotorUnitType type, int value)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            param[1] = GetMotorUnitInfo(type);
            var bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, param, 2, bytes.Length);
            return GetCmd(EnumCommonMotorCmdType.CMD_SET_MAXCLS, param);
        }

        /// <summary>
        /// 设置电机闭环最小速度
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="type">单位类型</param>
        /// <param name="value">最小速度</param>
        /// <returns></returns>
        public byte[] SetMotorMinSpeedCommand(byte channel, EnumMotorUnitType type, int value)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            param[1] = GetMotorUnitInfo(type);
            var bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, param, 2, bytes.Length);
            return GetCmd(EnumCommonMotorCmdType.CMD_SET_MINCLS, param);
        }


        /// <summary>
        /// 获取电机回环控制模式
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public byte[] GetMotorLoopCommand(byte channel)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            return GetCmd(EnumCommonMotorCmdType.CMD_GET_LOOP, param);
        }

        /// <summary>
        /// 获取电机当前的控制模式
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public byte[] GetMotorControlModeCommand(byte channel)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            return GetCmd(EnumCommonMotorCmdType.CMD_GET_MCTL, param);
        }

        /// <summary>
        /// 获取电机当前限位掩码状态
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public byte[] GetMotorLimMaskCommand(byte channel)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            return GetCmd(EnumCommonMotorCmdType.CMD_GET_SLIM, param);
        }

        /// <summary>
        /// 获取电机当前PID调节脉冲阈值
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public byte[] GetMotorPidThrCommand(byte channel)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            return GetCmd(EnumCommonMotorCmdType.CMD_GET_THQ, param);
        }

        /// <summary>
        /// 获取电机当前PID调节最大次数
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public byte[] GetMotorPidMaxCountCommand(byte channel)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            return GetCmd(EnumCommonMotorCmdType.CMD_GET_THT, param);
        }

        /// <summary>
        /// 获取电机点动阈值脉冲
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public byte[] GetMotorMicroThrCountCommand(byte channel)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            return GetCmd(EnumCommonMotorCmdType.CMD_GET_THMICRO, param);
        }

        /// <summary>
        /// 获取电机点动距离
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public byte[] GetMotorMicroLenCommand(byte channel)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            return GetCmd(EnumCommonMotorCmdType.CMD_GET_MICROLEN, param);
        }

        /// <summary>
        /// 获取电机的PID参数
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public byte[] GetMotorPidCommand(byte channel)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            return GetCmd(EnumCommonMotorCmdType.CMD_GET_PID, param);
        }

        /// <summary>
        /// 获取电机的最大软限位
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public byte[] GetMotorMaxPosCommand(byte channel)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            return GetCmd(EnumCommonMotorCmdType.CMD_GET_MAXSPOS, param);
        }

        /// <summary>
        /// 获取电机的最小软限位
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public byte[] GetMotorMinPosCommand(byte channel)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            return GetCmd(EnumCommonMotorCmdType.CMD_GET_MINSPOS, param);
        }


        /// <summary>
        /// 获取电机的最大速度
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public byte[] GetMotorMaxSpeedCommand(byte channel)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            return GetCmd(EnumCommonMotorCmdType.CMD_GET_MAXCLS, param);
        }

        /// <summary>
        /// 获取电机的最小速度
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public byte[] GetMotorMinSpeedCommand(byte channel)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            return GetCmd(EnumCommonMotorCmdType.CMD_GET_MINCLS, param);
        }


        /// <summary>
        /// 设置电机的轴类型
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="type">轴类型</param>
        /// <returns></returns>
        public byte[] SetMotorAxisTypeCommand(byte channel, EnumMotorAxisType type)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            param[1] = GetMotorAxisTypeInfo(type);
            return GetCmd(EnumCommonMotorCmdType.CMD_SET_AXTYPE, param);
        }


        /// <summary>
        /// 设置电机的轴参数单位类型
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="type">参数单位类型</param>
        /// <returns></returns>
        public byte[] SetMotorAxisUnitCommand(byte channel, EnumMotorUnitType type)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            param[1] = GetMotorUnitInfo(type);
            return GetCmd(EnumCommonMotorCmdType.CMD_SET_AXUNIT, param);
        }

        /// <summary>
        /// 设置电机的类型
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="type">电机类型</param>
        /// <returns></returns>
        public byte[] SetMotorTypeCommand(byte channel, EnumMotorType type)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            param[1] = GetMotorTypeInfo(type);
            return GetCmd(EnumCommonMotorCmdType.CMD_SET_MTYPE, param);
        }


        /// <summary>
        /// 设置电机的参数转换系数
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="value">参数转换系数</param>
        /// <returns></returns>
        public byte[] SetMotorAxisCoefCommand(byte channel, float value)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            var bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, param, 1, bytes.Length);
            return GetCmd(EnumCommonMotorCmdType.CMD_SET_AXCOEF, param);
        }


        /// <summary>
        /// 获取电机的轴类型
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public byte[] GetMotorAxisTypeCommand(byte channel)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            return GetCmd(EnumCommonMotorCmdType.CMD_GET_AXTYPE, param);
        }


        /// <summary>
        /// 获取电机的轴参数单位类型
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <param name="type">参数单位类型</param>
        /// <returns></returns>
        public byte[] GetMotorAxisUnitCommand(byte channel)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            return GetCmd(EnumCommonMotorCmdType.CMD_GET_AXUNIT, param);
        }

        /// <summary>
        /// 获取电机的类型
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public byte[] GetMotorTypeCommand(byte channel)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            return GetCmd(EnumCommonMotorCmdType.CMD_GET_MTYPE, param);
        }


        /// <summary>
        /// 获取电机的参数转换系数
        /// </summary>
        /// <param name="motorId">电机编号</param>
        /// <returns></returns>
        public byte[] GetMotorAxisCoefCommand(byte channel)
        {
            byte[] param = new byte[6];
            param[0] = channel;
            return GetCmd(EnumCommonMotorCmdType.CMD_GET_AXCOEF, param);
        }

        /// <summary>
        /// 设置电机补偿时候的臂长
        /// </summary>
        /// <param name="channel">电机编号</param>
        /// <param name="length">补偿臂长，单位mm</param>
        /// <returns></returns>
        public byte[] SetMotorTLinkCommand(byte channel, float length)
        {
            byte[] param = new byte[10];
            param[0] = channel;
            param[1] = 0x01;
            var bytes = BitConverter.GetBytes(length * 1000);
            Array.Copy(bytes, 0, param, 2, bytes.Length);
            return GetCmd(EnumCommonMotorCmdType.CMD_SET_TLINK, param);
        }

        /// <summary>
        /// 设置电机补偿时候的臂长
        /// </summary>
        /// <param name="channel">电机编号</param>
        /// <returns></returns>
        public byte[] GetMotorTLinkCommand(byte channel)
        {
            byte[] param = new byte[10];
            param[0] = channel;
            return GetCmd(EnumCommonMotorCmdType.CMD_GET_TLINK, param);
        }

        /// <summary>
        /// 设置样品钉高度和样品高度
        /// </summary>
        /// <param name="channel">电机编号</param>
        /// <param name="nailH">样品钉高度，单位mm</param>
        /// <param name="height">样品高度，单位mm</param>
        /// <returns></returns>
        public byte[] SetSampleHeightCommand(byte channel, float nailH, float height)
        {
            byte[] param = new byte[14];
            param[0] = channel;
            param[1] = 0x01;
            var bytes = BitConverter.GetBytes(nailH * 1000);
            Array.Copy(bytes, 0, param, 2, bytes.Length);
            bytes = BitConverter.GetBytes(height * 1000);
            Array.Copy(bytes, 0, param, 6, bytes.Length);
            return GetCmd(EnumCommonMotorCmdType.CMD_SET_SAMPLEH, param);
        }

        /// <summary>
        /// 获取样品钉高度和样品高度
        /// </summary>
        /// <param name="channel">电机编号</param>
        /// <returns></returns>
        public byte[] GetSampleHeightCommand(byte channel)
        {
            byte[] param = new byte[10];
            param[0] = channel;
            return GetCmd(EnumCommonMotorCmdType.CMD_GET_SAMPLEH, param);
        }

        /// <summary>
        /// 设置电机相对坐标参数指令
        /// </summary>
        /// <param name="channel"><电机编号/param>
        /// <param name="length">相对距离，单位mm</param>
        /// <param name="dir">方向极性</param>
        /// <returns></returns>
        public byte[] SetMotorPhlehCommand(byte channel, float length)
        {
            byte[] param = new byte[10];
            param[0] = channel;
            param[1] = 0x01;
            var bytes = BitConverter.GetBytes(length * 1000);
            Array.Copy(bytes, 0, param, 2, bytes.Length);
            return GetCmd(EnumCommonMotorCmdType.CMD_SET_PHLEH, param);
        }

        /// <summary>
        /// 获取电机相对坐标参数指令
        /// </summary>
        /// <param name="channel">电机编号</param>
        /// <returns></returns>
        public byte[] GetMotorPhlehCommand(byte channel)
        {
            byte[] param = new byte[10];
            param[0] = channel;
            return GetCmd(EnumCommonMotorCmdType.CMD_GET_PHLEH, param);
        }
        /// <summary>
        /// 获取硬件版本号
        /// </summary>
        /// <returns></returns>
        public byte[] GetHardwareVersion()
        {
            byte[] cmd = BitConverter.GetBytes((ushort)ENUM_ZEPGEN_TYPE.CMD_SYS_GETHWV);
            byte[] data = new byte[5];
            return GetCmd(DeviceAddr, DeviceID, cmd, data);
        }
        /// <summary>
        /// 获取固件版本号
        /// </summary>
        /// <returns></returns>
        public byte[] GetFirmwareVersion()
        {
            byte[] cmd = BitConverter.GetBytes((ushort)ENUM_ZEPGEN_TYPE.CMD_SYS_GETFMV);
            byte[] data = new byte[5];
            return GetCmd(DeviceAddr, DeviceID, cmd, data);
        }
    }
    public class NewMotor5ControllerrPacket
    {
        public NewMotor5ControllerrPacket(DataPacket packet)
        {
            this.packet = packet;
        }

        public EnumCommonMotorCmdType CmdType { get { return (EnumCommonMotorCmdType)BitConverter.ToUInt16(this.packet.command); } }
        public EnumMotorId MotorId { get { return (EnumMotorId)BitConverter.ToUInt16(this.packet.id); } }
        public byte[] DataSource { get => packet.data; }

        private DataPacket packet;
    }

    public class NewMotor5ControllerParser
    {
        public NewMotor5ControllerParser()
        {
            _parser = new ZepGenericProtocolParser();
            _parser.PacketReceivedEvent += GenericPacketReceived;
        }

        private void GenericPacketReceived(object sender, DataPacket packet)
        {
            PacketReceivedEvent(this, packet);
        }

        public event EventHandler<DataPacket> PacketReceivedEvent;


        public void ReceiveBytes(byte[] data)
        {
            _parser.ReceiveBytes(data);
        }


        private ZepGenericProtocolParser _parser;
    }

}
