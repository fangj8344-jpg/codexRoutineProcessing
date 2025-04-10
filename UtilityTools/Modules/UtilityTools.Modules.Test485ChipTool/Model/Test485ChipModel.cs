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
using System.Collections;
using System.Security.Permissions;
using System.Windows.Markup;
using System.Collections.ObjectModel;

namespace UtilityTools.Modules.Test485ChipTool.Model
{
    public class Test485ChipModel : BindableBase
    {
        #region --------------Construct--------------
        public Test485ChipModel(IContainerProvider containerProvider)
        {
            _containerProvider = containerProvider;
            _dialogHostService = containerProvider.Resolve<IDialogHostService>();
            
            //Serial Port
            SerialPortService = new SerialPortService();
            SerialPortService.Name = "串口传输";
            SerialPortService.IsBinary = true;

            //Ethernet port Udp
            var netUdp = new UdpNetAsyncDevice();
            netUdp.Name = "网口传输";
            netUdp.DeviceInstance.TargetIp = "192.168.1.88";
            netUdp.DeviceInstance.TargetPort = 5000;
            netUdp.DeviceInstance.HostIp = "192.168.1.34";
            netUdp.DeviceInstance.HostPort = 5001;
            netUdp.IsBinary = true;

            NetUdpService = netUdp;

            mParser_SerialPort = new Test485ChipToolDataPacketProtocolParser();
            mParser_SerialPort.Service = SerialPortService;

            mParser_NetUdp = new Test485ChipToolDataPacketProtocolParser();
            mParser_NetUdp.Service = NetUdpService;

            Test485ChipModel.ECommanName cmd = Test485ChipModel.ECommanName.cmdGetSTATUS;
            string IpAddress = "192.168.1.88";
            Protocol.Test485ChipToolProtocol.GetSendMessage( ref _testMessage, 
                IpAddress,(ushort)cmd);
            Protocol.Test485ChipToolProtocol.GetRecvMessage(ref _recvMessage, 
                IpAddress,(ushort)cmd);

            SerialPortService.UpdateResponse += SerialPortService_UpdateResponse;
            mParser_SerialPort.PacketReceivedEvent += SerialPortService_PacketReceivedEvent;

            NetUdpService.UpdateResponse += NetUdpService_UpdateResponse;
            mParser_NetUdp.PacketReceivedEvent += NetUdpService_PacketReceivedEvent;

            ClearCommand = new DelegateCommand(clearReadySign);
            TestCommand = new DelegateCommand<string>(Test);
            messageQueue = new Queue();

            mnThreadNum = 5;

            //fill the mList_IsReady
            InitReadySign(mnThreadNum);

            //fill the mList_udpClient
            InitUdpClient(mnThreadNum);                       
        }
        #endregion
        #region -------------Field----------------------------
        public enum ECommanName : ushort
        {
            cmdGetVac = 0x0800,//读真空规数值
            cmdGetPID = 0X0806,//获取PID参数
            cmdGetSTATUS = 0x0501,//获取高压状态
        }
      
        private List<UdpClient> mList_udpClient = new List<UdpClient>();
        private int mnThreadNum;

