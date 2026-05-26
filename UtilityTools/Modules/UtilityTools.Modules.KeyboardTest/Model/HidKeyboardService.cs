using HidSharp;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UtilityTools.Modules.KeyboardTest.Model
{
    public class HidKeyboardService : IDisposable
    {
        private const int VendorId = 0x1234;
        private const int ProductId = 0x003B;
        private const int ReportLength = 64;
        private const int ReportIdOffset = 1; // HidSharp 在 Windows 上 Read 会包含 Report ID 前缀字节

        private HidStream _stream;
        private HidDevice _device;
        private CancellationTokenSource _cts;
        private Task _readTask;
        private byte[] _lastReport;
        private readonly object _lock = new object();

        public event EventHandler<byte[]> ReportReceived;
        public event EventHandler<bool> ConnectionChanged;

        public bool IsConnected { get; private set; }
        public string DeviceInfo { get; private set; } = "未连接";

        public bool Connect()
        {
            try
            {
                _device = DeviceList.Local.GetHidDeviceOrNull(VendorId, ProductId);
                if (_device == null)
                {
                    DeviceInfo = $"未找到设备 (VID:0x{VendorId:X4} PID:0x{ProductId:X4})";
                    LogManager.GetCurrentClassLogger().Warn(DeviceInfo);
                    return false;
                }

                if (!_device.TryOpen(out _stream))
                {
                    DeviceInfo = "设备打开失败（可能被占用）";
                    LogManager.GetCurrentClassLogger().Error(DeviceInfo);
                    return false;
                }

                _stream.ReadTimeout = 1000;
                IsConnected = true;
                DeviceInfo = $"{_device.GetProductName()} [已连接]";
                ConnectionChanged?.Invoke(this, true);

                StartReading();
                return true;
            }
            catch (Exception ex)
            {
                DeviceInfo = $"连接异常: {ex.Message}";
                LogManager.GetCurrentClassLogger().Error($"HID Connect failed: {ex.Message}");
                return false;
            }
        }

        public void Disconnect()
        {
            StopReading();
            _stream?.Close();
            _stream?.Dispose();
            _stream = null;
            IsConnected = false;
            DeviceInfo = "已断开";
            ConnectionChanged?.Invoke(this, false);
        }

        private void StartReading()
        {
            _cts = new CancellationTokenSource();
            _readTask = Task.Run(() => ReadLoop(_cts.Token), _cts.Token);
        }

        private void StopReading()
        {
            _cts?.Cancel();
            try { _readTask?.Wait(500); } catch { }
            _cts?.Dispose();
            _cts = null;
        }

        private void ReadLoop(CancellationToken token)
        {
            // HidSharp 在 Windows 上返回 Report ID + 64字节数据 = 65字节
            int rawLen = _device.GetMaxInputReportLength();
            var buffer = new byte[rawLen];

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_stream == null) break;

                    int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead >= ReportLength + ReportIdOffset)
                    {
                        // 跳过 Report ID 字节，取后面 64 字节（和 Python hidapi 行为对齐）
                        var report = new byte[ReportLength];
                        Array.Copy(buffer, ReportIdOffset, report, 0, ReportLength);

                        lock (_lock)
                        {
                            _lastReport = report;
                        }

                        ReportReceived?.Invoke(this, report);
                    }
                }
                catch (TimeoutException)
                {
                    // 正常超时，继续读
                }
                catch (Exception ex)
                {
                    if (!token.IsCancellationRequested)
                    {
                        LogManager.GetCurrentClassLogger().Error($"HID Read error: {ex.Message}");
                        IsConnected = false;
                        ConnectionChanged?.Invoke(this, false);
                        break;
                    }
                }
            }
        }

        public byte[] GetLastReport()
        {
            lock (_lock)
            {
                return _lastReport;
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
