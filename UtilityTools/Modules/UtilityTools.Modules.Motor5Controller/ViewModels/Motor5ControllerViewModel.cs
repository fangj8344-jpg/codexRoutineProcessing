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
using UtilityTools.Core.Model;
using UtilityTools.Core.Helper;
using System.Collections;

namespace UtilityTools.Modules.Motor5Controller.ViewModels
{
    internal class Motor5ControllerViewModel : RegionViewModelBase
    {
        #region ------------Constructor------------
        public Motor5ControllerViewModel(IDialogHostService dialogHostService, IContainerProvider containerProvider)
            : base(containerProvider)
        {
            CacheCallBackDate = new List<byte>();
            MotorList = CretetModels();
            this._containerProvider = containerProvider;
            this._dialogHostService = dialogHostService;
            InitCommand();
            InitProperty();
            //消息提示
            aggregator = containerProvider.Resolve<IEventAggregator>();
            //下位机数据回调
            Service.UpdateResponse += Service_UpdateResponse;
        }
        #endregion

        #region ------------Field------------
        private List<byte> _cachecallbackdate;
        private bool _isConnected;
        private ObservableCollection<MotorModel> _motorlist;
        private SerialPort comm = new SerialPort();
        private readonly IDialogHostService _dialogHostService;
        private readonly IContainerProvider _containerProvider;
        private readonly IEventAggregator aggregator;
        #endregion

        #region ------------Property------------
        /// <summary>
        /// 电机模型
        /// </summary>
        //public MotorModel Motor
        //{
        //    get { return _motor; }
        //    set { _motor = value; RaisePropertyChanged(); }
        //}

        public ObservableCollection<MotorModel> MotorList
        {
            get { return _motorlist; }
            set { _motorlist = value; RaisePropertyChanged(); }
        }

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


        /// <summary>
        /// 指定回调信息缓存
        /// </summary>
        public List<byte> CacheCallBackDate
        {
            get { return _cachecallbackdate; }
            set { _cachecallbackdate = value; RaisePropertyChanged(); }
        }

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
        /// 初始化电机信息
        /// </summary>
        /// <returns></returns>
        private ObservableCollection<MotorModel> CretetModels() {
            ObservableCollection<MotorModel> List = new ObservableCollection<MotorModel>();
            for (int i = 1; i < 6; i++)
            {
                MotorModel NewModel = new MotorModel();
                NewModel.MotorNo = i;
                NewModel.MotorName = "电机" + i.ToString();
                List.Add(NewModel);
            }
            return List;
        }

        /// <summary>
        /// 初始化指令
        /// </summary>
        private void InitCommand()
        {
            ShowDeviceCommand = new DelegateCommand(ShowDevice);
            foreach (var item in MotorList)
            {
                item.ButtonEventCommand = new DelegateCommand<Object>(ButtonEvent);
            }
        }


