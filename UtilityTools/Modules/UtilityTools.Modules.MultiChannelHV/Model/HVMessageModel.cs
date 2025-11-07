using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UtilityTools.Modules.MultiChannelHV.Entity;

namespace UtilityTools.Modules.MultiChannelHV.Model
{
    public  class HVMessageModel:BindableBase
    {
        public HVMessageModel(  MultiChannelHVEntity multiChannelHVEntity, byte channel) 
        {
            _multiChannelHVEntity = multiChannelHVEntity;
            _channel = channel;
            if (_channel == 13)
            {
                MaxHV = 6000;
            }
            InitCommand();
        }
        public HVMessageModel()
        {
            
        }
        private MultiChannelHVEntity _multiChannelHVEntity;
        private readonly byte _channel;
        public byte Channel
        {
            get { return _channel; }
        }
        private float _readHV = 0;
        [JsonIgnore]
        public float ReadHV  
        {
            get { return _readHV; }
            set { _readHV = value;  RaisePropertyChanged(); }
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
        private ushort _maxHV = 2000;

        public ushort MaxHV
        {
            get { return _maxHV; }
            set { _maxHV = value; RaisePropertyChanged() ; }
        }
        [JsonIgnore]
        public DelegateCommand SetIVCommand { get; set; }
        [JsonIgnore]
        public DelegateCommand GetIVCommand { get; set; }

        private void InitCommand()
        {
            SetIVCommand = new DelegateCommand(() => _multiChannelHVEntity?.SetIVCommand(Channel, WriteHV));
            GetIVCommand = new DelegateCommand(() => _multiChannelHVEntity?.GetIVCommand(Channel));
        }
        

    }

    public class DataContainer
    {
        public DataContainer(MultiChannelHVModel multiChannelHVModel)
        {
            MultiChannelHVModel = multiChannelHVModel;
        }
        public MultiChannelHVModel MultiChannelHVModel { get; set; }
        public void SaveData()
        {
            string json = JsonSerializer.Serialize(MultiChannelHVModel);
            if (!Directory.Exists("data"))
            {
                Directory.CreateDirectory("data");
            }
            File.WriteAllText("data\\MultiChannelHVModel.json", json);
        }
        public void LoadData() 
        {
            if (File.Exists("data\\MultiChannelHVModel.json"))
            {
                string jsonString = File.ReadAllText("data\\MultiChannelHVModel.json");
                var Model = JsonSerializer.Deserialize<MultiChannelHVModel>(jsonString);
                MultiChannelHVModel.MaxHV = Model.MaxHV;
                MultiChannelHVModel.WriteHV = Model.WriteHV;
                for (int i = 0; i < MultiChannelHVModel.HVMessageModels.Count; i++) 
                {
                    MultiChannelHVModel.HVMessageModels[i].WriteHV = Model.HVMessageModels[i].WriteHV;
                    MultiChannelHVModel.HVMessageModels[i].MaxHV = Model.HVMessageModels[i].MaxHV;
                }
            }  
        }
    }
    
}
