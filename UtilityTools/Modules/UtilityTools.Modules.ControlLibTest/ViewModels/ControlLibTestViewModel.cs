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
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Xml.Linq;
using UtilityTools.Core.Dialog;
using UtilityTools.Core.Extension;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.ControlLibTest.Model;
using Zeptools.CommonLib.Model;
using Zeptools.ControlLib.Entity;

namespace UtilityTools.Modules.ControlLibTest.ViewModels
{
    internal class ControlLibTestViewModel : RegionViewModelBase
    {
        #region ------------Constructor------------
        public ControlLibTestViewModel(IDialogHostService dialogHostService, IContainerProvider containerProvider)
            :base(containerProvider)
        {
            _dialogHostService = dialogHostService;
            //消息提示
            _aggregator = containerProvider.Resolve<IEventAggregator>();
            _entity = new ServerEntity();

            InitProperty();
        }

        #endregion

        #region ------------Field------------
        private IDialogHostService _dialogHostService; 
        private readonly IEventAggregator _aggregator;
        ServerEntity _entity;
        #endregion

        #region ------------Property------------
        private ObservableCollection<CCSModel> _ccsModels = new ObservableCollection<CCSModel>();

        public ObservableCollection<CCSModel> CCSModels
        {
            get { return _ccsModels; }
            set { _ccsModels = value; RaisePropertyChanged(); }
        }

        private CCSModel _selectedCCSModelForValue; 
       
        public CCSModel SelectedCCSModelForValue
        {
            get { return _selectedCCSModelForValue; }
            set { _selectedCCSModelForValue = value; RaisePropertyChanged(); }
        }

        private CCSModel _selectedCCSModelForRelay;

        public CCSModel SelectedCCSModelForRelay
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

        private FanModel _selectedFanModel;

        public FanModel SelectedFanModel
        {
            get { return _selectedFanModel; }
            set { _selectedFanModel = value; RaisePropertyChanged(); }
        }

        private EthModel _ethModel;

        public EthModel EthModel
        {
            get { return _ethModel; }
            set { _ethModel = value; RaisePropertyChanged(); }
        }

        public DelegateCommand SetIPCommand { get; set; }

        public DelegateCommand GetIPCommand { get; set; }

      

        private TempModel _tempModel;

        public TempModel TempModel
        {
            get { return _tempModel; }
            set { _tempModel = value; RaisePropertyChanged(); }
        }

        public DelegateCommand GetAllTempCommand { get; set; }

        public DelegateCommand GetTemp1Command { get; set; }

        public DelegateCommand GetTemp2Command { get; set; }

       

        private VacModel _vacModel;

        public VacModel VacModel
        {
            get { return _vacModel; }
            set { _vacModel = value; RaisePropertyChanged(); }
        }



       
        public DelegateCommand<string> GetVacCommand { get; set; }

        private ObservableCollection<BseModel> _bseModels = new ObservableCollection<BseModel>();

        public ObservableCollection<BseModel> BseModels
        {
            get { return _bseModels; }
            set { _bseModels = value; RaisePropertyChanged(); }
        }

        private BseModel _bseModelForValue;

        public BseModel BseModelForValue
        {
            get { return _bseModelForValue; }
            set { _bseModelForValue = value; RaisePropertyChanged(); }
        }

        private BseModel _bseModelForNeg;

        public BseModel BseModelForNeg
        {
            get { return _bseModelForNeg; }
            set { _bseModelForNeg = value; RaisePropertyChanged(); }
        }

        private BseModel _bseModelForPos;

        public BseModel BseModelForPos
        {
            get { return _bseModelForPos; }
            set { _bseModelForPos = value; RaisePropertyChanged(); }
        }

        private SeModel _seModel;

        public SeModel SeModel
        {
            get { return _seModel; }
            set { _seModel = value; RaisePropertyChanged(); }
        }

        private ScanModel _scanModel;

        public ScanModel ScanModel
        {
            get { return _scanModel; }
            set { _scanModel = value; RaisePropertyChanged(); }
        }

        private LedModel _ledModels;

