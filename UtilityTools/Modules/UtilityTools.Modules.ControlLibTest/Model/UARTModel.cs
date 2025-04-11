using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Modules.ControlLibTest.Model
{
    internal class UARTModel: BindableBase
    {
        #region ------------Constructor------------
        public UARTModel() {
            this.SetUartCommand = new DelegateCommand(SetUart);       
        }

        private void SetUart()
        {
            uint Value = Convert.ToUInt32(this.Value.Split(":")[1]);
            SetBaudRateFunc?.Invoke(this.Series, Value);
        }
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

        private string _value="";

        public string Value
        {
            get { return _value; }
            set { _value = value; RaisePropertyChanged(); }
        }

        private byte _Series;

        public byte Series
        {
            get { return _Series; }
            set { _Series = value; RaisePropertyChanged(); }
        }

        
        public DelegateCommand SetUartCommand { get; set; }

        public Action<byte,uint>? SetBaudRateFunc { get; set; }
        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
