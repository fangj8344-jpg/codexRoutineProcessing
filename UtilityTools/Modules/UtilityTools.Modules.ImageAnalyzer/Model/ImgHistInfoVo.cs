using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Modules.ImageAnalyzer.Model
{
    public class ImgHistInfoVo : BindableBase
    {
        public ImgHistInfoVo() { }

        # region ------------Field------------
        private int _height;
        private int _width;
        private int _channels;
        private int _depth;

        private int _pixelMax;
        private int _pixelMin;
        private int _pixelMode; // 众数
        private float _pixelMean;
        private int _pixelMedian;
        private int _pixelSpan;
        private float _pixelStd;
        private float _pixelVar;
        private float _pixelSpanPercentage;

        private string _desc;


        # endregion

        # region ------------Property------------
        public int Height
        {
            get { return _height; }
            set { _height = value; RaisePropertyChanged(); }
        }        
        
        public int Width
        {
            get { return _width; }
            set { _width = value; RaisePropertyChanged(); }
        }

        public int Channels
        {
            get { return _channels; }
            set { _channels = value; RaisePropertyChanged(); }
        }

        public int Depth
        {
            get { return _depth; }
            set { _depth = value; RaisePropertyChanged(); }
        }

        public int PixelMax
        {
            get { return _pixelMax; }
            set { _pixelMax = value; RaisePropertyChanged(); }
        }

        public int PixelMin
        {
            get { return _pixelMin; }
            set { _pixelMin = value; RaisePropertyChanged(); }
        }

        public float PixelMean
        {
            get { return _pixelMean; }
            set { _pixelMean = value; RaisePropertyChanged(); }
        }
        public int PixelMode
        {
            get { return _pixelMode; }
            set { _pixelMode = value; RaisePropertyChanged(); }
        }

        public int PixelMedian
        {
            get { return _pixelMedian; }
            set { _pixelMedian = value; RaisePropertyChanged(); }
        }

        public int PixelSpan
        {
            get { return _pixelSpan; }
            set { _pixelSpan = value; RaisePropertyChanged(); }
        }     
        
        public float PixelStd
        {
            get { return _pixelStd; }
            set { _pixelStd = value; RaisePropertyChanged(); }
        }     
        
        public float PixelVar
        {
            get { return _pixelVar; }
            set { _pixelVar = value; RaisePropertyChanged(); }
        }     
        public float PixelSpanPercentage
        {
            get { return _pixelSpanPercentage; }
            set { _pixelSpanPercentage = value; RaisePropertyChanged(); }
        }

        public string Desc
        {
            get { return _desc; }
            set { _desc = value; }
        }

        public double LeftX { get; internal set; }
        public double LeftY { get; internal set; }
        public double RightX { get; internal set; }
        public double RightY { get; internal set; }
        #endregion
    }
}
