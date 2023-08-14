#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Services
 * 唯一标识：58e4cf69-25b7-481c-bd73-603fb2c0d660
 * 文件名：ServiceFactory
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/13 11:16:16
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
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Model;
using UtilityTools.Services.Interfaces;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.Services.Services;

namespace UtilityTools.Services
{
    public class ServiceFactory : IServiceFactory
    {
        #region ------------Constructor------------
        #endregion

        #region ------------Field------------
        private static Dictionary<string, ICameraService> CameraServices = new Dictionary<string, ICameraService>();
        private static Dictionary<string, IUsbService> CyUsbServices = new Dictionary<string, IUsbService>();
        private static Dictionary<string, IAsynRWService> AsynRWServices = new Dictionary<string, IAsynRWService>();
        private static Dictionary<string, ISyncRWService> SyncRWServices = new Dictionary<string, ISyncRWService>();
        #endregion

        #region ------------Property------------
        public bool IsVirtualDevice { get; }
        #endregion

        #region ------------PublicMethod------------
        /// <summary>
        /// 获取异步读写设备
        /// </summary>
        /// <param name="type">设备类型</param>
        /// <returns></returns>
        public IAsynRWService GetAsynRWService(string type)
        {
            lock (this)
            {
                if (IsVirtualDevice)
                    type = "VirtualDevice";

                if (AsynRWServices.ContainsKey(type))
                    return AsynRWServices[type];
                switch (type)
                {
                    case "VirtualDevice":
                        {
                            IAsynRWService device = new VirtualAsynService();
                            AsynRWServices.Add(type, device);
                            return device;
                        }
                    case "SPVM":    // SerialPort of VacMonitor(串口真空监控设备)
                        {
                            return new SerialPortService();
                        }
                    // 串口高压设备
                    case "SPHV":
                        //{
                        //    var usbInfo = new UsbInfoModel(0x7523, 0x1A86);
                        //    IAsynRWService device = new SerialPortService(usbInfo);
                        //    AsynRWServices.Add(type, device);
                        //    return device;
                        //}
                    // TCP网口高压设备
                    case "TNHV":
                        //{
                        //    var netInfo = new NetInfoModel();
                        //    var content = ConfigurationManager.AppSettings["HostIP"];
                        //    if (HardwareMethod.IsIPAddress(content))
                        //        netInfo.HostIP = content;
                        //    else
                        //        netInfo.HostIP = "192.168.1.33";
                        //    content = ConfigurationManager.AppSettings["HVBoardNetHostPort"];
                        //    if (!string.IsNullOrEmpty(content) && int.TryParse(content, out int hostPort))
                        //        netInfo.HostPort = hostPort;
                        //    else
                        //        netInfo.HostPort = 8702;
                        //    content = ConfigurationManager.AppSettings["HVBoardNetIP"];
                        //    if (HardwareMethod.IsIPAddress(content))
                        //        netInfo.TargetIP = content;
                        //    else
                        //        netInfo.TargetIP = "192.168.1.12";
                        //    content = ConfigurationManager.AppSettings["HVBoardNetPort"];
                        //    if (!string.IsNullOrEmpty(content) && int.TryParse(content, out int targetPort))
                        //        netInfo.TargetPort = targetPort;
                        //    else
                        //        netInfo.TargetPort = 8802;
                        //    IAsynRWDevice device = new TcpNetAsyncDevice(netInfo);
                        //    AsynRWDevices.Add(type, device);
                        //    return device;
                        //}
                    // TCP网口SE设备
                    case "TNSE":
                        //{
                        //    var netInfo = new NetInfoModel();
                        //    var content = ConfigurationManager.AppSettings["HostIP"];
                        //    if (HardwareMethod.IsIPAddress(content))
                        //        netInfo.HostIP = content;
                        //    else
                        //        netInfo.HostIP = "192.168.1.33";
                        //    content = ConfigurationManager.AppSettings["SEBoardHostPort"];
                        //    if (!string.IsNullOrEmpty(content) && int.TryParse(content, out int hostPort))
                        //        netInfo.HostPort = hostPort;
                        //    else
                        //        netInfo.HostPort = 8703;
                        //    content = ConfigurationManager.AppSettings["SEBoardIP"];
                        //    if (HardwareMethod.IsIPAddress(content))
                        //        netInfo.TargetIP = content;
                        //    else
                        //        netInfo.TargetIP = "192.168.1.12";
                        //    content = ConfigurationManager.AppSettings["SEBoardPort"];
                        //    if (!string.IsNullOrEmpty(content) && int.TryParse(content, out int targetPort))
                        //        netInfo.TargetPort = targetPort;
                        //    else
                        //        netInfo.TargetPort = 8803;
                        //    IAsynRWDevice device = new TcpNetAsyncDevice(netInfo);
                        //    AsynRWDevices.Add(type, device);
                        //    return device;
                        //}
                    // TCP真空检测设备
                    case "TNVM":
                        //{
                        //    var netInfo = new NetInfoModel();
                        //    var content = ConfigurationManager.AppSettings["HostIP"];
                        //    if (HardwareMethod.IsIPAddress(content))
                        //        netInfo.HostIP = content;
                        //    else
                        //        netInfo.HostIP = "192.168.1.33";
                        //    content = ConfigurationManager.AppSettings["VacMonitorHostPort"];
                        //    if (!string.IsNullOrEmpty(content) && int.TryParse(content, out int hostPort))
                        //        netInfo.HostPort = hostPort;
                        //    else
                        //        netInfo.HostPort = 8702;
                        //    content = ConfigurationManager.AppSettings["VacMonitorIP"];
                        //    if (HardwareMethod.IsIPAddress(content))
                        //        netInfo.TargetIP = content;
                        //    else
                        //        netInfo.TargetIP = "192.168.1.12";
                        //    content = ConfigurationManager.AppSettings["VacMonitorPort"];
                        //    if (!string.IsNullOrEmpty(content) && int.TryParse(content, out int targetPort))
                        //        netInfo.TargetPort = targetPort;
                        //    else
                        //        netInfo.TargetPort = 8802;
                        //    IAsynRWDevice device = new TcpNetAsyncDevice(netInfo);
                        //    AsynRWDevices.Add(type, device);
                        //    return device;
                        //}
                    case "TNSM":
                        //{
                        //    var netInfo = new NetInfoModel();
                        //    var content = ConfigurationManager.AppSettings["HostIP"];
                        //    if (HardwareMethod.IsIPAddress(content))
                        //        netInfo.HostIP = content;
                        //    else
                        //        netInfo.HostIP = "192.168.1.33";
                        //    content = ConfigurationManager.AppSettings["Stm28BoardHostPort"];
                        //    if (!string.IsNullOrEmpty(content) && int.TryParse(content, out int hostPort))
                        //        netInfo.HostPort = hostPort;
                        //    else
                        //        netInfo.HostPort = 8704;
                        //    content = ConfigurationManager.AppSettings["Stm28BoardIP"];
                        //    if (HardwareMethod.IsIPAddress(content))
                        //        netInfo.TargetIP = content;
                        //    else
                        //        netInfo.TargetIP = "192.168.1.12";
                        //    content = ConfigurationManager.AppSettings["Stm28BoardPort"];
                        //    if (!string.IsNullOrEmpty(content) && int.TryParse(content, out int targetPort))
                        //        netInfo.TargetPort = targetPort;
                        //    else
                        //        netInfo.TargetPort = 8804;
                        //    IAsynRWDevice device = new TcpNetAsyncDevice(netInfo);
                        //    AsynRWDevices.Add(type, device);
                        //    return device;
                        //}
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// 获取相机设备
        /// </summary>
        /// <param name="type">设备类型</param>
        /// <returns></returns>
        public ICameraService GetCameraService(string type)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取同步读写设备
        /// </summary>
        /// <param name="type">设备类型</param>
        /// <returns></returns>
        public ISyncRWService GetSyncRWService(string type)
        {
            lock (this)
            {
                if (IsVirtualDevice)
                    type = "VirtualDevice";

                if (SyncRWServices.ContainsKey(type))
                    return SyncRWServices[type];
                switch (type)
                {
                    case "VirtualDevice":
                        {
                            ISyncRWService device = new VirtualSyncService();
                            SyncRWServices.Add(type, device);
                            return device;
                        }
                    // UDP网口主控设备
                    case "UNCB":
                        {
                            ISyncRWService device = new UdpNetSyncDevice();
                            SyncRWServices.Add(type, device);
                            return device;
                        }
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// 获取USB设备
        /// </summary>
        /// <param name="type">设备类型</param>
        /// <returns></returns>
        public IUsbService GetUsbService(string type)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion

    }
}
