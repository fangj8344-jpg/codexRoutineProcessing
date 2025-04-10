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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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
        public static readonly ushort ECmdName;

        //public static readonly ushort cmdGetVac = 0x0800;
        //public static readonly ushort cmdGetPID = 0X0806;
        //public static readonly ushort cmdGetSTATUS = 0x0501;
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

        public static byte[] GetCmd(string IpAddress, ushort address,
            ushort command, ushort deviceID, byte[] data)
        {
            // data段固定36字节，不足用零填充
            if (data.Length < 36)
            {
                var tmp = new byte[36];
                Array.Clear(tmp, 0, tmp.Length);

                Buffer.BlockCopy(data, 0, tmp, 0, data.Length);
                data = tmp;
            }

            IPAddress ipAddress = IPAddress.Parse(IpAddress);
            byte[] Ip = ipAddress.GetAddressBytes();//得到4字节IP数组
            Array.Reverse(Ip);//反转字节数组 反转为小端
            var addr = BitConverter.GetBytes(address);

            byte[] combinedAddr = new byte[Ip.Length + addr.Length];
            Array.Copy(Ip, 0, combinedAddr, 0, Ip.Length);
            Array.Copy(addr, 0, combinedAddr, Ip.Length, addr.Length);

            var cmd = BitConverter.GetBytes(command);
            var id = BitConverter.GetBytes(deviceID);
            return ZepGenericProtocol.GetCmd(combinedAddr, id, cmd, data);
        }


        public static void GetSendMessage(ref byte[] bytes, string IpAddress,ushort cmd,
            ushort addr = 0x0201, ushort deviceID = 0x0201)
        {
            byte[] bytes_temp = new byte[36];
            bytes = GetCmd(IpAddress, addr, cmd, deviceID, bytes_temp);
          /*  bytes[57] = 0X2B;
            bytes[58] = 0XAB;
            bytes[59] = 0XF3;
            bytes[60] = 0X40;
            bytes[61] = 0X92;
            bytes[62] = 0XE1;*/

        }
        public static void GetRecvMessage(ref byte[] bytes, string IpAddress, ushort cmd,
            ushort addr = 0x0201, ushort deviceID = 0x0101)
        {
            byte[] bytes_temp = new byte[36];
            bytes = GetCmd(IpAddress, addr, cmd, deviceID, bytes_temp);
        /*    bytes[53] = 0x01;
            bytes[57] = 0X2B;
            bytes[58] = 0XAB;
            bytes[59] = 0XF3;
            bytes[60] = 0X40;
            bytes[61] = 0X53;
            bytes[62] = 0X2d;*/

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
