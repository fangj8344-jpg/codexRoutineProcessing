using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Helper
{
    public class StrHelper
    {
        public static string HexStr(byte[] bytes, string sep = " ")
        {
            return string.Join(sep, bytes.Select(x => x.ToString("X2")));
        }
    }
}
