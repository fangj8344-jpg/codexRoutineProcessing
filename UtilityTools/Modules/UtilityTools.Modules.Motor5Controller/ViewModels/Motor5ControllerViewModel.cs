#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.Motor5Controller.ViewModels
 * 唯一标识：fc6911fc-8414-4ad4-b571-1e5140f1645b
 * 文件名：Motor5ControllerViewModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/14 16:16:38
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

using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Modules.Motor5Controller.Model;
using System.Windows.Media.Media3D;
using UtilityTools.Core.Mvvm;
using UtilityTools.Services.Interfaces;
using System.Security.Policy;
using Prism.Commands;
using Prism.Services.Dialogs;
using System.Collections.ObjectModel;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.Core.Dialog;
using System.IO.Ports;
using System.Windows;
using System.Reflection;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Interop;
using System.Windows.Documents;
using Prism.Events;
using UtilityTools.Core.Extension;

namespace UtilityTools.Modules.Motor5Controller.ViewModels
{
    internal class Motor5ControllerViewModel : RegionViewModelBase
    {
        #region ------------Constructor------------
        public Motor5ControllerViewModel(IDialogHostService dialogHostService, IContainerProvider containerProvider)
            : base(containerProvider)
        {
            One = containerProvider.Resolve<OneModel>();
            this._containerProvider = containerProvider;
            this._dialogHostService = dialogHostService;
            InitCommand();
            InitProperty();
            aggregator = containerProvider.Resolve<IEventAggregator>();
            Service.UpdateResponse += Service_UpdateResponse;
        }
        #endregion

        #region ------------Field------------
        private OneModel _one;
        private SerialPort comm = new SerialPort();
        private readonly IDialogHostService _dialogHostService;
        private readonly IContainerProvider _containerProvider;
        public readonly IEventAggregator aggregator;
        #endregion

        #region ------------Property------------


        /// <summary>
        /// 电机一模型
        /// </summary>
        public OneModel One
        {
            get { return _one; }
            set { _one = value; RaisePropertyChanged(); }
        }


