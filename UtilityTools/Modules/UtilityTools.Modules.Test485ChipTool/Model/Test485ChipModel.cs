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
            Protocol.Test485ChipToolProtocol.GetTestMessage( ref _testMessage);
            Protocol.Test485ChipToolProtocol.GetRecvMessage(ref _recvMessage);
            SerialPortService.UpdateResponse += SerialPortService_UpdateResponse;
            NetUdpService.UpdateResponse += NetUdpService_UpdateResponse;
            _parser1.PacketReceivedEvent += SerialPortService_PacketReceivedEvent;
            _parser2.PacketReceivedEvent += NetUdpService_PacketReceivedEvent;
            ClearCommand = new DelegateCommand(clear);
            TestCommand = new DelegateCommand<string>(Test);
            messageQueue = new Queue();
            InitUdpClient();
            udpRecv1 = new Thread(ThreadReceive1);
            udpRecv2 = new Thread(ThreadReceive2);
            udpRecv3 = new Thread(ThreadReceive3);
            udpRecv4 = new Thread(ThreadReceive4);
            udpRecv5 = new Thread(ThreadReceive5);
            
            
        }
        #endregion
        #region -------------Field----------------------------
  
        private Thread udpRecv1;
        private Thread udpRecv2;
        private Thread udpRecv3;
        private Thread udpRecv4;
        private Thread udpRecv5;
        private UdpClient udpClient1;
        private UdpClient udpClient2;
        private UdpClient udpClient3;
        private UdpClient udpClient4;
        private UdpClient udpClient5;


        private IContainerProvider _containerProvider;
        private readonly IDialogHostService _dialogHostService;
        private Test485ChipToolDataPacketProtocolParser _parser1;
        private Test485ChipToolDataPacketProtocolParser _parser2;
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
        /// 提示颜色串口5
        /// </summary>
        public bool? IsReady5
        {
            get { return _isReady4; }
            set { _isReady4 = value; RaisePropertyChanged(); }
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
            Log += "网口接收消息：" + BitConverter.ToString(e.packet.GetBytes()) + "\n\r";
        }
        private void SerialPortService_PacketReceivedEvent(object? sender, Test485ChipToolDataPacket e)
        {
            Log += "串口接收消息：" + BitConverter.ToString(e.packet.GetBytes()) + "\n\r";
        }

        private void clear()
        {
         
            IsReady1 = null;
            IsReady2 = null;
            IsReady3 = null;
            IsReady4 = null;
            IsReady5 = null;
            Log = "";
        }

       
        private  async void  Test(object obj)
        {
            switch (obj)
            {
                case "1": SendMessage1(); break;
                case "2": SendMessage2(); break;
                case "3": SendMessage3(); break;
                case "4": SendMessage4(); break;
                case "5": SendMessage5(); break;
                case "0":
                    SendMessage1(); 
                    SendMessage2(); 
                    SendMessage3();
                    SendMessage4();
                    SendMessage5();
                    break;
            }
        }
        private void SendMessage1()
        {
            IPEndPoint remotePoint = new IPEndPoint(IPAddress.Parse("192.168.1.88"), 5001);
            udpClient1.Send(_testMessage, _testMessage.Length, remotePoint);
            Log += "网口1发送消息：" + BitConverter.ToString(_testMessage) + "\n\r";
            udpRecv1 = new Thread(ThreadReceive1);
            udpRecv1.Start();
        }
        private void SendMessage2()
        {
            IPEndPoint remotePoint = new IPEndPoint(IPAddress.Parse("192.168.1.88"), 5002);
            udpClient2.Send(_testMessage, _testMessage.Length, remotePoint);
            Log += "网口2发送消息：" + BitConverter.ToString(_testMessage) + "\n\r";
            udpRecv2 = new Thread(ThreadReceive2);
            udpRecv2.Start();
        }
        private void SendMessage3()
        {
            IPEndPoint remotePoint = new IPEndPoint(IPAddress.Parse("192.168.1.88"), 5003);
            Log += "网口3发送消息：" + BitConverter.ToString(_testMessage) + "\n\r";
            udpClient3.Send(_testMessage, _testMessage.Length, remotePoint);
            udpRecv3 = new Thread(ThreadReceive3);
            udpRecv3.Start();
        }
        private void SendMessage4()
        {
            IPEndPoint remotePoint = new IPEndPoint(IPAddress.Parse("192.168.1.88"), 5004);
            Log += "网口4发送消息：" + BitConverter.ToString(_testMessage) + "\n\r";
            udpClient4.Send(_testMessage, _testMessage.Length, remotePoint);
            udpRecv4 = new Thread(ThreadReceive4);
            udpRecv4.Start();
        }
        private void SendMessage5()
        {
            IPEndPoint remotePoint = new IPEndPoint(IPAddress.Parse("192.168.1.88"), 5005);
            Log += "网口5发送消息：" + BitConverter.ToString(_testMessage) + "\n\r";
            udpClient5.Send(_testMessage, _testMessage.Length, remotePoint);
            udpRecv5 = new Thread(ThreadReceive5);
            udpRecv5.Start();
        }
        private void ThreadReceive1()
        {
            IPEndPoint remotePoint = new IPEndPoint(IPAddress.Parse("192.168.1.88"), 5001);
            byte[] data = new byte[64];
            udpClient1.Client.ReceiveTimeout = 1000;
            try
            {
                data = udpClient1.Receive(ref remotePoint);
                Log += "网口1接收消息：" + BitConverter.ToString(data) + "\n\r";
            }
            catch (Exception e)
            {
                Log += "串口1接收超时"+ e.ToString() + "\n\r";
                IsReady1 = false;
            }
            finally
            {
                if (CompareMessage(data, _recvMessage))
                {

                    IsReady1 = true;
                }
                else
                {
                    IsReady1 = false;
                }
            }

        }
        private void ThreadReceive2()
        {
           
            IPEndPoint remotePoint = new IPEndPoint(IPAddress.Parse("192.168.1.88"), 5002);
            udpClient2.Client.ReceiveTimeout = 1000;
            byte[] data = new byte[64];
            try
            {
                
                data = udpClient2.Receive(ref remotePoint);
                Log += "网口2接收消息：" + BitConverter.ToString(data) + "\n\r";

            }
            catch (Exception e)
            {
                Log += "串口2接收超时" + e.ToString() + "\n\r";
                IsReady2 = false;
            }
            finally
            {
                if (CompareMessage(data, _recvMessage))
                {

                    IsReady2 = true;
                }
                else
                {
                    IsReady2 = false;
                }
            }

        }
        private void ThreadReceive3()
        {
            
            IPEndPoint remotePoint = new IPEndPoint(IPAddress.Parse("192.168.1.88"), 5003);
            udpClient3.Client.ReceiveTimeout = 1000;
            byte[] data = new byte[64];
            try
            {

                data = udpClient3.Receive(ref remotePoint);
                Log += "网口3接收消息：" + BitConverter.ToString(data) + "\n\r";

            }
            catch (Exception)
            {
                Log += "串口3接收超时" + "\n\r";
                IsReady3 = false;
            }
            finally
            {
                if (CompareMessage(data, _recvMessage))
                {

                    IsReady3 = true;
                }
                else
                {
                    IsReady3 = false;
                }
            }

        }
        private void ThreadReceive4()
        {
            
            IPEndPoint remotePoint = new IPEndPoint(IPAddress.Parse("192.168.1.88"), 5004);
            udpClient4.Client.ReceiveTimeout = 1000;
            byte[] data = new byte[64];
            try
            {

                data = udpClient4.Receive(ref remotePoint);
                Log += "网口4接收消息：" + BitConverter.ToString(data) + "\n\r";

            }
            catch (Exception e)
            {
                Log += "串口4接收超时" +e.ToString()+ "\n\r";
                IsReady4 = false;
            }
            finally
            {
                if (CompareMessage(data, _recvMessage))
                {

                    IsReady4 = true;
                }
                else
                {
                    IsReady4 = false;
                }
            }
        }
        private void ThreadReceive5()
        {
           
            IPEndPoint remotePoint = new IPEndPoint(IPAddress.Parse("192.168.1.88"), 5005);
            udpClient5.Client.ReceiveTimeout = 1000;
            byte[] data = new byte[64];
            try
            {

                data = udpClient5.Receive(ref remotePoint);
                Log += "网口5接收消息：" + BitConverter.ToString(data) + "\n\r";

            }
            catch (Exception e)
            {
                Log += "串口5接收超时" +e.ToString()+ "\n\r";
                IsReady5 = false;
            }
            finally
            {
                if (CompareMessage(data, _recvMessage))
                {

                    IsReady5 = true;
                }
                else
                {
                    IsReady5 = false;
                }
            }

        }
        private void ThreadStart()
        {
            if (udpRecv1 != null)
            {
                udpRecv1.Start();
            }
            if (udpRecv2 != null)
            {
                udpRecv2.Start();
            }
            if (udpRecv3 != null)
            {
                udpRecv3.Start();
            }
            if (udpRecv4 != null)
            {
                udpRecv4.Start();
            }
            if (udpRecv5 != null)
            {
                udpRecv5.Start();
            }
        }
        private void InitUdpClient()
        {
            int locatePort1 = FreePort.FindNextAvailableUDPPort(5000);
            IPAddress locateIpAddr1 = IPAddress.Any;
            IPEndPoint locatePoint1 = new IPEndPoint(locateIpAddr1, locatePort1);
            udpClient1 = new UdpClient(locatePoint1);

            int locatePort2 = FreePort.FindNextAvailableUDPPort(5000);
            IPAddress locateIpAddr2 = IPAddress.Any;
            IPEndPoint locatePoint2 = new IPEndPoint(locateIpAddr2, locatePort2);
            udpClient2 = new UdpClient(locatePoint2);

            int locatePort3 = FreePort.FindNextAvailableUDPPort(5000);
            IPAddress locateIpAddr3 = IPAddress.Any;
            IPEndPoint locatePoint3 = new IPEndPoint(locateIpAddr3, locatePort3);
            udpClient3 = new UdpClient(locatePoint3);

            int locatePort4 = FreePort.FindNextAvailableUDPPort(5000);
            IPAddress locateIpAddr4 = IPAddress.Any;
            IPEndPoint locatePoint4 = new IPEndPoint(locateIpAddr4, locatePort4);
            udpClient4 = new UdpClient(locatePoint4);

            int locatePort5 = FreePort.FindNextAvailableUDPPort(5000);
            IPAddress locateIpAddr5 = IPAddress.Any;
            IPEndPoint locatePoint5 = new IPEndPoint(locateIpAddr5, locatePort5);
            udpClient5 = new UdpClient(locatePoint5);
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
