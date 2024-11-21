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

using System.Windows.Forms;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.IO;
using System.Text.Json;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Extension;
using UtilityTools.Core.Helper;
using UtilityTools.Modules.OtaTool.Protocol;
using UtilityTools.Services.Interfaces.IServices;
using System.Threading;
using Prism.Events;
using UtilityTools.Services.Services;
using System.Timers;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace UtilityTools.Modules.OtaTool.Model
{
    public class OtaModel : BindableBase
    {
        #region ------------Constructor------------
        public OtaModel(IContainerProvider containerProvider)
        {
            _containerProvider = containerProvider;
            _dialogHostService = containerProvider.Resolve<IDialogHostService>();
            SerialPortService = new SerialPortService();
            SerialPortService.Name = "升级串口";
            SerialPortService.IsBinary = true;
            NetUdpService = new UdpNetAsyncDevice();
            NetUdpService.Name = "升级网口";
            NetUdpService.IsBinary = true;
            UpdateCommand = new DelegateCommand(Update);
            LoadPackFileCommand = new DelegateCommand(LoadPackFile);

            _parser1 = new OtaToolProtocolParser();
            _parser2 = new OtaToolProtocolParser();
            SerialPortService.UpdateResponse += SerialPortService_UpdateResponse;
            NetUdpService.UpdateResponse += NetUdpService_UpdateResponse;
            _parser1.PacketReceivedEvent += Parser_PacketReceivedEvent;
            _parser2.PacketReceivedEvent += Parser_PacketReceivedEvent;

        }

        ~OtaModel()
        {
            if (SerialPortService != null)
            {
                SerialPortService.Close();
            }
        }
        #endregion

        #region ------------Field------------
        IEventAggregator _eventAggregator;
        private IContainerProvider _containerProvider;
        private readonly IDialogHostService _dialogHostService;
        private OtaToolProtocolParser _parser1;
        private OtaToolProtocolParser _parser2;

        private bool _isUpdating = false;
        private System.Timers.Timer _timer;
        private int _interval = 1000;

        #endregion

        #region ------------Property------------
        CancellationTokenSource source { get; set; }
        /// <summary>
        /// 串口异步通信服务
        /// </summary>
        public IAsynRWService SerialPortService { get; set; }

        private IAsynRWService _netUdpService;
        /// <summary>
        /// 网口异步通信服务
        /// </summary>
        public IAsynRWService NetUdpService
        {
            get { return _netUdpService; }
            set { _netUdpService = value; RaisePropertyChanged(); }
        }
        private string _upgradeButtonName = "开始升级";
        public string UpgradeButtonName
        {
            get { return _upgradeButtonName; }
            set { _upgradeButtonName = value; RaisePropertyChanged(); }
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

        private string _log = "日志区域";

        public string Log
        {
            get { return _log; }
            set { _log = value; RaisePropertyChanged(); }
        }

        private DevelopmentBoardMessage _developmentBoardMessage;
        /// <summary>
        /// 升级包信息
        /// </summary>
        public DevelopmentBoardMessage DevelopmentBoardMessage
        {
            get { return _developmentBoardMessage; }
            set { _developmentBoardMessage = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 升级数据
        /// </summary>
        public byte[] UpdateData { get; set; }


        private uint _maxFrameCount = 100;
        /// <summary>
        /// 最大传输帧数
        /// </summary>
        public uint MaxFrameCount
        {
            get { return _maxFrameCount; }
            set { _maxFrameCount = value; RaisePropertyChanged(); }
        }

        private uint _curFrameCount = 0;
        /// <summary>
        /// 当前传输帧数
        /// </summary>
        public uint CurFrameCount
        {
            get { return _curFrameCount; }
            set { _curFrameCount = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 更新数据的CRC校验码
        /// </summary>
        public ushort UpdateDataCrc { get; set; }

        private string _sourcePath;
        /// <summary>
        /// 选择的文件路径
        /// </summary>
        public string SourcePath
        {
            get { return _sourcePath; }
            set { _sourcePath = value; RaisePropertyChanged(); }
        }

        private string _tips;
        /// <summary>
        /// 提示信息
        /// </summary>
        public string Tips
        {
            get { return _tips; }
            set { _tips = value; RaisePropertyChanged(); }
        }

        #endregion

        #region ------------Command-----------
        public DelegateCommand UpdateCommand { get; set; }
        public DelegateCommand CmdTestCommand { get; set; }
        public DelegateCommand LoadPackFileCommand { get; set; }
        #endregion

        #region ------------PublicMethod------------
        public void StartRequestStatus()
        {
            if (_timer == null)
            {
                _timer = new System.Timers.Timer();
                _timer.AutoReset = true;
                _timer.Interval = _interval;
                _timer.Elapsed += Timer_Elapsed;
            }
            _timer.Start();
        }

        #endregion

        #region ------------PrivateMethod------------

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if(DevelopmentBoardMessage != null && DevelopmentBoardMessage.DeviceID.HasValue) 
            {
                var requestCmd = OtaProtocol.GetDeviceStatusCmd(DevelopmentBoardMessage.DeviceID.Value);
                if (SerialPortService.IsOpen)
                {
                    SerialPortService.SendMsg(requestCmd);
                }
                if (NetUdpService.IsOpen)
                {
                    NetUdpService.SendMsg(requestCmd);
                }
            }
            
        }

        private void LoadPackFile()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Title = "请选择升级文件";
            openFileDialog.Filter = "压缩文件|*.zip";
            Nullable<bool> result = openFileDialog.ShowDialog();
            if (result == true)
            {
                if (Unpack(openFileDialog.FileName))
                {
                    SourcePath = openFileDialog.FileName;
                }
            }
        }
        /// <summary>
        /// 升级
        /// </summary>
        private async void Update()
        {
            if (string.IsNullOrWhiteSpace(SourcePath))
            {
                await _dialogHostService.Information("升级错误", "请选择合适的升级文件");
                return;
            }

            if (DevelopmentBoardMessage == null || !DevelopmentBoardMessage.DeviceID.HasValue)
            {
                await _dialogHostService.Information("升级错误", "升级信息文件异常");
                return;
            }

            // 检查是否连接设备
            if ((SerialPortService == null || !SerialPortService.IsOpen) && (NetUdpService == null || !NetUdpService.IsOpen))
            {
                await _dialogHostService.Information("升级错误", "设备未连接，无法升级");
                return;
            }

            // 检查设备类型是否匹配
            var requestCmd = OtaProtocol.GetUpdateFrameInfoCmd(DevelopmentBoardMessage.DeviceID.Value);
            if (SerialPortService.IsOpen)
            {
                SerialPortService.SendMsg(requestCmd);
            }

            if (NetUdpService.IsOpen)
            {
                NetUdpService.SendMsg(requestCmd);
            }
        }

        /// <summary>
        /// 拆包
        /// </summary>
        private bool Unpack(string filePath)
        {
            string testPath = Path.GetDirectoryName(filePath) + @"\" + Path.GetFileNameWithoutExtension(filePath);
            if (Directory.Exists(testPath))
            {
                Tips = "当前文件夹下存在同名文件";
                return false;
            }
            //先解压文件
            string destinationPath = ZipCompress.ZipExtract(filePath);
            //再将文件中的json文件读取出来
            string jsonString = File.ReadAllText(Path.Combine(destinationPath, "DevelopmentBoardMessage.json"));
            DevelopmentBoardMessage = JsonSerializer.Deserialize<DevelopmentBoardMessage>(jsonString);
            if (DevelopmentBoardMessage != null)
            {
                var path = Path.Combine(destinationPath, DevelopmentBoardMessage.FileName);
                if (File.Exists(path))
                {
                    UpdateData = File.ReadAllBytes(path);
                    var length = UpdateData.Length;
                    MaxFrameCount = (uint)(length + 31) / 32;
                    CurFrameCount = 0;
                    UpdateDataCrc = CRCHelper.Data_GetCRC16(UpdateData, 0, length);
                    return true;
                }
            }
            Directory.Delete(destinationPath);
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPortService_UpdateResponse(object? sender, byte[] e)
        {
            _parser1.ReceiveBytes(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NetUdpService_UpdateResponse(object? sender, byte[] e)
        {
            _parser2.ReceiveBytes(e);
        }

        /// <summary>
        /// 开发板回报接收事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Parser_PacketReceivedEvent(object? sender, OtaToolDataPacket e)
        {
            var service = sender as IAsynRWService;
            switch (e.CmdType)
            {
                case EnumOtaCommandType.OTA_GET_HWV:
                    break;

                case EnumOtaCommandType.OTA_GET_FMV://接收到消息后发送数据

                    break;

                case EnumOtaCommandType.OTA_SYS_BROADCAST:

                    break;

                case EnumOtaCommandType.OTA_GET_UPGRADE_FMV:
                    {
                        if (!_isUpdating)
                        {
                            return;
                        }
                        //还需要判断一下版本号
                        byte majorVersion = e.DataSource[0];
                        byte minorVersion = e.DataSource[1];
                        ushort year = BitConverter.ToUInt16(e.DataSource, 2);
                        byte mouth = e.DataSource[4];
                        byte day = e.DataSource[5];
                        ushort crc = BitConverter.ToUInt16(e.DataSource, 6);

                        if (crc != UpdateDataCrc)
                        {
                            var cmd = OtaProtocol.GetRequestOtaCmd((uint)UpdateData.Length, MaxFrameCount, DevelopmentBoardMessage.DeviceID.Value, UpdateDataCrc);
                            service?.SendMsg(cmd);
                        }
                    }
                    break;
                case EnumOtaCommandType.OTA_GET_STATUS:
                    
                    break;
                case EnumOtaCommandType.OTA_REQUEST:
                    {
                        if (!_isUpdating)
                        {
                            return;
                        }
                        CurFrameCount = 0;
                        var data = new byte[32];
                        Array.Copy(UpdateData, CurFrameCount * 32, data, 0, 32);
                        var cmd = OtaProtocol.GetTransferOtaCmd(CurFrameCount, data, DevelopmentBoardMessage.DeviceID.Value);
                        service?.SendMsg(cmd);
                    }
                    break;
                case EnumOtaCommandType.OTA_ABORT:
                    _isUpdating = false;
                    break;
                case EnumOtaCommandType.OTA_TRANSFER://接收到回报发送下一个回报
                    {
                        uint frameID = BitConverter.ToUInt32(e.DataSource, 0);
                        if (!_isUpdating)
                        {
                            return;
                        }
                        CurFrameCount = frameID + 1;
                        if (CurFrameCount < MaxFrameCount)
                        {
                            var data = new byte[32];
                            Array.Copy(UpdateData, CurFrameCount * 32, data, 0, 32);
                            var cmd = OtaProtocol.GetTransferOtaCmd(CurFrameCount, data, DevelopmentBoardMessage.DeviceID.Value);
                            service?.SendMsg(cmd);
                        }
                        else 
                        {
                            var cmd = OtaProtocol.GetRestartCmd(DevelopmentBoardMessage.DeviceID.Value);
                            service?.SendMsg(cmd);
                        }
                    }
                    break;
                case EnumOtaCommandType.OTA_RESTART:
                    if (!_isUpdating)
                    {
                        return;
                    }
                    break;
            }
        }
        private void Wait()
        {

        }

    }


    #endregion

    #region ------------StaticMethod------------
    #endregion

}
