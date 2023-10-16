using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Helper
{
    public static class BiSect
    {
        public static int BiSectLeft(ref List<int> nums, int x)
        {
            int lo = 0, hi = nums.Count;
            while (lo < hi)
            {
                int mid = lo + (hi - lo) / 2;
                if (nums[mid] < x)
                {
                    lo = mid + 1;
                }
                else
                {
                    hi = mid;
                }
            }
            return lo;
        }
    }
}
