using NLog;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Modules.IonPump.Protocol
{


    public enum EnumCommand
    {
        [Description("获取产品编号")]
        MODEL_NUMBER = 0x01,

        [Description("获取版本信息")]
        VERSION = 0x02,

        MASTER_RESET = 0x07,
        SET_ARC_DETECT = 0x91,
        GET_ARC_DETECT = 0x92,

        [Description("获取电流")]
        READ_CURRENT = 0x0a,

        [Description("获取压强")]
        READ_PRESSURE = 0x0b,

        [Description("获取电压")]
        READ_VOLTAGE = 0x0c,

        [Description("获取电源转态")]
        GET_SUPPLY_STATUS = 0x0d,

        [Description("设置压强单位")]
        SET_PRESS_UNITS = 0x0e,

        GET_PUMP_SIZE = 0x11,
        SET_PUMP_SIZE = 0x12,
        GET_CAL_FACTOR = 0x1d,
        SET_CAL_FACTOR = 0x1e,
        SET_AUTO_RESTART = 0x33,
        GET_AUTO_RESTART = 0x34,

        [Description("启动泵")]
        START_PUMP = 0x37,

        [Description("停止泵")]
        STOP_PUMP = 0x38,

        GET_SETPOINT = 0x3c,
        SET_SETPOINT = 0x3d,
        LOCK_KEYPAD = 0x44,
        UNLOCK_KEYPAD = 0x45,
        GET_ANALOG_MODE = 0x50,
        SET_ANALOG_MODE = 0x51,
        IS_HIGH_VOLTAGE_ON = 0x61,
        SET_SERIAL_ADDRESS = 0x62,
        SET_HV_AUTORECOVERY = 0x68,
        GET_HV_AUTORECOVERY = 0x69,
        SET_FIRMWARE_UPDATE = 0x8f,
        SET_COMM_MODE = 0xd3,
        GET_COMMON_MODE = 0xd4,
        GET_SET_SERIAL_COMM = 0x46,
        GET_SET_ETHERNET_IP = 0x47,
        GET_SET_ETHERNET_MASK = 0x48,
        GET_SET_ETHERNET_GTWY = 0x49,
        GET_ETHERNET_MAC = 0x4a,
        SET_COMM_INTERFACE = 0x4b,
        INITIATE_FEA = 0x4c,
        GET_FEA_DATA = 0x4d,
        INITIATE_HIPOT = 0x52,
        GET_SET_HIPOT_TARGET = 0x53,
        GET_SET_FOLDBACK_VOLTS = 0x54,
        GET_SET_FOLDBACK_PRES = 0x55
    }

    public enum EnumCommInterface
    {
        RS232 = 0,
        RS422 = 1,
        RS485 = 2,
        RS485_FD = 3,
        ETHERNET = 4,
        USB = 5
    }

    public enum EnumGammaPressureUnit
    {
        [Description("Torr")]
        T = 0,

        [Description("Mbar")]
        M = 1,

        [Description("Pascal")]
        P = 2,
    }



    public class CommandPacketBuilder
    {
        public static readonly char START = '~';
        public static readonly char TERMINATOR = '\r';

        public CommandPacketBuilder()
        {
            this.addr = 5;
        }

        public CommandPacket GetCmdSysModel()
        {
            return GetReadCmd(EnumCommand.MODEL_NUMBER);
        }

        public CommandPacket GetCmdSysVersion()
        {
            return GetReadCmd(EnumCommand.VERSION);
        }

        public CommandPacket GetCmdHvReadCurrent()
        {
            return GetReadCmd(EnumCommand.READ_CURRENT);
        }

        public CommandPacket GetCmdHvReadPressure()
        {
            return GetReadCmd(EnumCommand.READ_PRESSURE);
        }


        public CommandPacket GetCmdHvReadVoltage()
        {
            return GetReadCmd(EnumCommand.READ_VOLTAGE);
        }

        public CommandPacket GetReadCmd(EnumCommand cmd)
        {
            var packet = new CommandPacket();
            packet.cmd = cmd;
            packet.address = string.Format("{0:X2}", this.addr);
            packet.command = string.Format("{0:X2}", (int)cmd);
            return packet;
        }


        private int addr;
    }

    public class CommandPacket
    {
        public EnumCommand cmd;
        public string address; // 2
        public string command; // 2
        public string data;

        public byte[] GetBytes()
        {
            string cmd_str = $"{CommandPacketBuilder.START} {address} {command} ";
            if (!string.IsNullOrEmpty(data))
            {
                cmd_str += $"{data} ";
            }
            string crc = GammaIonPumpProtocol.CalcCrc(cmd_str, 1);
            cmd_str += $"{crc}{CommandPacketBuilder.TERMINATOR}";
            return Encoding.ASCII.GetBytes(cmd_str);
        }
    }


    public class ResponsePacket
    {
        public static readonly string STATUS_OK = "OK";
        public static readonly string STATUS_ERROR = "ER";

        public byte[] bytes;

        public string Status; // 2 OK/ER
        public string ResponseCode; // 2
        public string Data;
        public bool CrcValid;

        public static ResponsePacket ParseFromBytes(byte[] bytes)
        {
            var resp_str = Encoding.ASCII.GetString(bytes);
            var parts = resp_str.Trim().Split();
            var crc = GammaIonPumpProtocol.CalcCrc(resp_str, 0, resp_str.Length - 3);
            string data = "";
            if (5 <= parts.Length)
            {
                data = string.Join(' ', parts[3..^1]);
            }
            var packet = new ResponsePacket
            {
                bytes = bytes,
                Status = parts[1],
                ResponseCode = parts[2],
                Data = data,
                CrcValid = crc == parts[^2],
            };
            return packet;
        }
    }


    public class ResponsePacketParser
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        public ResponsePacketParser(int BufferSize = 256)
        {
            _buffer = new byte[BufferSize];
            _idx = 0;
            _buffer_size = BufferSize;
            addr = 5;
        }

        public event EventHandler<ResponsePacket> PacketReceivedEvent;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ReceiveBytes(byte[] data)
        {
            if (_buffer.Length < _idx + data.Length)
            {
                LOGGER.Debug($"解析缓存不足, 扩容 {_buffer.Length} -> {_idx + data.Length}");
                byte[] _tmp = new byte[_idx + data.Length];
                Buffer.BlockCopy(_buffer, 0, _tmp, 0, _buffer.Length);
                _buffer = _tmp;
            }
            Buffer.BlockCopy(data, 0, _buffer, _idx, data.Length);
            _idx += data.Length;

            int st = 0;
            int header_pos;
            while ((header_pos = FindHeaderPos(st)) >= 0)
            {
                int pos = FindCharPos(CommandPacketBuilder.TERMINATOR, header_pos + 1);
                if (0 <= pos)
                {
                    int ed = pos + 1;
                    var packet = ResponsePacket.ParseFromBytes(_buffer[header_pos..ed]);
                    AsyncNotifyRespReceived(this, packet);
                    ShiftAndResetBuffer(ed);
                }
                else
                {
                    break;
                }
            }
        }

        private int FindHeaderPos(int start)
        {
            var header = Encoding.ASCII.GetBytes(string.Format("{0:X2} ", addr));
            int i = start;
            while (i < _idx - header.Length)
            {
                if (_buffer[i..(i + header.Length)].SequenceEqual(header))
                {
                    return i;
                }
                i++;
            }

            return -1;
        }

        private int FindCharPos(char c, int start)
        {
            int i = start;
            while (i < _idx)
            {
                if (_buffer[i] == c)
                {
                    return i;
                }

                i++;
            }

            return -1;
        }

        private void ShiftAndResetBuffer(int ed)
        {
            int left = _idx - ed;
            if (left > 0)
            {
                Buffer.BlockCopy(_buffer, ed, _buffer, 0, left);
                Array.Clear(_buffer, left, _buffer.Length - left);
                _idx = left;
            }
            else
            {
                if (_buffer.Length > _buffer_size)
                {
                    _buffer = new byte[_buffer_size];
                }
                Array.Clear(_buffer);
                _idx = 0;
            }
        }

        private void AsyncNotifyRespReceived(object sender, ResponsePacket packet)
        {
            Task.Run(() =>
            {
                PacketReceivedEvent(sender, packet);
            });
        }


        private byte[] _buffer;
        private int _idx;
        private int _buffer_size;
        private int addr;
    }


    class GammaIonPumpProtocol
    {
        public static readonly ResponsePacket TIMEOUT = new ResponsePacket { Status = "ER", Data = "TimeOut", CrcValid = true };

        public static string CalcCrc(string DataStr, int st = 0, int ed = -1)
        {
            if (ed == -1)
            {
                ed = DataStr.Length;
            }

            int s = 0;
            for (int i = st; i < ed; i++)
            {
                s += (int)DataStr[i];
            }

            s = s % 256;
            return string.Format("{0:X2}", s);
        }
    }
}
