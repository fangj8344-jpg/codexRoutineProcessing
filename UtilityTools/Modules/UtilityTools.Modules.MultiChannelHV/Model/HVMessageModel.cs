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
            InitCommand();
        }
        private MultiChannelHVEntity _multiChannelHVEntity;
        private readonly byte _channel;
        public byte Channel
        {
            get { return _channel; }
        }
        private int _readHV = 0;
        public int ReadHV  
        {
            get { return _readHV; }
            set { _readHV = value;  RaisePropertyChanged(); }
        }
        private int _writeHV = 0;
        public int WriteHV
        {
            get { return _writeHV; }
            set { _writeHV = value; RaisePropertyChanged(); }
        }
        public DelegateCommand SetIVCommand { get; set; }
        public DelegateCommand GetIVCommand { get; set; }

        private void InitCommand()
        {
            SetIVCommand = new DelegateCommand(() => _multiChannelHVEntity?.SetIVCommand(Channel, WriteHV));
            GetIVCommand = new DelegateCommand(() => _multiChannelHVEntity?.GetIVCommand(Channel));
        }
        

    }
}
