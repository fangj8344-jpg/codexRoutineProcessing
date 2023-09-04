#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Services.Comms
 * 唯一标识：2aaa58c1-ce8d-4180-90c0-c159330e4dc9
 * 文件名：TcpClientComm
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/31 8:59:23
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

using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilityTools.Core.Helper;
using UtilityTools.Services.Interfaces.IComm;

namespace UtilityTools.Services.Comms
{
    /// <summary>
    /// 封装TcpClient的设备操作
    /// </summary>
    public class TcpClientComm : ICommunication
    {
        #region ------------Constructor------------
        public TcpClientComm(string ip, int port)
        {
            Name = $"{ip}:{port}";
            _ip = ip;
            _port = port;
            _readBytesBuff = new byte[1024];
        }
        #endregion

        #region ------------Field------------
        private string _ip;
        private int _port;

        private TcpClient _tcpClient;
        private bool _isTryingCon = false;
        private bool _isClose = false;
        private int _isConnected = -1;

        private byte[] _readBytesBuff;
        #endregion

        #region ------------Property------------
        /// <summary>
        /// 设备名称
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 是否已经连接
        /// </summary>
        public bool IsConnect { get => _isConnected == 1; }
        /// <summary>
        /// 通讯报文是否是二进制
        /// </summary>
        public bool IsBinary { get; set; }
        #endregion

        #region ------------Event------------
        /// <summary>
        /// 连接状态修改事件
        /// </summary>
        public event EventHandler<bool> ConnectStateChangedEvent;
        /// <summary>
        /// 接收到数据事件
        /// </summary>
        public event EventHandler<byte[]> ReceiveDataEvent;
        #endregion

        #region ------------PublicMethod------------
        /// <summary>
        /// 连接函数，支持无限制重连
        /// </summary>
        public void Connect()
        {
            if (_isTryingCon) return;
            try
            {
                if (_tcpClient != null)
                {
                    _tcpClient.Close();
                }
                _isClose = false;
                _tcpClient = new TcpClient();
                _isTryingCon = true;
                _tcpClient.BeginConnect(_ip, _port, ConnectCallback, null);
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error($"{Name} 连接异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 关闭函数
        /// </summary>
        public void Close()
        {
            _isClose = true;
            if (_tcpClient != null && (_tcpClient.Client == null || _tcpClient.Client.Connected))
            {
                _tcpClient.Close();
            }
        }

        /// <summary>
        /// 发送函数
        /// </summary>
        /// <param name="data">发送数据</param>
        /// <param name="offset">偏移量</param>
        /// <param name="length">数据长度</param>
        public void SendMsg(byte[] data, int offset, int length)
        {
            try
            {
                LogManager.GetCurrentClassLogger().Debug($"{Name} 发送：{ParseMsgToString(data, offset, length)}");
                _tcpClient.GetStream().BeginWrite(data, offset, length, (ar) =>
                {
                    _tcpClient.GetStream().EndWrite(ar);
                }, null);
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error($"{Name} 发送异常：{ex.Message}");
            }
        }

        public void SendMsg(byte[] data, int length)
        {
            SendMsg(data, 0, length);
        }

        public void SendMsg(byte[] data)
        {
            SendMsg(data, data.Length);
        }

        /// <summary>
        /// 开启ping检查
        /// </summary>
        public void StartConnectedCheck()
        {
            Task.Factory.StartNew(() =>
            {
                Ping ping = new Ping();
                while (!_isClose)
                {
                    try
                    {
                        if (!HardwareMethod.PingRemoteIP(_ip))
                        {
                            SetCon(0);
                            Connect();
                        }
                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        LogManager.GetCurrentClassLogger().Error($"{Name} Ping检测异常：{ex.Message}");
                    }
                }
            });
        }

        /// <summary>
        /// 将消息解析成字符串
        /// </summary>
        /// <param name="data">消息报文</param>
        /// <param name="offset">消息偏置</param>
        /// <param name="length">消息长度</param>
        /// <returns></returns>
        public string ParseMsgToString(byte[] data, int offset, int length)
        {
            if (IsBinary)
            {
                string result = "";
                for (int i = offset; i < length; i++)
                {
                    result += String.Format(" 0x{0:X2}", data[i]);
                }
                return result;
            }
            else
            {
                return Encoding.Default.GetString(data, offset, length).Trim('\n');
            }
        }

        public string ParseMsgToString(byte[] data, int length)
        {
            return ParseMsgToString(data, 0, length);
        }

        public string ParseMsgToString(byte[] data)
        {
            return ParseMsgToString(data, data.Length);
        }
        #endregion

        #region ------------PrivateMethod------------
        /// <summary>
        /// 异步连接回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectCallback(IAsyncResult ar)
        {
            if (_isClose) return;
            try
            {
                if (_tcpClient.Connected == false)
                {
                    SetCon(0);
                    _tcpClient.Close();
                    _tcpClient = new TcpClient();
                    _isTryingCon = true;
                    _tcpClient.BeginConnect(_ip, _port, ConnectCallback, null);
                }
                else
                {
                    SetCon(1);
                    _tcpClient.Client.IOControl(IOControlCode.KeepAliveValues, KeepAlive(1, 500, 500), null);
                    _isTryingCon = false;
                    _tcpClient.EndConnect(ar);
                    _tcpClient.GetStream().BeginRead(_readBytesBuff, 0, _readBytesBuff.Length, ReceiveCallback, null);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error($"{Name} 连接回调异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 异步读取回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int len = _tcpClient.GetStream().EndRead(ar);
                if (len > 0)
                {
                    byte[] buffer = new byte[len];
                    Array.Copy(_readBytesBuff, buffer, len);
                    LogManager.GetCurrentClassLogger().Debug($"{Name} 接收：{ParseMsgToString(buffer)}");
                    ReceiveDataEvent?.Invoke(this, buffer);
                    _tcpClient.GetStream().BeginRead(_readBytesBuff, 0, _readBytesBuff.Length, ReceiveCallback, null);
                }
                else
                {
                    Connect();
                }
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error($"{Name} 接收回调异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 连接状态设置函数
        /// </summary>
        /// <param name="_con">-1：未初始化；0：未连接；1：已连接</param>
        private void SetCon(int _con)
        {
            if (_isConnected != _con)
            {
                bool rel = false;
                if (_con == 1)
                {
                    rel = true;
                }
                _isConnected = _con;
                ConnectStateChangedEvent?.Invoke(this, rel);
            }
        }

        /// <summary>
        /// 设置保持连接参数
        /// </summary>
        /// <param name="onOff"></param>
        /// <param name="keepAliveTime"></param>
        /// <param name="keepAliveInterval"></param>
        /// <returns></returns>
        private byte[] KeepAlive(int onOff, int keepAliveTime, int keepAliveInterval)
        {
            byte[] buffer = new byte[12];
            BitConverter.GetBytes(onOff).CopyTo(buffer, 0);
            BitConverter.GetBytes(keepAliveTime).CopyTo(buffer, 4);
            BitConverter.GetBytes(keepAliveInterval).CopyTo(buffer, 8);
            return buffer;
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
