using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using UtilityTools.Core.Extension;
using UtilityTools.Core.Mvvm;
using UtilityTools.Modules.KeyboardTest.Model;

namespace UtilityTools.Modules.KeyboardTest.ViewModels
{
    public class KeyboardTestViewModel : RegionViewModelBase
    {
        #region Constructor
        public KeyboardTestViewModel(IContainerProvider containerProvider)
            : base(containerProvider)
        {
            _aggregator = containerProvider.Resolve<IEventAggregator>();
            _hidService = new HidKeyboardService();
            _hidService.ReportReceived += OnReportReceived;
            _hidService.ConnectionChanged += OnConnectionChanged;

            InitButtons();
            InitKnobs();
            InitCommands();
        }
        #endregion

        #region Field
        private readonly IEventAggregator _aggregator;
        private readonly HidKeyboardService _hidService;
        private byte[] _prevReport = new byte[64];

        private static readonly HashSet<int> ButtonBytes = new() { 8, 9, 10, 11, 12 };
        #endregion

        #region Property
        public ObservableCollection<ButtonModel> LeftButtons { get; private set; }
        public ObservableCollection<ButtonModel> RightButtons { get; private set; }
        public ObservableCollection<KnobModel> Knobs { get; private set; }

        private ObservableCollection<string> _logs;
        public ObservableCollection<string> Logs
        {
            get => _logs;
            set { _logs = value; RaisePropertyChanged(); }
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set { _isConnected = value; RaisePropertyChanged(); }
        }

        private string _deviceInfo = "未连接";
        public string DeviceInfo
        {
            get => _deviceInfo;
            set { _deviceInfo = value; RaisePropertyChanged(); }
        }

        private string _lastTriggered = "—";
        public string LastTriggered
        {
            get => _lastTriggered;
            set { _lastTriggered = value; RaisePropertyChanged(); }
        }

        private int _packetCount;
        public int PacketCount
        {
            get => _packetCount;
            set { _packetCount = value; RaisePropertyChanged(); }
        }
        #endregion

        #region Command
        public DelegateCommand ConnectCommand { get; private set; }
        public DelegateCommand DisconnectCommand { get; private set; }
        public DelegateCommand ResetAllCommand { get; private set; }
        public DelegateCommand ClearLogCommand { get; private set; }
        #endregion

        #region Private Method
        private void InitCommands()
        {
            ConnectCommand = new DelegateCommand(DoConnect);
            DisconnectCommand = new DelegateCommand(DoDisconnect);
            ResetAllCommand = new DelegateCommand(ResetAll);
            ClearLogCommand = new DelegateCommand(() => Logs.Clear());
        }

        private void InitButtons()
        {
            LeftButtons = new ObservableCollection<ButtonModel>
            {
                new ButtonModel("B1", "电子方位线1", 9, 1),
                new ButtonModel("B2", "电子方位线2", 10, 1),
                new ButtonModel("B3", "报警确认", 9, 4),
                new ButtonModel("B4", "海图图层", 12, 7),
                new ButtonModel("B5", "偏心", 10, 3),
                new ButtonModel("B6", "警戒区", 10, 4),
                new ButtonModel("B7", "捕获/激活", 9, 2),
                new ButtonModel("B8", "目标取消", 10, 2),
                new ButtonModel("B9", "目标信息", 8, 2),
            };

            RightButtons = new ObservableCollection<ButtonModel>
            {
                new ButtonModel("B10", "活动距标圈1", 11, 5),
                new ButtonModel("B11", "活动距标圈2", 9, 5),
                new ButtonModel("B12", "待机/发射", 11, 1),
                new ButtonModel("B13", "显示模式", 10, 5),
                new ButtonModel("B14", "脉宽调节", 8, 5),
                new ButtonModel("B15", "运动模式", 11, 3),
                new ButtonModel("B16", "量程+", 9, 6),
                new ButtonModel("B17", "量程-", 8, 6),
                new ButtonModel("B18", "船艏线关", 11, 2),
                new ButtonModel("B19", "人员落水", 8, 4),
            };

            Logs = new ObservableCollection<string>();
        }

        private void InitKnobs()
        {
            // 旋钮: id, name, rotationByteIndex, isLarge, pressByteIndex, pressBitIndex
            Knobs = new ObservableCollection<KnobModel>
            {
                new KnobModel("K1", "左大旋钮", 3, true, 8, 0),
                new KnobModel("K2", "亮度", 19, false, 9, 3),
                new KnobModel("K3", "调谐", 5, false, 8, 7),
                new KnobModel("K4", "增益", 16, false, 11, 7),
                new KnobModel("K5", "海浪", 17, false, 10, 7),
                new KnobModel("K6", "雨雪", 18, false, 9, 7),
                new KnobModel("K7", "右大旋钮", 4, true, 9, 0),
            };
        }

