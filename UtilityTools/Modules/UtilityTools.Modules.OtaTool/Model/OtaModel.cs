#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.OtaTool.Model
 * 唯一标识：2982f77f-c369-4c2d-b0fc-72a6e4b539d6
 * 文件名：OtaModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/10/12 13:45:17
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

using Microsoft.Win32;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Extension;
using UtilityTools.Modules.OtaTool.Protocol;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Modules.OtaTool.Model
{
    public class OtaModel :BindableBase
    {
        #region ------------Constructor------------
        public OtaModel(IContainerProvider containerProvider)
        {
            _containerProvider = containerProvider;
            _dialogHostService = containerProvider.Resolve<IDialogHostService>();

            UpdateCommand = new DelegateCommand(Update);
        }
        #endregion

        #region ------------Field------------
        private IContainerProvider _containerProvider;
        private readonly IDialogHostService _dialogHostService;
        #endregion

        #region ------------Property------------
        private IAsynRWService _serialPortService;
        /// <summary>
        /// 串口异步通信服务
        /// </summary>
        public IAsynRWService SerialPortService
        {
            get { return _serialPortService; }
            set { _serialPortService = value; RaisePropertyChanged(); }
        }

        private IAsynRWService _netUdpService;
        /// <summary>
        /// 串口异步通信服务
        /// </summary>
        public IAsynRWService NetUdpService
        {
            get { return _netUdpService; }
            set { _netUdpService = value; RaisePropertyChanged(); }
        }

        private string _status = "未连接";
        /// <summary>
        /// OTA状态
        /// 1.未连接
        /// 2.已连接
        /// 3.正在请求升级
        /// 4.正在上传升级文件
        /// 5.上传文件成功
        /// 6.正在重启设备
        /// </summary>
        public string Status
        {
            get { return _status; }
            set { _status = value; RaisePropertyChanged(); }
        }

        private string _updateFile;
        /// <summary>
        /// 升级文件
        /// </summary>
        public string UpdateFile
        {
            get { return _updateFile; }
            set { _updateFile = value; RaisePropertyChanged(); }
        }

        private int _maxFrameCount = 100;
        /// <summary>
        /// 最大传输帧数
        /// </summary>
        public int MaxFrameCount
        {
            get { return _maxFrameCount; }
            set { _maxFrameCount = value; RaisePropertyChanged(); }
        }

        private int _curFrameIndex = 0;
        /// <summary>
        /// 进度值
        /// </summary>
        public int CurFrameIndex
        {
            get { return _curFrameIndex; }
            set { _curFrameIndex = value; RaisePropertyChanged(); }
        }

        private string _log = "日志区域";

        public string Log
        {
            get { return _log; }
            set { _log = value; RaisePropertyChanged(); }
        }

        #endregion

        #region ------------Command------------
        public DelegateCommand UpdateCommand { get; set; }

        private async void Update()
        {
            if ((SerialPortService == null || !SerialPortService.IsOpen) &&
                (NetUdpService == null || !NetUdpService.IsOpen))
            {
                await _dialogHostService.Information("升级错误", "设备未连接，无法升级");
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Title = "请选择升级文件";
            dialog.Filter = "升级文件|*.bin;*.hex";
            var result = dialog.ShowDialog();
            if (result != null && result == true)
            {
                UpdateFile = dialog.FileName;

                if (File.Exists(UpdateFile))
                {
                    using (FileStream stream = File.Open(UpdateFile, FileMode.Open))
                    { 
                        int length = (int)stream.Length;

                        MaxFrameCount = (length / 32) + ((length % 32 > 0) ? 1 : 0);
                        ushort crc = 0;

                        var cmd = OtaProtocol.GetRequestOtaCmd(length, MaxFrameCount, crc);
                    }
                }

            }
        }
        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
