#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.ControlLibTest.ViewModels
 * 唯一标识：b4333184-7773-424a-9e4c-65021e6b4822
 * 文件名：ControlLibTestViewModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/2/26 14:26:57
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
using Devices.Interface;
using Devices.Method;
using OpenCvSharp;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Xml.Linq;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Extension;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.ControlLibTest.Model;
using ZemModel.Entity;
using Zeptools.CoreControl;

namespace UtilityTools.Modules.ControlLibTest.ViewModels
{
    internal class ControlLibTestViewModel : RegionViewModelBase
    {
        #region ------------Constructor------------
        public ControlLibTestViewModel(IDialogHostService dialogHostService, IContainerProvider containerProvider)
            : base(containerProvider)
        {
            _dialogHostService = dialogHostService;
            //消息提示
            _aggregator = containerProvider.Resolve<IEventAggregator>();

            _device = DeviceFactory.GetAsyRWDevice("Protocol");
            if (_device != null)
            {
                _device.Name = "协议主控板";
                _device.Open();
                var entity = new ControlEntity(_device);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                entity.CreateBroadcastSocket(endPoint);
                entity.CommandResponseEvent += Entity_CommandResponseEvent;
                _bseControl = entity;
                _ccsControl = entity;
                _dacControl = entity;
                _ethControl = entity;
                _fanControl = entity;
                //_ledControl = new ControlEntity(_device);
                _relayControl = entity;
                _scanControl = entity;
                _seControl = entity;
                _tempControl = entity;
                _uartControl = entity;
                //_vacControl = new ControlEntity(_device);
            }

            InitProperty();
        }

        #endregion

        #region ------------Field------------
        private IDialogHostService? _dialogHostService = null;
        private IEventAggregator? _aggregator = null;
        private IAsynRWDevice? _device = null;
        private IBseControl? _bseControl = null;
        private ICCSControl? _ccsControl = null;
        private IDacControl? _dacControl = null;
        private IEthControl? _ethControl = null;
        private IFanControl? _fanControl = null;
        //private ControlEntity? _ledControl = null;
        private IRelayControl? _relayControl = null;
        private IScanControl? _scanControl = null;
        private ISeControl? _seControl = null;
        private ITempControl? _tempControl = null;
        private IUartControl? _uartControl = null;
        //private ControlEntity? _vacControl = null;
        #endregion

        #region ------------Property------------
        private ObservableCollection<CCSModel> _ccsModels = new ObservableCollection<CCSModel>();

        public ObservableCollection<CCSModel> CCSModels
        {
            get { return _ccsModels; }
            set { _ccsModels = value; RaisePropertyChanged(); }
        }

        private CCSModel? _selectedCCSModelForValue = null;

        public CCSModel? SelectedCCSModelForValue
        {
            get { return _selectedCCSModelForValue; }
            set { _selectedCCSModelForValue = value; RaisePropertyChanged(); }
        }

        private CCSModel? _selectedCCSModelForRelay = null;

