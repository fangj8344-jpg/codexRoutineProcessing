#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Services.Services
 * 唯一标识：c2c4780e-8387-4ce5-a94b-14997e2dc211
 * 文件名：UdpNetSyncDevice
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/13 11:13:17
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
using System.Net.Sockets;
using System.Text;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Model;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Services.Services
{
    public class UdpNetSyncDevice : INetService, ISyncRWService
    {
        #region ------------Constructor------------
        public UdpNetSyncDevice()
        {
            DeviceInstance = new NetConfigModel(SocketType.Dgram, ProtocolType.Udp);
        }
        #endregion

        #region ------------Field------------
        private static readonly object _obj = new object();         // 加锁对象
        #endregion

        #region ------------Property------------
        /// <summary>
        /// 设备是否打开
        /// </summary>
        public bool IsOpen
        {
            get
            {
                if (DeviceInstance != null && DeviceInstance.Socket != null)
                {
                    return DeviceInstance.Socket.Connected;
                }
                return false;
            }
        }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 网络配置信息
        /// </summary>
        public NetConfigModel DeviceInstance { get; set; }

        /// <summary>
        /// 数据传输是否采用二进制传输
        /// </summary>
        public bool IsBinary { get; set; }
        #endregion

        #region ------------PublicMethod------------
        /// <summary>
        /// 打开设备
        /// </summary>
        /// <returns>打开结果</returns>
        public bool Open()
        {
            if (DeviceInstance == null || DeviceInstance.Socket == null)
            {
                throw new Exception($"{Name}的网络设备句柄不能为NULL！");
            }

            if (DeviceInstance.Socket.Connected)
                return true;

            return DeviceInstance.Open(); ;
        }

        /// <summary>
        /// 关闭设备
        /// </summary>
        /// <returns>关闭结果</returns>
        public void Close()
        {
            DeviceInstance.Close();
        }

        /// <summary>
        /// 网络UDP请求
        /// </summary>
        /// <param name="cmdName">指令名称</param>
        /// <param name="cmd">指令集</param>
        /// <param name="response">回包数据</param>
        /// <param name="length">回包数据长度</param>
        /// <param name="waitTime">等待超时时间</param>
        /// <exception cref="Exception">发送异常</exception>
        public void Request(string cmdName, byte[] cmd, out byte[] response, out int length, int waitTime)
        {
            response = null;
            length = 0;

            if (!IsOpen)
            {
                LogManager.GetCurrentClassLogger().Error($"{Name}设备未连接");
                return;
            }

            lock (_obj)
            {
                int sendSize = DeviceInstance.Send(cmd);
                if (sendSize != cmd.Length)
                {
                    throw new Exception($"{Name} Send Length Error : {sendSize}/{cmd.Length}");
                }
                DeviceInstance.SetWaitTimeOut(waitTime);
                response = new byte[128];
                length = 0;
                string debug = Encoding.UTF8.GetString(cmd);
                string resStr = string.Empty;
                string backStr = cmdName;
                if (cmdName.Contains("SendHandshake"))
                {
                    backStr = "handshake";
                }
                while (!resStr.Contains(backStr))
                {
                    try
                    {
                        length = DeviceInstance.Receive(response);
                        resStr = Encoding.UTF8.GetString(response, 0, length);
                        if (IsBinary)
                        {
                            LogManager.GetCurrentClassLogger().Debug($"{Name} response : {DataTypeCaster.ByteArrayToString(response, length)}");
                        }
                        else
                        {
                            LogManager.GetCurrentClassLogger().Debug($"{Name} response : {resStr}");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.GetCurrentClassLogger().Debug($"{Name} Receive failed : {ex.Message}");
                        length = 0;
                        break;
                    }
                }
                return;
            }
        }

        /// <summary>
        /// 获取设备句柄
        /// </summary>
        /// <returns></returns>
        public object GetHandle()
        {
            return DeviceInstance;
        }

        /// <summary>
        /// 设置句柄
        /// </summary>
        public void SetHandle(object handle)
        {
            if (handle is NetConfigModel sp)
                DeviceInstance = sp;
        }

        /// <summary>
        /// 获取指令字符串，用于日志或者打印信息
        /// </summary>
        /// <returns></returns>
        public string GetCmdString(byte[] cmd, int length)
        {
            if (IsBinary)
            {
                return DataTypeCaster.ByteArrayToString(cmd, length);
            }
            else
            {
                return Encoding.Default.GetString(cmd, 0, length);
            }
        }
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
