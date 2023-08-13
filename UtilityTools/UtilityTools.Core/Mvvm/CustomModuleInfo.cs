#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Mvvm
 * 唯一标识：f91a4d7d-ca09-4d6e-a70d-379623fbff75
 * 文件名：CustomModuleInfo
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/13 9:34:20
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Mvvm
{
    public class CustomModuleInfo : ModuleInfo
    {
        #region ------------Constructor------------
        public CustomModuleInfo() : base() { }

        public CustomModuleInfo(string name, string type, params string[] dependsOn)
            : base(name, type, dependsOn) { }

        public CustomModuleInfo(string name, string type)
            : base(name, type) { }

        public CustomModuleInfo(Type moduleType)
            : base(moduleType) { }

        public CustomModuleInfo(Type moduleType, string moduleName)
            : base(moduleType, moduleName) { }

        public CustomModuleInfo(Type moduleType, string moduleName, InitializationMode initializationMode)
            : base(moduleName, moduleType.AssemblyQualifiedName) { }
        #endregion

        #region ------------Field------------
        #endregion

        #region ------------Property------------
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
