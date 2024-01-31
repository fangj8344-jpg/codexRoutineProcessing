using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Modules.IonPump.Protocol
{
    public enum EnumWriteRespType
    {
        [Description("命令执行成功")]
        SUCCESS = 0x06,

        [Description("命令执行不成功")]
        FAILED = 0x15,

        [Description("非法窗口号")]
        ILLEGAL_WIN = 0x32,

        [Description("数据类型错误（字节长度错误）")]
        DATA_TYPE_ERROR = 0x33,

        [Description("数据超过允许范围")]
        DATA_EXCEEDS_RANGE = 0x34,

        [Description("命令为只读或暂时不可执行")]
        READONLY_CMD = 0x35,
    }

    public enum EnumWinType
    {
        RW_MODE = 8,

        [Description("开关高压")]
        RW_HV_ON_OF = 11,
        RW_BAUD_RATE = 108,
        R_STATUS = 205,
        R_ERROR_CODE = 206,
        RW_CONTROLLER_MODEL = 319,
        RW_CONTROLLER_SERIAL_NUMBER = 323,
        RW_RS485_ADDR = 503,
        RW_SERIAL_TYPE = 504,
        RW_PRESSURE_UNIT = 600,
        RW_AUTOSTART = 601,
        RW_PROTECT = 602,
        RW_FIXED_OR_STEP = 603,
        RW_DEVICE_NUMBER_CH1 = 610,
        RW_MAX_POWER = 612,
        RW_TARGET_VOLTAGE_CH1 = 613,
        RW_PROTECT_CURRENT_CH1 = 614,
        RW_SET_POINT_CH1 = 615,
        R_TEMP_POWER_SECTION = 800,
        R_TEMP_INTERNAL_CONTROLLER = 801,
        R_STATUS_SET_POINT = 804,
        R_VOLTAGE = 810,
        R_CURRENT = 811,
        R_PRESSURE = 812,
        RW_LABEL = 890
    }

    public enum EnumCmdType
    {
        READ = 0,
        WRITE = 1
    }

    public enum EnumErrorCode
    {
        NO_ERROR = 0,
        OVER_TEMP = 4,
        INTERLOCK_CABLE = 32,
        SHORT_CIRCUIT = 64,
        PROTECT = 128,
    }

    public enum EnumPressureUnit
    {
        Torr = 0,
        mBar = 1,
        Pa = 2,
    }

    public static class Helper
    {
        public static readonly byte STX = 0x02;
        public static readonly byte ADDR = 0x80;
        public static readonly byte ETX = 0x03;

        public static byte[] IntToWin(int win)
        {
            var w = new int[] { 0x30 | (win / 100), 0x30 | ((win % 100) / 10), 0x30 | (win % 10) };
            return w.Select(x => (byte)x).ToArray();
        }

        public static byte[] CalcCRC(byte[] data, int st, int ed)
        {
            byte crc = data[st];
            for (int j = st + 1; j < ed; j++)
            {
                crc ^= data[j];
            }
            string hs = crc.ToString("X2");
            return Encoding.ASCII.GetBytes(hs);
        }

        public static int FindHeaderPos(byte[] data, int st, int length)
        {
            int i = st;
            int ed = st + length - 1;
            while (i < ed)
            {
                if (data[i] == STX && data[i + 1] == ADDR)
                {
                    return i;
                }
                i++;
            }

            return -1;
        }

        public static IEnumerable<int> FindHeaderPositons(byte[] data, int length)
        {
            int i = 0;
            while (i < length)
            {
                if (data[i] == STX && i + 1 < length && data[i + 1] == ADDR)
                {
                    yield return i;
                }
                i++;
            }
        }

        public static int SearchReadRespPacket(byte[] data, int st, int ed)
        {
            byte crc = data[st + 1];
            for (int j = st + 2; j < ed - 2; j++)
            {
                crc ^= data[j];
                string hs = crc.ToString("X2");
                if (data[j] == ETX)
                {
                    var curr_crc = Encoding.ASCII.GetBytes(hs);
                    if (curr_crc.SequenceEqual(data[(j + 1)..(j + 3)]))
                    {
                        return j + 3;
                    }
                }
            }
            return 0;
        }

        public static bool IsCrcValid(byte[] data, int st, int length)
        {
            var crc = CalcCRC(data, st + 1, length - 2);
            return crc.SequenceEqual(data[(st + length - 2)..(st + length)]);
        }
    }


    public struct DataPacket
    {
        public DataPacket(EnumWinType win, string data_str, EnumCmdType cmdType)
        {
            STX = Helper.STX;
            ADDR = Helper.ADDR;
            WIN = Helper.IntToWin((int)win);
            COM = cmdType == EnumCmdType.READ ? (byte)0x30 : (byte)0x31;
            DATA = Encoding.ASCII.GetBytes(data_str);
            ETX = Helper.ETX;
            CRC = new byte[] { 0x00, 0x00 };
        }

        public DataPacket(byte[] win, byte com, byte[] data, byte[] crc)
        {
            STX = Helper.STX;
            ADDR = Helper.ADDR;
            WIN = win.ToArray();
            COM = com;
            DATA = data.ToArray();
            ETX = Helper.ETX;
            CRC = crc.ToArray();
        }

        public byte STX;
        public byte ADDR;
        public byte[] WIN;
        public byte COM;
        public byte[] DATA;
        public byte ETX;
        public byte[] CRC;

        public byte[] GetBytes()
        {
            byte[] ret = new byte[1 + 1 + WIN.Length + 1 + DATA.Length + 1 + CRC.Length];
            int i = 0;
            ret[i++] = STX;
            ret[i++] = ADDR;
            Buffer.BlockCopy(WIN, 0, ret, i, WIN.Length);
            i += WIN.Length;
            ret[i++] = COM;
            Buffer.BlockCopy(DATA, 0, ret, i, DATA.Length);
            i += DATA.Length;
            ret[i++] = ETX;

            byte[] crc = Helper.CalcCRC(ret, 1, i);
            Buffer.BlockCopy(crc, 0, ret, i, crc.Length);
            return ret;
        }

        public readonly string GetDataStr()
        {
            return Encoding.ASCII.GetString(DATA);
        }

        public readonly EnumWinType GetWinType()
        {
            return (EnumWinType)int.Parse(Encoding.ASCII.GetString(WIN));
        }

        public static DataPacket ParseFromBytes(byte[] data)
        {
            DataPacket pkt = new DataPacket(data[2..5], data[5], data[6..^3], data[^2..]);
            return pkt;
        }
    }

    public static class AgilentIonPumpProtocol
    {

        public static byte[] GetSimpleReadCmd(EnumWinType win)
        {
            DataPacket packet = new DataPacket(win, "0", EnumCmdType.READ);
            return packet.GetBytes();
        }

        public static byte[] QueryStatusCmd()
        {
            return GetSimpleReadCmd(EnumWinType.R_STATUS);
        }

        public static byte[] QueryErrorCodeCmd()
        {
            return GetSimpleReadCmd(EnumWinType.R_ERROR_CODE);
        }


        public static byte[] QueryPressureCmd()
        {
            return GetSimpleReadCmd(EnumWinType.R_PRESSURE);
        }

        public static byte[] QueryPressureUnitCmd()
        {
            return GetSimpleReadCmd(EnumWinType.RW_PRESSURE_UNIT);
        }

        public static byte[] QueryCurrentCmd()
        {
            return GetSimpleReadCmd(EnumWinType.R_CURRENT);
        }

        public static byte[] QueryTempCmd()
        {
            return GetSimpleReadCmd(EnumWinType.R_TEMP_POWER_SECTION);
        }

        public static byte[] QueryMaxPowerCmd()
        {
            return GetSimpleReadCmd(EnumWinType.RW_MAX_POWER);
        }

        public static byte[] QueryVoltageCmd()
        {
            return GetSimpleReadCmd(EnumWinType.R_VOLTAGE);
        }

        public static byte[] SetTargetVoltage(int targetVoltage)
        {
            if (targetVoltage < 0) targetVoltage = 0;
            if (targetVoltage > 7000) targetVoltage = 7000;

            DataPacket packet = new DataPacket(EnumWinType.RW_TARGET_VOLTAGE_CH1, $"{targetVoltage}", EnumCmdType.WRITE);
            return packet.GetBytes();
        }

        public static byte[] IncreaseVoltage()
        {
            DataPacket packet = new DataPacket(EnumWinType.RW_HV_ON_OF, "1", EnumCmdType.WRITE);
            return packet.GetBytes();
        }

        public static byte[] DecreaseVoltage()
        {
            DataPacket packet = new DataPacket(EnumWinType.RW_HV_ON_OF, "0", EnumCmdType.WRITE);
            return packet.GetBytes();
        }
    }

    public class Resp
    {
        public Resp(EnumCmdType cmdType, byte[] data)
        {
            CmdType = cmdType;
            _data = data;
        }

        public EnumWriteRespType GetWriteRespType()
        {
            byte b = _data[2];
            EnumWriteRespType e = (EnumWriteRespType)b;
            return e;
        }

        public DataPacket GetReadResp()
        {
            return DataPacket.ParseFromBytes(_data);
        }

        public EnumCmdType CmdType { get; set; }
        public byte[] _data;
    }


    public class RespParser
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        public RespParser()
        {
            _buffer = new byte[256];
            _idx = 0;
        }

        public event EventHandler<Resp> RespReceivedEvent;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ReceiveBytes(byte[] data)
        {
            Buffer.BlockCopy(data, 0, _buffer, _idx, data.Length);
            _idx += data.Length;

            int st = 0;
            int header_pos;
            while ((header_pos = Helper.FindHeaderPos(_buffer, st, _idx)) >= 0)
            {
                if (6 <= _idx - header_pos)
                {
                    if (Helper.IsCrcValid(_buffer, header_pos, 6))
                    {
                        int ed = header_pos + 6;
                        var resp = new Resp(EnumCmdType.WRITE, _buffer[header_pos..ed]);
                        AsyncNotifyRespReceived(this, resp);
                        ShiftAndResetBuffer(ed);
                        continue;
                    }
                }

                if (10 <= _idx - header_pos)
                {
                    int length = Helper.SearchReadRespPacket(_buffer, header_pos, _idx);
                    if (length > 0)
                    {
                        int ed = header_pos + length;
                        var resp = new Resp(EnumCmdType.READ, _buffer[header_pos..ed]);
                        AsyncNotifyRespReceived(this, resp);
                        ShiftAndResetBuffer(ed);
                        continue;
                    }
                }

                st = header_pos + 2;
            }
        }

        private void AsyncNotifyRespReceived(object sender, Resp resp)
        {
            Task.Run(() =>
            {
                RespReceivedEvent(sender, resp);
            });
        }


        private void ShiftAndResetBuffer(int ed)
        {
            int left = _idx - ed;
            if (left > 0)
            {
                Buffer.BlockCopy(_buffer, ed, _buffer, 0, left);
                Array.Clear(_buffer, left, _buffer.Length - left);
            }
            else
            {
                Array.Clear(_buffer);
            }
            _idx = 0;
        }

        private byte[] _buffer;
        private int _idx;
    }
}
