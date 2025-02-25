using CsvHelper;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using UtilityTools.Core.Dialog;
using UtilityTools.Modules.Test485ChipTool.Protocol;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.Services.Services;
using System.Windows.Forms;
using UtilityTools.Modules.Test485ChipTool.ViewModels;
using System.Net.NetworkInformation;
using System.Diagnostics;
using UtilityTools.Core.Protocol;

namespace UtilityTools.Modules.Test485ChipTool.Model
{
    public class Test485ChipModel : BindableBase
    {
        #region --------------Construct--------------
        public Test485ChipModel(IContainerProvider containerProvider)
        {
            _containerProvider = containerProvider;
            _dialogHostService = containerProvider.Resolve<IDialogHostService>();
            SerialPortService = new SerialPortService();
            SerialPortService.Name = "串口传输";
            SerialPortService.IsBinary = true;
            var netUdp = new UdpNetAsyncDevice();
            netUdp.Name = "网口传输";
            netUdp.DeviceInstance.TargetIp = "192.168.1.88";
            netUdp.DeviceInstance.TargetPort = 5000;
            netUdp.DeviceInstance.HostIp = "192.168.1.34";
            netUdp.DeviceInstance.HostPort = 5001;
            netUdp.IsBinary = true;
            NetUdpService = netUdp;
            _parser1 = new Test485ChipToolDataPacketProtocolParser();
            _parser2 = new Test485ChipToolDataPacketProtocolParser();
            _parser1.Service = SerialPortService;
            _parser2.Service = NetUdpService;
            _message = Protocol.Test485ChipToolProtocol.GetRequestHardInfoCmd(0X02);
            SerialPortService.UpdateResponse += SerialPortService_UpdateResponse;
            NetUdpService.UpdateResponse += NetUdpService_UpdateResponse;
            _parser1.PacketReceivedEvent += SerialPortService_PacketReceivedEvent;
            _parser2.PacketReceivedEvent += NetUdpService_PacketReceivedEvent;
            ClearCommand = new DelegateCommand(clear);
            TestCommand = new DelegateCommand<string>(Test);
            zep = new ZepGenericProtocolParser();
            zep.PacketReceivedEvent += Zep_PacketReceivedEvent;
        }
        #endregion
        #region -------------Field----------------------------
        private bool  testMode = false;
        private string parameter = null;
        private IContainerProvider _containerProvider;
        private readonly IDialogHostService _dialogHostService;
        private Test485ChipToolDataPacketProtocolParser _parser1;
        private Test485ChipToolDataPacketProtocolParser _parser2;
        ZepGenericProtocolParser zep ;
        private UdpClient udpClient = null;
        private byte[] _message;
 
        #endregion
        #region------------------------Property--------------------------
        private bool _isConnect = false;
        /// <summary>
        /// 是否连接
        /// </summary>
        public bool IsConnect
        {
            get { return _isConnect; }
            set { _isConnect = value;RaisePropertyChanged(); }
        }
        private bool? _isReady1 = null;
        /// <summary>
        /// 提示颜色串口1
        /// </summary>
        public bool? IsReady1
        {
            get { return _isReady1; }
            set { _isReady1 = value; RaisePropertyChanged(); }
        }
        private bool? _isReady2 = null;
        /// <summary>
        /// 提示颜色串口2
        /// </summary>
        public bool? IsReady2
        {
            get { return _isReady2; }
            set { _isReady2 = value; RaisePropertyChanged(); }
        }
        private bool? _isReady3 = null;
        /// <summary>
        /// 提示颜色串口3
        /// </summary>
        public bool? IsReady3
        {
            get { return _isReady3; }
            set { _isReady3 = value; RaisePropertyChanged(); }
        }
        private bool? _isReady4 = null;
        /// <summary>
        /// 提示颜色串口4
        /// </summary>
        public bool? IsReady4
        {
            get { return _isReady4; }
            set { _isReady4 = value; RaisePropertyChanged(); }
        }
        private bool? _isReady5 = null;
        /// <summary>
        /// 提示颜色串口4
        /// </summary>
        public bool? IsReady5
        {
            get { return _isReady4; }
            set { _isReady4 = value; RaisePropertyChanged(); }
        }

        private string _netSendMessage;
        /// <summary>
        /// 网口发送的消息
        /// </summary>
        public string NetSendMessage
        {
            get { return _netSendMessage; }
            set { _netSendMessage = value; RaisePropertyChanged(); }
        }
        private string _netReceiveMessage;
        /// <summary>
        /// 网口接收的消息
        /// </summary>
        public string NetReceiveMessage
        {
            get { return _netReceiveMessage; }
            set { _netReceiveMessage = value;RaisePropertyChanged(); }
        }

