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

namespace UtilityTools.Modules.Test485ChipTool.Protocol
{

   


        public class Test485ChipToolDataPacket
        {
            public Test485ChipToolDataPacket(DataPacket packet)
            {
                this.packet = packet;
            }
        
            public DataPacket packet;
        }


        public class Test485ChipToolDataPacketProtocolParser
        {
            public Test485ChipToolDataPacketProtocolParser()
            {
                _parser = new ZepGenericProtocolParser();
                _parser.PacketReceivedEvent += GeneriaPackReceived;
            }
            private void GeneriaPackReceived(object sender, DataPacket packet)
            {
                PacketReceivedEvent(this, new Test485ChipToolDataPacket(packet));
            }

            public event EventHandler<Test485ChipToolDataPacket> PacketReceivedEvent;

            public void ReceiveBytes(byte[] data)
            {
                _parser.ReceiveBytes(data);
            }

            private ZepGenericProtocolParser _parser;
            public IAsynRWService Service;
        }




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
            /// 读真空1的读数
            /// </summary>
            /// <param name="deviceID"></param>
            /// <returns></returns>
            public static byte[] GetCH1Vac(int deviceID = 0x0101)
            {
            byte[] bytes = new byte[36];
            bytes[0] = 0x01;
            return GetCmd(cmdGetVac, deviceID, bytes);
            }
            /// <summary>
            /// 读真空2的读数
            /// </summary>
            /// <param name="deviceID"></param>
            /// <returns></returns>
            public static byte[] GetCH2Vac(int deviceID = 0x0101)
            {
                byte[] bytes = new byte[36];
                bytes[0] = 0x02;
                return GetCmd(cmdGetVac, deviceID, bytes);
            }
            /// <summary>
            /// 读真空3的读数
            /// </summary>
            /// <param name="deviceID"></param>
            /// <returns></returns>
            public static byte[] GetCH3Vac(int deviceID = 0x0101)
            {
                byte[] bytes = new byte[36];
                bytes[0] = 0x03;
                return GetCmd(cmdGetVac, deviceID, bytes);
            }
            /// <summary>
            /// 读真空4的读数
            /// </summary>
            /// <param name="deviceID"></param>
            /// <returns></returns>
            public static byte[] GetCH4Vac(int deviceID = 0x0101)
            {
                byte[] bytes = new byte[36];
                bytes[0] = 0x04;
                return GetCmd(cmdGetVac, deviceID, bytes);
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
        public static void GetTestMessage(ref byte[] bytes)
        {
            bytes = GetPID();
            bytes[57] = 0X2B;
            bytes[58] = 0XAB;
            bytes[59] = 0XF3;
            bytes[60] = 0X40;
            bytes[61] = 0X92;
            bytes[62] = 0XE1;
                      
        }
        public static void GetRecvMessage(ref byte[] bytes)
        {
            bytes = GetPID();
            bytes[53] = 0x01;
            bytes[57] = 0X2B;
            bytes[58] = 0XAB;
            bytes[59] = 0XF3;
            bytes[60] = 0X40;
            bytes[61] = 0X53;
            bytes[62] = 0X2d;

        }
        #endregion

    }
    public class FreePort
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
        
    }

}
