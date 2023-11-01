using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace UtilityTools.Core.Converter
{
    public class Obj2BoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool returnValue = true;
            if (value == DependencyProperty.UnsetValue || value == null)
            {
                returnValue = false;
            }
            return returnValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}