using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace UtilityTools.Core.Helper
{
    public static class CvHelper
    {
        public static Mat CalcGrayHist(Mat gray)
        {
            int depth = gray.ElemSize1() * 8;
            int bins = 1 << depth;
            Mat hist = new Mat();
            Cv2.CalcHist(new Mat[] { gray }, new int[] {0}, null, hist, 1, new int[] { bins }, new Rangef[] { new Rangef(0, bins) });
            return hist;
        }
    }
}
