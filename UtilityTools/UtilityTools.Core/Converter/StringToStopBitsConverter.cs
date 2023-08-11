#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Converter
 * 唯一标识：ba3448de-84bb-45cb-9ae4-f47471bd7b12
 * 文件名：StringToStopBitsConverter
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/11 17:53:36
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
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UtilityTools.Core.Converter
{
    [ValueConversion(typeof(StopBits), typeof(string))]
    public class StringToStopBitsConverter : IValueConverter
    {
        #region ------------Constructor------------
        #endregion

        #region ------------Field------------
        #endregion

        #region ------------Property------------
        #endregion

        #region ------------PublicMethod------------
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StopBits stopBits)
            {
                switch (stopBits)
                {
                    case StopBits.One:
                        return "1";
                    case StopBits.Two:
                        return "2";
                    case StopBits.OnePointFive:
                        return "1.5";
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is String stopBits)
            {
                switch (stopBits)
                {
                    case "1":
                        return StopBits.One;
                    case "2":
                        return StopBits.Two;
                    case "1.5":
                        return StopBits.OnePointFive;
                }
            }
            return StopBits.None;

        }
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
