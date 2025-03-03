#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.OtaTool.ViewModels
 * 唯一标识：920de0b4-f1ff-46f3-b733-143f88169bf7
 * 文件名：OtaToolViewModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/10/12 11:56:47
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

using Microsoft.VisualBasic;
using OpenCvSharp.Dnn;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.OtaTool.Model;
using UtilityTools.Modules.OtaTool.Protocol;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.Services.Services;

namespace UtilityTools.Modules.OtaTool.ViewModels
{
    public class OtaToolViewModel : RegionViewModelBase
    {
        #region ------------Constructor------------
        public OtaToolViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService)
            : base(containerProvider)
        {
            _dialogHostService = dialogHostService;
            InitProperty();
            InitCommand();
        }
        #endregion

        #region ------------Field------------
        private readonly IDialogHostService _dialogHostService;
        #endregion

        #region ------------Property------------
        private bool _isConnected;
        /// <summary>
        /// 设备是否连接
        /// </summary>
        public bool IsConnected
        {
            get { return _isConnected; }
            set 
            { 
                _isConnected = value;
                RaisePropertyChanged();
            }
        }

        private bool _netIsConnected;
        /// <summary>
        /// 网络设备是否连接
        /// </summary>
        public bool NetIsConnected
        {
            get { return _netIsConnected; }
            set 
            { 
                _netIsConnected = value;
                RaisePropertyChanged();
            }
        }

        private OtaModel _model;
        /// <summary>
        /// OTA升级模型
        /// </summary>
        public OtaModel Model
        {
            get { return _model; }
            set { _model = value; RaisePropertyChanged(); }
        }
        private int? _port = 5001;

        public int? Port
        {
            get { return _port; }
            set { _port = value;RaisePropertyChanged(); }
        }

        #endregion

        #region ------------Command------------
        public DelegateCommand ShowDeviceCommand { get; set; }

        public DelegateCommand ShowNetDeviceCommand { get; set; }
        public DelegateCommand ConnectPrepareCommand { get; set; }
        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        /// <summary>
        /// 初始化指令
        /// </summary>
        private void InitCommand()
        {
            ShowDeviceCommand = new DelegateCommand(ShowDevice);
            ShowNetDeviceCommand = new DelegateCommand(ShowNetDevice);
            ConnectPrepareCommand = new DelegateCommand(ConnectPrepare);
        }

        /// <summary>
        /// 初始化属性
        /// </summary>
        private void InitProperty()
        {
            Model = containerProvider.Resolve<OtaModel>();
        }

        /// <summary>
        /// 显示设备连接弹窗
        /// </summary>
        private async void ShowDevice()
        {
            DialogParameters parameter = new DialogParameters();
            parameter.Add("Value", Model.SerialPortService);
            var diaglogResult = await this._dialogHostService.ShowDialog("SerialPortView", parameter, CommonModel.OtaToolRegionName);
            if (diaglogResult == null)
                return;
            if (diaglogResult.Result == ButtonResult.OK && diaglogResult.Parameters.ContainsKey("Value"))
            {
                var value = diaglogResult.Parameters.GetValue<IAsynRWService>("Value");
                if (value != null)
                {

                    Model.SerialPortService = value;
                    IsConnected = Model.SerialPortService.IsOpen;
                    Model.StartRequestStatus();
                }
            }
        }