        private IContainerProvider _containerProvider;
        private readonly IDialogHostService _dialogHostService;
        private Test485ChipToolDataPacketProtocolParser mParser_SerialPort;
        private Test485ChipToolDataPacketProtocolParser mParser_NetUdp;
        ZepGenericProtocolParser zep ;
        private byte[] _testMessage;
        private byte[] _recvMessage;
        private Queue messageQueue;
        #endregion
        #region------------------------Property--------------------------
        private string _log;
        /// <summary>
        /// 日志
        /// </summary>
        public string Log
        {
            get { return _log; }
            set { _log = value; RaisePropertyChanged(); }
        }
        private bool _isConnect = false;
        /// <summary>
        /// 是否连接
        /// </summary>
        public bool IsConnect
        {
            get { return _isConnect; }
            set { _isConnect = value;RaisePropertyChanged(); }
        }
        /// <summary>
        /// 提示颜色串口
        /// </summary>
        private ObservableCollection<bool?> _readyStates = null;
        public ObservableCollection<bool?> ReadySign
        {
            get => _readyStates;
            set
            {
                _readyStates = value;
                RaisePropertyChanged(nameof(ReadySign));
            }
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

        private void SerialPortService_UpdateResponse(object? sender, byte[] e)
        {
            mParser_SerialPort.ReceiveBytes(e);
        }

        private void SerialPortService_PacketReceivedEvent(object? sender, Test485ChipToolDataPacket e)
        {
            Log += "串口接收消息：" + BitConverter.ToString(e.packet.GetBytes()) + "\n\r";
        }

        private void NetUdpService_UpdateResponse(object? sender, byte[] e)
        {
            mParser_NetUdp.ReceiveBytes(e);
        }

        private void NetUdpService_PacketReceivedEvent(object? sender, Test485ChipToolDataPacket e)
        {
            Log += "网口接收消息：" + BitConverter.ToString(e.packet.GetBytes()) + "\n\r";
        }


        private void clearReadySign()
        {
            for (int i = 0; i < ReadySign.Count(); ++i)
            {
                ReadySign[i] = null;
            }             
        }

       
        private  async void  Test(object obj)
        {
            string str = (string)obj;
            if (int.TryParse(str, out int nIndex))
            {
                if (nIndex == 0)
                {
                    for (int i = 0; i < mnThreadNum; i++)
                    {
                        SendMessage(i);
                    }
                }
                else if (nIndex > 0 && nIndex <= 5)
                {
                    SendMessage(nIndex - 1);//list下标从0开始
                }
            }
            else
            {
                // 转换失败的处理
            }           
        }
        private void SendMessage(int nIndex)
        {
            IPEndPoint remotePoint = new IPEndPoint(IPAddress.Parse("192.168.1.88"), 5000 + (nIndex + 1));
            mList_udpClient[nIndex].Send(_testMessage, _testMessage.Length, remotePoint);
            Log += "网口" + nIndex.ToString() +"发送消息：" +
                BitConverter.ToString(_testMessage) + "\n\r";
            Thread UdpRecvThread = new Thread(() => ThreadReceive(nIndex));
            UdpRecvThread.Start();//start thread  
        }

        private void ThreadReceive(int nIndex)
        {
            bool bHasRecvData = false;
            int nLANIndex = nIndex + 1;
            IPEndPoint remotePoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = new byte[64];
            mList_udpClient[nIndex].Client.ReceiveTimeout = 1000;
            try
            {
                data = mList_udpClient[nIndex].Receive(ref remotePoint);
                Log += "网口" + (nLANIndex).ToString() + "接收消息：" + 
                    BitConverter.ToString(data) + "\n\r";
                bHasRecvData = true;
            }
            catch (Exception e)
            {
                Log += "网口" + (nLANIndex).ToString() + "接收超时"+
                    e.ToString() + "\n\r";
                ReadySign[nIndex] = false;
            }
            finally
            {
                //if (CompareMessage(data, _recvMessage))
                if(bHasRecvData)
                {

                    ReadySign[nIndex] = true;
                }
                else
                {
                    ReadySign[nIndex] = false;
                }
            }

        }

        private void InitReadySign(int nReadySignNum)
        {            
            if (ReadySign == null)
            {
                ReadySign = new ObservableCollection<bool?>();
            }

            ReadySign.Clear();

            for (int i = 0; i < nReadySignNum; i++)
            {
                ReadySign.Add(null);
            }
        }
        private void InitUdpClient(int nUdpClientNum)
        {
            for(int i = 0; i < nUdpClientNum; ++i)
            {                
                var udpClient = new UdpClient();
                mList_udpClient.Add(udpClient);
            }
        }
        private bool CompareMessage(byte[] sourceMessage, byte[] TargetMessage)
        {
            if (sourceMessage.Length != TargetMessage.Length && TargetMessage == null)
            {
                return false;
            }
            for (int i = 0; i < 64; i++)
            {
                if (sourceMessage[i] != TargetMessage[i])
                {
                    return false;
                }
            }
            return true;
        }
       
      
        #endregion
    }
}
