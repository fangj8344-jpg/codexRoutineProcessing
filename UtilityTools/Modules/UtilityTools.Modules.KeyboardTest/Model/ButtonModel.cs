using Prism.Mvvm;
using System;

namespace UtilityTools.Modules.KeyboardTest.Model
{
    public class ButtonModel : BindableBase
    {
        public ButtonModel(string id, string name, int byteIndex, int bitIndex)
        {
            Id = id;
            Name = name;
            ByteIndex = byteIndex;
            BitIndex = bitIndex;
        }

        public string Id { get; }
        public string Name { get; }
        public int ByteIndex { get; }
        public int BitIndex { get; }

        private bool _isPressed;
        public bool IsPressed
        {
            get => _isPressed;
            set
            {
                if (SetProperty(ref _isPressed, value))
                {
                    StateText = value ? "按下" : "松开";
                    if (value) PressCount++;
                    LastTriggerTime = DateTime.Now;
                }
            }
        }

        private int _pressCount;
        public int PressCount
        {
            get => _pressCount;
            set => SetProperty(ref _pressCount, value);
        }

        private DateTime? _lastTriggerTime;
        public DateTime? LastTriggerTime
        {
            get => _lastTriggerTime;
            set => SetProperty(ref _lastTriggerTime, value);
        }

        private string _stateText = "—";
        public string StateText
        {
            get => _stateText;
            set => SetProperty(ref _stateText, value);
        }

        /// <summary>
        /// 根据HID报文字节检测按钮状态（低电平有效）
        /// </summary>
        public void UpdateFromReport(byte byteValue)
        {
            bool pressed = (byteValue & (1 << BitIndex)) == 0;
            IsPressed = pressed;
        }

        public void Reset()
        {
            _isPressed = false;
            RaisePropertyChanged(nameof(IsPressed));
            PressCount = 0;
            LastTriggerTime = null;
            StateText = "—";
        }
    }
}