        public CCSModel? SelectedCCSModelForRelay
        {
            get { return _selectedCCSModelForRelay; }
            set { _selectedCCSModelForRelay = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<FanModel> _fanModels = new ObservableCollection<FanModel>();

        public ObservableCollection<FanModel> FanModels
        {
            get { return _fanModels; }
            set { _fanModels = value; RaisePropertyChanged(); }
        }

        private FanModel? _selectedFanModel = null;

        public FanModel? SelectedFanModel
        {
            get { return _selectedFanModel; }
            set { _selectedFanModel = value; RaisePropertyChanged(); }
        }

        private EthModel? _ethModel = null;

        public EthModel? EthModel
        {
            get { return _ethModel; }
            set { _ethModel = value; RaisePropertyChanged(); }
        }

        public DelegateCommand? SetIPCommand { get; set; } = null;

        public DelegateCommand? GetIPCommand { get; set; } = null;

        private TempModel? _tempModel = null;

        public TempModel? TempModel
        {
            get { return _tempModel; }
            set { _tempModel = value; RaisePropertyChanged(); }
        }

        public DelegateCommand? GetAllTempCommand { get; set; } = null;

        public DelegateCommand? GetTemp1Command { get; set; } = null;

        public DelegateCommand? GetTemp2Command { get; set; } = null;

        private VacModel? _vacModel = null;

        public VacModel? VacModel
        {
            get { return _vacModel; }
            set { _vacModel = value; RaisePropertyChanged(); }
        }

        public DelegateCommand<string>? GetVacCommand { get; set; } = null;

        private ObservableCollection<BseModel> _bseModels = new ObservableCollection<BseModel>();

        public ObservableCollection<BseModel> BseModels
        {
            get { return _bseModels; }
            set { _bseModels = value; RaisePropertyChanged(); }
        }

        private BseModel? _bseModelForValue = null;

        public BseModel? BseModelForValue
        {
            get { return _bseModelForValue; }
            set { _bseModelForValue = value; RaisePropertyChanged(); }
        }

        private BseModel? _bseModelForNeg = null;

        public BseModel? BseModelForNeg
        {
            get { return _bseModelForNeg; }
            set { _bseModelForNeg = value; RaisePropertyChanged(); }
        }

        private BseModel? _bseModelForPos = null;

        public BseModel? BseModelForPos
        {
            get { return _bseModelForPos; }
            set { _bseModelForPos = value; RaisePropertyChanged(); }
        }

        private SeModel? _seModel = null;

        public SeModel? SeModel
        {
            get { return _seModel; }
            set { _seModel = value; RaisePropertyChanged(); }
        }

        private ScanModel? _scanModel = null;

        public ScanModel? ScanModel
        {
            get { return _scanModel; }
            set { _scanModel = value; RaisePropertyChanged(); }
        }

        private LedModel? _ledModels = null;

        public LedModel? LedModels
        {
            get { return _ledModels; }
            set { _ledModels = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<RelayModel> _relayModels = new ObservableCollection<RelayModel>();

        public ObservableCollection<RelayModel> RelayModels
        {
            get { return _relayModels; }
            set { _relayModels = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<UARTModel> _UartModels = new ObservableCollection<UARTModel>();

        public ObservableCollection<UARTModel> UartModels
        {
            get { return _UartModels; }
            set { _UartModels = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<DACModel> _dacModels = new ObservableCollection<DACModel>();

        public ObservableCollection<DACModel> DacModels
        {
            get { return _dacModels; }
            set { _dacModels = value; RaisePropertyChanged(); }
        }

        private DACModel? _selectedDacModel = null;

        public DACModel? SelectedDacModel
        {
            get { return _selectedDacModel; }
            set { _selectedDacModel = value; RaisePropertyChanged(); }
        }

        public DelegateCommand GetRelayStatusCommand { get; set; }

        private string _relayStatus;

        public string RelayStatus
        {
            get { return _relayStatus; }
            set { _relayStatus = value; RaisePropertyChanged(); }
        }

        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------

        private void InitProperty()
        {
            if (_ccsControl != null)
            {
                CCSModels.Add(new CCSModel() { Name = "CHA0", RealName = "AligX1", RelayName = "CCSk2", RelayChannel = (byte)0x02, Channel = (byte)0xA0, HasRelay = true, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false, SetValueFunc = _ccsControl.SetAligX1, SetStateFunc = _ccsControl.SetAligX1Polarity });
                CCSModels.Add(new CCSModel() { Name = "CHA1", RealName = "AligX2", RelayName = "CCSk3", RelayChannel = (byte)0x03, Channel = (byte)0xA1, HasRelay = true, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false, SetValueFunc = _ccsControl.SetAligX2, SetStateFunc = _ccsControl.SetAligX2Polarity });
                CCSModels.Add(new CCSModel() { Name = "CHA2", RealName = "CompressA", RelayName = "CCSkX", RelayChannel = (byte)0x0F, Channel = (byte)0xA2, HasRelay = false, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false, SetValueFunc = _ccsControl.SetCompressLensA, SetStateFunc = null });
                CCSModels.Add(new CCSModel() { Name = "CHB0", RealName = "CompressB", RelayName = "CCSkX", RelayChannel = (byte)0x0F, Channel = (byte)0xB0, HasRelay = false, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false, SetValueFunc = _ccsControl.SetCompressLensB, SetStateFunc = null });
                CCSModels.Add(new CCSModel() { Name = "CHB1", RealName = "AstigA", RelayName = "CCSk6", RelayChannel = (byte)0x06, Channel = (byte)0xB1, HasRelay = true, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false, SetValueFunc = _ccsControl.SetAstigA, SetStateFunc = _ccsControl.SetAstigAPolarity });
                CCSModels.Add(new CCSModel() { Name = "CHB2", RealName = "AstigB", RelayName = "CCSk7", RelayChannel = (byte)0x07, Channel = (byte)0xB2, HasRelay = true, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false, SetValueFunc = _ccsControl.SetAstigB, SetStateFunc = _ccsControl.SetAstigBPolarity });
                CCSModels.Add(new CCSModel() { Name = "CHB3", RealName = "AstigC", RelayName = "CCSk5", RelayChannel = (byte)0x05, Channel = (byte)0xB3, HasRelay = true, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false, SetValueFunc = _ccsControl.SetAstigC, SetStateFunc = _ccsControl.SetAstigCPolarity });
                CCSModels.Add(new CCSModel() { Name = "CHB4", RealName = "AstigD", RelayName = "CCSk4", RelayChannel = (byte)0x04, Channel = (byte)0xB4, HasRelay = true, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false, SetValueFunc = _ccsControl.SetAstigD, SetStateFunc = _ccsControl.SetAstigDPolarity });
                CCSModels.Add(new CCSModel() { Name = "CHB5", RealName = "AligY1", RelayName = "CCSk0", RelayChannel = (byte)0x00, Channel = (byte)0xB5, HasRelay = true, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false, SetValueFunc = _ccsControl.SetAligY1, SetStateFunc = _ccsControl.SetAligY1Polarity });
                CCSModels.Add(new CCSModel() { Name = "CHB7", RealName = "OB", RelayName = "CCSkX", RelayChannel = (byte)0x0F, Channel = (byte)0xB7, HasRelay = false, Value = 0, MinValue = 0, MaxValue = 0xFFFF, RelayState = false, SetValueFunc = _ccsControl.SetObjectiveLens, SetStateFunc = null });
                CCSModels.Add(new CCSModel() { Name = "CHB8", RealName = "AligY2", RelayName = "CCSk1", RelayChannel = (byte)0x01, Channel = (byte)0xB8, HasRelay = true, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false, SetValueFunc = _ccsControl.SetAligY2, SetStateFunc = _ccsControl.SetAligY2Polarity });
            }

            SelectedCCSModelForValue = CCSModels[0];
            SelectedCCSModelForRelay = CCSModels[0];

            if (_fanControl != null)
            {
                FanModels.Add(new FanModel() { Name = "风扇1", Channel = (byte)0x01, MinSpeed = 0, MaxSpeed = 100, Speed = 0, SetValueFunc = _fanControl.SetFan1 });
                FanModels.Add(new FanModel() { Name = "风扇2", Channel = (byte)0x02, MinSpeed = 0, MaxSpeed = 100, Speed = 0, SetValueFunc = _fanControl.SetFan2 });
                FanModels.Add(new FanModel() { Name = "风扇3", Channel = (byte)0x03, MinSpeed = 0, MaxSpeed = 100, Speed = 0, SetValueFunc = _fanControl.SetFan3 });
            }

            SelectedFanModel = FanModels[0];

            EthModel = new EthModel();
            SetIPCommand = new DelegateCommand(SetIP);
            GetIPCommand = new DelegateCommand(GetIP);

            TempModel = new TempModel();
            GetAllTempCommand = new DelegateCommand(GetAllTemp);
            GetTemp1Command = new DelegateCommand(GetTemp1);
            GetTemp2Command = new DelegateCommand(GetTemp2);
            //SetUartCommand = new DelegateCommand<string>(SetUart);

            VacModel = new VacModel();
            //GetVacCommand = new DelegateCommand<string>(GetVac);

            if (_bseControl != null)
            {
                BseModels.Add(new BseModel() { Name = "CH1", Channel = (byte)0x01, Value = 0, MinValue = 0, MaxValue = 0x0FFF, Negative = false, Positive = false, SetValueFunc = _bseControl.SetBSECH1, SetNegStateFunc = _bseControl.SetBSECH1_R, SetPosStateFunc = _bseControl.SetBSECH1_P });
                BseModels.Add(new BseModel() { Name = "CH2", Channel = (byte)0x02, Value = 0, MinValue = 0, MaxValue = 0x0FFF, Negative = false, Positive = false, SetValueFunc = _bseControl.SetBSECH2, SetNegStateFunc = _bseControl.SetBSECH2_R, SetPosStateFunc = _bseControl.SetBSECH2_P });
                BseModels.Add(new BseModel() { Name = "CH3", Channel = (byte)0x03, Value = 0, MinValue = 0, MaxValue = 0x0FFF, Negative = false, Positive = false, SetValueFunc = _bseControl.SetBSECH3, SetNegStateFunc = _bseControl.SetBSECH3_R, SetPosStateFunc = _bseControl.SetBSECH3_P });
                BseModels.Add(new BseModel() { Name = "CH4", Channel = (byte)0x04, Value = 0, MinValue = 0, MaxValue = 0x0FFF, Negative = false, Positive = false, SetValueFunc = _bseControl.SetBSECH4, SetNegStateFunc = _bseControl.SetBSECH4_R, SetPosStateFunc = _bseControl.SetBSECH4_P });
            }

            BseModelForValue = BseModels[0];
            BseModelForNeg = BseModels[0];
            BseModelForPos = BseModels[0];

            SeModel = new SeModel();

            ScanModel = new ScanModel();

            RelayModels.Add(new RelayModel() { Name = "CH0", Channel = (byte)0x00, Enable = false });
            RelayModels.Add(new RelayModel() { Name = "CH1", Channel = (byte)0x01, Enable = false });
            RelayModels.Add(new RelayModel() { Name = "CH2", Channel = (byte)0x02, Enable = false });
            RelayModels.Add(new RelayModel() { Name = "CH3", Channel = (byte)0x03, Enable = false });
            RelayModels.Add(new RelayModel() { Name = "CH4", Channel = (byte)0x04, Enable = false });
            RelayModels.Add(new RelayModel() { Name = "CH5", Channel = (byte)0x05, Enable = false });
            RelayModels.Add(new RelayModel() { Name = "CH6", Channel = (byte)0x06, Enable = false });
            RelayModels.Add(new RelayModel() { Name = "CH7", Channel = (byte)0x07, Enable = false });
            RelayModels.Add(new RelayModel() { Name = "CH8", Channel = (byte)0x08, Enable = false });
            RelayModels.Add(new RelayModel() { Name = "CH9", Channel = (byte)0x09, Enable = false });
            RelayModels.Add(new RelayModel() { Name = "CHA", Channel = (byte)0x0A, Enable = false });
            RelayModels.Add(new RelayModel() { Name = "CHB", Channel = (byte)0x0B, Enable = false });
            RelayModels.Add(new RelayModel() { Name = "CHC", Channel = (byte)0x0C, Enable = false });
            RelayModels.Add(new RelayModel() { Name = "CHD", Channel = (byte)0x0D, Enable = false });
            RelayModels.Add(new RelayModel() { Name = "CHE", Channel = (byte)0x0E, Enable = false });
            RelayModels.Add(new RelayModel() { Name = "CHF", Channel = (byte)0x0F, Enable = false });

            GetRelayStatusCommand = new DelegateCommand(GetRelayStatus);

            if (_dacControl != null)
            {
                DacModels.Add(new DACModel() { Name = "CHA", Channel = (byte)0x00, Value = 0, MinValue = 0, MaxValue = 0x0FFF, SetValueFunc = _dacControl.SetDACCHA });
                DacModels.Add(new DACModel() { Name = "CHB", Channel = (byte)0x01, Value = 0, MinValue = 0, MaxValue = 0x0FFF, SetValueFunc = _dacControl.SetDACCHB });
                DacModels.Add(new DACModel() { Name = "CHC", Channel = (byte)0x02, Value = 0, MinValue = 0, MaxValue = 0x0FFF, SetValueFunc = _dacControl.SetDACCHC });
                DacModels.Add(new DACModel() { Name = "CHD", Channel = (byte)0x03, Value = 0, MinValue = 0, MaxValue = 0x0FFF, SetValueFunc = _dacControl.SetDACCHD });
            }

            SelectedDacModel = DacModels[0];

            UartModels.Add(new UARTModel() { Name = "光耦串口", Series = (byte)0x00, Value = "115200", SetBaudRateFunc = this.SetUart });
            UartModels.Add(new UARTModel() { Name = "RS485_1", Series = (byte)0x01, Value = "115200", SetBaudRateFunc = this.SetUart });
            UartModels.Add(new UARTModel() { Name = "RS485_2", Series = (byte)0x02, Value = "115200", SetBaudRateFunc = this.SetUart });
            UartModels.Add(new UARTModel() { Name = "RS485_3", Series = (byte)0x03, Value = "115200", SetBaudRateFunc = this.SetUart });


            LedModels = new LedModel();

            BindPropertyChanged();

        }

        private void GetRelayStatus()
        {
            if (_relayControl == null)
                return;
            _relayControl.GetRelayState();
        }

        private void BindPropertyChanged()
        {
            foreach (var model in CCSModels)
            {
                model.PropertyChanged += CCSModel_PropertyChanged;
            }

            foreach (var model in FanModels)
            {
                model.PropertyChanged += FanModel_PropertyChanged;
            }

            foreach (var model in BseModels)
            {
                model.PropertyChanged += BseModel_PropertyChanged;
            }

            SeModel.PropertyChanged += SeModel_PropertyChanged;

            ScanModel.PropertyChanged += ScanModel_PropertyChanged;

            foreach (var model in RelayModels)
            {
                model.PropertyChanged += RelayModel_PropertyChanged;
            }

            foreach (var model in DacModels)
            {
                model.PropertyChanged += DacModel_PropertyChanged;
            }
            //foreach (var model in UartModels)
            //{
            //    model.PropertyChanged += UartModel_PropertyChanged;
            //}           
            LedModels.UpdateLightFunc += SetLedModel;


        }
        
        private void ScanModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_scanControl == null)
                return;
            var model = sender as ScanModel;
            if (model != null)
            {
                switch (e.PropertyName)
                {
                    case "AmpX":
                        {
                            _scanControl.SetMainAmpX(model.AmpX);
                        }
                        break;
                    case "AmpY":
                        {
                            _scanControl.SetMainAmpY(model.AmpY);
                        }
                        break;
                    case "AmpTX":
                        {
                            _scanControl.SetMainTAmpX(model.AmpTX);
                        }
                        break;
                    case "AmpTY":
                        {
                            _scanControl.SetMainTAmpY(model.AmpTY);
                        }
                        break;
                    case "AnalogSpinA":
                        {
                            _scanControl.SetMainAnalogSpinCHA(model.AnalogSpinA);
                        }
                        break;
                    case "AnalogSpinB":
                        {
                            _scanControl.SetMainAnalogSpinCHB(model.AnalogSpinB);
                        }
                        break;
                    case "AnalogSpinC":
                        {
                            _scanControl.SetMainAnalogSpinCHC(model.AnalogSpinC);
                        }
                        break;
                    case "AnalogSpinD":
                        {
                            _scanControl.SetMainAnalogSpinCHD(model.AnalogSpinD);
                        }
                        break;
                    case "K1":
                        {
                            _scanControl.SetMainSCANK1(model.K1);
                        }
                        break;
                    case "K2":
                        {
                            _scanControl.SetMainSCANK2(model.K2);
                        }
                        break;
                    case "K3":
                        {
                            _scanControl.SetMainSCANK3(model.K3);
                        }
                        break;
                    case "K4":
                        {
                            _scanControl.SetMainSCANK4(model.K4);
                        }
                        break;
                    case "Sw":
                        {
                            _scanControl.SetMainSCANSW(model.Sw);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void SeModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_seControl == null)
                return;
            var model = sender as SeModel;
            if (model != null)
            {
                switch (e.PropertyName)
                {
                    case "Value":
                        {
                            _seControl.SetSECH0(model.Value);
                        }
                        break;
                    case "Enable":
                        {
                            _seControl.SetSEEnable(model.Enable);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void CCSModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(_ccsControl == null) return;
            var model = sender as CCSModel;
            if (model != null)
            {
                switch (e.PropertyName)
                {
                    case "Value":
                        if (model == SelectedCCSModelForValue)
                        {
                             _ccsControl.SetCCSCHValue(model.Channel, model.Value);
                        }
                        else
                        {
                            model.SetValueFunc?.Invoke(model.Value);
                        }
                        break;
                    case "RelayState":
                        if (model == SelectedCCSModelForRelay)
                        {
                            _ccsControl.SetCCSk(model.RelayChannel, model.RelayState);
                        }
                        else
                        {
                            model.SetStateFunc?.Invoke(model.RelayState);
                        }
                        break;
                }
            }
        }

        private void FanModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(_fanControl == null) return;
            var model = sender as FanModel;
            if (model != null)
            {
                switch (e.PropertyName)
                {
                    case "Speed":
                        if (model == SelectedFanModel)
                        {
                            _fanControl.SetFan(model.Channel, model.Speed);
                        }
                        else
                        {
                            model.SetValueFunc?.Invoke(model.Speed);
                        }
                        break;
                }
            }
        }

        private void BseModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(_bseControl == null) return;
            var model = sender as BseModel;
            if (model != null)
            {
                switch (e.PropertyName)
                {
                    case "Value":
                        if (model == BseModelForValue)
                        {
                            _bseControl.SetBSECH(model.Channel, model.Value);
                        }
                        else
                        {
                            model.SetValueFunc?.Invoke(model.Value);
                        }
                        break;
                    case "Positive":
                        if (model == BseModelForPos)
                        {
                            _bseControl.SetBSECH_P(model.Channel, model.Positive);
                        }
                        else
                        {
                            model.SetPosStateFunc?.Invoke(model.Positive);
                        }
                        break;
                    case "Negative":
                        if (model == BseModelForNeg)
                        {
                            _bseControl.SetBSECH_R(model.Channel, model.Negative);
                        }
                        else
                        {
                            model.SetNegStateFunc?.Invoke(model.Negative);
                        }
                        break;
                }
            }
        }

        private void RelayModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_relayControl == null)
                return;
            var model = sender as RelayModel;
            if (model != null)
            {
                 _relayControl.SetRelayState(model.Channel, model.Enable);
            }
        }
        private void DacModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(_dacControl == null) return;
            var model = sender as DACModel;
            if (model != null)
            {
                switch (e.PropertyName)
                {
                    case "Value":
                        if (model == SelectedDacModel)
                        {
                           _dacControl.SetDAC(model.Channel, model.Value);
                        }
                        else
                        {
                            model.SetValueFunc?.Invoke(model.Value);
                        }
                        break;
                }
            }
        }


        //private void UartModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    var model = sender as UARTModel;
        //    if (model != null)
        //    {
        //        uint Value = Convert.ToUInt32(model.Value.Split(":")[1]);
        //        var result = _entity.SetBaudRate(model.Series, Value);
        //        ShowResponseResult(result);
        //    }
        //}

        private void SetUart(byte Series, uint BaudRate)
        {
            if (_uartControl == null)
                return ;
            _uartControl.SetBaudRate(Series, BaudRate);
        }

         /// <summary>
         /// 设置led暂时没有
         /// </summary>
         /// <param name="ContentLength"></param>
         /// <param name="Content"></param>
        private void SetLedModel(byte ContentLength, byte[] Content)
        {
            //if (_ledControl == null)
                return ;
            //var result = _ledControl.SetLightMsg(ContentLength, Content);
        
        }



        private void SetIP()
        {
            if (_ethControl == null || EthModel == null)
                return;
            string[] ip = EthModel.SetIP.Split(".");
            _ethControl.SetIP(Convert.ToByte(ip[0]), Convert.ToByte(ip[1]), Convert.ToByte(ip[2]), Convert.ToByte(ip[3]));
        }

        private void GetIP()
        {
            if (_ethControl == null || EthModel == null)
                return;
            _ethControl.GetIP();
        }
        private void GetAllTemp()
        {
            if (_tempControl == null || TempModel == null)
                return;
            _tempControl.GetTemp();
        }

        private void GetTemp1()
        {
            if (_tempControl == null || TempModel == null)
                return;
            _tempControl.GetTemp1();
        }

        private void GetTemp2()
        {
            if (_tempControl == null || TempModel == null)
                return;
            _tempControl.GetTemp2();
        }

        /*
        private void GetVac(string obj)
        {
            if (_vacControl == null || VacModel == null)
                return;
            byte channel = Convert.ToByte(obj);
           
            ResponseProto response = _vacControl.GetVAC(channel);
            if (response != null && response.Result != false && response.Params.Split(",").Length == 3)
            {
                switch (response.Params.Split(",")[0])
                {
                    case "01":
                        try
                        {
                            VacModel.Value1 = float.Parse(response.Params.Split(",")[2]);
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("   '{0}' is invalid using  ", response.Params.Split(",")[2]);
                        }

                        break;
                    case "02":
                        try
                        {
                            VacModel.Value2 = float.Parse(response.Params.Split(",")[2]);
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("   '{0}' is invalid using  ", response.Params.Split(",")[2]);
                        }
                        break;
                    case "03":
                        try
                        {
                            VacModel.Value3 = float.Parse(response.Params.Split(",")[2]);
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("   '{0}' is invalid using  ", response.Params.Split(",")[2]);
                        }
                        break;
                    case "04":
                        try
                        {
                            VacModel.Value4 = float.Parse(response.Params.Split(",")[2]);
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("   '{0}' is invalid using  ", response.Params.Split(",")[2]);
                        }
                        break;
                }
     

            }
            ShowResponseResult(response);
        }
        */

        private void ShowResponseResult(ResponseProto? response)
        {

            if (response != null && response.Result == false)
            {
                _aggregator.SendMessage(response.Message);
            }
        }
        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="response"></paramm
        private void ShowResponseResult(ResponseProto? response)
        {

            if (response != null && response.Result == false)
            {
                _aggregator.SendMessage(response.Message);
            }
        }
        */
        private void SendMessage(byte[] message)
        {

            if (message != null&& _device!=null)
            {
                _device.SendMsg(message);
            }
        }
        private void Receive()
        {
            
        }


        private void Entity_CommandResponseEvent(object? sender, ResponseProto e)
        {
            if(e.Result == false) 
            {
                return;
            }

            if (ushort.TryParse(e.Response, out ushort result))
            {
                switch(result) 
                {
                    case (ushort)ENUM_ETH_CMD.CMD_GetIP:
                        EthModel.RealIP = e.Params;
                        break;
                    case (ushort)ENUM_RELAY_CMD.CMD_GetRelay:
                        RelayStatus = e.Params;
                        break;
                }
            }
        }

        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
