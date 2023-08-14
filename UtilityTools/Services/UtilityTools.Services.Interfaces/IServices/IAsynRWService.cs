using System;
using System.Collections.Generic;
using System.Text;
using UtilityTools.Core.Model;

namespace UtilityTools.Services.Interfaces.IServices
{
    /// <summary>
    /// 异步读写服务
    /// </summary>
    public interface IAsynRWService
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

        #endregion

        #region Event
        /// <summary>
        /// 更新回包数据事件
        /// </summary>
        event EventHandler<byte[]> UpdateResponse;
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
        /// 串口数据发送接口
        /// </summary>
        /// <param name="cmdCode">指令类型</param>
        /// <param name="cmd">指令参数</param>
        void SendMsg(byte[] cmd);

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
