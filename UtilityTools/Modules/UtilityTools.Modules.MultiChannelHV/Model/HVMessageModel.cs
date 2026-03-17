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
                MaxIV = 6000;
            }
            InitCommand();
        }
        public HVMessageModel()
        {
            
        }
        private ushort _endBufferIv;
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
                if (value > MaxIV)
                {
                    _writeHV = MaxIV;
                }
                else
                {
                    _writeHV = value;
                }
                
                RaisePropertyChanged();
            }
        }
        private int _setHv = 0;
        /// <summary>
        /// 记录的设置值
        /// </summary>
        public int SetHv
        {
            get { return _setHv; }
            set { _setHv = value; RaisePropertyChanged(); }
        }
        private ushort _maxIV = 2000;

        public ushort MaxIV
        {
            get { return _maxIV; }
            set { _maxIV = value; RaisePropertyChanged() ; }
        }
        private ushort _step = 200;
        public ushort Step
        {
            get { return _step; }
            set 
            {
                
                if (value > 500)
                {
                    _step = 500;
                }
                else if (value < 5)
                {
                    _step = 5;
                }
                else 
                {
                    _step = value;
                }
                RaisePropertyChanged(); }
        }
        private int _bufferIv;
        private int BufferIv
        {
            get { return _bufferIv; }
            set 
            {
                if (value < 0)
                {
                    _bufferIv = 0;
                }
                else if (value > MaxIV)
                {
                    _bufferIv = MaxIV;
                }
                else
                {
                    _bufferIv = value;
                }
            }
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
            if (_multiChannelHVEntity?.SerialPortService == null || _multiChannelHVEntity?.SerialPortService.IsOpen == false)
            {
                return;
            }
            _endBufferIv = WriteHV;
            _bufferIv = SetHv;
            if (_endBufferIv > SetHv)
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
        private void SetEntityIV(ushort iv) 
        { 
            _multiChannelHVEntity?.SetIVCommand(_channel, iv);
            SetHv = iv;
        }
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_direction)
            {

                var differ = _endBufferIv - _bufferIv;

                if (_endBufferIv < _bufferIv || _endBufferIv < _bufferIv + Step)
                {
                    SetEntityIV(_endBufferIv);
                    StopTimer();
                }
                else
                {
                    _bufferIv = (ushort)(_bufferIv + Step);
                    if (_bufferIv > _endBufferIv)
                    {
                        SetEntityIV(_endBufferIv);
                        StopTimer();
                        return;
                    }
                    SetEntityIV((ushort)_bufferIv);
                }
            }
            else
            {
                var differ = _endBufferIv - _bufferIv;
                if (_endBufferIv > _bufferIv || _endBufferIv > _bufferIv - Step || _bufferIv - Step > MaxIV || _bufferIv > MaxIV)
                {
                    SetEntityIV(_endBufferIv);
                    StopTimer();
                }
                else
                {
                    _bufferIv = (_bufferIv - Step);
                    if (_bufferIv > MaxIV)
                    {
                        SetEntityIV(_endBufferIv);
                        StopTimer();
                        return;
                    }
                    SetEntityIV((ushort)_bufferIv);

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
                MultiChannelHVModel.HvStep = Model.HvStep;
                for (int i = 0; i < MultiChannelHVModel.HVMessageModels.Count; i++) 
                {
                    MultiChannelHVModel.HVMessageModels[i].WriteHV = Model.HVMessageModels[i].WriteHV;
                    MultiChannelHVModel.HVMessageModels[i].MaxIV = Model.HVMessageModels[i].MaxIV;
                    MultiChannelHVModel.HVMessageModels[i].Step = Model.HVMessageModels[i].Step;
                }
            }  
        }
    }
    
}
