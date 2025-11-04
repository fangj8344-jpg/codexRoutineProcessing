using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private MultiChannelHVEntity _multiChannelHVEntity;
        private readonly byte _channel;
        public HVMaxModel hVMaxModel;
        public byte Channel
        {
            get { return _channel; }
        }
        private float _readHV = 0;
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
        public DelegateCommand SetIVCommand { get; set; }
        public DelegateCommand GetIVCommand { get; set; }

        private void InitCommand()
        {
            SetIVCommand = new DelegateCommand(() => _multiChannelHVEntity?.SetIVCommand(Channel, WriteHV));
            GetIVCommand = new DelegateCommand(() => _multiChannelHVEntity?.GetIVCommand(Channel));
        }
        

    }

    public class HVMaxModel
    {
        public HVMaxModel()
        {
            HVMax = 30000;
            IV1_12Max = 2000;
            IV13Max = 6000;
        }
        public float HVMax { get; set; }
        public float IV1_12Max { get; set; }
        public float IV13Max { get; set; }
    }
}
