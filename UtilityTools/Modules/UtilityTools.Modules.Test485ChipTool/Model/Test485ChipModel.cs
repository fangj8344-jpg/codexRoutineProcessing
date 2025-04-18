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
using System.Windows.Automation.Text;
using System.ComponentModel;
using System.Reflection;
using static UtilityTools.Modules.Test485ChipTool.Model.Test485ChipModel.EnumHelper;

namespace UtilityTools.Modules.Test485ChipTool.Model
{
    /// <summary>
    /// 通讯协议类型
    /// </summary>
    public enum EConnectionType
    {
        [Description("UDP连接")]
        Udp,
        //[Description("TCP")]
        //Tcp,
        //[Description("串口")]
        //SerialPort,
    }

    /// <summary>
    /// 指令码
    /// </summary>
    public enum ECommanName : ushort
    {        
        [Description("获取高压状态")]
        cmdGetSTATUS = 0x0501,
        [Description("读真空规数值")]
        cmdGetVac = 0x0800,
        [Description("获取PID参数")]
        cmdGetPID = 0X0806,
    }

    /// <summary>
    /// 地址码
    /// </summary>
    public enum EAddressCode : ushort
    {
        [Description("0x0201")]
        addressCode_0201 = 0x0201,
    }

    /// <summary>
    /// 设备码
    /// </summary>
    public enum EDeviceID : ushort
    {
        [Description("0x0201")]
        deviceID_0201 = 0x0201,
    }


    public class Test485ChipModel : BindableBase
    {
        #region --------------Construct--------------
        public Test485ChipModel(IContainerProvider containerProvider)
        {
            //对话框所需
            _containerProvider = containerProvider;
            _dialogHostService = containerProvider.Resolve<IDialogHostService>();

            //注册WPF响应         
            ClickTest = new DelegateCommand<string>(Test);//测试模块

            SetDefaultParams();           
        }
        #endregion

        #region -------------Field----------------------------
        private IContainerProvider _containerProvider;
        private readonly IDialogHostService _dialogHostService;

        string _hostIp;
        byte[] _packetmsg;//打包后的报文
        bool _init;//初始化标志 
        short _portStatr = 5001;
        short _portEnd = 5005;

        List<short> _portList;//待通讯的端口号
        Dictionary<short,ITestUnit> _testUnitDict;//存放测试单元
       
        #endregion
        #region------------------------Property--------------------------
        /// <summary>
        /// 枚举辅助类
        /// </summary>
        public static class EnumHelper
        {
            public class EnumItem<T> where T : Enum
            {
                public string Description { get; set; }//枚举的描述信息
                public T Value { get; set; }//枚举的值
                public int NumericValue => Convert.ToInt32(Value);

                public override string ToString() => Description;
            }

            public static List<EnumItem<T>> GetEnumItems<T>() where T : Enum
            {
                var list = new List<EnumItem<T>>();

                foreach (T value in Enum.GetValues(typeof(T)))
                {
                    var fieldInfo = typeof(T).GetField(value.ToString());
                    var description = fieldInfo.GetCustomAttribute<DescriptionAttribute>()?.Description
                                    ?? $"{value} (0x{Convert.ToInt32(value):X4})";

                    list.Add(new EnumItem<T>
                    {
                        Description = description,
                        Value = value
                    });
                }

                return list;
            }

            public static string GetEnumDescription<T>(T value) where T : Enum
            {
                var fieldInfo = value.GetType().GetField(value.ToString());
                return fieldInfo.GetCustomAttribute<DescriptionAttribute>()?.Description ?? value.ToString();
            }
        }

        /// <summary>
        /// 通讯协议
        /// </summary>
        public List<EnumItem<EConnectionType>> ConnectionTypes { get; } =
                new List<EnumItem<EConnectionType>>();
        // 当前选中的命令
        private EnumItem<EConnectionType> _selectedConnectionType;
        public EnumItem<EConnectionType> SelectedConnectionType
        {
            get => _selectedConnectionType;
            set
            {
                if (_selectedConnectionType != value)
                {
                    _selectedConnectionType = value;
                    RaisePropertyChanged();
                    if(!_init)
                    {//除构造函数初始化以外 修改值即重新打包报文
                        _packetmsg = PacketMsg();
                    }
                }
            }
        }

