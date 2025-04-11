#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.ControlLibTest.Model
 * 唯一标识：c92c002e-582b-40ed-b527-220c7285c789
 * 文件名：BseModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/2/26 16:42:38
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
    internal class BseModel : BindableBase
    {
        #region ------------Constructor------------
        #endregion

        #region ------------Field------------
        #endregion

        #region ------------Property------------
        private string _name = "";

        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(); }
        }

        private ushort _value;

        public ushort Value
        {
            get { return _value; }
            set { _value = value; RaisePropertyChanged(); }
        }

        private ushort _minValue;

        public ushort MinValue
        {
            get { return _minValue; }
            set { _minValue = value; RaisePropertyChanged(); }
        }

        private ushort _maxValue;

        public ushort MaxValue
        {
            get { return _maxValue; }
            set { _maxValue = value; RaisePropertyChanged(); }
        }

        private bool _positive;

        public bool Positive
        {
            get { return _positive; }
            set { _positive = value; RaisePropertyChanged(); }
        }

        private bool _negative;

        public bool Negative
        {
            get { return _negative; }
            set { _negative = value; RaisePropertyChanged(); }
        }

        private byte _channel;

        public byte Channel
        {
            get { return _channel; }
            set { _channel = value; RaisePropertyChanged(); }
        }

        public Action<ushort>? SetValueFunc { get; set; }

        public Action<bool>? SetNegStateFunc { get; set; }

        public Action<bool>? SetPosStateFunc { get; set; }
        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
