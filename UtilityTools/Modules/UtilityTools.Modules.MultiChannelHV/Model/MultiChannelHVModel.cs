using CsvHelper;
using HarfBuzzSharp;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System.Collections;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Timers;
using TouchSocket.Core;
using UtilityTools.Modules.MultiChannelHighVoltageCabinetControl.Protocol;
using UtilityTools.Modules.MultiChannelHV.Entity;
using UtilityTools.Services.Interfaces.IServices;
using UtilityTools.Services.Services;
using static UtilityTools.Modules.MultiChannelHighVoltageCabinetControl.Protocol.MultiChannelHVProtocol;

namespace UtilityTools.Modules.MultiChannelHV.Model
{
    public class MultiChannelHVModel : BindableBase
    {
        public MultiChannelHVModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            Init();
            InitCommand();
        }
        public MultiChannelHVModel()
        {
           
        }
        private readonly IDialogService _dialogService;
        private TaskCompletionSource<string> _waitingInitReply;
        private System.Timers.Timer _timer;
        private System.Timers.Timer _setHvTimer;
        private ushort _bufferHv;
        private ushort _endBufferHv;
        private bool _direction = true;
        private MultiChannelHVParser _parser;
        private DataContainer dataContainer;
        public MultiChannelHVEntity MultiChannelHVEntity;
        
        private IAsynRWService _serialPortService;
        /// <summary>
        /// 串口异步通信服务
        /// </summary>
        [JsonIgnore]
        public IAsynRWService SerialPortService
        {
            get { return _serialPortService; }
            set { _serialPortService = value; RaisePropertyChanged(); }
        }

        private IAsynRWService _netUdpService;

        /// <summary>
        /// 网口异步通信服务
        /// </summary>
        [JsonIgnore]
        public IAsynRWService NetUdpService
        {
            get { return _netUdpService; }
            set { _netUdpService = value; RaisePropertyChanged(); }
        }
        private ObservableCollection<HVMessageModel> _hVMessageModels;
        public ObservableCollection<HVMessageModel> HVMessageModels
        {
            get { return _hVMessageModels; }
            set { _hVMessageModels = value; RaisePropertyChanged(); }
        }
        private bool _isSetting = false;
        [JsonIgnore]
        public bool IsSetting
        {
            get { return _isSetting; }
            set { _isSetting = value; RaisePropertyChanged(); }
        }
        private float _readHV = 0;
        [JsonIgnore]
        public float ReadHV
        {
            get { return _readHV; }
            set { _readHV = value; RaisePropertyChanged(); }
        }
        private float _readI = 0;
        [JsonIgnore]
        public float ReadI
        {
            get { return _readI; }
            set { _readI = value; RaisePropertyChanged(); }
        }
        private ushort _writeHV = 0;

        public ushort WriteHV
        {
            get { return _writeHV; }
            set 
            {
                if (value > MaxHV)
                {
                    _writeHV = MaxHV;
                }
                else
                {
                    _writeHV = value;
                } 
                
                RaisePropertyChanged();
            }
        }
        private ushort _maxHV = 30000;
        public ushort MaxHV
        {
            get { return _maxHV; }
            set { _maxHV = value; RaisePropertyChanged() ; }
        }
        private ushort _hvStep = 500;
        /// <summary>
        /// 步进值
        /// </summary>
        public ushort HvStep
        {
            get { return _hvStep; }
            set 
                { _hvStep = value; 
                RaisePropertyChanged() ; }
        }
        private MultiChannelHVInitState _hvInitState = MultiChannelHVInitState.IDLE;
        /// <summary>
        /// 初始化状态
        /// </summary>
          [JsonIgnore]
        public MultiChannelHVInitState HVInitState

        {
            get { return _hvInitState; }
            set { _hvInitState = value; RaisePropertyChanged(); }
        }
        private string _prompt;
        /// <summary>
        /// 界面提示
        /// </summary>
        [JsonIgnore]
        public string Prompt
        {
            get { return _prompt; }
            set { _prompt = value; RaisePropertyChanged(); }
        }
      
