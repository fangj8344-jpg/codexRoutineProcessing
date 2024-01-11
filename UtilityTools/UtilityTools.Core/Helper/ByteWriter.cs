#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Helper
 * 唯一标识：b0b443d6-0f93-4f66-adc0-5bedca0b1099
 * 文件名：ByteWriter
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/11/24 17:59:34
 * 版本：V1.0.0
 * 描述：
 *
 * ----------------------------------------------------------------
 * 修改人：
 * 时间：
 * 修改说明：
 *
 * 版本：V1.0.1
 *----------------------------------------------------------------*/
#endregion

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Helper
{
    public delegate void SetCheckoutDelegate(ref byte[] buf, int length);

    public class ByteWriter : IDisposable
    {
        #region Constructor
        public ByteWriter(int length)
        {
            Length = length;
            _bytes = ArrayPool<byte>.Shared.Rent(length);
            _offset = 0;
        }

        ~ByteWriter()
        {
            Dispose();
        }
        #endregion

        #region Field
        private byte[] _bytes;
        private int _offset;
        #endregion

        #region Property
        /// <summary>
        /// 数据长度
        /// </summary>
        public int Length { get; set; }
        #endregion

        #region PublicMethod
        public void Write(byte v)
        {
            _bytes[_offset++] = v;
        }

        public void Write(bool v)
        {
            _bytes[_offset++] = v ? (byte)0x01 : (byte)0x00;
        }

        public void Write(short v)
        {
            _bytes[_offset++] = (byte)v;
            _bytes[_offset++] = (byte)(v >> 8);
        }

        public void Write(ushort v)
        {
            _bytes[_offset++] = (byte)v;
            _bytes[_offset++] = (byte)(v >> 8);
        }

        public void Write(int v)
        {
            _bytes[_offset++] = (byte)v;
            _bytes[_offset++] = (byte)(v >> 8);
            _bytes[_offset++] = (byte)(v >> 16);
            _bytes[_offset++] = (byte)(v >> 24);
        }

        public void Write(uint v)
        {
            _bytes[_offset++] = (byte)v;
            _bytes[_offset++] = (byte)(v >> 8);
            _bytes[_offset++] = (byte)(v >> 16);
            _bytes[_offset++] = (byte)(v >> 24);
        }

        public void Write(long v)
        {
            _bytes[_offset++] = (byte)v;
            _bytes[_offset++] = (byte)(v >> 8);
            _bytes[_offset++] = (byte)(v >> 16);
            _bytes[_offset++] = (byte)(v >> 24);
            _bytes[_offset++] = (byte)(v >> 32);
            _bytes[_offset++] = (byte)(v >> 40);
            _bytes[_offset++] = (byte)(v >> 56);
        }

        public void Write(ulong v)
        {

            _bytes[_offset++] = (byte)v;
            _bytes[_offset++] = (byte)(v >> 8);
            _bytes[_offset++] = (byte)(v >> 16);
            _bytes[_offset++] = (byte)(v >> 24);
            _bytes[_offset++] = (byte)(v >> 32);
            _bytes[_offset++] = (byte)(v >> 40);
            _bytes[_offset++] = (byte)(v >> 56);
        }

        public void Write(float v)
        {
            Write(BitConverter.GetBytes(v));
        }

        public void Write(double v)
        {
            Write(BitConverter.GetBytes(v));
        }

        public void Write(string v)
        {
            byte[] strBytes = System.Text.Encoding.Default.GetBytes(v);
            Write(strBytes);
        }

        public void Write(byte[] v)
        {
            for (int i = 0; i < v.Length; i++)
            {
                Write(v[i]);
            }
        }

        public byte[] EndWrite(SetCheckoutDelegate func = null)
        {
            func?.Invoke(ref _bytes, Length);
            byte[] bytes = new byte[Length];
            Array.Copy(_bytes, bytes, Length);
            return bytes;
        }

        public byte[] EndWrite(bool isRealLenth)
        {
            int length = Length;
            if (isRealLenth)
            {
                length = _offset;
            }
            byte[] bytes = new byte[length];
            Array.Copy(_bytes, bytes, length);
            return bytes;
        }

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(_bytes);
        }
        #endregion
    }
}
