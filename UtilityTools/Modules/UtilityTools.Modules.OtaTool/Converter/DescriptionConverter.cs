using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UtilityTools.Modules.OtaTool.Converter
{
    internal class DescriptionConverter : IValueConverter
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
            int? deviceID = (int?)value;
            string description = GetDataDescription(deviceID);
            return description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
        #endregion

        #region ------------PrivateMethod------------
        
        private string GetDataDescription(int? deviceID)
        {
            string deviceIDDescription;
            switch (deviceID)
            {
                case 0x0100: deviceIDDescription = "主控制板"; break;
                case 0x0101: deviceIDDescription = "真空控制板"; break;
                case 0x0102: deviceIDDescription = "灯带控制板"; break;
                case 0x0200: deviceIDDescription = "20kv高压箱"; break;
                case 0x0300: deviceIDDescription = "五轴电机控制板"; break;
                default: deviceIDDescription = "测试控制板"; break;
            }
            return deviceIDDescription;
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion
        }
    
}
