using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Modules.ImageAnalyzer.Model
{
    public class SpectrumPeakVo : BindableBase
    {
        public SpectrumPeakVo() { }

        private int _id;
        private double _freq;
        private double _magnitude;

        public int ID
        {
            get { return _id; }
            set { _id = value; RaisePropertyChanged(); }
        }

        public double Freq
        {
            get { return _freq; }
            set { _freq = value; RaisePropertyChanged(); }
        }

        public double Magnitude
        {
            get { return _magnitude; }
            set { _magnitude = value; RaisePropertyChanged(); }
        }

    }
}
