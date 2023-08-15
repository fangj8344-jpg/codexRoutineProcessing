#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Services.Services
 * 唯一标识：04b93640-164e-4ed4-a415-d26a79b059b1
 * 文件名：TcpNetSyncService
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/13 11:12:12
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

using System;
using System.Collections.Generic;
using System.Text;
using UtilityTools.Core.Helper;
using UtilityTools.Core.Model;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Services.Services
{
    public class TcpNetSyncService : INetService, ISyncRWService
    {
        #region ------------Constructor------------
        #endregion

        #region ------------Field------------
        #endregion

        #region ------------Property------------
        public NetConfigModel DeviceInstance { get; set; }

        public bool IsOpen
        {
            get
            {
                if (DeviceInstance != null && DeviceInstance.Socket != null)
                {
                    return DeviceInstance.Socket.Connected;
                }
                return false;
            }
        }

        public string Name { get; set; }

        public bool IsBinary { get; set; }

        #endregion

        #region ------------PublicMethod------------

        public void Close()
        {
            throw new NotImplementedException();
        }

        public object GetHandle()
        {
            throw new NotImplementedException();
        }

        public bool Open()
        {
            throw new NotImplementedException();
        }

        public void Request(string cmdName, byte[] cmd, out byte[] response, out int length, int waitTime)
        {
            throw new NotImplementedException();
        }

        public void SetHandle(object obj)
        {
            throw new NotImplementedException();
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
                return Encoding.Default.GetString(cmd, 0, length);
            }
        }
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion

    }
}
