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
using System.Threading.Tasks;

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
            var netUdp = new UdpNetAsyncDevice();
            netUdp.Name = "升级网口";
            netUdp.DeviceInstance.TargetIp = "192.168.1.88";
            netUdp.DeviceInstance.TargetPort = 5000;
            netUdp.DeviceInstance.HostIp = "192.168.1.34";
            netUdp.DeviceInstance.HostPort = 5001;
            netUdp.IsBinary = true;
            NetUdpService = netUdp;
            UpdateCommand = new DelegateCommand(Update);
            LoadPackFileCommand = new DelegateCommand(LoadPackFile);

            _parser1 = new OtaToolProtocolParser();
            _parser1.Service = SerialPortService;
            _parser2 = new OtaToolProtocolParser();
            _parser2.Service = NetUdpService;
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
        private bool _isRestart = false;
        private bool _isTimeout = false;
        private bool _isStartUpgrade = false;
        private System.Timers.Timer _timer;
        private System.Timers.Timer _confirmTheUpgradeTimer;
        private int _interval = 1000;
        private int _confirmInterval = 500;

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
        public void ConfirmTheUpgrade()
        {
            if (_confirmTheUpgradeTimer == null)
            {
                _confirmTheUpgradeTimer = new System.Timers.Timer();
                _confirmTheUpgradeTimer.AutoReset = true;
                _confirmTheUpgradeTimer.Interval = _confirmInterval;
                _confirmTheUpgradeTimer.Elapsed += ConfirmTheUpgradeTimer_Elapsed;
            }
            _confirmTheUpgradeTimer.Start();
        }



        #endregion

        #region ------------PrivateMethod------------

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (DevelopmentBoardMessage != null && DevelopmentBoardMessage.DeviceID.HasValue)
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
        private void ConfirmTheUpgradeTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (DevelopmentBoardMessage != null && DevelopmentBoardMessage.DeviceID.HasValue)
            {
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
            if (_isUpdating)
            {
                Reset();
                var stopCmd = OtaProtocol.GetAbortUpdateCmd(DevelopmentBoardMessage.DeviceID.Value);
                if (SerialPortService.IsOpen)
                {
                    SerialPortService.SendMsg(stopCmd);
                }

                if (NetUdpService.IsOpen)
                {
                    NetUdpService.SendMsg(stopCmd);
                }
                Tips = "停止固件升级中";

                return;
            }

            if (string.IsNullOrWhiteSpace(SourcePath))
            {
                await _dialogHostService.Information("升级错误", "请选择合适的升级文件");//一个提示弹窗
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

            _isUpdating = true;
            UpgradeButtonName = "停止升级";
            _isStartUpgrade = true;
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
            bool result = false;
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
                    var data = File.ReadAllBytes(path);
                    var length = data.Length;
                    MaxFrameCount = (uint)(length + 31) / 32;
                    UpdateData = new byte[MaxFrameCount * 32];
                    Array.Fill<byte>(UpdateData, 0xFF);
                    Array.Copy(data, UpdateData, length);
                    CurFrameCount = 0;
                    UpdateDataCrc = CRCHelper.Data_GetCRC16(UpdateData, 0, UpdateData.Length);
                    result = true;
                }
            }
            Directory.Delete(destinationPath, true);
            return result;
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
            _isTimeout = false;
            var parser = sender as OtaToolProtocolParser;
            IAsynRWService? service = null;
            if (parser != null)
            {
                service = parser.Service;
            }
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
                        var firmwareVersim = DevelopmentBoardMessage.VersionNumber.Split('.');
                        var majorfirmwareVersim = firmwareVersim[0];
                        var minorfirmwareVersim = firmwareVersim[1];
                        //是否是重启查询后的结果
                        if (_isRestart == true)
                        {
                            if (majorfirmwareVersim[0] == 'v' || (majorfirmwareVersim[0] == 'V'))
                            {
                                majorfirmwareVersim = majorfirmwareVersim.Substring(1, majorfirmwareVersim.Length - 1);
                            }

                            if (Convert.ToUInt32(majorfirmwareVersim) == majorVersion && Convert.ToUInt32(minorfirmwareVersim) == minorVersion && crc == UpdateDataCrc)
                            {
                                Tips = "固件升级成功";
                            }
                            else
                            {
                                if (crc == UpdateDataCrc)
                                {
                                    Tips = "固件升级失败,请核对版本号是否输入正确";
                                }
                                else 
                                {
                                    Tips = "固件升级失败";
                                }
                                Reset();
                            }
                        }
                        else
                        {
                            //如果crc是0xffff 表示开发板是第一次进行升级
                            //当前固件版本和开发板固件版本crc不同时进行升级操作
                            if ((((Convert.ToUInt32(majorfirmwareVersim) != majorVersion || Convert.ToUInt32(minorfirmwareVersim) != minorVersion || crc != UpdateDataCrc ))
                                || (crc == 0xffff && majorVersion == 1 && minorVersion == 0)) 
                                && _isStartUpgrade == true)
                            {
                                var cmd = OtaProtocol.GetRequestOtaCmd((uint)UpdateData.Length, MaxFrameCount, DevelopmentBoardMessage.DeviceID.Value, UpdateDataCrc);
                                service?.SendMsg(cmd);
                                _isStartUpgrade = false;
                                Tips = "开始升级中，请耐心等待";
                            }
                            else
                            {
                                Reset();
                                Tips = "当前固件已是最新版本";
                            }
                           
                        }
                        _isRestart = false;
                        if (_confirmTheUpgradeTimer != null && _confirmTheUpgradeTimer.Enabled == true)
                        {
                            _confirmTheUpgradeTimer.Stop();
                            
                        } 
                    }
                    break;
                case EnumOtaCommandType.OTA_GET_STATUS:
                    var stateByte = e.DataSource[0];
                    if (stateByte == 0x00)
                    {
                        Status = "正常运行";
                    }
                    else
                    {
                        Status = "固件升级中";
                    }
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
                    Reset();
                    Tips = "已停止固件升级";
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
                            _isTimeout = true;
                            ExecuteWithTimeoutAsync(5000);
                        }
                        else
                        {
                            var cmd = OtaProtocol.GetRestartCmd(DevelopmentBoardMessage.DeviceID.Value);
                            _isRestart = true;
                            service?.SendMsg(cmd);
                            ConfirmTheUpgrade();
                            _isTimeout = true;
                            ExecuteWithTimeoutAsync(10000);
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
        public async Task WaitisTimeout()
        {
            await Task.Run(() => 
            {
                while (_isTimeout) 
                {
                    Thread.Sleep(0);
                }
            });
        }

        public async Task ExecuteWithTimeoutAsync( int timeout)
        {
            Task task = WaitisTimeout();
            Task timeoutTask = Task.Delay(timeout);

            Task firstCompletedTask = await Task.WhenAny(task, timeoutTask);

            if (firstCompletedTask == timeoutTask)
            {
                if (_isRestart == true)
                {
                    Tips = "开发板重启后长时间无响应。";
                }
                else
                {
                    Tips = "开发板传输数据时长时间无响应。";
                }

                task.Dispose();
            }
            else
            {
                timeoutTask.Dispose();
            }
        }
        private void Reset()
        {
            _isStartUpgrade = false;
            _isUpdating = false;
            UpgradeButtonName = "开始升级";
            CurFrameCount = 0;
        }

    }
    


    #endregion

    #region ------------StaticMethod------------
    #endregion

}