        private bool _isConnected;
        /// <summary>
        /// 是否已经连接设备
        /// </summary>
        public bool IsConnected
        {
            get { return _isConnected; }
            set { _isConnected = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 下位机服务端接口
        /// </summary>
        public IAsynRWService Service { get; set; }

        #endregion

        #region ------------Command------------
        public DelegateCommand ShowDeviceCommand { get; set; }
        public string DialogHostName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DelegateCommand SaveCommand { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DelegateCommand CancelCommand { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
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
            One.ButtonEventCommand = new DelegateCommand<Object>(ButtonEvent);
        }

        /// <summary>
        /// 初始化属性
        /// </summary>
        private void InitProperty()
        {
            IsConnected = false;
            Service = _containerProvider.Resolve<IServiceFactory>().GetAsynRWService("SPMC");
        }

        /// <summary>
        /// 显示设备弹窗
        /// </summary>
        private async void ShowDevice()
        {
            DialogParameters parameter = new DialogParameters();
            parameter.Add("Value", Service);
            var diaglogResult = await this._dialogHostService.ShowDialog("SerialPortView", parameter, CommonModel.Motor5ControllerRegionName);
            if (diaglogResult == null)
                return;
            if (diaglogResult.Result == ButtonResult.OK && diaglogResult.Parameters.ContainsKey("Value"))
            {
                var value = diaglogResult.Parameters.GetValue<IAsynRWService>("Value");
                if(value != null) 
                {
                    Service = value;
                    IsConnected = Service.IsOpen;
                }
            }
        }


        /// <summary>
        /// 执行命令事件：Button按钮
        /// </summary>
        /// <param name="obj"></param>
        private void ButtonEvent(Object obj) {
            Button button = obj as Button;
            var Name = button.Content;
            switch (Name) {
                case "使能":
                    ExecuteCommand((int)DataLengthEnum.Four, CreateFunctionCode((int)FunctionCodeEnum.电机使能), CreateSeatNo((int)SeatNoEnum.电机1));
                    break;
                case "失能":
                    ExecuteCommand((int)DataLengthEnum.Four, CreateFunctionCode((int)FunctionCodeEnum.电机失能), CreateSeatNo((int)SeatNoEnum.电机1));
                    break;
                case "设置目标位置":
                    ExecuteCommand((int)DataLengthEnum.Eight, CreateFunctionCode((int)FunctionCodeEnum.设置目标位置), CreateSeatNo((int)SeatNoEnum.电机1));
                    break;
                case "获取目标位置":
                    ExecuteCommand((int)DataLengthEnum.Four, CreateFunctionCode((int)FunctionCodeEnum.获取目标位置), CreateSeatNo((int)SeatNoEnum.电机1));
                    break;
                case "获取实时位置":
                    ExecuteCommand((int)DataLengthEnum.Four, CreateFunctionCode((int)FunctionCodeEnum.获取实时位置), CreateSeatNo((int)SeatNoEnum.电机1));
                    break;
                case "设置原点位置":
                    ExecuteCommand((int)DataLengthEnum.Eight, CreateFunctionCode((int)FunctionCodeEnum.设置原点位置), CreateSeatNo((int)SeatNoEnum.电机1));
                    break;
                case "获取原点位置":
                    ExecuteCommand((int)DataLengthEnum.Four, CreateFunctionCode((int)FunctionCodeEnum.获取原点位置), CreateSeatNo((int)SeatNoEnum.电机1));
                    break;
                case "设置零点位置":
                    ExecuteCommand((int)DataLengthEnum.Eight, CreateFunctionCode((int)FunctionCodeEnum.获取零点位置), CreateSeatNo((int)SeatNoEnum.电机1));
                    break;
                case "获取零点位置":
                    ExecuteCommand((int)DataLengthEnum.Four, CreateFunctionCode((int)FunctionCodeEnum.获取零点位置), CreateSeatNo((int)SeatNoEnum.电机1));
                    break;
                case "回到原点":
                    ExecuteCommand((int)DataLengthEnum.Four, CreateFunctionCode((int)FunctionCodeEnum.设置回到原点位置), CreateSeatNo((int)SeatNoEnum.电机1));
                    break;
                case "回到零点":
                    ExecuteCommand((int)DataLengthEnum.Four, CreateFunctionCode((int)FunctionCodeEnum.设置回到零点位置), CreateSeatNo((int)SeatNoEnum.电机1));
                    break;
            }
        }


        /// <summary>
        /// 生成报文并发送消息
        /// </summary>
        /// <param name="Type"></param>
        private byte[] ExecuteCommand(int DataLength, byte FunctionCode, byte SeatNo)
        {
            //动态生成报文
            var Bytes = GetVacuumValue(DataLength, FunctionCode, SeatNo);
            //发送报文
            SendMsg(Bytes);
            return Bytes;
        }


        /// <summary>
        /// 动态生成报文
        /// </summary>
        /// <param name="dataLength">数据位长度</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="No">机位</param>
        /// <returns></returns>
        private static byte[] GetVacuumValue(int DataLength, byte FunctionCode, byte SeatNo)
        {
            if (DataLength == 4)
            {
                byte[] bytes = new byte[4];
                bytes[0] = 0x53;
                bytes[1] = 0x04;
                bytes[2] = FunctionCode;
                bytes[3] = SeatNo;
                return bytes;
            }
            else if (DataLength == 8)
            {
                byte[] bytes = new byte[8];
                bytes[0] = 0x53;
                bytes[1] = 0x08;
                bytes[2] = FunctionCode;
                bytes[3] = SeatNo;
                bytes[4] = new byte();
                bytes[5] = new byte();
                bytes[6] = new byte();
                bytes[7] = new byte();
                return bytes;
            }
            else
            {
                return new byte[0];
            }
        }


        /// <summary>
        /// 获取报文-功能码
        /// </summary>
        /// <param name="Code"></param>
        /// <returns></returns>
        private static byte CreateFunctionCode(int Code) {
            byte Value = new byte();
            switch (Code) {
                case 1:
                    Value = 0x01;
                    break;
                case 2:
                    Value = 0x02;
                    break;
                case 3:
                    Value = 0x03;
                    break;
                case 4:
                    Value = 0x04;
                    break;
                case 5:
                    Value = 0x05;
                    break;
                case 6:
                    Value = 0x06;
                    break;
                case 7:
                    Value = 0x07;
                    break;
                case 8:
                    Value = 0x08;
                    break;
                case 9:
                    Value = 0x09;
                    break;
                case 10:
                    Value = 0x0A;
                    break;
                case 11:
                    Value = 0x0B;
                    break;
            }
            return Value;
        }


        /// <summary>
        /// 获取报文-机位码
        /// </summary>
        /// <param name="Code"></param>
        /// <returns></returns>
        private static byte CreateSeatNo(int Code)
        {
            byte Value = new byte();
            switch (Code)
            {
                case 1:
                    Value = 0x01;
                    break;
                case 2:
                    Value = 0x02;
                    break;
                case 3:
                    Value = 0x03;
                    break;
                case 4:
                    Value = 0x04;
                    break;
                case 5:
                    Value = 0x05;
                    break;
            }
            return Value;
        }


        /// <summary>
        /// 数据位长度枚举
        /// </summary>
        private enum DataLengthEnum {

            Four = 4,
            Eight = 8
        }


        /// <summary>
        /// 功能码枚举
        /// </summary>
        private enum FunctionCodeEnum
        {
            电机使能 = 1,
            电机失能,
            设置目标位置,
            获取目标位置,
            获取实时位置,
            获取原点位置,
            设置原点位置,
            获取零点位置,
            设置零点位置,
            设置回到原点位置,
            设置回到零点位置
        }

        /// <summary>
        /// 功能码枚举
        /// </summary>
        private enum SeatNoEnum
        {
            电机1 = 1,
            电机2,
            电机3,
            电机4,
            电机5
        }



        /// <summary>
        /// 串口通信：发送消息
        /// </summary>
        /// <param name="msg"></param>
        private void SendMsg(byte[] msg)
        {
            Service.SendMsg(msg);
        }

        /// <summary>
        /// 串口通信：接收回调信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Service_UpdateResponse(object sender, byte[] e)
        {
            List<int> ListInfos = new List<int>(); ;
            var MessageInfo = ByteZH(e);
            aggregator.SendMessage($"执行命令，接收回调信息: {MessageInfo}！");
        }

        /// <summary>
        /// 报文转换
        /// </summary>
        /// <returns></returns>
        private string ByteZH(byte[] e) {
            List<string> ListInfos = new List<string>(); ;
            var msg = e;
            if (e.Length > 0)
            {
                foreach (byte b in e)
                {
                    var ByteInfo = b.ToString("X2");
                    ListInfos.Add(ByteInfo.ToString());
                }
            }
            return string.Join(",", ListInfos);
        }

      

        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
