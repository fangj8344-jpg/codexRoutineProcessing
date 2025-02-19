using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Model;

namespace UtilityTools.Core.Helper
{
    /// <summary>
    /// 开发板烧录的文件信息
    /// </summary>
    public class DevelopmentBoardMessage : BindableBase
    {
        private string _fileName;
        /// <summary>
        /// 升级文件名
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; RaisePropertyChanged(); }
        }

        private EnumDeviceID? _deviceIDEnum;
        /// <summary>
        /// 升级设备类型枚举类型
        /// </summary>
        public EnumDeviceID? DeviceIDEnum
        {
            get { return _deviceIDEnum; }
            set { _deviceIDEnum = value; DeviceID = (int?)value; RaisePropertyChanged(); }
        }
        private int? _deviceID;
        /// <summary>
        /// 升级设备类型
        /// </summary>
        public int? DeviceID
        {
            get { return _deviceID; }
            set { _deviceID = value; RaisePropertyChanged(); }
        }

        private string _versionNumber;
        /// <summary>
        /// 版本号
        /// </summary>
        public string VersionNumber
        {
            get { return _versionNumber; }
            set { _versionNumber = value; RaisePropertyChanged(); }
        }

        private string _updateInformation;
        /// <summary>
        /// 版本升级信息
        /// </summary>
        public string UpdateInformation
        {
            get { return _updateInformation; }
            set { _updateInformation = value; RaisePropertyChanged(); }
        }

        private string _description;
        /// <summary>
        /// 升级文件描述
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; RaisePropertyChanged(); }
        }

        private string _hardwareVersion;
        /// <summary>
        /// 硬件版本信息
        /// </summary>
        public string HardwareVersion
        {
            get { return _hardwareVersion; }
            set { _hardwareVersion = value; RaisePropertyChanged(); }
        }
        private string _firmwareName;
        //固件名称
        public string FirmwareName
        {
            get { return _firmwareName; }
            set { _firmwareName = value; RaisePropertyChanged(); }
        }
    }
    
}