        /// <summary>
        /// 指令码
        /// </summary>
        public List<EnumItem<ECommanName>> CommandNames { get; } =
                new List<EnumItem<ECommanName>>();
        // 当前选中的命令
        private EnumItem<ECommanName> _selectedCommandName;
        public EnumItem<ECommanName> SelectedCommandName
        {
            get => _selectedCommandName;
            set
            {
                if (_selectedCommandName != value)
                {
                    _selectedCommandName = value;
                    RaisePropertyChanged();
                    if (!_init)
                    {//除构造函数初始化以外 修改值即重新打包报文
                        _packetmsg = PacketMsg();
                    }
                }
            }
        }

        /// <summary>
        /// 地址码
        /// </summary>
        public List<EnumItem<EAddressCode>> AddressCodes { get; } =
                new List<EnumItem<EAddressCode>>();
        // 当前选中的地址码
        private EnumItem<EAddressCode> _selectedAddressCode;
        public EnumItem<EAddressCode> SelectedAddressCode
        {
            get => _selectedAddressCode;
            set
            {
                if (_selectedAddressCode != value)
                {
                    _selectedAddressCode = value;
                    RaisePropertyChanged();
                    if (!_init)
                    {//除构造函数初始化以外 修改值即重新打包报文
                        _packetmsg = PacketMsg();
                    }
                }
            }
        }

        /// <summary>
        /// 设备码
        /// </summary>
        public List<EnumItem<EDeviceID>> DeviceIDs { get; } =
                new List<EnumItem<EDeviceID>>();
        // 当前选中的设备码
        private EnumItem<EDeviceID> _selectedDeviceID;
        public EnumItem<EDeviceID> SelectedDeviceID
        {
            get => _selectedDeviceID;
            set
            {
                if (_selectedDeviceID != value)
                {
                    _selectedDeviceID = value;
                    RaisePropertyChanged();
                    if (!_init)
                    {//除构造函数初始化以外 修改值即重新打包报文
                        _packetmsg = PacketMsg();
                    }
                }
            }
        }

        /// <summary>
        /// 目标IP地址
        /// </summary>
        private string _remoteIp;
        public string RemoteIp
        {
            get { return _remoteIp; }
            set
            {
                if (_remoteIp != value)
                {
                    _remoteIp = value;
                    RaisePropertyChanged();
                    if (!_init)
                    {//除构造函数初始化以外 修改值即重新打包报文
                        _packetmsg = PacketMsg();
                        InitTestUnitDict();
                    }
                }
            }
        }

        /// <summary>
        /// 日志
        /// </summary>
        private string _log;
        public string Log
        {
            get { return _log; }
            set { _log = value + "\r\n"; RaisePropertyChanged(); }
        }

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
        /*暂未用上的动态绑定变量
        private short _remotePort_Start;
        /// <summary>
        /// 测试端口范围-起始端口
        /// </summary>
        public short RemotePort_Start
        {
            get { return _remotePort_Start; }
            set
            {
                if (_remotePort_Start != value)
                {
                    _remotePort_Start = value;
                    RaisePropertyChanged();
                }
            }
        }

        private short _remotePort_End;
        /// <summary>
        /// 测试端口范围-终止端口
        /// </summary>
        public short RemotePort_End
        {
            get { return _remotePort_End; }
            set
            {
                if (_remotePort_End != value)
                {
                    _remotePort_End = value;
                    RaisePropertyChanged();
                }
            }
        }

        private short _hostIp;
        /// <summary>
        /// 本地IP地址
        /// </summary>
        public short HostIp
        {
            get { return _hostIp; }
            set
            {
                if (_hostIp != value)
                {
                    _hostIp = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _hostPort;
        /// <summary>
        /// 本机端口
        /// </summary>
        public string HostPort
        {
            get { return _hostPort; }
            set
            {
                if (_hostPort != value)
                {
                    _hostPort = value;
                    RaisePropertyChanged();
                }
            }
        }        
        */
        #endregion
        #region --------------------Command----------------------     
        /// <summary>
        /// WPF响应-测试开始
        /// </summary>
        public DelegateCommand<string> ClickTest { get; set; }

