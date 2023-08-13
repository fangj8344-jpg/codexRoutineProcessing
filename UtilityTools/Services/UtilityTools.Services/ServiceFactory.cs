#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Services
 * 唯一标识：58e4cf69-25b7-481c-bd73-603fb2c0d660
 * 文件名：ServiceFactory
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/13 11:16:16
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
using UtilityTools.Services.Interfaces;
using UtilityTools.Services.Interfaces.IServices;

namespace UtilityTools.Services
{
    public class ServiceFactory : IServiceFactory
    {
        #region ------------Constructor------------
        #endregion

        #region ------------Field------------
        #endregion

        #region ------------Property------------
        #endregion

        #region ------------PublicMethod------------
        public IAsynRWService GetAsynRWDevice(string type)
        {
            throw new NotImplementedException();
        }

        public ICameraService GetCameraDevice(string type)
        {
            throw new NotImplementedException();
        }

        public ISyncRWService GetSyncRWDevice(string type)
        {
            throw new NotImplementedException();
        }

        public IUsbService GetUsbDevice(string type)
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
