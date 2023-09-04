#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Helper
 * 唯一标识：b727b6dc-5a5c-4bef-a3c0-38d293180173
 * 文件名：HardwareMethod
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/13 13:54:50
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

using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Management;
using Microsoft.VisualBasic.CompilerServices;
using NLog;

namespace UtilityTools.Core.Helper
{
    public static class HardwareMethod
    {
        /// <summary>
        /// 根据USB设备ID信息查找设备名称
        /// </summary>
        /// <param name="deviceID"></param>
        /// <returns></returns>
        public static string SearchDeviceByID(string deviceID)
        {
            if(string.IsNullOrEmpty(deviceID))
                return string.Empty;
            string[] available_spectrometers = SerialPort.GetPortNames();
            ManagementObjectCollection.ManagementObjectEnumerator enumerator = null;
            string commData = "";
            ManagementObjectSearcher mObjs = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM WIN32_PnPEntity");
            try
            {
                enumerator = mObjs.Get().GetEnumerator();
                while (enumerator.MoveNext())
                {
                    ManagementObject current = (ManagementObject)enumerator.Current;

                    if (current == null)
                        continue;

                    if (Strings.InStr(Conversions.ToString(current["Caption"]), "(COM", CompareMethod.Binary) <= 0)
                    {
                        continue;
                    }
                    //foreach (var property in current.Properties)
                    //{
                    //    Console.WriteLine(property.Name + ":" + property.Value);
                    //}
                    if (current["DeviceID"].ToString().Contains(deviceID))
                    {
                        var name = current["Name"].ToString();
                        //if (name.Contains('(') && name.Contains(')'))
                        //{
                        //    commData = name.Split('(', ')')[1];
                        //}
                        commData = name;
                        break;
                    }
                }
            }
            finally
            {
                if (enumerator != null)
                {
                    enumerator.Dispose();
                }
            }
            return commData;
        }

        /// <summary>
        /// 设置IP地址，掩码和网关
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="submask"></param>
        /// <param name="getway"></param>
        public static void SetIPAddress(string ip, string submask, string getway)
        {
            SetIPAddress(new string[] { ip }, new string[] { submask }, new string[] { getway }, null);
        }

        /// <summary>
        /// 设置IP地址，掩码，网关和DNS
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="submask"></param>
        /// <param name="getway"></param>
        /// <param name="dns"></param>
        public static void SetIPAddress(string[] ip, string[] submask, string[] getway, string[] dns)
        {
            ManagementClass wmi = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = wmi.GetInstances();
            ManagementBaseObject inPar = null;
            ManagementBaseObject outPar = null;
            string targetIP = GetTargetTypeIP(NetworkInterfaceType.Ethernet);
            if (targetIP == null || ip[0] == null || targetIP == ip[0])
                return;

            foreach (ManagementObject mo in moc)
            {
                //如果没有启用IP设置的网络设备则跳过
                if (!(bool)mo["IPEnabled"])
                    continue;

                string caption = mo["Caption"].ToString();
                string description = mo["Description"].ToString();
                string curIP = ((string[])mo["IPAddress"])[0];

                if (curIP != targetIP)
                {
                    continue;
                }

                //设置IP地址和掩码
                if (ip != null && submask != null)
                {
                    inPar = mo.GetMethodParameters("EnableStatic");
                    var testIP = inPar["IPAddress"];
                    var testMask = inPar["SubnetMask"];
                    inPar["IPAddress"] = ip;
                    inPar["SubnetMask"] = submask;
                    outPar = mo.InvokeMethod("EnableStatic", inPar, null);
                    inPar = mo.GetMethodParameters("EnableStatic");
                    testIP = inPar["IPAddress"];
                    testMask = inPar["SubnetMask"];
                }

                //设置网关地址
                if (getway != null)
                {
                    inPar = mo.GetMethodParameters("SetGateways");
                    inPar["DefaultIPGateway"] = getway;
                    outPar = mo.InvokeMethod("SetGateways", inPar, null);
                }

                //设置DNS地址
                if (dns != null)
                {
                    inPar = mo.GetMethodParameters("SetDNSServerSearchOrder");
                    inPar["DNSServerSearchOrder"] = dns;
                    outPar = mo.InvokeMethod("SetDNSServerSearchOrder", inPar, null);
                }

                //return;
            }
        }

        /// <summary>
        /// 判断是否符合IP地址格式
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsIPAddress(string ip)
        {
            if (string.IsNullOrEmpty(ip))
                return false;

            //将完整的IP以“.”为界限分组
            string[] arr = ip.Split('.');

            //判断IP是否为四组数组成
            if (arr.Length != 4)
                return false;


            //正则表达式，1~3位整数
            string pattern = @"\d{1,3}";
            for (int i = 0; i < arr.Length; i++)
            {
                string d = arr[i];


                //判断IP开头是否为0
                if (i == 0 && d == "0")
                    return false;


                //判断IP是否是由1~3位数组成
                if (!Regex.IsMatch(d, pattern))
                    return false;

                if (d != "0")
                {
                    //判断IP的每组数是否全为0
                    d = d.TrimStart('0');
                    if (d == "")
                        return false;

                    //判断IP每组数是否大于255
                    if (int.Parse(d) > 255)
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 获取本地IP列表中的第一个IP
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIP()
        {
            string addressIP = string.Empty;
            foreach (IPAddress ipAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ipAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    return ipAddress.ToString();
                }
            }

            return addressIP;
        }

        /// <summary>
        /// 获取目标类型的IP
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTargetTypeIP(NetworkInterfaceType type)
        {
            NetworkInterface[] interfacesInformation = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface networkInterface in interfacesInformation)
            {
                bool bIsInternet = networkInterface.Name.Contains("以太网");
                bool bIsOpen = networkInterface.OperationalStatus == OperationalStatus.Up;
                bool bIsEthernet = networkInterface.NetworkInterfaceType == type;

                if (bIsInternet && bIsEthernet)
                {
                    IPInterfaceProperties properties = networkInterface.GetIPProperties();
                    foreach (var ipAddressInformation in properties.UnicastAddresses)
                    {
                        if (ipAddressInformation.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            return ipAddressInformation.Address.ToString();
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 获取本地IP列表中与输入IP组成局域网的地址
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static string GetLANIP(string ip)
        {
            var ret = IPAddress.TryParse(ip, out var targetAdr);
            if (ret == false)
                return null;
            return GetLANIP(targetAdr).ToString();
        }

        /// <summary>
        /// 获取本地IP列表中与输入IP组成局域网的地址
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static IPAddress GetLANIP(IPAddress ip)
        {
            byte[] targetBytes = ip.GetAddressBytes();
            IPAddress addressIP = IPAddress.None;
            foreach (IPAddress ipAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ipAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    byte[] bytes = ipAddress.GetAddressBytes();
                    if (bytes[0] == targetBytes[0] &&
                        bytes[1] == targetBytes[1] &&
                        bytes[2] == targetBytes[2])
                    {
                        addressIP = ipAddress;
                    }
                }
            }

            return addressIP;
        }

        /// <summary>
        /// PING指定地址
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool PingRemoteIP(string ip)
        {
            try 
            {
                Ping ping = new Ping();
                PingReply pingReply = ping.Send(ip, 1000);
                return pingReply.Status != IPStatus.Success;
            }
            catch (Exception ex) 
            {
                LogManager.GetCurrentClassLogger().Error($"PING {ip} 异常：{ex.Message}");
                return false;
            }
        }
    }
}
