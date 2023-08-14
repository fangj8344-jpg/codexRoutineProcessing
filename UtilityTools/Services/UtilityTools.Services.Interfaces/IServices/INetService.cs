#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Services.Interfaces.IServices
 * 唯一标识：620baf93-7619-4666-b111-ab7d6c33778e
 * 文件名：INetDevice
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/13 10:52:24
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
using UtilityTools.Core.Model;

namespace UtilityTools.Services.Interfaces.IServices
{
    public interface INetService
    {
        NetConfigModel DeviceInstance
        {
            get;
            set;
        }
    }
}
