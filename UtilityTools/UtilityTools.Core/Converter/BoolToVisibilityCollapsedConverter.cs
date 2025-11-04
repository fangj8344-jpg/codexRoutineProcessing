using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace UtilityTools.Core.Converter
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityCollapsedConverter: IValueConverter
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
            if (value is bool b)
            {
                if (b )
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
