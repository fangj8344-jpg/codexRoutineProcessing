using DryIoc;
using OpenCvSharp;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using UtilityTools.Core.Helper;


namespace UtilityTools.Modules.ImageAnalyzer.Model
{
    class ImgSpectrumInfo
    {
        public ImgSpectrumInfo(string path, bool is_column_dft = false)
        {
            var _meta = ImageMetadata.ReadMetadata(path);
            float sample_freq = _meta.FreqADC * 1.0f / _meta.AvgLength;

            _is_column_dft = is_column_dft;
            src = Cv2.ImRead(path, ImreadModes.Unchanged);
            int channels = src.Channels();
            Mat gray = src;
            if (channels != 1)
            {
                gray = new Mat();
                Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
            }

            if (_meta.IsSignificantBitValid())
            {
                var sbp = new SignificantBitParser(_meta.SignificantBit);
                gray &= sbp.SignificantMask;
                gray /= Math.Pow(2, sbp.SignificantLow);
            }

            if (_is_column_dft)
            {
                gray = gray.T();
            }

            spectrum = CvHelper.CalcSpectrum(gray);
            spectrum_mean = spectrum.Reduce(ReduceDimension.Row, ReduceTypes.Avg, (int)MatType.CV_32FC1);

            _points = new List<DataPoint>();
            int n = src.Rows;

            if (_is_column_dft)
            {
                sample_freq /= n;
            }

            _freq_resolution = sample_freq / n;
            List<float> freqs = Enumerable.Range(-n / 2, n).Select(i => i * _freq_resolution).ToList();
            var indexer = spectrum_mean.GetGenericIndexer<float>();
            for (int i = 0; i < freqs.Count; i++)
            {
                if (freqs[i] > 0)
                {
                    _points.Add(new DataPoint(freqs[i], indexer[0, i]));
                }
            }

            Mat graySpectrum = new Mat(spectrum, new Rect(n / 2, 0, n / 2, n));
            Cv2.Log(graySpectrum + Scalar.All(1), graySpectrum);
            Cv2.Normalize(graySpectrum, graySpectrum, 0, 255, NormTypes.MinMax);
            graySpectrum.ConvertTo(graySpectrum, MatType.CV_8U);

            _spectrumColor = new Mat();
            Cv2.ApplyColorMap(graySpectrum.T(), _spectrumColor, ColormapTypes.Jet);
            _spectrumColor = _spectrumColor.Flip(FlipMode.X);
        }

        private bool _is_column_dft;
        private Mat src;
        private Mat spectrum;
        private Mat spectrum_mean;
        private Mat _spectrumColor;

        private float _freq_resolution;
        public float FreqResolution { get { return _freq_resolution; } }

        List<DataPoint> _points;
        public List<DataPoint> Points { get { return _points; } }

        public Mat SpectrumColor { get { return _spectrumColor; } }

        public bool IsColumnDFT { get { return _is_column_dft; } }
    }
}
