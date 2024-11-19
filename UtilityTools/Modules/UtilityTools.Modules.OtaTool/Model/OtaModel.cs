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
using System.Windows.Forms;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Extension;
using UtilityTools.Core.Helper;
using UtilityTools.Modules.OtaTool.Protocol;
using UtilityTools.Services.Interfaces.IServices;
using static OpenCvSharp.Stitcher;
using CsvHelper.Configuration.Attributes;
using System.IO.Ports;
using System.Runtime.Intrinsics.Arm;
using System.Windows.Markup;
using System.Threading;
using UtilityTools.Core.Protocol;
using CsvHelper;
using System.Threading.Tasks;
using OpenCvSharp.XImgProc;
using Prism.Events;
using UtilityTools.Services.Interfaces;

namespace UtilityTools.Modules.OtaTool.Model
{
    public class OtaModel : BindableBase
    {
        #region ------------Constructor------------
        public OtaModel(IContainerProvider containerProvider)
        {
            _containerProvider = containerProvider;
            _dialogHostService = containerProvider.Resolve<IDialogHostService>();
            SerialPortService = _containerProvider.Resolve<IServiceFactory>().GetAsynRWService("GSP");
            UpdateCommand = new DelegateCommand(Update);
            LoadPackFileCommand = new DelegateCommand(LoadPackFile);
           
            _parser = new OtaToolProtocolParser();
            SerialPortService.UpdateResponse += _serialPortService_UpdateResponse;
            _parser.PacketReceivedEvent += Parser_PacketReceivedEvent;
            

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
        private OtaToolProtocolParser _parser;
        private byte _majorVersionNumber;
        private byte _subVersionNumber;
        private bool _isStartUpgrade = false;
        private bool _isUpgradePacketReceived = false;
        private bool _isEnddatePacket = false;
        private byte[] _programContent;
        private byte[] _programeFileContent;
        private int _programeFileContentAdder = 0;
        /// <summary>
        /// 帧序号
        /// </summary>
        private UInt32 _frameSerialNumber = 0;

        #endregion

        #region ------------Property------------
        CancellationTokenSource source {  get; set; }
        /// <summary>
        /// 串口异步通信服务
        /// </summary>
        public IAsynRWService SerialPortService{get;set;}

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
        private UInt32 _expectFrameCount = 100;
        /// <summary>
        /// 预期传输帧数
        /// </summary>
        public UInt32 ExpectFrameCount
        {
            get { return _expectFrameCount; }
            set { _expectFrameCount = value; RaisePropertyChanged(); }
        }


        private string _log = "日志区域";

        public string Log
        {
            get { return _log; }
            set { _log = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 设备ID
        /// </summary>
        private EnumDeviceID? deviceID;
        public EnumDeviceID? DeviceID
        {
            get { return deviceID; }
            set { deviceID = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 板卡类型
        /// </summary>
        private string _developmentBoardType;
        public string DevelopmentBoardType
        {
            get { return _developmentBoardType; }
            set { _developmentBoardType = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 烧录文件的版本号
        /// </summary>
        private string versionNumber;
        public string VersionNumber
        {
            get { return versionNumber; }
            set { versionNumber = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 版本变更信息
        /// </summary>
        private string updataInformation;
        public string UpdataInformation
        {
            get { return updataInformation; }
            set { updataInformation = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// 备注信息
        /// </summary>
        private string description;
        public string Description
        {
            get { return description; }
            set { description = value; RaisePropertyChanged(); }
        }
       
        private string _sourcePath;
        /// <summary>
        /// 选择的文件路径
        /// </summary>
        public string SourcePath
        {
            get { return _sourcePath; }
            set { _sourcePath = value; RaisePropertyChanged(); }
        }


        private int curFrameIndex = 0;
        /// <summary>
        /// 进度条实时信息
        /// </summary>
        public int CurFrameIndex
        {
            get { return curFrameIndex; }
            set { curFrameIndex = value; RaisePropertyChanged(); }
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

        /// <summary>
        /// 开发板信息
        /// </summary>
        public DevelopmentBoardMessage developmentBoardMessage { get; set; }

        #endregion

        #region ------------Command-----------
        public DelegateCommand UpdateCommand { get; set; }
        public DelegateCommand CmdTestCommand { get; set; }
        public DelegateCommand LoadPackFileCommand { get; set; }
        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        private void LoadPackFile()
        {

            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Title = "请选择升级文件";
            openFileDialog.Filter = "压缩文件|*.zip";
            Nullable<bool> result = openFileDialog.ShowDialog();
            if (result == true)
            {
                SourcePath = openFileDialog.FileName;
            }
            else
            {
                return;
            }

        }
        /// <summary>
        /// 升级
        /// </summary>
        private async void Update()
        {
            source = new CancellationTokenSource();
            if (UpgradeButtonName == "开始升级")
            {
                UpgradeButtonName = "停止升级";
                if ((SerialPortService == null || !SerialPortService.IsOpen) && (NetUdpService == null || !NetUdpService.IsOpen))
                {
                    await _dialogHostService.Information("升级错误", "设备未连接，无法升级");
                    UpgradeButtonName = "开始升级";
                    return;
                }
                string FirmwarePath = Unpack(); //拿到升级的固件信息
                SerialPortService.SendMsg(OtaProtocol.GetCmd(EnumOtaCommandType.OTA_GET_UPGRADE_FMV, DeviceID, new byte[36]));//查询固件版本信息
                if (!WaitReceiveFMV(5000))
                {
                    Tips = "开发板没有响应";
                    UpgradeButtonName = "开始升级";
                    return;
                }
                if (_isStartUpgrade == true)
                {
                    startFirmwareUpdate(FirmwarePath);//发送固件升级命令
                }
                QueryDeviceStatue();
                SendUpgradePacket();
               
                //UpgradeButtonName = "开始升级";
            }
            else
            {
                
                StopUpdate();
                _isStartUpgrade = false;
                UpgradeButtonName = "开始升级";
            }
            
        }
        private  bool WaitReceiveFMV(int timeout)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            Task task1 = Task.Run(() => 
            {
                while (_isStartUpgrade != true ) 
                {
                    token.ThrowIfCancellationRequested();   
                }
            },token);
            task1.Start();
            bool result = task1.Wait(timeout);
            if (result)
            {
                cts.Cancel();
            }
            task1.Dispose();
            cts.Dispose();
            return result;
           
         
        }
        private bool WaitReceiveTransfer(int timeout)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            Task task1 = Task.Run(() =>
            {
                while (_isUpgradePacketReceived != true)
                {
                    token.ThrowIfCancellationRequested();
                }
            }, token);
            task1.Start();
            bool result = task1.Wait(timeout);
            if (result)
            {
                cts.Cancel();
            }
            task1.Dispose();
            cts.Dispose();
            return result;


        }
       
        /// <summary>
        /// 固件升级命令
        /// </summary>
        /// <param name="measureFilePath"></param>
        /// <exception cref="ArgumentException"></exception>
        private void startFirmwareUpdate(string measureFilePath)
        {
            byte[] dataBytes = new byte[36];
            UInt32 fileSize = 0;
            Array.Clear(dataBytes, 0, dataBytes.Length);
            if (File.Exists(measureFilePath))
            {
                FileInfo file = new FileInfo(measureFilePath);
                fileSize = (UInt32)file.Length;
                var fileSizeBytes = BitConverter.GetBytes(fileSize);
                UInt16 crc16 = 0;
                //计算长度
                FileInfo fileInfo = new FileInfo(measureFilePath);
                using (var fileStream = fileInfo.OpenRead())
                {
                    StreamReader readFile = new StreamReader(fileStream);
                    readFile.BaseStream.Seek(0, SeekOrigin.Begin);
                    _programeFileContent = new byte[fileSize];
                    readFile.BaseStream.Read(_programeFileContent, 0, (int)fileSize);
                    crc16 = CRCHelper.Data_GetCRC16(_programeFileContent, 0, (int)fileSize);
                    _programeFileContentAdder = 0;
                }
                byte[] data = new byte[36];
                fileSizeBytes.CopyTo(data, 0);
                ExpectFrameCount = BitConverter.ToUInt32(fileSizeBytes) / 32;
                if (BitConverter.ToUInt32(fileSizeBytes) % 32 != 0)
                {
                    ExpectFrameCount++;
                }
                BitConverter.GetBytes(ExpectFrameCount).CopyTo(data, fileSizeBytes.Length);
                BitConverter.GetBytes(crc16).CopyTo(data, fileSizeBytes.Length + BitConverter.GetBytes(ExpectFrameCount).Length);
                SerialPortService.SendMsg(OtaProtocol.GetCmd(EnumOtaCommandType.OTA_GET_FMV, DeviceID, data));
            }
            else
            {
                throw new ArgumentException("文件路径不存在");
                return;
            }
        }
        /// <summary>
        /// 停止升级指令
        /// </summary>
        private void StopUpdate()
        {
            SerialPortService.SendMsg(OtaProtocol.GetCmd(EnumOtaCommandType.OTA_ABORT, DeviceID, new byte[36]));
        }
  
        
        /// <summary>
        /// 传输完成，重启升级
        /// </summary>
        private void RestartAndUpgrade()
        {
            SerialPortService.SendMsg(OtaProtocol.GetCmd(EnumOtaCommandType.OTA_RESTART, DeviceID,new byte[36]));
        }


        private async void QueryDeviceStatue()
        {
            await Task.Run(() =>
            {
                while (_isStartUpgrade == true)
                {
                    Thread.Sleep(100);
                    SerialPortService.SendMsg(OtaProtocol.GetCmd(EnumOtaCommandType.OTA_GET_STATUS, DeviceID, new byte[36]));
                }
            });
        }
        private async void SendUpgradePacket()
        {
        //继续发送固件升级数据传输
            await Task.Run(() =>
                {
                    _programContent = new byte[32];
                    _programeFileContentAdder = 0;
                    int length = _programeFileContent.Length;
                    while (_isStartUpgrade == true)
                    {
                        if (WaitReceiveTransfer(5000))
                        {
                            if (_isUpgradePacketReceived && length >= _programeFileContentAdder)
                            {
                                if (length > _programeFileContentAdder + 32)
                                {
                                    _programContent = _programeFileContent[_programeFileContentAdder..(_programeFileContentAdder + 32)];
                                    SendUpgradePackeCmd(_frameSerialNumber, _programContent);
                                    _isUpgradePacketReceived = false;
                                    _programeFileContentAdder += 32;
                                }
                                else if (length == _programeFileContentAdder)
                                {
                                    //传输重启的命令
                                    RestartAndUpgrade();
                                    Tips = "数据传输完成，正在等待开发板重启";
                                    return;
                                }
                                else
                                {
                                    _programContent = _programeFileContent[_programeFileContentAdder..(length)];
                                    SendUpgradePackeCmd(_frameSerialNumber, _programContent);
                                    _isUpgradePacketReceived = false;
                                    _programeFileContentAdder = length;

                                }
                            }
                        }
                        else
                        {
                            Tips = "等待开发板回包超时";
                            return;
                        }
                        
                        
                    }
                    Tips = "数据传输失败";

                });
        }
        private void SendUpgradePackeCmd(UInt32 frameSerialNumber, byte[] data)
        {
            byte[] buffer = new byte[36];
            var fsn = BitConverter.GetBytes(frameSerialNumber);
            Buffer.BlockCopy(fsn,0, buffer,0,fsn.Length);
            Buffer.BlockCopy(data, 0, buffer, fsn.Length, data.Length);
            SerialPortService.SendMsg(OtaProtocol.GetCmd(EnumOtaCommandType.OTA_TRANSFER, DeviceID, buffer));
        }

        /// <summary>
        /// 拆包
        /// </summary>
        /// <returns> 升级固件的路径名</returns>
        private string Unpack()
        {
            string testPath = Path.GetDirectoryName(SourcePath) + @"\" + Path.GetFileNameWithoutExtension(SourcePath);
            if (Directory.Exists(testPath))
            {
                Tips = "当前文件夹下存在同名文件";
                return null;
            }
            //先解压文件
            string destinationPath = ZipCompress.ZipExtract(SourcePath);
            //再将文件中的json文件读取出来
            string jsonString = File.ReadAllText(destinationPath + @"\DevelopmentBoardMessage.json");
            developmentBoardMessage = new DevelopmentBoardMessage();
            developmentBoardMessage = JsonSerializer.Deserialize<DevelopmentBoardMessage>(jsonString);
            DeviceID = OtaProtocol.DeviceIdTransformEnum(developmentBoardMessage.DevelopmentBoardType);
            DevelopmentBoardType = OtaProtocol.DeviceIdTransformString(developmentBoardMessage.DevelopmentBoardType);
            VersionNumber = developmentBoardMessage.VersionNumber;
            //从版本号中获取主版本号和子版本号
            _majorVersionNumber = Convert.ToByte(VersionNumber.Split('.')[0]);
            _subVersionNumber = Convert.ToByte(VersionNumber.Split('.')[1]);
            UpdataInformation = developmentBoardMessage.UpdataInformation;
            Description = developmentBoardMessage.Description;
            var test = Directory.GetFiles(destinationPath, "*.bin");
            if (test != null && test.Length > 0)
            {
                return Directory.GetFiles(destinationPath, "*.bin")[0];
            }
            else
            {
                Tips = "当前解压包中不包含升级固件.bin文件";
            }
            return null;
        }
     
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _serialPortService_UpdateResponse(object? sender, byte[] e)
        {
            _parser.ReceiveBytes(e);
        }
        
       /// <summary>
       /// 开发板回报接收事件
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
        private void Parser_PacketReceivedEvent(object? sender, OtaToolDataPacket e)
        {
            switch (e.CmdType)
            {
                case EnumOtaCommandType.OTA_GET_HWV:
                    break;

                case EnumOtaCommandType.OTA_GET_FMV://接收到消息后发送数据

                    break;

                case EnumOtaCommandType.OTA_SYS_BRADCAST:

                    break;

                case EnumOtaCommandType.OTA_GET_UPGRADE_FMV:
                    //还需要判断一下版本号
                    _isStartUpgrade = true;
                    
                    break;


                case EnumOtaCommandType.OTA_GET_STATUS:
                    _isUpgradePacketReceived = true;
                    break;
                case EnumOtaCommandType.OTA_REQUEST:

                    break;
                case EnumOtaCommandType.OTA_ABORT:

                    break;
                case EnumOtaCommandType.OTA_TRANSFER://接收到回报发送下一个回报
                    _isUpgradePacketReceived = true;

                    break;
                case EnumOtaCommandType.OTA_RESTART:

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
