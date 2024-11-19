using NLog;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Helper;

namespace UtilityTools.Core.Protocol
{
    public struct DataPacket
    {
        public static readonly byte[] HEADER = Encoding.ASCII.GetBytes("$Zep:");
        public static readonly byte EOF = Convert.ToByte('%');
        public static readonly int NONE_DATA_BYTES = 28;


        public DataPacket(byte[] command, byte[] data)
        {
            Debug.Assert(command.Length == 2);

            if (data.Length < 36)
            {
                var tmp = new byte[36];
                Array.Copy(data, tmp, data.Length);
                data = tmp;
            }

            length = (UInt16)(data.Length + NONE_DATA_BYTES);

            this.command = command;
            this.data = data;
            timestamp = BitConverter.GetBytes((UInt32)DateTimeOffset.Now.ToUnixTimeSeconds());
        }


        public UInt16 length;
        public byte[] addr = new byte[6];
        public byte[] id = new byte[2];
        public byte[] command = new byte[2];
        public byte[] data;
        public byte[] error_code = new byte[4];
        public byte[] timestamp = new byte[4];

        public byte[] GetBytes()
        {
            byte[] ret = new byte[length];
            int i = 0;
            byte[] tmp;

            Buffer.BlockCopy(HEADER, 0, ret, i, HEADER.Length);
            i += HEADER.Length;

            tmp = BitConverter.GetBytes(length);
            Buffer.BlockCopy(tmp, 0, ret, i, tmp.Length);
            i += tmp.Length;

            Buffer.BlockCopy(addr, 0, ret, i, addr.Length);
            i += addr.Length;

            Buffer.BlockCopy(id, 0, ret, i, id.Length);
            i += id.Length;

            Buffer.BlockCopy(command, 0, ret, i, command.Length);
            i += command.Length;

            Buffer.BlockCopy(data, 0, ret, i, data.Length);
            i += data.Length;

            Buffer.BlockCopy(error_code, 0, ret, i, error_code.Length);
            i += error_code.Length;

            Buffer.BlockCopy(timestamp, 0, ret, i, timestamp.Length);
            i += timestamp.Length;

            ushort crc = CRCHelper.Data_GetCRC16(ret, 0, length - 3);
            tmp = new byte[2];
            tmp[0] = (byte)((crc & 0xFF00) >> 8);
            tmp[1] = (byte)(crc & 0xFF);
            Buffer.BlockCopy(tmp, 0, ret, i, tmp.Length);
            i += tmp.Length;

            ret[i] = EOF;
            return ret;
        }

        public static DataPacket ParseFromBytes(byte[] bytes)
        {
            int cmd_st = HEADER.Length + 2 + 6 + 2;
            int data_st = cmd_st + 2;
            int data_ed = data_st + (bytes.Length - NONE_DATA_BYTES);

            var cmd = bytes[cmd_st..(cmd_st + 2)];
            var data = bytes[data_st..data_ed];

            var packet = new DataPacket(cmd, data);

            int addr_st = HEADER.Length + 2;
            Buffer.BlockCopy(bytes, addr_st, packet.addr, 0, packet.addr.Length);

            int id_st = addr_st + packet.addr.Length;
            Buffer.BlockCopy(bytes, id_st, packet.id, 0, packet.id.Length);

            int ec_st = data_st + packet.data.Length;
            Buffer.BlockCopy(bytes, ec_st, packet.error_code, 0, packet.error_code.Length);

            int ts_st = ec_st + packet.error_code.Length;
            Buffer.BlockCopy(bytes, ts_st, packet.timestamp, 0, packet.timestamp.Length);

            return packet;
        }
    }

    public class ZepGenericProtocol
    {
        public static byte[] GetCmd(byte[] id, byte[] command, byte[] data)
        {
            var packet = new DataPacket(command, data);
            packet.id = id;
            return packet.GetBytes();
        }
    }


    public class ZepGenericProtocolParser
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        public ZepGenericProtocolParser(int BufferSize = 256)
        {
            _buffer = new byte[BufferSize];
            _idx = 0;

            this._buffer_size = BufferSize;
        }

        public event EventHandler<DataPacket> PacketReceivedEvent;


        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ReceiveBytes(byte[] data) //接收函数
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
                if (_idx < header_pos + 2) break;//不明白

                int length = BitConverter.ToInt16(_buffer, header_pos + DataPacket.HEADER.Length);
                int ed = header_pos + length;
                if (_idx < ed) break;//表示此包不全

                if (_buffer[ed - 1] == DataPacket.EOF)//表示数据完整
                {
                    var crc = BitConverter.GetBytes(CRCHelper.Data_GetCRC16(_buffer, header_pos, header_pos + length - 3));//CRC校验
                    if (crc.SequenceEqual(_buffer[(header_pos + length - 3)..(header_pos + length - 1)]))//判断CRC
                    {
                        var packet = DataPacket.ParseFromBytes(_buffer[header_pos..ed]);
                        AsyncNotifyRespReceived(this, packet);
                        ShiftAndResetBuffer(ed);
                        continue;
                    }
                }

                st = header_pos + DataPacket.HEADER.Length;
            }
        }

        private void ShiftAndResetBuffer(int ed) //
        {
            int left = _idx - ed;
            if (left > 0)
            {
                Buffer.BlockCopy(_buffer, ed, _buffer, 0, left);
                Array.Clear(_buffer, left, _buffer.Length - left);
            }
            else
            {
                if (_buffer.Length > _buffer_size)
                {
                    _buffer = new byte[_buffer_size];
                }
                Array.Clear(_buffer);
            }
            _idx = 0;
        }

        private int FindHeaderPos(int start)
        {
            int i = start;
            while (i < _idx - DataPacket.HEADER.Length)
            {
                if (_buffer[i..(i + DataPacket.HEADER.Length)].SequenceEqual(DataPacket.HEADER))
                {
                    return i;
                }
                i++;
            }

            return -1;
        }


        private void AsyncNotifyRespReceived(object sender, DataPacket packet)
        {
            Task.Run(() =>
            {
                PacketReceivedEvent(sender, packet);
            });
        }


        private byte[] _buffer;
        private int _idx;
        private int _buffer_size;
    }
}