        /// <summary>
        /// 显示设备连接弹窗
        /// </summary>
        private async void ShowNetDevice()
        {
            DialogParameters parameter = new DialogParameters();
            parameter.Add("Value", Model.NetUdpService);//传递参数用来读写
            var diaglogResult = await this._dialogHostService.ShowDialog("NetConfigView", parameter, CommonModel.OtaToolRegionName);
            if (diaglogResult == null)
                return;
            if (diaglogResult.Result == ButtonResult.OK && diaglogResult.Parameters.ContainsKey("Value"))
            {
                var value = diaglogResult.Parameters.GetValue<IAsynRWService>("Value");
                if (value != null)
                {
                    Model.NetUdpService = value;
                    NetIsConnected = Model.NetUdpService.IsOpen;
                    Model.StartRequestStatus();
                }
                
            }
        }
        public async void ConnectPrepare()
        {
            
            
            try
            {
                if (Model.DevelopmentBoardMessage.DeviceID == null)
                {
                    await UtilityTools.Core.Extension.DialogExtension.Information(this._dialogHostService, "提示信息", "升级文件错误", CommonModel.OtaToolRegionName);
                    return;
                }
            }
            catch
            {
                await UtilityTools.Core.Extension.DialogExtension.Information(this._dialogHostService, "提示信息", "请加载升级文件", CommonModel.OtaToolRegionName);
                return;
            }
            
           
            Model._udpClient.Client.ReceiveTimeout = 2000;
            IPAddress ipAddress = IPAddress.Broadcast;
            IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, (int)Port);
            var message = EthProtocol.GetIP(Model.DevelopmentBoardMessage.DeviceID);
            Model._udpClient.Send(message, remoteEndPoint);
            Model.Log += "发送广播查询ip：" + BitConverter.ToString(message)+"\n\r";
            await Task.Run( () =>
            {
                try 
                {
                    Model._packetReceived =  Model._udpClient.Receive(ref remoteEndPoint);
                    Model.Log += "获取到目标ip：" + BitConverter.ToString(Model._packetReceived) + "\n\r";
     
                }
                catch (Exception e)
                {
                    //超时强制更改对方的ip地址
                    byte[] ip = new byte[36];
                    ip[0] = (byte)192;
                    ip[1] = (byte)168;
                    ip[2] = (byte)1;
                    ip[3] = (byte)88;
                    var bytes = EthProtocol.SetIP(ip, Model.DevelopmentBoardMessage.DeviceID);
                    Model._udpClient.Send(bytes, remoteEndPoint);
                    Model.Log += "等待回报超时，广播发送强制更改对方IP：" + BitConverter.ToString(bytes) + "\n\r";
                }
                finally 
                {

                    if (Model._packetReceived == null)
                    {
                        
                        var net = new UdpNetAsyncDevice();
                        net.DeviceInstance.TargetIp = "192.168.1.88";
                        net.DeviceInstance.TargetPort = (int)Port;
                        net.DeviceInstance.HostPort = FreePort.FindNextAvailableUDPPort(5000);
                        net.DeviceInstance.HostIp = Model.locateIpAddr.ToString();
                        Model.NetUdpService = net;
                        Model.InitNetConfig();
                    }
                    else
                    {
                        var packet = UtilityTools.Core.Protocol.DataPacket.ParseFromBytes(Model._packetReceived);
                        if (packet.length == 64 && BitConverter.ToInt16(packet.command) == 0x0632)
                        {
                            string ip = packet.data[0].ToString() + "." + packet.data[1].ToString() + "." + packet.data[2].ToString() + "." + packet.data[3].ToString();
                            var net = new UdpNetAsyncDevice();
                            net.DeviceInstance.TargetIp = ip;
                            net.DeviceInstance.TargetPort =(int)Port;
                            net.DeviceInstance.HostPort = Model.locatePort;
                            net.DeviceInstance.HostIp = Model.locateIpAddr.ToString();
                            Model.NetUdpService = net;
                            Model.InitNetConfig();
                        }
                        else
                        {
                            byte[] ip = new byte[36];
                            ip[0] = (byte)192;
                            ip[1] = (byte)168;
                            ip[2] = (byte)1;
                            ip[3] = (byte)88;
                            var bytes = EthProtocol.SetIP(ip, Model.DevelopmentBoardMessage.DeviceID);
                            Model._udpClient.Send(bytes, remoteEndPoint);
                            Model.Log += "广播发送强制更改对方IP：" + BitConverter.ToString(bytes) + "\n\r";
                            Model._udpClient.Close();
                        }
                    }
                    
                }
               
            });
        }

        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
