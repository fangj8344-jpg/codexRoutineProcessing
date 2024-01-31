#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Services.Services
 * 唯一标识：10f5b546-9976-4f41-b07e-c78f32d64c06
 * 文件名：SerialPortService
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/13 11:05:09
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

using NLog;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Threading;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Model;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Services.Services
{
    public class SerialPortService : IAsynRWService, ISerialPortService
    {
        #region ------------Constructor------------
        public SerialPortService()
        {
            _sendThread = new Thread(SendThreadFunction);
            _sendThread.IsBackground = true;
            _sendThreadToken = new CancellationTokenSource();
            _sendQueue = new ConcurrentQueue<byte[]>();
            DeviceInstance = new SerialPortModel();
            DeviceInstance.SerialPort.DataReceived += SerialPort_DataReceived;
            MinWriteInterval = 100;
        }

        #endregion

        #region ------------Field------------
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private Thread _sendThread;
        private CancellationTokenSource _sendThreadToken;

        private ConcurrentQueue<byte[]> _sendQueue;
        #endregion

        #region ------------Property------------
        /// <summary>
        /// USB信息
        /// </summary>
        public UsbInfoModel? UsbInfo { get; set; }

        /// <summary>
        /// 设备实例
        /// </summary>
        public SerialPortModel DeviceInstance { get; set; }

        /// <summary>
        /// 设备是否打开
        /// </summary>
        public bool IsOpen
        {
            get
            {
                if (DeviceInstance != null && DeviceInstance.SerialPort != null)
                {
                    return DeviceInstance.SerialPort.IsOpen;
                }

                return false;
            }
        }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 数据传输是否采用二进制传输
        /// </summary>
        public bool IsBinary { get; set; }

        /// <summary>
        /// 两次写入最小间隔 ms
        /// </summary>
        public int MinWriteInterval { get; set; }

        /// <summary>
        /// 连接测试
        /// </summary>
        public DelegateConnectTestCommand ConnectTest { get; set; }
        #endregion

        #region ------------Event------------
        /// <summary>
        /// 更新回包数据事件
        /// </summary>
        public event EventHandler<byte[]> UpdateResponse;
        #endregion

        #region ------------PublicMethod------------
        /// <summary>
        /// 打开设备，需要捕获异常，可能存在异常情况
        /// </summary>
        /// <returns>打开结果</returns>
        public bool Open()
        {
            if (DeviceInstance == null || DeviceInstance.SerialPort == null)
            {
                throw new Exception($"{Name} 的串口设备句柄不能为NULL！");
            }

            if (DeviceInstance.SerialPort.IsOpen)
                return true;

            if (UsbInfo != null)
            {
                var portName = HardwareMethod.SearchDeviceByID(UsbInfo.Info);
                if (!string.IsNullOrEmpty(portName))
                {
                    DeviceInstance.PortName = portName;
                }
            }

            if (string.IsNullOrEmpty(DeviceInstance.PortName))
            {
                throw new Exception($"{Name} 的串口设备名称不能为空！");
            }

            if (DeviceInstance.Open())
            {
                if (!_sendThread.IsAlive)
                {
                    _sendThread = new Thread(SendThreadFunction);
                    _sendThread.IsBackground = true;
                    _sendThread.Start();
                }
            }

            return DeviceInstance.SerialPort.IsOpen;
        }

        /// <summary>
        /// 关闭设备
        /// </summary>
        /// <returns>关闭结果</returns>
        public void Close()
        {
            if (DeviceInstance.Close())
            {
                StopSendThread();
            }
        }

        /// <summary>
        /// 串口数据发送接口
        /// </summary>
        /// <param name="cmdCode">指令类型</param>
        /// <param name="cmd">指令参数</param>
        public void SendMsg(byte[] cmd)
        {
            if (IsOpen)
            {
                _sendQueue.Enqueue(cmd);
            }
        }

        /// <summary>
        /// 获取设备句柄
        /// </summary>
        /// <returns></returns>
        public object GetHandle()
        {
            return DeviceInstance;
        }

        /// <summary>
        /// 设置句柄
        /// </summary>
        public void SetHandle(object handle)
        {
            if (handle is SerialPortModel sp)
                DeviceInstance = sp;
        }

        /// <summary>
        /// 获取指令字符串，用于日志或者打印信息
        /// </summary>
        /// <returns></returns>
        public string GetCmdString(byte[] cmd, int length)
        {
            if (IsBinary)
            {
                return DataTypeCaster.ByteArrayToString(cmd, length);
            }
            else
            {
                return Encoding.Default.GetString(cmd, 0, length).Trim('\n');
            }
        }
        #endregion

        #region ------------PrivateMethod------------

        /// <summary>
        /// 终止发送线程
        /// </summary>
        private void StopSendThread()
        {
            _sendThreadToken.Cancel();
            _sendThread.Join();
        }


        /// <summary>
        /// 发送线程
        /// </summary>
        private void SendThreadFunction()
        {
            LOGGER.Debug($"{Name}开启发送线程");
            var token = _sendThreadToken.Token;
            try
            {
                Stopwatch sw = new();
                while (!token.IsCancellationRequested)
                {
                    if (_sendQueue.TryDequeue(out var cmd))
                    {
                        sw.Restart();
                        DeviceInstance.SerialPort.Write(cmd, 0, cmd.Length);
                        LOGGER.Debug($"{Name} 发送 : {GetCmdString(cmd, cmd.Length)}");
                        sw.Stop();

                        int evitation = MinWriteInterval - (int)sw.ElapsedMilliseconds;
                        if (evitation > 0)
                        {
                            Thread.Sleep(evitation);
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
            }
            catch (Exception ex)
            {
                LOGGER.Error($"{Name} 发送线程异常：{ex.Message}");
            }
            finally
            {
                LOGGER.Debug($"{Name} 结束发送线程");
            }
        }

        /// <summary>
        /// 串口数据接收回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (sender is SerialPort dev && e.EventType == SerialData.Chars)
            {
                try
                {
                    int size = dev.BytesToRead;
                    byte[] response = new byte[size];
                    int realLen = dev.Read(response, 0, size);

                    LOGGER.Debug($"{Name} 接收 : {GetCmdString(response, realLen)}");

                    UpdateResponse?.Invoke(this, response);
                }
                catch (Exception ex)
                {
                    LOGGER.Error(ex.Message);
                    return;
                }
            }
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
