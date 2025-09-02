using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Model
{
    public class SteadyCoolPIDModel : BindableBase
    {
        private float _steadySteadyCoolp;

        public float SteadySteadyCoolp
        {
            get { return _steadySteadyCoolp; }
            set { _steadySteadyCoolp = value; RaisePropertyChanged(); }
        }

        private float _steadySteadyCooli;

        public float SteadySteadyCooli
        {
            get { return _steadySteadyCooli; }
            set { _steadySteadyCooli = value; RaisePropertyChanged(); }
        }

        private float _steadySteadyCoold;

        public float SteadySteadyCoold
        {
            get { return _steadySteadyCoold; }
            set { _steadySteadyCoold = value; RaisePropertyChanged(); }
        }
    }
}