        #endregion
        #region ---------------------PublicMethood----------------------------
        /// <summary>
        /// 初始化动态绑定的值
        /// </summary>
        public void SetDefaultParams()
        {
            _hostIp = GetLocalIPv4();

            _init = true;//防止在SetPropery的过程中进入RaisePropertyChanged
            SetPropery();
            _init = false;//init over

            _packetmsg = PacketMsg();

            //打印报文
            string hexString = BitConverter.ToString(_packetmsg);
            Console.WriteLine(hexString);

            InitTestUnitDict();
        }

        /// <summary>
        /// 填充下拉框的值
        /// </summary>
        public void SetPropery()
        {
            //协议
            foreach (EConnectionType cmd in Enum.GetValues(typeof(EConnectionType)))
            {
                var fieldInfo = cmd.GetType().GetField(cmd.ToString());
                var description = fieldInfo.GetCustomAttribute<DescriptionAttribute>()?.Description ?? cmd.ToString();

                ConnectionTypes.Add(new EnumItem<EConnectionType>
                {
                    Description = description,
                    Value = cmd
                });
            }
            // 设置默认选择
            SelectedConnectionType = ConnectionTypes.FirstOrDefault();

            //命令码
            foreach (ECommanName cmd in Enum.GetValues(typeof(ECommanName)))
            {
                var fieldInfo = cmd.GetType().GetField(cmd.ToString());
                var description = fieldInfo.GetCustomAttribute<DescriptionAttribute>()?.Description ?? cmd.ToString();

                CommandNames.Add(new EnumItem<ECommanName>
                {
                    Description = description,
                    Value = cmd
                });
            }
            SelectedCommandName = CommandNames.First();

            //地址码
            foreach (EAddressCode cmd in Enum.GetValues(typeof(EAddressCode)))
            {
                var fieldInfo = cmd.GetType().GetField(cmd.ToString());
                var description = fieldInfo.GetCustomAttribute<DescriptionAttribute>()?.Description ?? cmd.ToString();

                AddressCodes.Add(new EnumItem<EAddressCode>
                {
                    Description = description,
                    Value = cmd
                });
            }
            SelectedAddressCode = AddressCodes.First();

            //设备码
            foreach (EDeviceID cmd in Enum.GetValues(typeof(EDeviceID)))
            {
                var fieldInfo = cmd.GetType().GetField(cmd.ToString());
                var description = fieldInfo.GetCustomAttribute<DescriptionAttribute>()?.Description ?? cmd.ToString();

                DeviceIDs.Add(new EnumItem<EDeviceID>
                {
                    Description = description,
                    Value = cmd
                });
            }
            SelectedDeviceID = DeviceIDs.First();

            //默认远程地址
            RemoteIp = "192.168.1.88";

            //按钮状态信标
            if (ReadySign == null)
            {
                ReadySign = new ObservableCollection<bool?>();
            }
            ReadySign.Clear();
            for (int i = 0; i < _portEnd - _portStatr + 1; i++)
            {
                ReadySign.Add(null);
            }
        }

        /// <summary>
        /// 打包命令报文
        /// </summary>
        public byte[] PacketMsg()
        {
            byte[] buffer = new byte[36];
            IPAddress ipAddress = IPAddress.Parse(RemoteIp);
            byte[] IpAddress = ipAddress.GetAddressBytes();//得到4字节IP数组
            Array.Reverse(IpAddress);//反转字节数组 反转为小端
            var addressCode = BitConverter.GetBytes((ushort)SelectedAddressCode.NumericValue);

            byte[] combinedAddr = new byte[IpAddress.Length + addressCode.Length];
            Array.Copy(IpAddress, 0, combinedAddr, 0, IpAddress.Length);
            Array.Copy(addressCode, 0, combinedAddr, IpAddress.Length, addressCode.Length);

            var cmd = BitConverter.GetBytes((ushort)SelectedCommandName.NumericValue);
            var id = BitConverter.GetBytes((ushort)SelectedDeviceID.NumericValue);
            return ZepGenericProtocol.GetCmd(combinedAddr, id, cmd, buffer);
        }

