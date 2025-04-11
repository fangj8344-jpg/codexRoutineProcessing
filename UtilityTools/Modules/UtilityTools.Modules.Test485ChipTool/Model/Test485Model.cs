using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilityTools.Modules.Test485ChipTool.Protocol;

namespace UtilityTools.Modules.Test485ChipTool.Model
{
    public class Test485Model:BindableBase  
    {
        #region
        public Test485Model(string hostIp,int hostPort,string remoteIp,int remotePort) 
        {

            _remotePort = remotePort;
            hostIPEndPoint = new IPEndPoint(IPAddress.Parse(hostIp), hostPort);
            remoteIPEndPoint = new IPEndPoint(IPAddress.Parse(remoteIp), remotePort);
            udpClient = new UdpClient(hostIPEndPoint);
            //_udpclient = new UdpClientService.UdpClientService(hostIp, hostPort, remoteIp, remotePort);
            //_udpclient.Connect();
            //_udpclient.ReceiveDataEvent += ReceiveDataEvent;
            udpRecv1 = new Thread(ThreadReceive);
            udpRecv1.Start();
            Log += $"{_remotePort}校验1:{BitConverter.ToString(Test485ChipToolProtocol.GetCheckMessage())}\n\r"+ $"{_remotePort}校验2:{BitConverter.ToString(Test485ChipToolProtocol.GetCheckMessage2())}\n\r";
        }


        #endregion
        #region
        private bool flag =false;
        private UdpClient udpClient;
        //private UdpClientService.UdpClientService _udpclient;
        private IPEndPoint remoteIPEndPoint = null;
        private IPEndPoint hostIPEndPoint = null;
        private Thread udpRecv1;
        
        private int _remotePort;
        #endregion

        #region
        private string _log;
        public string Log
        {
            get { return _log; }
            set { _log = value; RaisePropertyChanged(); }
        }
        private bool? _isCheckOK = null;
        /// <summary>
        /// 检查结果
        /// </summary>
        public bool? IsCheckOK
        {
            get { return _isCheckOK; }
            set { _isCheckOK = value; RaisePropertyChanged(); }
        }
       

        #endregion

        #region
        
        /*public async void  CheckTest()
        {
            byte[] sendMessage = new byte[64];
            Test485ChipToolProtocol.GetTestMessage(ref sendMessage);
            try 
            {
                _udpclient.SendMsg(sendMessage);
            }
            catch (Exception ex) 
            {
                IsCheckOK = false;
            }
            await Task.Run(() => 
            {
                Thread.Sleep(1200);
                if (IsCheckOK != true)
                {
                    IsCheckOK = false;
                }
            });
        }*/
        public  void CheckTest()
        {
            byte[] sendMessage = Test485ChipToolProtocol.GetSendMessage();
            Console.WriteLine(BitConverter.ToString(sendMessage));
            flag = true;
            udpClient.Send(sendMessage,sendMessage.Length, remoteIPEndPoint);
            Log += $"{_remotePort}发送:{BitConverter.ToString(sendMessage)}\n\r" ;
        }
        #endregion

        #region
        private void ReceiveDataEvent(object? sender, byte[] bytes)
        {
            byte[] message = Test485ChipToolProtocol.GetSendMessage();

            for (int i = 0; i < message.Length; i++)
            {
                if (message[i] != bytes[i])
                {
                    IsCheckOK = false;
                    return;
                }
            }
            IsCheckOK = true;

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
        private void ThreadReceive()
        {

           while (true) 
            {
                if (!flag)
                {
                    Thread.Sleep(0);
                }
                else
                {
                    flag = false;
                    udpClient.Client.ReceiveTimeout = 1000;
                    byte[] buffer = new byte[64];
                    try
                    {
                        buffer = udpClient.Receive(ref remoteIPEndPoint);
                        Log += $"{_remotePort}接收:{BitConverter.ToString(buffer)}\n\r";
                        byte[] message = Test485ChipToolProtocol.GetCheckMessage(); 
                        byte[] message2 = Test485ChipToolProtocol.GetCheckMessage2();
                        if (CompareMessage(message, buffer) || CompareMessage(message2, buffer))
                        {
                            IsCheckOK = true;
                        }
                        else
                        {
                            IsCheckOK = false;
                        }
                    }
                    catch (Exception e)
                    {
                        IsCheckOK = false;
                        Log += $"{_remotePort}接收:超时\n\r";
                    }
                }
            }

        }
        #endregion

    }
}
