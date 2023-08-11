#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Converter
 * 唯一标识：a8d0d58b-2a4f-4d54-8267-6bf197efdae8
 * 文件名：StringToParityConverter
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/11 17:53:07
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
    [ValueConversion(typeof(Parity), typeof(string))]
    public class StringToParityConverter : IValueConverter
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
            if (value is Parity parity)
            {
                switch (parity)
                {
                    case Parity.Odd:
                        return "奇校验";
                    case Parity.Even:
                        return "偶校验";
                    case Parity.Mark:
                        return "恒1";
                    case Parity.Space:
                        return "恒0";
                }
            }
            return "无";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is String parity)
            {
                switch (parity)
                {
                    case "奇校验":
                        return Parity.Odd;
                    case "偶校验":
                        return Parity.Even;
                    case "恒1":
                        return Parity.Mark;
                    case "恒0":
                        return Parity.Space;
                }
            }
            return Parity.None;

        }
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
