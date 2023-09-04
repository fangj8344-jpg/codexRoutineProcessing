using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Services.Interfaces.IComm
{
    public interface ICommunication
    {
        #region ------------Property------------
        /// <summary>
        /// 设备名称
        /// </summary>
        string Name { get; }
        /// <summary>
        /// 是否已经连接
        /// </summary>
        bool IsConnect { get; }
        #endregion

        #region ------------Event------------
        /// <summary>
        /// 连接状态修改事件
        /// </summary>
        event EventHandler<bool> ConnectStateChangedEvent;
        /// <summary>
        /// 接收到数据事件
        /// </summary>
        event EventHandler<byte[]> ReceiveDataEvent;
        #endregion

        #region ------------PublicMethod------------
        void Connect();
        void Close();
        void SendMsg(byte[] data, int offset, int length);
        void StartConnectedCheck();
        #endregion
    }
}
