using System;
using System.Collections.Generic;
using System.Text;
using UtilityTools.Core.Model;

namespace UtilityTools.Services.Interfaces.IServices
{
    /// <summary>
    /// 异步读写服务
    /// </summary>
    public interface IAsynRWService : IBaseService
    {
        #region Event
        /// <summary>
        /// 更新回包数据事件
        /// </summary>
        event EventHandler<byte[]> UpdateResponse;
        #endregion


        #region publicFunction
        /// <summary>
        /// 串口数据发送接口
        /// </summary>
        /// <param name="cmd">指令参数</param>
        void SendMsg(byte[] cmd);
        #endregion
    }
}