        private string _firmwareVersion = "";
        /// <summary>
        /// 固件版本
        /// </summary>
        [JsonIgnore]
        public string FirmwareVersion
        {
            get { return _firmwareVersion; }
            set { _firmwareVersion = value; RaisePropertyChanged(); }
        }
        [JsonIgnore]
        public DelegateCommand SetHVCommand { get; set; }
        [JsonIgnore]
        public DelegateCommand GetHVCommand { get; set; }
        [JsonIgnore]
        public DelegateCommand SetInitCommand { get; set; }
        [JsonIgnore]
        public DelegateCommand GetInitStateCommand { get; set; }
        [JsonIgnore]
        public DelegateCommand IErrorClearCommand { get; set; }
        [JsonIgnore]
        public DelegateCommand DisableOutputCommand { get; set; }
        [JsonIgnore]
        public DelegateCommand InitMultiChannelHVCommand { get; set; }
        [JsonIgnore]
        public DelegateCommand SettingCompleteCommand { get; set; }
        [JsonIgnore]
        public DelegateCommand ShowPasswordDialogCommand { get; set; }
        [JsonIgnore]
        public DelegateCommand SaveDaraCommand { get; set; }

        private void InitCommand()
        {
            InitMultiChannelHVCommand = new DelegateCommand(InitMultiChannelHV);
            SetHVCommand = new DelegateCommand(SetHv);
            GetHVCommand = new DelegateCommand(() => MultiChannelHVEntity?.GetHVCommand());
            SetInitCommand = new DelegateCommand(() => MultiChannelHVEntity?.SetInitCommand());
            GetInitStateCommand = new DelegateCommand(() => MultiChannelHVEntity?.GetInitStateCommand());
            IErrorClearCommand = new DelegateCommand(() => MultiChannelHVEntity?.IErrorClearCommand());
            DisableOutputCommand = new DelegateCommand(() => MultiChannelHVEntity?.DisableOutputCommand());
            ShowPasswordDialogCommand = new DelegateCommand(ShowPasswordDialog);
            SettingCompleteCommand = new DelegateCommand(()=> IsSetting = false);
            SaveDaraCommand = new DelegateCommand(() => dataContainer.SaveData());
        }
        private void Init()
        {
            _parser = new MultiChannelHVParser();
            SerialPortService = new SerialPortService();
            SerialPortService.UpdateResponse += SerialPortService_UpdateResponse;
            SerialPortService.IsBinary = true;
            var netUdp = new UdpNetAsyncDevice();
            netUdp.Name = "升级网口";
            netUdp.DeviceInstance.TargetIp = "192.168.1.88";
            netUdp.DeviceInstance.TargetPort = 5003;
            netUdp.DeviceInstance.HostIp = "192.168.1.33";
            netUdp.DeviceInstance.HostPort = 6585;
            netUdp.IsBinary = true;
            NetUdpService = netUdp;
            NetUdpService.UpdateResponse += NetUdpService_UpdateResponse;
            _parser.PacketReceivedEvent += Parser_PacketReceivedEvent;

            MultiChannelHVEntity = new MultiChannelHVEntity(SerialPortService, NetUdpService);
            HVMessageModels = new ObservableCollection<HVMessageModel>();
            for (byte i = 1; i <= 13; i++)
            {
                HVMessageModel hVMessageModel = new HVMessageModel(MultiChannelHVEntity, i);
                HVMessageModels.Add(hVMessageModel);
            }
            dataContainer = new DataContainer(this);
            dataContainer.LoadData();
        }
        private void NetUdpService_UpdateResponse(object? sender, byte[] e)
        {
            _parser.ReceiveBytes(e);
        }