        private void DoConnect()
        {
            if (_hidService.IsConnected) return;

            bool success = _hidService.Connect();
            IsConnected = _hidService.IsConnected;
            DeviceInfo = _hidService.DeviceInfo;

            if (success)
            {
                AddLog("✓ HID 设备连接成功");
                _aggregator.SendMessage("键盘已连接");
            }
            else
            {
                AddLog($"✗ 连接失败: {DeviceInfo}");
                _aggregator.SendMessage(DeviceInfo);
            }
        }

        private void DoDisconnect()
        {
            _hidService.Disconnect();
            IsConnected = false;
            DeviceInfo = "已断开";
            _prevReport = new byte[64];
            AddLog("■ 设备已断开");
        }

        private void OnConnectionChanged(object sender, bool connected)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                IsConnected = connected;
                DeviceInfo = _hidService.DeviceInfo;
                if (!connected)
                {
                    _prevReport = new byte[64];
                    AddLog("⚠ 设备连接断开");
                }
            });
        }

        private void OnReportReceived(object sender, byte[] report)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                PacketCount++;
                ParseReportDiff(report);
                _prevReport = report;
            });
        }

        /// <summary>
        /// 帧差比较法：与 Python 版 _read_loop 完全一致
        /// prev 初始为全0，设备空闲时 button bytes 为 0xFF
        /// </summary>
        private void ParseReportDiff(byte[] data)
        {
            if (data.Length < 20) return;

            // 按钮+旋钮按下 检测（帧差比较，和Python一致）
            foreach (int byteIdx in ButtonBytes)
            {
                if (_prevReport[byteIdx] == data[byteIdx]) continue;

                int diff = _prevReport[byteIdx] ^ data[byteIdx];
                for (int bit = 0; bit < 8; bit++)
                {
                    if ((diff & (1 << bit)) == 0) continue;

                    bool pressed = (data[byteIdx] & (1 << bit)) == 0;

                    var btn = FindButton(byteIdx, bit);
                    if (btn != null)
                    {
                        btn.IsPressed = pressed;
                        string action = pressed ? "按下" : "松开";
                        LastTriggered = $"{btn.Id} {btn.Name} {action}";
                        AddLog($"[按钮] {btn.Id} {btn.Name} → {action}");
                    }

                    var knob = FindKnobPress(byteIdx, bit);
                    if (knob != null)
                    {
                        knob.IsPressed = pressed;
                        string action = pressed ? "按下" : "松开";
                        LastTriggered = $"{knob.Id} {knob.Name} {action}";
                        AddLog($"[旋钮按下] {knob.Id} {knob.Name} → {action}");
                    }
                }
            }

            // 旋钮旋转检测：非零即一个步进（和Python一致）
            foreach (var knob in Knobs)
            {
                byte val = data[knob.ByteIndex];
                if (val == 4)
                {
                    knob.UpdateRotation(val);
                    LastTriggered = $"{knob.Id} {knob.Name} {knob.Direction}";
                    AddLog($"[旋钮] {knob.Id} {knob.Name} → 正转 (累计={knob.AccumulatedValue:+0;-0;0})");
                }
                else if (val == 252)
                {
                    knob.UpdateRotation(val);
                    LastTriggered = $"{knob.Id} {knob.Name} {knob.Direction}";
                    AddLog($"[旋钮] {knob.Id} {knob.Name} → 反转 (累计={knob.AccumulatedValue:+0;-0;0})");
                }
            }
        }

        private ButtonModel FindButton(int byteIdx, int bitIdx)
        {
            foreach (var btn in LeftButtons)
                if (btn.ByteIndex == byteIdx && btn.BitIndex == bitIdx)
                    return btn;
            foreach (var btn in RightButtons)
                if (btn.ByteIndex == byteIdx && btn.BitIndex == bitIdx)
                    return btn;
            return null;
        }

        private KnobModel FindKnobPress(int byteIdx, int bitIdx)
        {
            foreach (var knob in Knobs)
                if (knob.PressByteIndex == byteIdx && knob.PressBitIndex == bitIdx)
                    return knob;
            return null;
        }

        private void AddLog(string message)
        {
            var entry = $"{DateTime.Now:HH:mm:ss.fff} {message}";
            Logs.Insert(0, entry);
            if (Logs.Count > 500)
                Logs.RemoveAt(Logs.Count - 1);
        }

        private void ResetAll()
        {
            foreach (var btn in LeftButtons) btn.Reset();
            foreach (var btn in RightButtons) btn.Reset();
            foreach (var knob in Knobs) knob.Reset();
            LastTriggered = "—";
            PacketCount = 0;
            AddLog("已重置所有状态");
        }
        #endregion
    }
}