        public LedModel LedModels
        {
            get { return _ledModels ;}
            set { _ledModels = value; RaisePropertyChanged();  }
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

        private DACModel _selectedDacModel;

        public DACModel SelectedDacModel
        {
            get { return _selectedDacModel; }
            set { _selectedDacModel = value; RaisePropertyChanged(); }
        }
        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------

        private void InitProperty()
        {
            CCSModels.Add(new CCSModel() { Name = "CHA0", RealName = "AligX1", RelayName = "CCSk2", RelayChannel = (byte)0x02, Channel = (byte)0xA0, HasRelay = true, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false, SetValueFunc = _entity.SetAligX1, SetStateFunc = _entity.SetAligX1Polarity});
            CCSModels.Add(new CCSModel() { Name = "CHA1", RealName = "AligX2", RelayName = "CCSk3", RelayChannel = (byte)0x03, Channel = (byte)0xA1, HasRelay = true, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false, SetValueFunc = _entity.SetAligX2, SetStateFunc = _entity.SetAligX2Polarity });
            CCSModels.Add(new CCSModel() { Name = "CHA2", RealName = "CompressA", RelayName = "CCSkX", RelayChannel = (byte)0x0F, Channel = (byte)0xA2, HasRelay = false, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false, SetValueFunc = _entity.SetCompressLensA, SetStateFunc = null });
            CCSModels.Add(new CCSModel() { Name = "CHB0", RealName = "CompressB", RelayName = "CCSkX", RelayChannel = (byte)0x0F, Channel = (byte)0xB0, HasRelay = false, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false, SetValueFunc = _entity.SetCompressLensB, SetStateFunc = null });
            CCSModels.Add(new CCSModel() { Name = "CHB1", RealName = "AstigA", RelayName = "CCSk6", RelayChannel = (byte)0x06, Channel = (byte)0xB1, HasRelay = true, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false, SetValueFunc = _entity.SetAstigA, SetStateFunc = _entity.SetAstigAPolarity });
            CCSModels.Add(new CCSModel() { Name = "CHB2", RealName = "AstigB", RelayName = "CCSk7", RelayChannel = (byte)0x07, Channel = (byte)0xB2, HasRelay = true, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false, SetValueFunc = _entity.SetAstigB, SetStateFunc = _entity.SetAstigBPolarity });
            CCSModels.Add(new CCSModel() { Name = "CHB3", RealName = "AstigC", RelayName = "CCSk5", RelayChannel = (byte)0x05, Channel = (byte)0xB3, HasRelay = true, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false, SetValueFunc = _entity.SetAstigC, SetStateFunc = _entity.SetAstigCPolarity });
            CCSModels.Add(new CCSModel() { Name = "CHB4", RealName = "AstigD", RelayName = "CCSk4", RelayChannel = (byte)0x04, Channel = (byte)0xB4, HasRelay = true, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false, SetValueFunc = _entity.SetAstigD, SetStateFunc = _entity.SetAstigDPolarity });
            CCSModels.Add(new CCSModel() { Name = "CHB5", RealName = "AligY1", RelayName = "CCSk0", RelayChannel = (byte)0x00, Channel = (byte)0xB5, HasRelay = true, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false, SetValueFunc = _entity.SetAligY1, SetStateFunc = _entity.SetAligY1Polarity });
            CCSModels.Add(new CCSModel() { Name = "CHB7", RealName = "OB", RelayName = "CCSkX", RelayChannel = (byte)0x0F, Channel = (byte)0xB7, HasRelay = false, Value = 0, MinValue = 0, MaxValue = 0xFFFF, RelayState = false, SetValueFunc = _entity.SetObjectiveLens, SetStateFunc = null });
            CCSModels.Add(new CCSModel() { Name = "CHB8", RealName = "AligY2", RelayName = "CCSk1", RelayChannel = (byte)0x01, Channel = (byte)0xB8, HasRelay = true, Value = 0, MinValue = 0, MaxValue = 0x0FFF, RelayState = false, SetValueFunc = _entity.SetAligY2, SetStateFunc = _entity.SetAligY2Polarity });

            
            SelectedCCSModelForValue = CCSModels[0];
            SelectedCCSModelForRelay = CCSModels[0];

            FanModels.Add(new FanModel() { Name = "风扇1", Channel = (byte)0x01, MinSpeed = 0, MaxSpeed = 100, Speed = 0, SetValueFunc = _entity.SetFan1 });
            FanModels.Add(new FanModel() { Name = "风扇2", Channel = (byte)0x02, MinSpeed = 0, MaxSpeed = 100, Speed = 0, SetValueFunc = _entity.SetFan2 });
            FanModels.Add(new FanModel() { Name = "风扇3", Channel = (byte)0x03, MinSpeed = 0, MaxSpeed = 100, Speed = 0, SetValueFunc = _entity.SetFan3 });

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
            GetVacCommand = new DelegateCommand<string>(GetVac);

            BseModels.Add(new BseModel() { Name = "CH1", Channel = (byte)0x01, Value = 0, MinValue = 0, MaxValue = 0x0FFF, Negative = false, Positive = false, SetValueFunc = _entity.SetBSECH1, SetNegStateFunc = _entity.SetBSECH1_R, SetPosStateFunc = _entity.SetBSECH1_P });
            BseModels.Add(new BseModel() { Name = "CH2", Channel = (byte)0x02, Value = 0, MinValue = 0, MaxValue = 0x0FFF, Negative = false, Positive = false, SetValueFunc = _entity.SetBSECH2, SetNegStateFunc = _entity.SetBSECH2_R, SetPosStateFunc = _entity.SetBSECH2_P });
            BseModels.Add(new BseModel() { Name = "CH3", Channel = (byte)0x03, Value = 0, MinValue = 0, MaxValue = 0x0FFF, Negative = false, Positive = false, SetValueFunc = _entity.SetBSECH3, SetNegStateFunc = _entity.SetBSECH3_R, SetPosStateFunc = _entity.SetBSECH3_P });
            BseModels.Add(new BseModel() { Name = "CH4", Channel = (byte)0x04, Value = 0, MinValue = 0, MaxValue = 0x0FFF, Negative = false, Positive = false, SetValueFunc = _entity.SetBSECH4, SetNegStateFunc = _entity.SetBSECH4_R, SetPosStateFunc = _entity.SetBSECH4_P });

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


            DacModels.Add(new DACModel() { Name = "CHA" , Channel = (byte)0x00, Value = 0, MinValue = 0, MaxValue = 0x0FFF, SetValueFunc = _entity.SetDACCHA });
            DacModels.Add(new DACModel() { Name = "CHB" , Channel = (byte)0x01, Value = 0, MinValue = 0, MaxValue = 0x0FFF, SetValueFunc = _entity.SetDACCHB });
            DacModels.Add(new DACModel() { Name = "CHC",  Channel = (byte)0x02, Value = 0, MinValue = 0, MaxValue = 0x0FFF, SetValueFunc = _entity.SetDACCHC });
            DacModels.Add(new DACModel() { Name = "CHD",  Channel = (byte)0x03, Value = 0, MinValue = 0, MaxValue = 0x0FFF, SetValueFunc = _entity.SetDACCHD });
            SelectedDacModel = DacModels[0];

            UartModels.Add(new UARTModel() { Name = "光耦串口", Series = (byte)0x00,  Value = "115200" , SetBaudRateFunc= this.SetUart });
            UartModels.Add(new UARTModel() { Name = "RS485_1",  Series = (byte)0x01,  Value = "115200", SetBaudRateFunc = this.SetUart });
            UartModels.Add(new UARTModel() { Name = "RS485_2",  Series = (byte)0x02,  Value = "115200", SetBaudRateFunc = this.SetUart });
            UartModels.Add(new UARTModel() { Name = "RS485_3",  Series = (byte)0x03,  Value = "115200", SetBaudRateFunc = this.SetUart });


            LedModels = new LedModel();

            BindPropertyChanged();

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

        private void CCSModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var model = sender as CCSModel;
            if(model != null) 
            {
                switch(e.PropertyName) 
                {
                    case "Value":
                        if (model == SelectedCCSModelForValue)
                        {
                            var result = _entity.SetCCSCHValue(model.Channel, model.Value);
                            ShowResponseResult(result);
                        }
                        else
                        {
                            var result = model.SetValueFunc?.Invoke(model.Value);
                            ShowResponseResult(result);
                        }
                        break;
                    case "RelayState":
                        if (model == SelectedCCSModelForRelay)
                        {
                            var result = _entity.SetCCSk(model.RelayChannel, model.RelayState);
                            ShowResponseResult(result);
                        }
                        else
                        {
                            var result = model.SetStateFunc?.Invoke(model.RelayState);
                            ShowResponseResult(result);
                        }
                        break;
                }
            }
        }

        private void FanModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var model = sender as FanModel;
            if (model != null)
            {
                switch (e.PropertyName)
                {
                    case "Speed":
                        if (model == SelectedFanModel)
                        {
                            var result = _entity.SetFan(model.Channel, model.Speed);
                            ShowResponseResult(result);
                        }
                        else
                        {
                            var result = model.SetValueFunc?.Invoke(model.Speed);
                            ShowResponseResult(result);
                        }
                        break;
                }
            }
        }

