using HarfBuzzSharp;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Timers;
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
        private ushort _bufferHv;
        // 底层定时器实例
        private System.Timers.Timer _timer;
        // 任务执行的回调方法（由外部传入）
        private readonly Action _taskAction;
        // 线程同步锁（确保操作定时器时的线程安全）
        private readonly object _lock = new object();
        // 标记定时器是否正在运行
        private bool _isRunning;
        private bool _direction = true;
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
        private ushort _step = 200;
        public ushort Step
        {
            get { return _step; }
            set { _step = value; RaisePropertyChanged(); }
        }
        [JsonIgnore]
        public DelegateCommand SetIVCommand { get; set; }
        [JsonIgnore]
        public DelegateCommand GetIVCommand { get; set; }

        private void InitCommand()
        {
            SetIVCommand = new DelegateCommand(SetIv);
            GetIVCommand = new DelegateCommand(() => _multiChannelHVEntity?.GetIVCommand(Channel));
        }

        private  void SetIv()
        {
            
            _bufferHv = WriteHV;
            if (_bufferHv > ReadHV)
            {
                _direction = true;
            }
            else
            {
                _direction = false;
            }
                StopTimer();
                InitTimer();
            
        }

        public void InitTimer()
        {
            // 1. 创建定时器，设置间隔时间（单位：毫秒，此处为 1000ms = 1秒）
            if (_timer == null)
            {
                _timer = new System.Timers.Timer(1000);
            }
            if (_timer.Enabled)
            {
                _timer.Stop();
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

            if (_direction)
            {
                var _middleBufferHv = (ushort)ReadHV;
                var differ = _bufferHv - _middleBufferHv;

                if (differ / Step > 0)
                {
                    _multiChannelHVEntity?.SetIVCommand(Channel, (ushort)(_middleBufferHv + Step));
                }
                else
                {
                    _multiChannelHVEntity?.SetIVCommand(Channel, _bufferHv);
                    StopTimer();
                }

            }
            else
            {
                var _middleBufferHv = (ushort)ReadHV;
                var differ = _middleBufferHv -  _bufferHv;

                if (differ / Step > 0)
                {
                    _multiChannelHVEntity?.SetIVCommand(Channel, (ushort)(_middleBufferHv - Step));
                }
                else
                {
                    _multiChannelHVEntity?.SetIVCommand(Channel, _bufferHv);
                    StopTimer();
                }
            }
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
                MultiChannelHVModel.Step = Model.Step;
                for (int i = 0; i < MultiChannelHVModel.HVMessageModels.Count; i++) 
                {
                    MultiChannelHVModel.HVMessageModels[i].WriteHV = Model.HVMessageModels[i].WriteHV;
                    MultiChannelHVModel.HVMessageModels[i].MaxHV = Model.HVMessageModels[i].MaxHV;
                    MultiChannelHVModel.HVMessageModels[i].Step = Model.HVMessageModels[i].Step;
                }
            }  
        }
    }
    
}
