using OpenCvSharp;
using System;

namespace UtilityTools.Core.Helper
{
    public static class CvHelper
    {
        public static Mat CalcGrayHist(Mat gray)
        {
            int depth = gray.ElemSize1() * 8;
            int bins = 1 << depth;
            Mat hist = new Mat();
            Cv2.CalcHist(new Mat[] { gray }, new int[] { 0 }, null, hist, 1, new int[] { bins }, new Rangef[] { new Rangef(0, bins) });
            return hist;
        }

        public static Mat CalcSpectrum(Mat gray)
        {
            int n = gray.Rows;
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

            //Mat spectrum = magnitude.Clone(); // 幅值

            Mat spectrum = magnitude / 4096.0;  // DBLSB


            //Mat spectrum = magnitude / 4096.0;  // DBFS
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