        private void BseModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var model = sender as BseModel;  
            if (model != null)
            {
                switch (e.PropertyName)
                {
                    case "Value":
                        if (model == BseModelForValue)
                        {
                            var result = _entity.SetBSECH(model.Channel, model.Value);
                            ShowResponseResult(result);
                        }
                        else
                        {
                            var result = model.SetValueFunc?.Invoke(model.Value);
                            ShowResponseResult(result);
                        }
                        break;
                    case "Positive":
                        if (model == BseModelForPos)
                        {
                            var result = _entity.SetBSECH_P(model.Channel,model.Positive);
                            ShowResponseResult(result);
                        }
                        else
                        {
                            var result = model.SetPosStateFunc?.Invoke(model.Positive);
                            ShowResponseResult(result);
                        }
                        break;
                    case "Negative":
                        if (model == BseModelForNeg)
                        {
                            var result = _entity.SetBSECH_R(model.Channel,model.Negative);
                            ShowResponseResult(result);
                        }
                        else
                        {
                           var result = model.SetNegStateFunc?.Invoke(model.Negative);
                            ShowResponseResult(result);
                        }
                        break;
                }
            }
        }

        private void RelayModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var model = sender as RelayModel;
            if(model != null)
            {
                var result = _entity.SetRelayState(model.Channel, model.Enable);
                ShowResponseResult(result);
            }
        }
        private void DacModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var model = sender as DACModel;
            if (model != null)
            {
                switch (e.PropertyName)
                {
                    case "Value":
                        if (model == SelectedDacModel)
                        {
                            var result = _entity.SetDAC(model.Channel, model.Value);
                            ShowResponseResult(result);
                        }
                        else
                        {
                            var result = model.SetValueFunc?.Invoke(model.Value);
                            ShowResponseResult(result);
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

        private ResponseProto SetUart(byte Series,uint BaudRate)
        {
            var result = _entity.SetBaudRate(Series, BaudRate);
            ShowResponseResult(result);
            return result;
        }
 
        
        private ResponseProto SetLedModel(byte ContentLength, byte[] Content)
        {
            var result = _entity.SetLightMsg(ContentLength, Content);
            ShowResponseResult(result);
            return result;
        }



        private void SetIP()
        {
            string[] ip = EthModel.SetIP.Split("."); 
            ResponseProto response = _entity.SetIP(Convert.ToByte(ip[0]), Convert.ToByte(ip[1]),Convert.ToByte(ip[2]), Convert.ToByte(ip[3]));

            ShowResponseResult(response);
        }

        private void GetIP()
        {
            ResponseProto response = _entity.HandShake();
            if (response!=null&&response.Params != null&&response.Result!=false)
            {
                 EthModel.RealIP = response.Params;
            }
            ShowResponseResult(response);
        }
        private void GetAllTemp()
        {
            ResponseProto response=_entity.GetTemp();
            if(response!=null&&response.Result != false&&response.Params.Split(",").Length == 2)
            {
                try
                {
                    TempModel.Value1 = float.Parse(response.Params.Split(",")[0]);
                    TempModel.Value2 = float.Parse(response.Params.Split(",")[1]);
                }
                catch (FormatException)
                {
                    Console.WriteLine("  param is invalid using  ");
                }
            }
            ShowResponseResult(response);
        }

        private void GetTemp1()
        {
            ResponseProto response = _entity.GetTemp1();
            if (response != null && response.Result != false && response.Params != null)
            {
                try
                {
                    TempModel.Value1 = float.Parse(response.Params);
                }
                catch (FormatException)
                {
                    Console.WriteLine("  param is invalid using  ");
                }
            }
            ShowResponseResult(response);
        }

        private void GetTemp2()
        {
            ResponseProto response = _entity.GetTemp2();
            if (response != null && response.Result != false && response.Params != null)
            {
                try
                {
                    TempModel.Value2 = float.Parse(response.Params);
                }
                catch (FormatException)
                {
                    Console.WriteLine("  param is invalid using  ");
                }
            }
            ShowResponseResult(response);
           
        }

        private void GetVac(string obj)
        {
            byte channel= Convert.ToByte(obj);
            ResponseProto response= _entity.GetVAC(channel);
            if(response != null&&response.Result!=false&&response.Params.Split(",").Length==3)
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

        private void ShowResponseResult(ResponseProto response)
        {
            if (response != null && response.Result == false)
            {
                _aggregator.SendMessage(response.Message);
            }
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
