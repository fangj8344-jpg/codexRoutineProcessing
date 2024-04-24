#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.ControlLibTest.Model
 * 唯一标识：1b5aa446-d4a4-451f-aea3-6f85598a2b12
 * 文件名：SeModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/2/26 16:57:29
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
    internal class SeModel : BindableBase
    {
        #region ------------Constructor------------
        #endregion

        #region ------------Field------------
        #endregion

        #region ------------Property------------
        private ushort _value = 0;

        public ushort Value
        {
            get { return _value; }
            set { _value = value; RaisePropertyChanged(); }
        }

        private ushort _minValue = 0;

        public ushort MinValue
        {
            get { return _minValue; }
            set { _minValue = value; RaisePropertyChanged(); }
        }

        private ushort _maxValue = 0xFFFF;

        public ushort MaxValue
        {
            get { return _maxValue; }
            set { _maxValue = value; RaisePropertyChanged(); }
        }

        private bool _enable = false;

        public bool Enable
        {
            get { return _enable; }
            set { _enable = value; RaisePropertyChanged(); }
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
