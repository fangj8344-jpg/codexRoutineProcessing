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
        private static IThingboardService ThingboardService;
        #endregion

        #region ------------Property------------
        public bool IsVirtualDevice { get; }

        public static IAsynRWService? GetAsyRWDevice(string v)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region ------------PublicMethod------------
        /// <summary>
        /// 获取异步读写设备
        /// </summary>
        /// <param name="type">设备类型</param>
        /// <returns></returns>
        public IAsynRWService GetAsynRWService(string type, string name = null)
        {
            lock (this)
            {
                if (IsVirtualDevice)
                    type = "VirtualDevice";

                if (AsynRWServices.ContainsKey(type))
                    return AsynRWServices[type];

                if (type == "GSP") // 通用串口设备
                {
                    string cache_key = $"GSP:{name}";
                    if (AsynRWServices.ContainsKey(cache_key))
                        return AsynRWServices[cache_key];
                }

                switch (type)
                {
                    case "VirtualDevice":
                        {
                            IAsynRWService device = new VirtualAsynService();
                            AsynRWServices.Add(type, device);
                            device.Name = "虚拟异步读写设备";
                            return device;
                        }
                    case "SPVM":    // SerialPort of VacMonitor(串口真空监控设备)
                        {
                            IAsynRWService device = new SerialPortService();
                            AsynRWServices.Add(type, device);
                            device.Name = "真空监控设备";
                            device.IsBinary = false;
                            return device;
                        }
                    case "SPCCS":    // SerialPort of CCS(串口真空监控设备)
                        {
                            IAsynRWService device = new SerialPortService();
                            AsynRWServices.Add(type, device);
                            device.Name = "CCS控制设备";
                            device.IsBinary = true;
                            device.MinWriteInterval = 0;
                            return device;
                        }
                    case "SPMC":    // SerialPort of Motor Controller
                        {
                            IAsynRWService device = new SerialPortService();
                            AsynRWServices.Add(type, device);
                            device.Name = "电机控制";
                            device.IsBinary = true;
                            return device;
                        }
                    case "SPHV":    // SerialPort of High Vol
                        {
                            IAsynRWService device = new SerialPortService();
                            AsynRWServices.Add(type, device);
                            device.Name = "高压控制";
                            device.IsBinary = true;
                            return device;
                        }
                    case "GSP": // General SerialPort device 通用串口设备
                        {
                            IAsynRWService device = new SerialPortService();
                            string cache_key = $"GSP-{name}";

                            AsynRWServices.Add(cache_key, device);
                            device.Name = name;
                            device.IsBinary = true;
                            return device;
                        }
                    case "UNHV":    // Udp Net of High Vol
                        {
                            IAsynRWService device = new UdpNetAsyncDevice();
                            AsynRWServices.Add(type, device);
                            device.Name = "高压控制";
                            device.IsBinary = true;
                            return device;
                        }
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
                            device.Name = "虚拟同步读写设备";
                            return device;
                        }
                    // UDP网口主控设备
                    case "UNCB":
                        {
                            ISyncRWService device = new UdpNetSyncDevice();
                            SyncRWServices.Add(type, device);
                            device.Name = "控制板";
                            device.IsBinary = false;
                            return device;
                        }
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// 获取Thingboard数据上传服务
        /// </summary>
        /// <returns></returns>
        public IThingboardService GetThingboardService()
        {
            //单例模式:如果已经创建过，直接返回
            if (ThingboardService == null)
            {
                ThingboardService = new ThingboardService();
            }
            return ThingboardService;
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
