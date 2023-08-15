#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Mvvm
 * 唯一标识：5812a21d-b717-4ece-bd00-525a374db578
 * 文件名：CustomBindableBase
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/15 10:50:45
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

using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Mvvm
{
    public class CustomBindableBase : BindableBase
    {
        #region ------------Constructor------------
        #endregion

        #region ------------Field------------
        #endregion

        #region ------------Property------------
        /// <summary>
        /// 属性变更事件回调函数
        /// </summary>
        public EventHandler PropertyChangedHandle { get; set; }
        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
