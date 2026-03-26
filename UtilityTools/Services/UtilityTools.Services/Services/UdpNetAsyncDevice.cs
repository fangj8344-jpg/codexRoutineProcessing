#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Services.Services
 * 唯一标识：2cfea093-e0d7-480d-b816-f7f3d8849d35
 * 文件名：UdpNetAsyncDevice
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/3/29 8:54:49
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
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Model;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Services.Services
{
    public class UdpNetAsyncDevice : INetService, IAsynRWService
    {
        #region ------------Constructor------------
        public UdpNetAsyncDevice()
        {
            _syncObject = new object();
            _sendQueue = new ConcurrentQueue<byte[]>();
            _importantSendQueue = new ConcurrentQueue<byte[]>();
            DeviceInstance = new UdpNetConfigModel();
            DeviceInstance.ReceiveDataEvent += DeviceInstance_ReceiveDataEvent;
            MinWriteInterval = 10;
            WaitInterval = 200;
            _sendEvent = new AutoResetEvent(false);
            _receiveEvent = new AutoResetEvent(false);

        }

        #endregion

        #region ------------Field------------
        private Thread _sendThread;
        private AutoResetEvent _sendEvent;
        private AutoResetEvent _receiveEvent;
        private CancellationTokenSource _sendThreadToken;
        private int _timeOutCount = 0;

        private object _syncObject;
        private ConcurrentQueue<byte[]> _sendQueue;
        private ConcurrentQueue<byte[]> _importantSendQueue;

      
        #endregion

        #region ------------Property------------
        /// <summary>
        /// 设备实例
        /// </summary>
        public NetConfigModel DeviceInstance { get; set; }

        /// <summary>
        /// 设备是否打开
        /// </summary>
        public bool IsOpen
        {
            get
            {
                if (DeviceInstance != null)
                {
                    return DeviceInstance.IsOpen();
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
        /// 连接测试
        /// </summary>
        public DelegateConnectTestCommand ConnectTest { get; set; }
        public bool ConnectSatus { get; set; } = false;
        public int SendQueueCount { get => _sendQueue.Count; }
        /// <summary>
        /// 两次写入最小间隔 ms, 特别是串口通讯需要根据设备情况进行设置
        /// </summary>
        public int MinWriteInterval { get; set; }
        public double WaitInterval { get; set; } = 200;
        #endregion

        #region ------------Event------------
        /// <summary>
        /// 更新回包数据事件
        /// </summary>
        public event EventHandler<byte[]> UpdateResponse;

        public event EventHandler<bool> ConnectStatusChanged;

        #endregion

        #region ------------PublicMethod------------
        /// <summary>
        /// 打开设备，需要捕获异常，可能存在异常情况
        /// </summary>
        /// <returns>打开结果</returns>
        public bool Open()
        {
            if (DeviceInstance.IsOpen())
                return true;

            if (DeviceInstance.Open())
            {
                if (_sendThreadToken != null)
                {
                    _sendThreadToken.Cancel();
                    _sendThreadToken.Dispose();
                    _sendThreadToken = null;
                }
                if (_sendThread != null)
                {
                    _sendThread.Interrupt();
                    _sendThread = null;
                }
                _sendThreadToken = new CancellationTokenSource();
                _sendThread = new Thread(SendThreadFunction);
                _sendThread.IsBackground = true;
                _sendThread.Start();
              
            }

            return DeviceInstance.IsOpen();
        }

        /// <summary>
        /// 关闭设备
        /// </summary>
        /// <returns>关闭结果</returns>
        public void Close()
        {
            StopSendThread();
            DeviceInstance.Close();
            _sendQueue?.Clear();
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
                Enqueue(cmd);
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
            if (handle is NetConfigModel sp)
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
        /// 进队操作
        /// </summary>
        /// <param name="data"></param>
        private void Enqueue(byte[] data)
        {
            _sendQueue.Enqueue(data);
            _sendEvent?.Set();
        }
        /// <summary>
        /// 进入优先队列
        /// </summary>
        /// <param name="cmd"></param>
        public void SendImportantMsg(byte[] cmd)
        {
            if (IsOpen)
            {
                _importantSendQueue.Enqueue(cmd);
                _sendEvent?.Set();

            }
        }

        /// <summary>
        /// 终止发送线程
        /// </summary>
        private void StopSendThread()
        {
           
            if (_sendThreadToken != null)
            {
                if (!_sendThreadToken.IsCancellationRequested)
                {
                    _sendThreadToken.Cancel();
                }
              
               

            }
            if (_sendThread != null && _sendThread.IsAlive)
            {
                //设置5s超时时间
                bool isThreadExited = _sendThread.Join(5000);
                if (isThreadExited)
                {
                    LogManager.GetCurrentClassLogger().Error($"发送线程正常退出  ");

                }
                else
                {
                    LogManager.GetCurrentClassLogger().Error($"发送线程退出超时");
                }

                _sendThread = null;

            }
            if (_sendThreadToken != null)
            {
                _sendThreadToken.Dispose();
                _sendThreadToken = null;
            }

        }

        /// <summary>
        /// 出队操作
        /// </summary>
        /// <returns></returns>
        private byte[] Dequeue()
        {
            while (true)
            {
                if (_sendQueue.TryDequeue(out var data))
                    return data;

                
            }
        }

        /// <summary>
        /// 发送线程
        /// </summary>
        private async void SendThreadFunction()
        {
            try
            {
                LogManager.GetCurrentClassLogger().Debug($"{Name}开启发送线程");
                if (_sendThreadToken == null)
                {
                    _sendThreadToken = new CancellationTokenSource();
                }
                var token = _sendThreadToken.Token;

                while (!token.IsCancellationRequested)
                {
                    if (_importantSendQueue.Count == 0 && _sendQueue.Count == 0)
                    {
                        _sendEvent?.WaitOne(100); 
                    }
                    if (token.IsCancellationRequested)
                    {
                        LogManager.GetCurrentClassLogger().Debug($"发送线程被主动关闭 ");
                        break;
                    } 
                    byte[] cmdToSend = null;
                    if (_importantSendQueue.TryDequeue(out var cmd))
                    {
                        cmdToSend = cmd;
                    }
                    else if (_sendQueue.TryDequeue(out cmd))
                    {
                        cmdToSend = cmd;
                    }
                    if (cmdToSend != null)
                    {
                        try
                        {
                            // 发送业务
                            DeviceInstance.Send(cmdToSend);
                            LogManager.GetCurrentClassLogger().Debug($"{Name}:IP:{DeviceInstance.TargetIp},Port:{DeviceInstance.TargetPort} 发送 : {BitConverter.ToString(cmdToSend).Replace("-", " ")}");
                        }
                        catch (Exception ex)
                        {
                            LogManager.GetCurrentClassLogger().Error($"发送数据失败 {ex.Message} ");
                        }
                        bool? result = _receiveEvent?.WaitOne((int)WaitInterval);
                        if (result == false)
                        {
                            _timeOutCount++;
                            if (_timeOutCount >= 3)
                            {
                                if (ConnectSatus == true)
                                {
                                    ConnectSatus = false;
                                    ConnectStatusChanged?.Invoke(this, ConnectSatus);
                                }

                            }
                        }
                        else 
                        {
                            _timeOutCount = 0;
                            if (ConnectSatus == false)
                            {
                                ConnectSatus = true;
                                ConnectStatusChanged?.Invoke(this, ConnectSatus);
                            }
                        }
                    }
                }
            } 
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error($"发送线程异常 {ex}");
            }
           
        }


        private void DeviceInstance_ReceiveDataEvent(object sender, byte[] response)
        {
            try
            {
                LogManager.GetCurrentClassLogger().Debug($"{Name}:IP:{DeviceInstance.TargetIp},Port:{DeviceInstance.TargetPort}  接收 : {BitConverter.ToString(response).Replace("-"," ")}");
                UpdateResponse?.Invoke(this, response); 
                _receiveEvent?.Set();
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error($" udp接收事件异常:{ex}");
                return;
            }
        }

        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
