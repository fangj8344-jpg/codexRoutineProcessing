#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Converter
 * 唯一标识：7f7ae5e5-ad06-4c12-be2f-0e217abb32e4
 * 文件名：IntToBitBoolConverter
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/11 16:39:27
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UtilityTools.Core.Converter
{
    [ValueConversion(typeof(int), typeof(bool))]
    public class IntToBitBoolConverter : IValueConverter
    {
        #region ------------Constructor------------
        #endregion

        #region ------------Field------------
        int _mask = 0;
        #endregion

        #region ------------Property------------
        #endregion

        #region ------------PublicMethod------------
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int mask = 0, index = 0;
            if (!int.TryParse(value.ToString(), out mask) || !int.TryParse(parameter.ToString(), out index))
            {
                return false;
            }
            _mask = mask;
            return GetIndexBitToBool(mask, index);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool enable = false;
            int index = 0;

            if (!bool.TryParse(value.ToString(), out enable) || !int.TryParse(parameter.ToString(), out index))
            {
                return _mask;
            }

            return SetIndexBitToBool(_mask, index, enable);
        }
        #endregion

        #region ------------PrivateMethod------------
        private bool GetIndexBitToBool(int mask, int index)
        {
            return ((mask >> index) & 1) == 1;
        }

        private int SetIndexBitToBool(int number, int index, bool value)
        {
            // 将位运算中的位置转换为相应的掩码
            int mask = 1 << index;

            if (value)
            {
                // 使用按位或运算符将特定位设置为 1
                int result = number | mask;
                return result;
            }
            else
            {
                // 使用按位与运算符将特定位设置为 0
                int result = number & ~mask;
                return result;
            }
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion

    }
}
