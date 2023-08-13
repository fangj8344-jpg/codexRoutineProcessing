#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Mvvm
 * 唯一标识：b834ddf6-f04b-46a0-8c74-a88f8a185b99
 * 文件名：CustomModuleAttribute
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/13 9:40:11
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

using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Mvvm
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CustomModuleAttribute : Attribute
    {
        #region ------------Constructor------------
        public CustomModuleAttribute()
        {
            ModuleName = "";
            OnDemand = true;
            Title = "";
            Tip = "";
            Icon = "";
        }
        #endregion

        #region ------------Field------------
        #endregion

        #region ------------Property------------
        //
        // 摘要:
        //     Gets or sets the name of the module.
        //
        // 值:
        //     The name of the module.
        public string ModuleName { get; set; }

        //
        // 摘要:
        //     Gets or sets the value indicating whether the module should be loaded OnDemand.
        public bool OnDemand { get; set; }

        public string Title { get; set; }
        public string Tip { get; set; }
        public string Icon { get; set; }

        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
