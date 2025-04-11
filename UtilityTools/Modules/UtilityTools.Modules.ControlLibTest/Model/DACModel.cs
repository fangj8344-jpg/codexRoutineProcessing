using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Modules.ControlLibTest.Model
{
    internal class DACModel : BindableBase
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


        private byte _channel;

        public byte Channel
        {
            get { return _channel; }
            set { _channel = value; RaisePropertyChanged(); }
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

        public Action<ushort>? SetValueFunc { get; set; }
        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