        private string _serialSendMessage;
        /// <summary>
        /// 串口发送的消息
        /// </summary>
        public string SerialSendMessage
        {
            get { return _serialSendMessage; }
            set { _serialSendMessage = value; RaisePropertyChanged(); }
        }
        private string _serialReceiveMessage;
        /// <summary>
        /// 串口接收的消息
        /// </summary>
        public string SerialReceiveMessage
        {
            get { return _serialReceiveMessage; }
            set { _serialReceiveMessage = value; RaisePropertyChanged(); }
        }
        private uint _signleTimeMessage = 1;
        /// <summary>
        /// 单次测试发送信息量
        /// </summary>
        public uint SignleTimeMessage
        {
            get { return _signleTimeMessage;}
            set { _signleTimeMessage = value; RaisePropertyChanged(); }
        }
        private uint _sendNumber = 0;
        /// <summary>
        /// 已发送信息数
        /// </summary>
        public uint SendNumber
        {
            get { return _sendNumber; }
            set { _sendNumber = value; RaisePropertyChanged(); }
        }
        private uint _sendInterval = 100;
        /// <summary>
        /// 每次发送信息时间间隔（毫秒）
        /// </summary>
        public uint SendInterval 
        {
            get { return _sendInterval; }
            set { _sendInterval = value; RaisePropertyChanged(); }
        }
        private uint _receiveNumber = 0;
        /// <summary>
        /// 接收到的数据
        /// </summary>
        public uint ReceiveNumber
        {
            get { return _receiveNumber; }
            set { _receiveNumber = value; RaisePropertyChanged(); }
        }
        private uint _correctNumber = 0;
        /// <summary>
        /// 核实正确的数量
        /// </summary>
        public uint CorrectNumber
        {
            get { return _correctNumber; }
            set { _correctNumber = value; RaisePropertyChanged(); }
        }
        #endregion

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
        #region --------------------Command----------------------
      

        public DelegateCommand ClearCommand { get; set; }
        public DelegateCommand<string> TestCommand { get; set; }
        public DelegateCommand NetTestCommand { get; set; }
        #endregion
        #region ---------------------PublicMethood----------------------------

        #endregion
        #region ----------------------PrivateMethod---------------------------

        private void NetUdpService_UpdateResponse(object? sender, byte[] e)
        {
            _parser2.ReceiveBytes(e);
        }

        private void SerialPortService_UpdateResponse(object? sender, byte[] e)
        {
            _parser1.ReceiveBytes(e);
        }
        private void NetUdpService_PacketReceivedEvent(object? sender, Test485ChipToolDataPacket e)
        {
            NetReceiveMessage = BitConverter.ToString(e.packet.GetBytes());
            var compareMessage = BitConverter.ToString(e.packet.GetBytes());
            var _messageString = BitConverter.ToString(_message);
            ReceiveNumber++;
            if (compareMessage == _messageString)
            {
                CorrectNumber++;
            }
            
            
            

        }
        private void SerialPortService_PacketReceivedEvent(object? sender, Test485ChipToolDataPacket e)
        {
            SerialReceiveMessage = BitConverter.ToString(e.packet.GetBytes(), 0, e.packet.GetBytes().Length);
            
            if (testMode == true)
            {
                if (SerialPortService.IsOpen)
                {
                    
                    SerialPortService.SendMsg(e.packet.GetBytes());
                    SerialSendMessage = BitConverter.ToString(e.packet.GetBytes(), 0, e.packet.GetBytes().Length);
                }
            }
    
        }

        private void clear()
        {
            SignleTimeMessage = 1;
            IsReady1 = null;
            IsReady2 = null;
            IsReady3 = null;
            IsReady4 = null;
            IsReady5 = null;
            SendNumber = 0;
            SendInterval = 100;
            ReceiveNumber = 0;
            CorrectNumber = 0;
            testMode = false;
            SerialReceiveMessage = "";
            SerialSendMessage = "";
            NetReceiveMessage = "";
            NetSendMessage = "";
        }

       
        private  async void  Test(object obj)
        {
            testMode = true;
            var number = SignleTimeMessage;
            int locatePort = FreePort.FindNextAvailableUDPPort(5000);
            IPAddress locateIpAddr = IPAddress.Any;
            IPEndPoint locatePoint = new IPEndPoint(locateIpAddr, locatePort);
            udpClient = new UdpClient(locatePoint);
            string remoteIP = "192.168.1.88";
            int remotePort;
            parameter = (string)obj;
            switch (parameter)
            {
                case "1": remotePort = 5001 ; break;
                case "2": remotePort = 5002; break;
                case "3": remotePort = 5003; break;
                case "4": remotePort = 5004; break;
                case "5": remotePort = 5005; break;
                default:  remotePort = 5000; break;
            }
            if (udpClient != null)
            {
                IPAddress remoteIp = IPAddress.Parse(remoteIP);
                IPEndPoint remotePoint = new IPEndPoint(remoteIp, remotePort);
               
                 await Task.Run( () => 
                {
                    while(number != 0)
                    {
                        number--;
                        udpClient.Send(_message, _message.Length, remotePoint);
                        NetSendMessage = BitConverter.ToString(_message);
                        SendNumber++;
                        Thread.Sleep((int)SendInterval);

                    }
                });
                await Task.Run(() =>
                {
                    while (testMode)
                    {
                        if (udpClient != null)
                        {
                            zep.ReceiveBytes(udpClient.Receive(ref remotePoint));
                        }
                    }
                });      
            }
        }
        private void Zep_PacketReceivedEvent(object? sender, DataPacket e)
        {
            ReceiveNumber++;
            NetReceiveMessage = BitConverter.ToString(e.GetBytes());
            if (NetReceiveMessage == BitConverter.ToString(_message))
            {
                CorrectNumber++;
                switch (parameter)
                {
                    case "1": IsReady1 = true; break;
                    case "2": IsReady2 = true; break;
                    case "3": IsReady3 = true; break;
                    case "4": IsReady4 = true; break;
                    case "5": IsReady5 = true; break;
                }
            }
            else
            {
                switch (parameter)
                {
                    case "1": IsReady1 = false; break;
                    case "2": IsReady2 = false; break;
                    case "3": IsReady3 = false; break;
                    case "4": IsReady4 = false; break;
                    case "5": IsReady5 = false; break;
                }
            }
        }
        #endregion
    }
}
