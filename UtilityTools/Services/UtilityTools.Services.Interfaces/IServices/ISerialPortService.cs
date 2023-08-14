using System;
using System.Collections.Generic;
using System.Text;
using UtilityTools.Core.Model;

namespace UtilityTools.Services.Interfaces.IServices
{
    public interface ISerialPortService
    {
        /// <summary>
        /// USB信息
        /// </summary>
        UsbInfoModel? UsbInfo { get; set; }

        /// <summary>
        /// 串口实例
        /// </summary>
        SerialPortModel DeviceInstance { get; set; }
    }
}
