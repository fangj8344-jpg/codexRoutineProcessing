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
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
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

        protected IPEndPoint _sendEndPoint;                           // UDP发送节点
        protected IPEndPoint _recvEndPoint;                           // UDP接收节点
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
                _sendEndPoint = new IPEndPoint(IPAddress.Parse(value), TargetPort);
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
                _sendEndPoint = new IPEndPoint(IPAddress.Parse(TargetIp), value);
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
                _recvEndPoint = new IPEndPoint(IPAddress.Parse(value), HostPort);
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
                _recvEndPoint = new IPEndPoint(IPAddress.Parse(HostIp), value);
            }
        }

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

        public int Receive(byte[] buffer)
        {
            return Socket.Receive(buffer);
        }
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }

    public class UdpNetConfigModel : NetConfigModel
    {
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

            Socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, IsSupportBroadcast);
            _sendEndPoint = new IPEndPoint(IPAddress.Parse(TargetIp), TargetPort);
            _recvEndPoint = new IPEndPoint(IPAddress.Parse(HostIp), HostPort);
            Socket.Bind(_recvEndPoint);
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
    }

    public class TcpNetConfigModel : NetConfigModel
    {
        #region ------------Property------------
        #endregion

        #region ------------PublicMethod------------
        /// <summary>
        /// 是否已经连接
        /// </summary>
        /// <returns></returns>
        public override bool IsOpen()
        {
            if(Socket != null) 
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
            if (Socket.LocalEndPoint == null)
            {
                Socket.Bind(_recvEndPoint);
            }
            if (Socket.RemoteEndPoint == null)
            { 
                Socket.Connect(_sendEndPoint);
            }
            return true;
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
    }
}
