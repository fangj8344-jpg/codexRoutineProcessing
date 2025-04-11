using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Model;
using UtilityTools.Core.Protocol;
using UtilityTools.Services.Interfaces.IServices;
using System.Windows.Media.Animation;
using System.Net.Sockets;

namespace UtilityTools.Modules.Test485ChipTool.Protocol
{


        public static class Test485ChipToolProtocol
        {
        #region ------------StaticMethod------------
        public static readonly ushort cmdGetVac = 0x0800;
        public static readonly ushort cmdGetPID = 0X0806;
            /// <summary>
            /// 指令生成方法
            /// </summary>
            /// <param name="command">指令类型</param>
            /// <param name="data">指令参数</param>
            /// <returns></returns>
            public static byte[] GetCmd(ushort command, int? deviceID, byte[] data)
            {
            var addr = 0x0101;
            
                // data段固定36字节，不足用零填充
                if (data.Length < 36)
                {
                    var tmp = new byte[36];
                    Array.Clear(tmp, 0, tmp.Length);

                    Buffer.BlockCopy(data, 0, tmp, 0, data.Length);
                    data = tmp;
                }

                var cmd = BitConverter.GetBytes(command);
                var id = BitConverter.GetBytes((ushort)deviceID);
                return ZepGenericProtocol.GetCmd(id, cmd, data);
            }

            /// <summary>
            /// 获取Pid参数
            /// </summary>
            /// <param name="deviceID"></param>
            /// <returns></returns>
            public static byte[] GetPID(int deviceID = 0x0101)
            {
                byte[] bytes = new byte[36];
                return GetCmd(cmdGetPID, deviceID, bytes);
            }
   
        public static byte[] GetSendMessage()
        {
            string SendMessage = "24 5A 65 70 3A 40 00 00 00 00 00 01 01 01 01 06 08 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 2B AB F3 40 52 D1 25";
            return hexStringToByteArray(SendMessage);

        }
        public static byte[] GetCheckMessage()
        {
            string CheckMessag = "24 5A 65 70 3A 40 00 00 00 00 00 00 00 01 01 06 08 00 00 00 00 CD CC CC 3E 52 49 1D 3A 0A D7 23 3C 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 2B AB F3 40 9F 30 25";
            return hexStringToByteArray(CheckMessag);
        }
        public static byte[] GetCheckMessage2()
        {
            string CheckMessag = "24 5A 65 70 3A 40 00 00 00 00 00 01 01 01 01 06 08 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 2B AB F3 40 93 1D 25";
            return hexStringToByteArray(CheckMessag);
        }
        /// <summary>
        /// byte数组转16进制字符串
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string byteArrayToHexString(byte[] data)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                builder.Append(string.Format("{0:X2} ", data[i]));
            }
            return builder.ToString().Trim();
        }

        /// <summary>
        /// 16进制字符串转byte数组
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] hexStringToByteArray(string data)
        {
            string[] chars = data.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            byte[] returnBytes = new byte[chars.Length];
            //逐个字符变为16进制字节数据
            for (int i = 0; i < chars.Length; i++)
            {
                returnBytes[i] = Convert.ToByte(chars[i], 16);
            }
            return returnBytes;
        }
        #endregion

    }
    public static class  FreePort
    {
        private const string PortReleaseGuid = "8875BD8E-4D5B-11DE-B2F4-691756D89593";

        /// <summary> 
        /// Check if startPort is available, incrementing and 
        /// checking again if it's in use until a free port is found 
        /// </summary> 
        /// <param name="startPort">The first port to check</param> 
        /// <returns>The first available port</returns> 
        public static int FindNextAvailableTCPPort(int startPort)
        {
            int port = startPort;
            bool isAvailable = true;

            var mutex = new Mutex(false,
                string.Concat("Global/", PortReleaseGuid));
            mutex.WaitOne();
            try
            {
                IPGlobalProperties ipGlobalProperties =
                    IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] endPoints =
                    ipGlobalProperties.GetActiveTcpListeners();

                do
                {
                    if (!isAvailable)
                    {
                        port++;
                        isAvailable = true;
                    }

                    foreach (IPEndPoint endPoint in endPoints)
                    {
                        if (endPoint.Port != port) continue;
                        isAvailable = false;
                        break;
                    }

                } while (!isAvailable && port < IPEndPoint.MaxPort);

                if (!isAvailable)
                    throw new ApplicationException("Not able to find a free TCP port.");

                return port;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        /// <summary> 
        /// Check if startPort is available, incrementing and 
        /// checking again if it's in use until a free port is found 
        /// </summary> 
        /// <param name="startPort">The first port to check</param> 
        /// <returns>The first available port</returns> 
        public static int FindNextAvailableUDPPort(int startPort)
        {
            int port = startPort;
            bool isAvailable = true;

            var mutex = new Mutex(false,
                string.Concat("Global/", PortReleaseGuid));
            mutex.WaitOne();
            try
            {
                IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] endPoints = ipGlobalProperties.GetActiveUdpListeners();

                do
                {
                    if (!isAvailable)
                    {
                        port++;
                        isAvailable = true;
                    }

                    foreach (IPEndPoint endPoint in endPoints)
                    {
                        if (endPoint.Port != port)
                            continue;
                        isAvailable = false;
                        break;
                    }

                } while (!isAvailable && port < IPEndPoint.MaxPort);

                if (!isAvailable)
                    throw new ApplicationException("Not able to find a free TCP port.");

                return port;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
        public static IPAddress FindIpv4IP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            return null;
        }
       

    }

}
