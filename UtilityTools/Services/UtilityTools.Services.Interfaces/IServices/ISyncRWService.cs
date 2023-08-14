using System;
using System.Collections.Generic;
using System.Text;

namespace UtilityTools.Services.Interfaces.IServices
{
    public interface ISyncRWService
    {
        #region Property
        /// <summary>
        /// 设备是否打开
        /// </summary>
        bool IsOpen
        {
            get;
        }

        /// <summary>
        /// 设备名称
        /// </summary>
        string Name
        {
            get;
            set;
        }

        /// <summary>
        /// 数据传输是否采用二进制传输
        /// </summary>
        bool IsBinary { get; set; }
        #endregion

        #region publicFunction
        /// <summary>
        /// 打开设备
        /// </summary>
        /// <returns>打开结果</returns>
        bool Open();

        /// <summary>
        /// 关闭设备
        /// </summary>
        /// <returns>关闭结果</returns>
        void Close();

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

        /// <summary>
        /// 获取服务句柄
        /// </summary>
        /// <returns></returns>
        object GetHandle();

        /// <summary>
        /// 设置句柄
        /// </summary>
        /// <param name="obj"></param>
        void SetHandle(object obj);
        #endregion
    }
}