        /// <summary>
        /// 填充测试单元
        /// </summary>
        public void InitTestUnitDict()
        {           
            /*  目前仅使用固定的5001-5005端口  不使用动态端口
            
            //RemotePort_Start = 5001;
            //RemotePort_End = 5005;  
            _portList?.Clear();
            _portList = new List<short>();
            if (RemotePort_Start == RemotePort_End)
            {
                _portList.Add(RemotePort_Start);
            }
            else
            {
                if(RemotePort_Start > RemotePort_End)
                {
                    short RemotePort = RemotePort_Start;
                    RemotePort_Start = RemotePort_End;
                    RemotePort_End = RemotePort;
                }

                for(int i = 0;i <= RemotePort_End - RemotePort_Start; i++)
                {
                    short RemotePort = (short)(RemotePort_Start + i);
                    _portList.Add(RemotePort);
                }
            }
            */
           
            _portList = new List<short>();

            //填充端口list
            for (short i = _portStatr; i <= _portEnd; i++)
            {
                _portList.Add(i);
            }

            //填充测试单元
            if (_testUnitDict != null)
            {
                foreach (var testUnit in _testUnitDict)
                {
                    testUnit.Value.Dispose(); // 显式释放
                }
                _testUnitDict.Clear();
            }
            _testUnitDict = new Dictionary<short, ITestUnit>();

            foreach (var Port in _portList)
            {               
                var udpServer = new UdpNetAsyncDevice();
                udpServer.DeviceInstance.TargetIp = _remoteIp;
                udpServer.DeviceInstance.TargetPort = Port;
                udpServer.DeviceInstance.HostIp = _hostIp;
                udpServer.DeviceInstance.HostPort = 0;

                var TestUnit = new SimpleTestUnit(udpServer, _packetmsg,
                    IsTestSuccess);

                _testUnitDict.Add(Port, TestUnit);              
            }
        }

        /// <summary>
        /// 遍历测试单元 开始测试
        /// </summary>
        public void TestStart()
        {
            foreach (var testUnit in _testUnitDict)
            {
                testUnit.Value.TestCom();
            }

            for(int i = 0; i< ReadySign.Count; i++)
            {
                ReadySign[i] = null;
            }
        }

        /// <summary>
        /// 遍历测试单元 获取每个单元的测试结果
        /// </summary>
        public void LogTestResult()
        {
            foreach (var testUnit in _testUnitDict)
            {
                Log += "串口:" + testUnit.Key + "通讯测试" + 
                    ((testUnit.Value.IsOK) ? "成功" : "失败") + "\n\r";

                int signIndex = (testUnit.Key - _portStatr);
                if (signIndex >= 0 &&  signIndex < ReadySign.Count)
                {
                    ReadySign[signIndex] = testUnit.Value.IsOK;
                }
                
            }
        }
        /// <summary>
        /// 获取本机IP地址
        /// </summary>
        public static string GetLocalIPv4()
        {
            string localIP = string.Empty;
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork) // IPv4
                {
                    localIP = ip.ToString();
                    break; // 取第一个 IPv4 地址
                }
            }

            return localIP;
        }
        #endregion
        #region ----------------------PrivateMethod---------------------------
        /// <summary>
        /// 测试接口
        /// </summary>
        private async void  Test(object obj)
        {
            Log = "";//每次测试前 清空右侧测试结果
            TestStart();
            await Task.Delay(1500);//延迟1.5s等待回包ing
            LogTestResult();
        }

        /// <summary>
        /// 触发回包即视为成功 如果需要比对报文 可以将recvMsg进行解析 
        /// </summary>
        private bool IsTestSuccess(byte[] recvMsg)
        {
            bool bRe = recvMsg.Length == _packetmsg.Length;
            return bRe;
        }
        #endregion
    }
}
