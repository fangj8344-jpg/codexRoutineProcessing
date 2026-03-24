using System;
using System.Collections.Generic;
using System.Text;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Services.Interfaces
{
    public interface IServiceFactory
    {
        /// <summary>
        /// 是否是仿真模式
        /// </summary>
        public bool IsVirtualDevice { get; }

        /// <summary>
        /// 获取相机设备
        /// </summary>
        /// <param name="type">设备类型</param>
        /// <returns></returns>
        public ICameraService GetCameraService(string type);

        /// <summary>
        /// 获取USB设备
        /// </summary>
        /// <param name="type">设备类型</param>
        /// <returns></returns>
        public IUsbService GetUsbService(string type);

        /// <summary>
        /// 获取异步读写设备
        /// </summary>
        /// <param name="type">设备类型</param>
        /// <param name="name">设备名称</param>
        /// <returns></returns>
        public IAsynRWService GetAsynRWService(string type, string name = null);

        /// <summary>
        /// 获取同步读写设备
        /// </summary>
        /// <param name="type">设备类型</param>
        /// <returns></returns>
        public ISyncRWService GetSyncRWService(string type);

        public IThingboardService GetThingboardService();
    }
}
