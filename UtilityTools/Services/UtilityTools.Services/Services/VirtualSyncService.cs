#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Services.Services
 * 唯一标识：d1fa5d6e-2e3a-4195-bbff-3887e63c700c
 * 文件名：VirtualSyncService
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/13 11:15:17
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
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Services.Services
{
    public class VirtualSyncService : ISyncRWService
    {
        #region ------------Constructor------------
        #endregion

        #region ------------Field------------
        #endregion

        #region ------------Property------------
        public bool IsOpen => throw new NotImplementedException();

        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsBinary { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


        /// <summary>
        /// 两次写入最小间隔 ms
        /// </summary>
        public int MinWriteInterval { get; set; }

        /// <summary>
        /// 连接测试
        /// </summary>
        public DelegateConnectTestCommand ConnectTest { get; set; }
        #endregion

        #region ------------PublicMethod------------

        public void Close()
        {
            throw new NotImplementedException();
        }

        public string GetCmdString(byte[] cmd, int length)
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
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion

    }
}
