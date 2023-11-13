using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Helper
{
    public class PeakFinder1D
    {
        // 参考scipy的实现 scipy\signal\_peak_finding_utils.pyx

        public PeakFinder1D(List<double> nums)
        {
            this.x = nums;
        }
        public List<int> FindPeaks(int wlen = 0, int topk = -1)
        {
            FindLocalMaxima1D();
            PeakProminences(wlen);

            if (0 < topk)
            {
                int n = prominences.Count;
                var idx = Enumerable.Range(0, n).ToArray();
                Array.Sort(idx, (a, b) => prominences[b].CompareTo(prominences[a]));
                List<int> ret = new List<int>();
                int i = 0;
                while (i < topk && i < n)
                {
                    ret.Add(midpoints[idx[i++]]);
                }
                return ret;
            }
            else
            {
                return midpoints;
            }
        }


        private void PeakProminences(int wlen)
        {
            var peaks = midpoints;
            left_bases = Enumerable.Repeat(0, peaks.Count()).ToList();
            right_bases = Enumerable.Repeat(0, peaks.Count()).ToList();
            prominences = Enumerable.Repeat(0.0f, peaks.Count()).ToList();

            for (int peak_nr = 0; peak_nr < peaks.Count(); peak_nr++)
            {
                int peak = peaks[peak_nr];
                int i_min = 0;
                int i_max = x.Count() - 1;

                if (2 <= wlen)
                {
                    // 如果设置了窗长，在peak两边各一半范围内搜索
                    i_min = Math.Max(i_min, peak - wlen / 2);
                    i_max = Math.Max(i_max, peak + wlen / 2);
                }

                int i = peak;
                left_bases[peak_nr] = peak;
                double left_min = x[peak];
                while (i_min <= i && x[i] <= x[peak])
                {
                    if (x[i] < left_min)
                    {
                        left_min = x[i];
                        left_bases[peak_nr] = i;
                    }
                    i--;
                }


                i = peak;
                right_bases[peak_nr] = peak;
                double right_min = x[peak];
                while (i <= i_max && x[i] <= x[peak])
                {
                    if (x[i] < right_min)
                    {
                        right_min = x[i];
                        right_bases[peak_nr] = i;
                    }
                    i++;
                }

                prominences[peak_nr] = (float)(x[peak] - Math.Max(left_min, right_min));
            }
        }

        private void FindLocalMaxima1D()
        {
            midpoints = Enumerable.Repeat(0, x.Count() / 2).ToList();
            left_edges = Enumerable.Repeat(0, x.Count() / 2).ToList();
            right_edges = Enumerable.Repeat(0, x.Count() / 2).ToList();

            int m = 0;
            int i = 1;
            int i_max = x.Count() - 1;

            while (i < i_max)
            {
                if (x[i - 1] < x[i])
                {
                    var i_ahead = i + 1;
                    while (i_ahead < i_max && x[i_ahead] == x[i])
                    {
                        i_ahead++;
                    }

                    if (x[i_ahead] < x[i])
                    {
                        left_edges[m] = i;
                        right_edges[m] = i_ahead - 1;
                        midpoints[m] = (left_edges[m] + right_edges[m]) / 2;
                        m++;
                        i = i_ahead;
                    }
                }

                i++;
            }

            midpoints.RemoveRange(m, midpoints.Count - m);
            left_edges.RemoveRange(m, left_edges.Count - m);
            right_edges.RemoveRange(m, right_edges.Count - m);
        }


        private readonly List<double> x;

        private List<int> midpoints;
        private List<int> left_edges;
        private List<int> right_edges;

        private List<int> left_bases;
        private List<int> right_bases;
        private List<float> prominences;
    }
}
