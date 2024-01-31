using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Services.Interfaces.IServices
{
    public delegate bool DelegateConnectTestCommand(IBaseService service);

    public interface IBaseService
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

        /// <summary>
        /// 两次写入最小间隔 ms, 特别是串口通讯需要根据设备情况进行设置
        /// </summary>
        int MinWriteInterval { get; set; }

        /// <summary>
        /// 连接测试
        /// </summary>
        public DelegateConnectTestCommand ConnectTest { get; set; }
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
        /// 获取服务句柄
        /// </summary>
        /// <returns></returns>
        object GetHandle();

        /// <summary>
        /// 设置句柄
        /// </summary>
        /// <param name="obj"></param>
        void SetHandle(object obj);

        /// <summary>
        /// 获取指令字符串，用于日志或者打印信息
        /// </summary>
        /// <returns></returns>
        string GetCmdString(byte[] cmd, int length);
        #endregion
    }
}