        private void SerialPortService_UpdateResponse(object? sender, byte[] e)
        {
            _parser.ReceiveBytes(e);
        }
        // 定时触发的方法
        public void InitTimer()
        {
            // 1. 创建定时器，设置间隔时间（单位：毫秒，此处为 1000ms = 1秒）
            if (_timer == null)
            {
                _timer = new System.Timers.Timer(1000);
            }
            // 2. 绑定定时触发的事件
            _timer.Elapsed += OnTimerElapsed;

            // 3. 设置是否重复触发（true = 循环触发，false = 只触发一次）
            _timer.AutoReset = true;

            // 4. 启动定时器
            _timer.Enabled = true;
        }
        public void StopTimer()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
        }
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            MultiChannelHVEntity.GetHVCommand();
            MultiChannelHVEntity.Get1To6IVCommand();
            MultiChannelHVEntity.Get7To13IVCommand();
            MultiChannelHVEntity.GetInitStateCommand();
        }
       
     
        
        /// <summary>
        /// 自动初始化
        /// </summary>
        private async void InitMultiChannelHV()
        {
            MultiChannelHVEntity.DisableOutputCommand();
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    _waitingInitReply = new TaskCompletionSource<string>();
                    MultiChannelHVEntity?.SetInitCommand();
                    var result = await _waitingInitReply.Task.WaitAsync(TimeSpan.FromMilliseconds(5000));
                    if (result == "InitSuccessful")
                    {
                        MultiChannelHVEntity?.IErrorClearCommand();
                        return;
                    }
                    else
                    {
                        
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Prompt =  $"第{i+1}次自动初始化错误";
                    continue;
                }
            } 
        }
        private void SetHv()
        {

            _endBufferHv = WriteHV;
            if (_endBufferHv > ReadHV)
            {
                _bufferHv = (ushort)ReadHV;
                _direction = true;
            }
            else
            {
                _bufferHv = (ushort)ReadHV;
                _direction = false;
            }
            StopSetHvTimer();
            InitSetHVTimer();

        }

        public void InitSetHVTimer()
        {
            // 1. 创建定时器，设置间隔时间（单位：毫秒，此处为 1000ms = 1秒）
            if (_setHvTimer == null)
            {
                _setHvTimer = new System.Timers.Timer(1000);
            }
            if (_setHvTimer.Enabled)
            {
                _setHvTimer.Stop();
            }
            // 2. 绑定定时触发的事件
            _setHvTimer.Elapsed += SetHvTimerElapsed;

            // 3. 设置是否重复触发（true = 循环触发，false = 只触发一次）
            _setHvTimer.AutoReset = true;

            // 4. 启动定时器
            _setHvTimer.Enabled = true;
        }
        public void StopSetHvTimer()
        {
            _setHvTimer?.Stop();
            _setHvTimer?.Dispose();
            _setHvTimer = null;
        }
        private void SetHvTimerElapsed(object sender, ElapsedEventArgs e)
        {

            if (_direction)
            {
                
                var differ = _endBufferHv - _bufferHv;
                
                if (_endBufferHv < _bufferHv || _endBufferHv<_bufferHv + HvStep)
                {

                    MultiChannelHVEntity?.SetHVCommand((ushort)(_endBufferHv));
                    StopSetHvTimer();
                }
                else
                {
                    _bufferHv = (ushort)(_bufferHv + HvStep);
                    if (_bufferHv > _endBufferHv)
                    {
                        MultiChannelHVEntity?.SetHVCommand((ushort)(_endBufferHv));
                        StopSetHvTimer();
                        return;
                    }
                    MultiChannelHVEntity?.SetHVCommand(_bufferHv);
                }
            }
            else
            {
                var differ = _endBufferHv - _bufferHv;
                if (_endBufferHv > _bufferHv || _endBufferHv > _bufferHv - HvStep || _bufferHv - HvStep > MaxHV|| _bufferHv> MaxHV)
                {

                    MultiChannelHVEntity?.SetHVCommand((ushort)(_endBufferHv));
                    StopSetHvTimer();
                }
                else
                {
                    _bufferHv = (ushort)(_bufferHv - HvStep);
                    if (_bufferHv > MaxHV)
                    {
                        MultiChannelHVEntity?.SetHVCommand(_endBufferHv);
                        StopSetHvTimer();
                        return;
                    }
                    MultiChannelHVEntity?.SetHVCommand(_bufferHv);

                }
            }
        }
        private void Parser_PacketReceivedEvent(object? sender, MultiChannelHVPacket e)
        {
            var data = e.DataSource;
            switch (e.CmdType)
            {
                //获取单路隔离电压
                case MultiChannelHVFunctionCode.GET_IV:
                    GET_IVReturnParsing(data); break;
                //获取1-6路隔离电压
                case MultiChannelHVFunctionCode.GET_P1_6_IV:
                    GET_P1_6_IVReturnParsing(data); break;
                //获取7-13路隔离电压
                case MultiChannelHVFunctionCode.GET_P7_13_IV:
                    GET_P7_13_IVReturnParsing(data); break;
                //设置单路隔离电压
                case MultiChannelHVFunctionCode.SET_IV:
                    SET_IVReturnParsing(data); break;
                //获取整机悬浮高压
                case MultiChannelHVFunctionCode.GET_HV:
                    GET_HVReturnParsing(data); break;
                //设置整机悬浮高压
                case MultiChannelHVFunctionCode.SET_HV:
                    SET_HVParsing(data); break;
                //设置高压控制板初始化
                case MultiChannelHVFunctionCode.SET_INIT:
                    SET_INITReturnParsing(data); break;
                //高压初始化状态查询
                case MultiChannelHVFunctionCode.GET_INIT_STATE:
                    GET_INIT_STATEReturnParsing(data); break;
                //隔离板错误清除
                case MultiChannelHVFunctionCode.ERROR_CLEAR:
                    ERROR_CLEARReturnParsing(data); break;
                //高压箱初始化取消，关闭输出
                case MultiChannelHVFunctionCode.DISABLE_OUTPUT:
                    DISABLE_OUTPUTReturnParsing(data); break;
                case MultiChannelHVFunctionCode.FV: GetFv(data); break;
            }

        }
        /// <summary>
        /// 获取单路隔离电压回报解析
        /// </summary>
        private void GET_IVReturnParsing(byte[] data)
        {
            var channel = data[0];
            var hv = BitConverter.ToSingle(data, 1);
            for (byte i = 0; i < HVMessageModels.Count; i++)
            {
                if (HVMessageModels[i].Channel == channel)
                {
                    HVMessageModels[i].ReadHV = hv;
                    return;
                }
            }
        }
        /// <summary>
        /// 获取1-6路隔离电压回报解析
        /// </summary>
        private void GET_P1_6_IVReturnParsing(byte[] data)
        {
            float[] ivs = new float[6];
            for (int i = 0; i < 6; i++)
            {
                var hv = BitConverter.ToSingle(data, i * 4);
                ivs[i] = hv;
            }
            for (int j = 0; j < 6; j++)
            {
                HVMessageModels[j].ReadHV = ivs[j];
            }
        }
        /// <summary>
        /// 获取7-13路隔离电压回报解析
        /// </summary>
        private void GET_P7_13_IVReturnParsing(byte[] data)
        {
            float[] ivs = new float[7];
            for (int i = 0; i < 7; i++)
            {
                var hv = BitConverter.ToSingle(data, i * 4);
                ivs[i] = hv;
            }
            for (int j = 0; j < 7; j++)
            {
                HVMessageModels[6 + j].ReadHV = ivs[j];
            }
        }
        /// <summary>
        /// 设置单路隔离电压回报解析
        /// </summary>
        private void SET_IVReturnParsing(byte[] data)
        {
            Prompt = " 下位机收到设置单路隔离电压命令";
        }
        /// <summary>
        /// 获取整机悬浮高压回报解析
        /// </summary>
        private void GET_HVReturnParsing(byte[] data)
        {
            var hv = BitConverter.ToSingle(data, 0);
            var i = BitConverter.ToSingle(data, 4);
            ReadHV = hv;
            ReadI = i;
        }
        /// <summary>
        /// 设置整机悬浮高压回报解析
        /// </summary>
        private void SET_HVParsing(byte[] data)
        {
            Prompt = " 下位机收到设置整机悬浮电压命令";
        }
        /// <summary>
        /// 设置高压控制板初始化回报解析
        /// </summary>
        private void SET_INITReturnParsing(byte[] data)
        {
            var initReturnResult = data[0];
            if (initReturnResult == 0xff)
            {
                //执行初始化
                Prompt = " 下位机收到设置高压控制板初始化命令，并执行初始化";
            }
            else
            {
                Prompt = " 下位机收到设置高压控制板初始化命令，初始化发生错误";
                _waitingInitReply?.TrySetResult("Execution Error");
            }

        }
        /// <summary>
        /// 高压初始化状态查询回报解析
        /// </summary>
        private void GET_INIT_STATEReturnParsing(byte[] data)
        {

            MultiChannelHVInitState initResult = (MultiChannelHVInitState)data[0];

            if (initResult == MultiChannelHVInitState.RUNNUNG)
            {
                _waitingInitReply?.TrySetResult("InitSuccessful");
            }
            else if (initResult == MultiChannelHVInitState.INITALIZING)
            {
                //发送初始化中
            }
            else
            {
                _waitingInitReply?.TrySetResult("InitError");
                
            }

            HVInitState = initResult;

        }
        /// <summary>
        /// 隔离板错误清除回报解析
        /// </summary>
        private void ERROR_CLEARReturnParsing(byte[] data)
        {
            var executionResult = data[0];
            if (executionResult == 0xff)
            {
                // 隔离板错误清除成功
                Prompt = " 下位机隔离板错误清除成功";
            }
            else
            {
                // 隔离板错误清除失败
                Prompt = " 下位机隔离板错误清除失败";
            }
        }
        /// <summary>
        /// 高压箱初始化取消，关闭输出回报解析
        /// </summary>
        private void DISABLE_OUTPUTReturnParsing(byte[] data)
        {
            var executionResult = data[0];
            if (executionResult == 0xff)
            {
                // 关闭输出成功
                HVInitState = MultiChannelHVInitState.IDLE;
                Prompt = " 下位机关闭输出成功";
            }
            else
            {
                Prompt = " 下位机关闭输出成功";
                // 关闭输出失败
            }
        }
        private void GetFv(byte[] data)
        {
            FirmwareVersion = Encoding.ASCII.GetString(data);
            var x = "a";
            
        }
        
        private void ShowPasswordDialog()
        {
            // 1. 准备传入对话框的参数
            var parameters = new DialogParameters();
            parameters.Add("Title", "请输入管理员密码"); // 传递标题参数

            // 2. 调用对话框（模态），并处理返回结果
            _dialogService.ShowDialog(
                "PasswordDialog", // 对话框注册的键名
                parameters,       // 传入的参数
                result =>         // 回调：处理返回结果
                {
                    if (result.Result == ButtonResult.OK)
                    {
                        // 从返回结果中获取密码
                        var enteredPassword = result.Parameters.GetValue<string>("EnteredPassword");
                        // 执行验证逻辑（例如和预设密码比较）
                        if (string.Equals(enteredPassword, "zeptools", StringComparison.Ordinal))
                        {
                            // 验证成功
                            IsSetting = true;
                            dataContainer.SaveData();
                        }
                    }
                    else if (result.Result == ButtonResult.Cancel)
                    {

                        // 用户取消
                    }
                }
            );
        }
    }
}
