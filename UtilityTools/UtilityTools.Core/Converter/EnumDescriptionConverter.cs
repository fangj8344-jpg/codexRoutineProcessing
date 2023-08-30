#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Converter
 * 唯一标识：a919714b-9ef3-4486-b5aa-c03feb04accd
 * 文件名：EnumDescriptionConverter
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/24 17:22:00
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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace UtilityTools.Core.Converter
{
    public class EnumDescriptionConverter : IValueConverter
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
            Enum myEnum = (Enum)value;
            string description = GetEnumDescription(myEnum);
            return description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
        #endregion

        #region ------------PrivateMethod------------
        private string GetEnumDescription(Enum enumObj)
        {
            FieldInfo fieldInfo = enumObj.GetType().GetField(enumObj.ToString());
            var descriptionAttr = fieldInfo
                .GetCustomAttributes(false)
                .OfType<DescriptionAttribute>()
                .Cast<DescriptionAttribute>()
                .SingleOrDefault();
            if (descriptionAttr == null)
            {
                return enumObj.ToString();
            }
            else
            {
                return descriptionAttr.Description;
            }
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
