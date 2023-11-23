using System;
using System.ComponentModel;
using UtilityTools.Core.Protocol;

namespace UtilityTools.Modules.Motor5Controller.Protocol
{
    public enum EnumMotor5CmdType
    {
        [Description("电机使能")]
        W_MOTOR_ON = 0x0001,

        [Description("电机失能")]
        W_MOTOR_OFF = 0x0002,

        [Description("设置目标位置")]
        W_TARGET_POSITION = 0x0003,

        [Description("获取目标位置")]
        R_TARGET_POSITION = 0x0004,

        [Description("获取实时位置")]
        R_CURR_POSITION = 0x0005,

        [Description("获取原点位置")]
        R_ORIG_POSITION = 0x0006,

        [Description("设置原点位置")]
        W_ORIG_POSITION = 0x0007,

        [Description("获取零点位置")]
        R_ZERO_POSITION = 0x0008,

        [Description("设置零点位置")]
        W_ZERO_POSITION = 0x0009,

        [Description("回到原点位置")]
        W_GOTO_ORIGIN = 0x000a,

        [Description("回到零点位置")]
        W_GOTO_ZERO = 0x000b,

        [Description("电机急停（锁死）")]
        W_EMG_STOP = 0x000c,

        [Description("相对位移")]
        W_MOVE_REL = 0x000d,

        [Description("绝对位移")]
        W_MOVE_ABS = 0x000e
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

        public static byte[] SetMotorOnCmd(EnumMotorId motorId)
        {
            return GetCmd(EnumMotor5CmdType.W_MOTOR_ON, motorId, Array.Empty<byte>());
        }

        public static byte[] SetMotorOffCmd(EnumMotorId motorId)
        {
            return GetCmd(EnumMotor5CmdType.W_MOTOR_OFF, motorId, Array.Empty<byte>());
        }

        public static byte[] GoToZeroCmd(EnumMotorId motorId)
        {
            return GetCmd(EnumMotor5CmdType.W_GOTO_ZERO, motorId, Array.Empty<byte>());
        }

        public static byte[] GoToOriginCmd(EnumMotorId motorId)
        {
            return GetCmd(EnumMotor5CmdType.W_GOTO_ORIGIN, motorId, Array.Empty<byte>());
        }

        public static byte[] EmgStop(EnumMotorId motorId)
        {
            return GetCmd(EnumMotor5CmdType.W_EMG_STOP, motorId, Array.Empty<byte>());
        }

        public static byte[] SetTargetPosition(EnumMotorId motorId, int position)
        {
            var data = BitConverter.GetBytes(position);
            return GetCmd(EnumMotor5CmdType.W_TARGET_POSITION, motorId, data);
        }


        public static byte[] QueryTargetPositionCmd(EnumMotorId motorId)
        {
            return GetSimpleReadCmd(motorId, EnumMotor5CmdType.R_TARGET_POSITION);
        }

        public static byte[] QueryCurrPositionCmd(EnumMotorId motorId)
        {
            return GetSimpleReadCmd(motorId, EnumMotor5CmdType.R_CURR_POSITION);
        }

        public static byte[] QueryZeroPositionCmd(EnumMotorId motorId)
        {
            return GetSimpleReadCmd(motorId, EnumMotor5CmdType.R_ZERO_POSITION);
        }


        private static byte[] GetSimpleReadCmd(EnumMotorId motorId, EnumMotor5CmdType command)
        {
            return GetCmd(EnumMotor5CmdType.W_MOTOR_ON, motorId, Array.Empty<byte>());
        }


        private static byte[] GetCmd(EnumMotor5CmdType command, EnumMotorId motorId, byte[] data)
        {
            // 五轴电机的data段固定36字节，不足用零填充
            if (data.Length < 36)
            {
                var tmp = new byte[36];
                Array.Clear(tmp, 0, tmp.Length);

                Buffer.BlockCopy(tmp, 0, data, 0, data.Length);
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
