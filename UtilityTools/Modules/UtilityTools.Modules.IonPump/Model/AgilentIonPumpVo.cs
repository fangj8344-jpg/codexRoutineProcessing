using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Modules.IonPump.Model
{
    public class AgilentIonPumpVo : BindableBase
    {
        #region  ------------Field------------
        #endregion


        # region ------------Property------------
        private string _status;
        public string Status
        {
            get { return _status; }
            set { _status = value; RaisePropertyChanged(); }
        }

        private string _errorCode;
        public string ErrorCode
        {
            get { return _errorCode; }
            set { _errorCode = value; RaisePropertyChanged(); }
        }


        private string _pressureUnit;
        public string PressureUnit
        {
            get { return _pressureUnit; }
            set { _pressureUnit = value; RaisePropertyChanged(); }
        }

        private string _maxPower;
        public string MaxPower
        {
            get { return _maxPower; }
            set { _maxPower = value; RaisePropertyChanged(); }
        }

        private string _temp;
        public string Temp
        {
            get { return _temp; }
            set { _temp = value; RaisePropertyChanged(); }
        }

        private string _voltage;
        public string Voltage
        {
            get { return _voltage; }
            set { _voltage = value; RaisePropertyChanged(); }
        }

        private string _pressure;
        public string Pressure
        {
            get { return _pressure; }
            set { _pressure = value; RaisePropertyChanged(); }
        }

        private string _current;
        public string Current
        {
            get { return _current; }
            set { _current = value; RaisePropertyChanged(); }
        }
        #endregion
    }
}
