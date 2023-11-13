using OpenCvSharp;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using UtilityTools.Core.Helper;
using UtilityTools.Modules.ImageAnalyzer.ViewModels;

namespace UtilityTools.Modules.ImageAnalyzer.Model
{
    public class ImgHistInfo
    {
        public ImgHistInfo(string path)
        {
            src = Cv2.ImRead(path, ImreadModes.Unchanged);
            int channels = src.Channels();
            Mat gray = src;
            if (channels != 1)
            {
                gray = new Mat();
                Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
            }

            hist = CvHelper.CalcGrayHist(gray);
            _binsCum = new List<int>();
            _points = new List<DataPoint>();
            graySum = gray.Sum()[0];

            var indexer = hist.GetGenericIndexer<float>();
            int c = 0;
            for (int i = 0; i < hist.Rows; i++)
            {
                float cnt = indexer[i, 0];
                if (cnt > 0)
                {
                    _points.Add(new DataPoint(i, cnt));
                    MaxY = Math.Max(MaxY, (int)cnt);
                    c += (int)cnt;
                    _binsCum.Add(c);
                }
            }

            nonezeros = _binsCum.Any() ? _binsCum.Last() : 0;
            MaxX = (int)_points.Last().X;
            MinX = (int)_points.First().X;
        }

        internal void SaveReportImage(string output)
        {
            int channels = src.Channels();
            Mat gray = src;
            if (channels != 1)
            {
                gray = new Mat();
                Cv2.CvtColor(src, gray, ColorConversionCodes.BGRA2BGR);
            }

            //Cv2.Normalize(gray, dst, 0, 1, NormTypes.MinMax); // 使用ConvertTo优化
            double dx = MaxX - MinX;
            var alpha = 255 / dx;
            var beta = -alpha * MinX;
            Mat dst = new Mat();
            gray.ConvertTo(dst, MatType.CV_8UC1, alpha, beta);
            //Cv2.CvtColor(dst, dst, ColorConversionCodes.GRAY2BGR); // 不压缩的话浪费空间了
            Cv2.ImWrite(output, dst);
        }

        internal ImgHistInfoVo GetHistInfo(int percentage)
        {
            int drop_cnt = (int)(nonezeros * (1 - percentage / 100.0) / 2); // 左右分别丢弃的点数

            (int left_idx, int n1, long s1, int right_idx, int n2, long s2) = CalcImgHistSpan(drop_cnt);
            var left_pt = _points[left_idx];
            var right_pt = _points[right_idx];

            // 从s中减掉多统计的数量
            s1 -= (long)((n1 - drop_cnt) * left_pt.X);
            s2 -= (long)((n2 - drop_cnt) * right_pt.X);
            int left_cnt = nonezeros - drop_cnt * 2;
            float mean = (float)(graySum - s1 - s2) / left_cnt; // 均值 = 总和 / 剩下点数
            var mode_pt = _points.GetRange(left_idx, right_idx - left_idx + 1).MaxBy(p => p.Y);

            // 方差
            double variance = 0;
            double v = 0;
            for (int i = left_idx + 1; i < right_idx; i++)
            {
                v = (_points[i].X - mean);
                variance += v * v * _points[i].Y;
            }

            v = left_pt.X - mean;
            variance += v * v * (n1 - drop_cnt);

            v = right_pt.X - mean;
            variance += v * v * (n2 - drop_cnt);
            variance /= (left_cnt - 1 + Double.Epsilon);


            // 中位数
            int median_idx = left_cnt / 2 + drop_cnt;
            int median_bin = BiSect.BiSectLeft(ref _binsCum, median_idx);
            var median_pt = _points[median_bin];

            int span = (int)(right_pt.X - left_pt.X);

            // 构造VO
            ImgHistInfoVo info = new ImgHistInfoVo();
            info.Height = src.Rows;
            info.Width = src.Cols;
            info.Channels = src.Channels();
            info.Depth = src.ElemSize1() * 8;
            info.PixelSpan = span;
            info.PixelSpanPercentage = percentage;
            info.Desc = $"{percentage}% 区间跨度 {span}";
            info.PixelMin = (int)left_pt.X;
            info.PixelMax = (int)right_pt.X;
            info.PixelMode = (int)mode_pt.X;
            info.PixelMean = mean;
            info.PixelMedian = (int)median_pt.X;
            info.PixelVar = (float)variance;
            info.PixelStd = (float)Math.Sqrt(variance);

            info.LeftX = left_pt.X;
            info.LeftY = left_pt.Y;
            info.RightX = right_pt.X;
            info.RightY = right_pt.Y;
            return info;
        }

        private (int, int, long, int, int, long) CalcImgHistSpan(int drop_cnt)
        {
            int left_idx = 0;
            int n1 = (int)_points[left_idx].Y;
            long s1 = (long)(_points[left_idx].Y * _points[left_idx].X);
            while (n1 < drop_cnt)
            {
                left_idx++;
                n1 += (int)_points[left_idx].Y;
                s1 += (long)(_points[left_idx].Y * _points[left_idx].X);
            }

            int right_idx = _points.Count - 1;
            int n2 = (int)_points[right_idx].Y;
            long s2 = (long)(_points[right_idx].Y * _points[right_idx].X);
            while (n2 < drop_cnt)
            {
                right_idx--;
                n2 += (int)_points[right_idx].Y;
                s2 += (long)(_points[right_idx].Y * _points[right_idx].X);
            }

            return (left_idx, n1, s1, right_idx, n2, s2);
        }

        private Mat src;
        private Mat hist;
        private int nonezeros;
        private double graySum;

        List<DataPoint> _points;
        List<int> _binsCum;
        public List<DataPoint> Points { get { return _points; } }
        public int MaxY { get; set; } = 0;
        public int MinX { get; set; } = 0;
        public int MaxX { get; set; } = 0;
    }
}
