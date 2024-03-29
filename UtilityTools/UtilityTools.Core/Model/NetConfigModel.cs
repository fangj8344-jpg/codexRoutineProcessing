#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Model
 * 唯一标识：a4c68a79-e0b0-4fbf-82fc-a13f6e9ba50d
 * 文件名：UdpConfig
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/10 17:36:06
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
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using System.Xml.Linq;
using UtilityTools.Core.Converter;

namespace UtilityTools.Core.Model
{
    public abstract class NetConfigModel : BindableBase
    {
        #region ------------Constructor------------
        public NetConfigModel()
        {
            TargetIp = "127.0.0.1";
            TargetPort = 0;
            HostIp = "127.0.0.1";
            HostPort = 0;
        }
        #endregion

        #region ------------Field------------
        private string _targetIp;
        private int _targetPort;
        private string _hostIp;
        private int _hostPort;

        protected EndPoint _sendEndPoint;                           // UDP发送节点
        protected EndPoint _recvEndPoint;                           // UDP接收节点
        #endregion

        #region ------------Property------------
        /// <summary>
        /// 网络句柄
        /// </summary>
        public Socket Socket { get; set; }

        /// <summary>
        /// 目标IP
        /// </summary>
        public string TargetIp
        {
            get { return _targetIp; }
            set
            {
                _targetIp = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 目标端口号
        /// </summary>
        public int TargetPort
        {
            get { return _targetPort; }
            set
            {
                _targetPort = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 本地IP
        /// </summary>
        public string HostIp
        {
            get { return _hostIp; }
            set
            {
                _hostIp = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 本地端口号
        /// </summary>
        public int HostPort
        {
            get { return _hostPort; }
            set
            {
                _hostPort = value;
                RaisePropertyChanged();
            }
        }

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
        /// 是否已经连接
        /// </summary>
        /// <returns></returns>
        public abstract bool IsOpen();

        /// <summary>
        /// 连接Socket，有异常抛出，外部捕获
        /// </summary>
        /// <returns></returns>
        public abstract bool Open();

        /// <summary>
        /// 断开Socket，有异常抛出，外部捕获
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int Send(byte[] data)
        {
            if (!IsOpen())
                return 0;
            return Socket.SendTo(data, _sendEndPoint);
        }

        /// <summary>
        /// 设置等待超时时间
        /// </summary>
        /// <param name="timeout">超时时间，单位ms</param>
        public void SetWaitTimeOut(int timeout)
        {
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout);
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

        #region ------------ProtectedMethod------------
        protected void ReceiveBuffer(byte[] data)
        {
            ReceiveDataEvent?.Invoke(this, data);
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }

    public class UdpNetConfigModel : NetConfigModel
    {
        #region ------------Property------------
        private byte[] _dataBuff = new byte[4096];
        #endregion

        #region ------------Property------------
        /// <summary>
        /// 是否支持广播
        /// </summary>
        public bool IsSupportBroadcast { get; set; } = true;
        #endregion

        #region ------------PublicMethod------------
        /// <summary>
        /// 是否已经连接
        /// </summary>
        /// <returns></returns>
        public override bool IsOpen()
        {
            return Socket != null;
        }

        /// <summary>
        /// 连接Socket，有异常抛出，外部捕获
        /// </summary>
        /// <returns></returns>
        public override bool Open()
        {
            if (Socket != null)
                return true;

            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, IsSupportBroadcast);
            _sendEndPoint = new IPEndPoint(IPAddress.Parse(TargetIp), TargetPort);
            _recvEndPoint = new IPEndPoint(IPAddress.Parse(HostIp), HostPort);
            Socket.Bind(_recvEndPoint);
            Socket.BeginReceiveFrom(_dataBuff, 0, _dataBuff.Length, SocketFlags.None, ref _sendEndPoint, ReceiveCallback, null);
            return true;
        }

        /// <summary>
        /// 断开Socket，有异常抛出，外部捕获
        /// </summary>
        public override void Close()
        {
            Socket?.Dispose();
            Socket = null;
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
                var buffSize = Socket.EndReceiveFrom(ar, ref _sendEndPoint);
                if (buffSize > 0)
                {
                    var buffer = new byte[buffSize];
                    Array.Copy(_dataBuff, buffer, buffSize);
                    LogManager.GetCurrentClassLogger().Debug($"接收来自{_sendEndPoint.ToString()}：{ParseMsgToString(buffer)}");
                    ReceiveBuffer(buffer);
                }
            }
            catch (SocketException ex)
            {
                LogManager.GetCurrentClassLogger().Fatal($"接收回调异常：{ex.Message}");
            }
            finally
            {
                if(Socket != null )
                {
                    Socket.BeginReceiveFrom(_dataBuff, 0, _dataBuff.Length, SocketFlags.None, ref _recvEndPoint, ReceiveCallback, null);
                }
            }
        }
        #endregion
    }

    public class TcpNetConfigModel : NetConfigModel
    {
        #region ------------Property------------
        private byte[] _dataBuff = new byte[4096];
        #endregion

        #region ------------Property------------
        #endregion

        #region ------------PublicMethod------------
        /// <summary>
        /// 是否已经连接
        /// </summary>
        /// <returns></returns>
        public override bool IsOpen()
        {
            if (Socket != null)
                return Socket.Connected;
            return false;
        }

        /// <summary>
        /// 连接Socket，有异常抛出，外部捕获
        /// </summary>
        /// <returns></returns>
        public override bool Open()
        {
            if (Socket == null)
            {
                Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            }
            if (Socket.Connected)
                return true;
            _recvEndPoint = new IPEndPoint(IPAddress.Parse(HostIp), HostPort);
            _sendEndPoint = new IPEndPoint(IPAddress.Parse(TargetIp), TargetPort);
            Socket.Bind(_recvEndPoint);
            Socket.Connect(_sendEndPoint);
            if (Socket.Connected)
            {
                Socket.BeginReceive(_dataBuff, 0, _dataBuff.Length, SocketFlags.None, ReceiveCallback, null);
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// 断开Socket，有异常抛出，外部捕获
        /// </summary>
        public override void Close()
        {
            if (Socket != null)
            {
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Close();
                Socket = null;
            }
        }
        #endregion

        #region
        /// <summary>
        /// 异步读取回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                if(Socket == null || !Socket.Connected) 
                {
                    return;
                }
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(TargetIp), TargetPort);
                var buffSize = Socket.EndReceiveFrom(ar, ref remoteEndPoint);
                if (buffSize > 0)
                {
                    var buffer = new byte[buffSize];
                    Array.Copy(_dataBuff, buffer, buffSize);
                    LogManager.GetCurrentClassLogger().Debug($"接收来自{remoteEndPoint.ToString()}：{ParseMsgToString(buffer)}");
                    ReceiveBuffer(buffer);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Fatal($"接收回调异常：{ex.Message}");
            }
            finally
            {
                Socket.BeginReceive(_dataBuff, 0, _dataBuff.Length, SocketFlags.None, ReceiveCallback, null);
            }
        }
        #endregion
    }
}
