#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.ControlLibTest.Model
 * 唯一标识：f2e5b2e5-4070-4382-b59f-2adfbf84fb87
 * 文件名：VacModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/2/26 16:40:21
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

namespace UtilityTools.Modules.ControlLibTest.Model
{
    internal class VacModel : BindableBase
    {
        #region ------------Constructor------------
        #endregion

        #region ------------Field------------
        #endregion

        #region ------------Property------------
        private float _value1;

        public float Value1
        {
            get { return _value1; }
            set { _value1 = value; RaisePropertyChanged(); }
        }

        private float _value2;

        public float Value2
        {
            get { return _value2; }
            set { _value2 = value; RaisePropertyChanged(); }
        }

        private float _value3;

        public float Value3
        {
            get { return _value3; }
            set { _value3 = value; RaisePropertyChanged(); }
        }

        private float _value4;

        public float Value4
        {
            get { return _value4; }
            set { _value4 = value; RaisePropertyChanged(); }
        }

        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
