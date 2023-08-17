using System;
using System.Collections.Generic;
using System.Text;

namespace UtilityTools.Services.Interfaces.IServices
{
    public interface ISyncRWService : IBaseService
    {
        #region publicFunction
        /// <summary>
        /// 网络UDP请求
        /// </summary>
        /// <param name="cmdName">指令名称</param>
        /// <param name="cmd">指令集</param>
        /// <param name="response">回包数据</param>
        /// <param name="length">回包数据长度</param>
        /// <param name="waitTime">等待超时时间</param>
        /// <exception cref="Exception">发送异常</exception>
        void Request(string cmdName, byte[] cmd, out byte[] response, out int length, int waitTime);

        #endregion
    }
}
