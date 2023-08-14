#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Services.Services
 * 唯一标识：ae46e8f8-4b97-4f19-aada-c16515c311c4
 * 文件名：VirtualAsynService
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/13 11:14:29
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
    public class VirtualAsynService : IAsynRWService
    {
        #region ------------Constructor------------
        #endregion

        #region ------------Field------------
        #endregion

        #region ------------Property------------
        public bool IsOpen => throw new NotImplementedException();

        public string Name { get; set; }

        public bool IsBinary { get; set; }

        #endregion

        #region ------------Event------------
        public event EventHandler<byte[]> UpdateResponse;
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

        public void SendMsg(byte[] cmd)
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
