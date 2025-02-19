using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UtilityTools.Modules.PacketBinTool.Converter
{
    internal class ToHex : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return "0x" + System.Convert.ToInt32(value).ToString("X");
            }
            catch(Exception) 
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return Int32.Parse(value.ToString().Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
            }
            catch(Exception) 
            {
                return value;
            }
        }
    }
}
