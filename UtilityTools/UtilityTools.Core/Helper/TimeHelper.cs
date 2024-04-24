#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Helper
 * 唯一标识：bdb21b7e-1994-44a5-ad5b-ab073d27178c
 * 文件名：TimeHelper
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/3/21 16:38:41
 * 版本：V1.0.0
 * 描述：
 *
 * ----------------------------------------------------------------
 * 修改人：
 * 时间：
 * 修改说明：
 *
 * 版本：V1.0.1
 *----------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Helper
{
    public static class TimeHelper
    {
        public static DateTime GetTimeFromLabview(double time)
        {
            long years = (1970 - 1904);
            long leapYears = years / 4 + 1;
            long difVal = (years - leapYears) * 365 * 24 * 60 * 60 + leapYears * 366 * 24 * 60 * 60 - 8 * 60 * 60;

            double realTimestamp = time - difVal;
            return DateTimeOffset.FromUnixTimeSeconds((long)realTimestamp).DateTime;
        }
    }
}