        /// <summary>
        /// 初始化属性
        /// </summary>
        private void InitProperty()
        {
            List<byte> CacheCallBackDate = new List<byte>();
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
        /// 电机执行命令事件：Button按钮
        /// </summary>
        /// <param name="obj"></param>
        private void ButtonEvent(Object obj) {
            Button button = obj as Button;
            var Model = (MotorModel)button.DataContext;
            string Name = (string)button.Content;
            switch (Name) {
                case "使能":
                    ExecuteCommand(GetLengthByFunctionCode(Name), CreateFunctionCode((int)FunctionCodeEnum.使能), CreateSeatNo(Model.MotorNo),0);
                    break;
                case "失能":
                    ExecuteCommand(GetLengthByFunctionCode(Name), CreateFunctionCode((int)FunctionCodeEnum.失能), CreateSeatNo(Model.MotorNo), 0);
                    break;
                case "设置目标位置":
                    ExecuteCommand(GetLengthByFunctionCode(Name), CreateFunctionCode((int)FunctionCodeEnum.设置目标位置), CreateSeatNo(Model.MotorNo), Model.SetTargetPosition);
                    break;
                case "获取目标位置":
                    ExecuteCommand(GetLengthByFunctionCode(Name), CreateFunctionCode((int)FunctionCodeEnum.获取目标位置), CreateSeatNo(Model.MotorNo), 0);
                    break;
                case "获取实时位置":
                    ExecuteCommand(GetLengthByFunctionCode(Name), CreateFunctionCode((int)FunctionCodeEnum.获取实时位置), CreateSeatNo(Model.MotorNo), 0);
                    break;
                case "设置原点位置":
                    ExecuteCommand(GetLengthByFunctionCode(Name), CreateFunctionCode((int)FunctionCodeEnum.设置原点位置), CreateSeatNo(Model.MotorNo),  Model.SetTargetPosition);
                    break;
                case "获取原点位置":
                    ExecuteCommand(GetLengthByFunctionCode(Name), CreateFunctionCode((int)FunctionCodeEnum.获取原点位置), CreateSeatNo(Model.MotorNo), 0);
                    break;
                case "设置零点位置":
                    ExecuteCommand(GetLengthByFunctionCode(Name), CreateFunctionCode((int)FunctionCodeEnum.设置零点位置), CreateSeatNo(Model.MotorNo), Model.SetZeroPosition);
                    break;
                case "获取零点位置":
                    ExecuteCommand(GetLengthByFunctionCode(Name), CreateFunctionCode((int)FunctionCodeEnum.获取零点位置), CreateSeatNo(Model.MotorNo), 0);
                    break;
                case "回到原点":
                    ExecuteCommand(GetLengthByFunctionCode(Name), CreateFunctionCode((int)FunctionCodeEnum.回到原点), CreateSeatNo(Model.MotorNo), 0);
                    break;
                case "回到零点":
                    ExecuteCommand(GetLengthByFunctionCode(Name), CreateFunctionCode((int)FunctionCodeEnum.回到零点), CreateSeatNo(Model.MotorNo), 0);
                    break;
            }
        }



        /// <summary>
        /// 生成报文并发送消息
        /// </summary>
        /// <param name="Type"></param>
        private void ExecuteCommand(int DataLength, byte FunctionCode, byte SeatNo,int PositionNum)
        {
            //动态生成报文
            var Bytes = GetVacuumValue(DataLength, FunctionCode, SeatNo, PositionNum);
            //发送报文
            SendMsg(Bytes);
        }


        /// <summary>
        /// 动态生成报文
        /// </summary>
        /// <param name="dataLength">数据位长度</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="No">机位</param>
        /// <param name="PositionNum">位置码</param>
        /// <returns></returns>
        private static byte[] GetVacuumValue(int DataLength, byte FunctionCode, byte SeatNo,int PositionNum)
        {
            if (DataLength == 8)
            {
                //数据处理（32位INT型数据；最高位表示符号位，1表示负数，0表示正数，其余31位表示数字位，
                //最大能表示0x 7F FF FF FF(2147483647(十进制))，最小能表示-0x 7F FF FF FF(-2147483647(十进制))）
                bool flag = PositionNum > 0;
                int positiveNum = flag ? PositionNum : -PositionNum;
                byte[] data = new byte[4];
                data[0] = (byte)((positiveNum & 0xFF000000) >> 24);
                data[1] = (byte)((positiveNum & 0xFF0000) >> 16);
                data[2] = (byte)((positiveNum & 0xFF00) >> 8);
                data[3] = (byte)((positiveNum & 0xFF));
                if (!flag)
                {
                    data[0] = (byte)(data[0] | 0x80);
                }
                else
                {
                    data[0] = (byte)(data[0] & 0x7F);
                }
                byte[] bytes = new byte[8];
                bytes[0] = 0x53;
                bytes[1] = 0x08;
                bytes[2] = FunctionCode;
                bytes[3] = SeatNo;
                bytes[4] = data[0];
                bytes[5] = data[1];
                bytes[6] = data[2];
                bytes[7] = data[3];
                return bytes;
            }
            else if (DataLength == 4)
            {
                byte[] bytes = new byte[4];
                bytes[0] = 0x53;
                bytes[1] = 0x04;
                bytes[2] = FunctionCode;
                bytes[3] = SeatNo;
                return bytes;
            }
            else {
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
        /// 返回操作数据位长度
        /// </summary>
        private int GetLengthByFunctionCode(string FunctionCodeName){
            var ResultInfo = 0;
            //四位数据位对应功能
            List<string> FourList = new List<string>();
            FourList.Add("使能");
            FourList.Add("失能");
            FourList.Add("获取目标位置");
            FourList.Add("获取实时位置");
            FourList.Add("获取原点位置");
            FourList.Add("获取零点位置");
            FourList.Add("回到原点");
            FourList.Add("回到零点");
            //八位数据位对应功能
            List<string> EightList = new List<string>();
            EightList.Add("设置目标位置");
            EightList.Add("设置原点位置");
            EightList.Add("设置零点位置");
            if (FourList.Contains(FunctionCodeName)) ResultInfo = 4;
            if (EightList.Contains(FunctionCodeName)) ResultInfo = 8;
            return ResultInfo;
        }


        /// <summary>
        /// 功能码枚举
        /// </summary>
        private enum FunctionCodeEnum
        {
            使能 = 1,
            失能,
            设置目标位置,
            获取目标位置,
            获取实时位置,
            获取原点位置,
            设置原点位置,
            获取零点位置,
            设置零点位置,
            回到原点,
            回到零点
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
            //报文处理（缓存处理）
            if (e.Count() > 0) {
                foreach (var item in e)
                {
                    CacheCallBackDate.Add(item);
                }
            }
            if (CacheCallBackDate.Count() >= 8) Service_CacheMessage(CacheCallBackDate);
        }

        /// <summary>
        /// 缓存：报文处理
        /// </summary>
        /// <param name="bytelist"></param>
        /// <returns></returns>
        private List<byte> Service_CacheMessage(List<byte> bytelist)
        {
            //读取缓存报文，根据约束；8位为一段完成报文信息
            if (bytelist.Count < 8)
            {
                return bytelist;
            }
            else
            {
                //取目标报文
                var DataByte = CacheCallBackDate.GetRange(0, 8);
                //清楚缓存已取目标报文
                CacheCallBackDate.RemoveRange(0, 8);
                byte[] Bytes = new byte[8];
                for ( int i = 0;  i < DataByte.Count();  i++)
                {
                    Bytes[i] = DataByte[i];
                }
                Service_CallbackProcessing(Bytes);
                return Service_CacheMessage(CacheCallBackDate);
            }
        }



        /// <summary>
        /// 报文解析：操作提示，页面数据动态渲染
        /// </summary>
        /// <param name="e"></param>
        private void Service_CallbackProcessing(byte[] e) {
            var DataInfo = DataTypeCaster.ByteArrayToString(e, e.Length);
            var DataList = DataInfo.Split(' ');
            if (DataList.Count() > 0) DataList = DataList.Where(q => q != "").ToArray();
            if (e.Length == 8 && DataList.Count() == 8)
            {
                if (MotorList.Where(q => q.MotorNo == Convert.ToInt32(DataList[3], 16)).Count() > 0)
                {
                    //解析机位信息（协议约束第四位为机位信息）
                    var Model = MotorList.Where(q => q.MotorNo == Convert.ToInt32(DataList[3], 16)).First();
                    //解析机位相关位置信息（协议约束后四位为位置相关信息,高位为正负数标识，0表示正数，1表示负数）
                    string PositionString = DataTypeCaster.ByteArrayToBinaryStr(e.Reverse().Take(4).Reverse().ToArray());
                    var NewPositionString = PositionString;
                    StringBuilder NewPositionBuilder = new StringBuilder(NewPositionString);
                    NewPositionBuilder[0] = '0';
                    NewPositionString = NewPositionBuilder.ToString();
                    var PositionValue = Convert.ToInt32(NewPositionString, 2);
                    PositionValue = PositionString.Substring(0, 1) == "0" ? PositionValue : -PositionValue;
                    //解析相关操作反馈（协议约束最后一位为操作反馈相关信息）
                    var OperateByte = Convert.ToInt32(DataList.Last(), 16);
                    //协议约束:第三位功能码
                    try
                    {
                        switch (DataList[2])
                        {
                            case "0x01":
                                if (OperateByte == 0)
                                {
                                    Model.MotorStatus.First().Value = "使能";
                                    aggregator.SendMessage($"操作成功,电机状态：使能");
                                }
                                else if (OperateByte == 1)
                                {
                                    aggregator.SendMessage($"使能失败，请核实指令");
                                }
                                else
                                {
                                    aggregator.SendMessage($"报文解析功能码有误，请核实指令");
                                }
                                break;
                            case "0x02":
                                if (OperateByte == 0)
                                {
                                    Model.MotorStatus.First().Value = "失能";
                                    aggregator.SendMessage($"操作成功,电机状态：失能");
                                }
                                else if (OperateByte == 1)
                                {
                                    aggregator.SendMessage($"失能失败，请核实指令");
                                }
                                else
                                {
                                    aggregator.SendMessage($"报文解析功能码有误，请核实指令");
                                }
                                break;
                            case "0x03":
                                Model.SetTargetPosition = PositionValue;
                                aggregator.SendMessage($"设置目标位置成功，目标位置：" + PositionValue.ToString());
                                break;
                            case "0x04":
                                Model.ObtainTargetPosition = PositionValue;
                                aggregator.SendMessage($"获取目标位置成功，目标位置：" + PositionValue.ToString());
                                break;
                            case "0x05":
                                Model.ObtainRealTimePosition = PositionValue;
                                aggregator.SendMessage($"获取实时位置成功，目标位置：" + PositionValue.ToString());
                                break;
                            case "0x06":
                                Model.ObtainOriginPosition = PositionValue;
                                aggregator.SendMessage($"获取原点位置成功，目标位置：" + PositionValue.ToString());
                                break;
                            case "0x07":
                                Model.SetOriginPosition = PositionValue;
                                aggregator.SendMessage($"设置原点位置成功，目标位置：" + PositionValue.ToString());
                                break;
                            case "0x08":
                                Model.ObtainZeroPosition = PositionValue;
                                aggregator.SendMessage($"获取零点位置成功，目标位置：" + PositionValue.ToString());
                                break;
                            case "0x09":
                                Model.SetZeroPosition = PositionValue;
                                aggregator.SendMessage($"设置零点位置成功，目标位置：" + PositionValue.ToString());
                                break;
                            case "0x0A":
                                if (OperateByte == 0) aggregator.SendMessage($"操作成功，回到原点");
                                else if (OperateByte == 1) aggregator.SendMessage($"操作失败，请核实指令");
                                break;
                            case "0x0B":
                                if (OperateByte == 0) aggregator.SendMessage($"操作成功，回到零点");
                                else if (OperateByte == 1) aggregator.SendMessage($"操作失败，请核实指令");
                                break;
                            default:
                                aggregator.SendMessage($"报文解析功能码有误，请核实指令");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        aggregator.SendMessage($"报文解析机位码有误，错误信息：" + ex.ToString());
                    }
                }
                else
                {
                    aggregator.SendMessage($"报文解析机位码有误，请核实指令");
                }
            }
            else
            {
                aggregator.SendMessage($"报文解析有误，请核实指令");
            }
        }



        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
