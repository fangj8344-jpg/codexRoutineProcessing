using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Printing;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using UtilityTools.Core.Model;
using UtilityTools.Modules.FZBPumpController.Protocol;
using UtilityTools.Services.Interfaces;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Modules.FZBPumpController.Model
{
    internal class FZBModel : BindableBase
    {
        #region ------------Constructor------------
        public FZBModel(IContainerProvider containerProvider) {
            _containerProvider = containerProvider;
            SerialPortService = _containerProvider.Resolve<IServiceFactory>().GetAsynRWService("SPHV");
            SerialPortService.UpdateResponse += Device_UpdateResponse;
            _response = new byte[4096];
            _responseLength = 0;
            //InitCommand();
            Logs = new ObservableCollection<string>();
            SendSwitchControlModels = new ObservableCollection<AllLabelsModel>();
            DataReadbackControlModels = new ObservableCollection<AllLabelsModel>();
            DataDeliveryControlModels = new ObservableCollection<AllLabelsModel>();


            SendSwitchControlModels.Add(new AllLabelsModel() { Name = "待机", Destination = "001", Value = false, Data = "", Param = "002", SetSendSwitchAction = SendSwitchControlMessage });
            SendSwitchControlModels.Add(new AllLabelsModel() { Name = "泵组", Destination = "001", Value = false, Data = "", Param = "010", SetSendSwitchAction = SendSwitchControlMessage });
            SendSwitchControlModels.Add(new AllLabelsModel() { Name = "电动泵", Destination = "001", Value = false, Data = "", Param = "023", SetSendSwitchAction = SendSwitchControlMessage });
            SendSwitchControlModels.Add(new AllLabelsModel() { Name = "转速设置模式", Destination = "001", Value = false, Data = "", Param = "026", SetSendSwitchAction = SendSwitchControlMessage });
            DataReadbackControlModels.Add(new AllLabelsModel() { Name = "错误代码", Destination = "001", Value = false, Data = "", Param = "303", DataReadbackControlAction = DataReadbackControlMessage });
            DataReadbackControlModels.Add(new AllLabelsModel() { Name = "设定转速(Hz)", Destination = "001", Value=false, Data = "", Param = "308" ,DataReadbackControlAction = DataReadbackControlMessage });
            DataReadbackControlModels.Add(new AllLabelsModel() { Name = "实际转速 (Hz)" , Destination = "001" , Value = false, Data = "", Param = "309" , DataReadbackControlAction = DataReadbackControlMessage });
            DataReadbackControlModels.Add(new AllLabelsModel() { Name = "驱动电流" , Destination = "001" , Value = false, Data = "", Param = "310" , DataReadbackControlAction = DataReadbackControlMessage });
            DataReadbackControlModels.Add(new AllLabelsModel() { Name = "泵运行时间" , Destination = "001" , Value = false, Data = "", Param = "311" , DataReadbackControlAction = DataReadbackControlMessage });
            DataReadbackControlModels.Add(new AllLabelsModel() { Name = "控制器固件版本" , Destination = "001" , Value = false, Data = "", Param = "312" , DataReadbackControlAction = DataReadbackControlMessage });
            DataReadbackControlModels.Add(new AllLabelsModel() { Name = "驱动电压" , Destination = "001" , Value = false, Data = "", Param = "313" , DataReadbackControlAction = DataReadbackControlMessage });
            DataReadbackControlModels.Add(new AllLabelsModel() { Name = "控制器运行时间" , Destination = "001" , Value = false, Data = "", Param = "314" , DataReadbackControlAction = DataReadbackControlMessage });
            DataReadbackControlModels.Add(new AllLabelsModel() { Name = "额定转速(Hz)" , Destination = "001" , Value = false, Data = "", Param = "315" , DataReadbackControlAction = DataReadbackControlMessage });
            DataReadbackControlModels.Add(new AllLabelsModel() { Name = "驱动功率" , Destination = "001" , Value = false, Data = "", Param = "316" , DataReadbackControlAction = DataReadbackControlMessage });
            DataReadbackControlModels.Add(new AllLabelsModel() { Name = "电子设备温度" , Destination = "001" , Value = false, Data = "", Param = "326" , DataReadbackControlAction = DataReadbackControlMessage });
            DataReadbackControlModels.Add(new AllLabelsModel() { Name = "泵底座温度" , Destination = "001" , Value = false, Data = "", Param = "330" , DataReadbackControlAction = DataReadbackControlMessage });
            DataReadbackControlModels.Add(new AllLabelsModel() { Name = "轴承温度" , Destination = "001" , Value = false, Data = "", Param = "342" , DataReadbackControlAction = DataReadbackControlMessage });
            DataReadbackControlModels.Add(new AllLabelsModel() { Name = "马达温度" , Destination = "001" , Value = false, Data = "", Param = "346" , DataReadbackControlAction = DataReadbackControlMessage  });
            DataReadbackControlModels.Add(new AllLabelsModel() { Name = "控制器的名称" , Destination = "001" , Value = false, Data = "", Param = "349" , DataReadbackControlAction = DataReadbackControlMessage });
            DataReadbackControlModels.Add(new AllLabelsModel() { Name = "控制器硬件版本"  , Destination = "001" , Value = false, Data = "", Param = "354" , DataReadbackControlAction = DataReadbackControlMessage });
            DataReadbackControlModels.Add(new AllLabelsModel() { Name = "错误代码历史记录，项 1" , Destination = "001" ,Value = false, Data = "", Param = "360" , DataReadbackControlAction = DataReadbackControlMessage });
            DataReadbackControlModels.Add(new AllLabelsModel() { Name = "设定转速(转/分)" , Destination = "001" , Value = false, Data = "", Param = "397" , DataReadbackControlAction = DataReadbackControlMessage });
            DataReadbackControlModels.Add(new AllLabelsModel() { Name = "实际转速(转/分)" , Destination = "001" , Value = false, Data = "", Param = "398" , DataReadbackControlAction = DataReadbackControlMessage });
            DataReadbackControlModels.Add(new AllLabelsModel() { Name = "额定转速(转/分)" , Destination = "001" , Value = false, Data = "", Param = "399" , DataReadbackControlAction = DataReadbackControlMessage });
            DataDeliveryControlModels.Add(new AllLabelsModel() { Name = "转速设置模式中的设定值" , Destination = "001" , Value = false, Data = "0", Param = "707" , DataDeliveryControlAction = DataDeliveryControlMessage });
            DataDeliveryControlModels.Add(new AllLabelsModel() { Name = "待机时转速设定值" , Destination = "001", Value = false, Data = "0", Param = "717" , DataDeliveryControlAction = DataDeliveryControlMessage });
            DataDeliveryControlModels.Add(new AllLabelsModel() { Name = "RS-485 接口地址", Destination = "001", Value = false, Data = "0", Param = "797" , DataDeliveryControlAction = DataDeliveryControlMessage });
        }
        #endregion

        #region ------------Field------------------
        private readonly IContainerProvider _containerProvider;

        byte[]? _response;
        byte[]? frame;
        int _responseLength;

        byte[]? addr = new byte[3];
        byte[]? command = new byte[2];
        byte[]? param = new byte[3];
        byte[]? data;
        byte[]? length = new byte[2];
        #endregion

        #region ------------Property---------------
        /// <summary>
        /// 开关控制
        /// </summary>
        private ObservableCollection<AllLabelsModel>? _sendSwitchControlModels;
        
        public ObservableCollection<AllLabelsModel>? SendSwitchControlModels
        {
            get { return _sendSwitchControlModels; }
            set { _sendSwitchControlModels = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 通讯日志
        /// </summary>
        private ObservableCollection<string> _logs;

        public ObservableCollection<string> Logs
        {
            get { return _logs; }
            set { _logs = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 数据回读
        /// </summary>
        private ObservableCollection<AllLabelsModel>? _dataReadbackControlModels;

        public ObservableCollection<AllLabelsModel>? DataReadbackControlModels
        {
            get { return _dataReadbackControlModels; }
            set { _dataReadbackControlModels = value; RaisePropertyChanged(); }
        }


        /// <summary>
        /// 数据下发
        /// </summary>
        private ObservableCollection<AllLabelsModel>? _dataDeliveryControlModels;

        public ObservableCollection<AllLabelsModel>? DataDeliveryControlModels
        {
            get { return _dataDeliveryControlModels;  }
            set { _dataDeliveryControlModels = value; RaisePropertyChanged(); }
        }

        /// <summary>   
        /// 串口异步通信服务
        /// </summary>
        private IAsynRWService _serialPortService;
        
        public IAsynRWService SerialPortService
        {
            get { return _serialPortService; }
            set { _serialPortService = value; RaisePropertyChanged(); }
        }
        #endregion


        #region ------------Command----------------
        #endregion

        #region ------------PublicMethod-------------
        /// <summary>
        /// 添加日志信息
        /// </summary>
        /// <param name="log">日志信息</param>
        /// <param name="isRead">是否是读取</param>
        public void AddLog(string log, bool isRead = true)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Logs == null)
                {
                    Logs = new ObservableCollection<string>();
                }

                if (isRead)
                {
                    Logs.Add($"{DateTime.Now.ToString("t")} 读取: {log}");
                }
                else
                {
                    Logs.Add($"{DateTime.Now.ToString("t")} 发送: {log}");
                }
            });

        }

        /// <summary>
        /// 数据发送
        /// </summary>
        /// <param name="bytes"></param>
        public void SendMsg(byte[] bytes)
        {
            if (SerialPortService.IsOpen)
                SerialPortService.SendMsg(bytes);
            else
            {
                AddLog("无设备连接", false);
                return;
            }
            AddLog(SerialPortService.GetCmdString(bytes, bytes.Length), false);
        }
        #endregion

        #region ------------PrivateMethod------------
        /// <summary>
        /// 开关控制
        /// </summary>
        /// <param name="IsOpen">开关</param>
        /// <param name="Destination">目的地址</param>
        /// <param name="Param">功能码</param>
        private void SendSwitchControlMessage(bool IsOpen, string Destination, string Param)
        {
            byte[] bytes = FZBPumpControllerProtocol.SendSwitchControlMessage(IsOpen, Destination, Param);
            SendMsg(bytes);
        }

        /// <summary>
        /// 数据回读
        /// </summary>
        /// <param name="Destination">目的地址</param>
        /// <param name="Param">功能码</param>
        private void DataReadbackControlMessage(string Destination,string Param) 
        {
            byte[] bytes = FZBPumpControllerProtocol.DataReadbackControlMessage(Destination, Param);
            SendMsg(bytes);
        }

        /// <summary>
        /// 数据下发
        /// </summary>
        /// <param name="Data">下发数据</param>
        /// <param name="Destination">目的地址</param>
        /// <param name="Param">功能码</param>
        private void DataDeliveryControlMessage(string Data,string Destination,string Param)
        {
            byte[] bytes = FZBPumpControllerProtocol.DataDeliveryControlMessage(Data,Destination,Param);
            SendMsg(bytes);
        }

        /// <summary>
        /// 查找功能参数对应对象
        /// </summary>
        /// <param name="param">功能参数</param>
        /// <returns></returns>
        private AllLabelsModel SerachObject(string param)
        {
            if (param.Equals("002") || param.Equals("010") || param.Equals("023") || param.Equals("026"))
            {
                foreach (AllLabelsModel allLabelsModel in SendSwitchControlModels)
                {
                    if (allLabelsModel.Param.Equals(param)) return allLabelsModel;
                }
            }

            else if (param.Equals("707") || param.Equals("717") || param.Equals("797"))
            {
                foreach (AllLabelsModel allLabelsModel in DataDeliveryControlModels)
                {
                    if (allLabelsModel.Param.Equals(param)) return allLabelsModel;
                }
            }
            else
            {
                foreach (AllLabelsModel allLabelsModel in DataReadbackControlModels)
                {
                    if (allLabelsModel.Param.Equals(param)) return allLabelsModel;
                }
            }
            return null;


        }
            /// <summary>
            /// 数据回包
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void Device_UpdateResponse(object sender, byte[] e)
            {
                if (e == null)
                    return;
                
                Array.Copy(e,0,_response,_responseLength,e.Length); 
                _responseLength += e.Length;
                
                var firstnumber = Array.IndexOf(_response,(byte)13,0,_responseLength);
                if (firstnumber == -1) return;
                frame = new byte[firstnumber+1];
                
                Array.Copy(_response, 0, frame, 0, firstnumber+1);
                Array.Copy(_response,firstnumber+1,_response,0, _response.Length-frame.Length-1);
                
                bool examine=FZBPumpControllerProtocol.Examine(frame);
                
                if (!examine)
                {
                    return;              
                }
                
                Array.Copy(frame, 0, addr, 0,3);
                Array.Copy(frame, 3, command, 0, 2);
                Array.Copy(frame, 5, param, 0, 3);
                Array.Copy(frame, 8, length, 0, 2);
                int index = Convert.ToInt16(Encoding.UTF8.GetString(length));
                data = new byte[index];
                Array.Copy(frame, 10, data, 0, data.Length);
                string receiverdata;
                string ParamsValue = Convert.ToInt16(Encoding.UTF8.GetString(param)).ToString("D3");
                AllLabelsModel allLabelsModel=SerachObject(ParamsValue);
                
                switch (ParamsValue)
                {
                    case "303":
                        receiverdata = Encoding.UTF8.GetString(data);   
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "306":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "308":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "309":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "310":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "311":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "312":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "313":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "314":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "315":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "316":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "326":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "330":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "342":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "346":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "349":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "354":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "360":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "361":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "362":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "363":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "364":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "365":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "366":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "367":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "368":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "369":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "397":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "398":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
                    case "399":
                        receiverdata = Encoding.UTF8.GetString(data);
                        allLabelsModel.Data += receiverdata;
                        break;
              }
            AddLog(SerialPortService.GetCmdString(frame, frame.Length));
            _responseLength -=frame.Length;
            }
        #endregion

        #region ------------StaticMethod------------
        #endregion
        }
}
