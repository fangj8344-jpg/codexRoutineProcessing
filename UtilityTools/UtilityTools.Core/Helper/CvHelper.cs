using OpenCvSharp;
using System;

namespace UtilityTools.Core.Helper
{
    public static class CvHelper
    {
        public static Mat CalcGrayHist(Mat gray, int histWindowSize)
        {
            int depth = gray.ElemSize1() * 8;
            int bins = 1 << depth;
            Mat hist = new Mat();
            Mat result = new Mat();
            Cv2.CalcHist(new Mat[] { gray }, new int[] { 0 }, null, hist, 1, new int[] { bins }, new Rangef[] { new Rangef(0, bins) }, true, true);
            Mat kernel = new Mat(histWindowSize, 1, hist.Type(), 1.0 / histWindowSize);
            Cv2.Filter2D(hist, result, hist.Type(), kernel);
            return result;
        }

        public static Mat CalcSpectrum(Mat gray)
        {
            int n = gray.Rows;
            int fft_size = Cv2.GetOptimalDFTSize(n);
            using var dft_in = new Mat();
            gray.ConvertTo(dft_in, MatType.CV_32FC1);
            Mat[] planes = { dft_in, Mat.Zeros(dft_in.Size(), MatType.CV_32F) };

            using var complex = new Mat();
            Cv2.Merge(planes, complex);

            using var dft = new Mat();
            Cv2.Dft(complex, dft, DftFlags.Rows | DftFlags.ComplexOutput);

            Cv2.Split(dft, out var dftPlanes);
            using var magnitude = new Mat();
            Cv2.Magnitude(dftPlanes[0], dftPlanes[1], magnitude);

            Mat spectrum = magnitude / (fft_size / 2.0);

            //spectrum = 20*log10(spectrum / 4096)  // DBFS
            //Mat spectrum = magnitude + Scalar.All(1);
            //Cv2.Log(spectrum, spectrum); // Cv2.Log = ln
            //spectrum /= Math.Log(10) * 20;

            // fftshift
            int cx = spectrum.Cols / 2;
            using var q0 = new Mat(spectrum, new Rect(0, 0, cx, spectrum.Rows));
            using var q1 = new Mat(spectrum, new Rect(cx, 0, cx, spectrum.Rows));
            using var tmp = new Mat();
            q0.CopyTo(tmp);
            q1.CopyTo(q0);
            tmp.CopyTo(q1);

            return spectrum;
        }
    }
}
