#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Services.Comms
 * 唯一标识：c3132743-5016-4601-b7ea-e319b1c7bc24
 * 文件名：UdpClientComm
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/31 9:00:24
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
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilityTools.Core.Helper;
using UtilityTools.Services.Interfaces.IComm;

namespace UtilityTools.Services.Comms
{
    public class UdpClientComm : ICommunication
    {
        #region ------------Constructor------------
        public UdpClientComm(string hostIp, int hostPort, string remoteIp, int remotePort)
        {
            Name = $"{hostIp}:{hostPort}<->{remoteIp}:{remotePort}";
            _hostIp = hostIp;
            _hostPort = hostPort;
            _remoteIp = remoteIp;
            _remotePort = remotePort;
        }
        #endregion

        #region ------------Field------------
        private string _hostIp;
        private int _hostPort;
        private string _remoteIp;
        private int _remotePort;

        private UdpClient _udpClient;
        private bool _isClose = true;
        private int _isConnected = -1;
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
        /// 连接函数
        /// </summary>
        public void Connect()
        {
            try
            {
                if (_udpClient != null)
                {
                    _udpClient.Close();
                }
                _isClose = false;
                var hostEndPoint = new IPEndPoint(IPAddress.Parse(_hostIp), _hostPort);
                var remoteEndPoint = new IPEndPoint(IPAddress.Parse(_remoteIp), _remotePort);
                _udpClient = new UdpClient(hostEndPoint);
                _udpClient.Connect(remoteEndPoint);
                _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                _udpClient.BeginReceive(ReceiveCallback, null);
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
            if (_udpClient != null)
            {
                _udpClient.Close();
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
                var sendData = new byte[length];
                Array.Copy(data, offset, sendData, 0, length);
                _udpClient.BeginSend(sendData, length, (ar) =>
                {
                    _udpClient.EndSend(ar);
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
                        if (!HardwareMethod.PingRemoteIP(_remoteIp))
                        {
                            SetCon(0);
                        }
                        else
                        {
                            SetCon(1);
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
        /// 异步读取回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                var remoteEndPoint = new IPEndPoint(IPAddress.Parse(_remoteIp), _remotePort);
                var buffer = _udpClient.EndReceive(ar, ref remoteEndPoint);
                if (buffer != null)
                {
                    LogManager.GetCurrentClassLogger().Debug($"{Name} 接收来自{remoteEndPoint.ToString()}：{ParseMsgToString(buffer)}");
                    ReceiveDataEvent?.Invoke(this, buffer);
                    _udpClient.BeginReceive(ReceiveCallback, null);
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
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
