using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using static OpenCvSharp.Stitcher;
using static UtilityTools.Modules.FDC12CHVBox.Protocol.FDC12CHVBoxProtocol;

namespace UtilityTools.Modules.FDC12CHVBox.Converter
{
    [ValueConversion(typeof(FDC12CHVBoxInitState), typeof(System.Windows.Media.Brush))]
    public class StatusToColorConverter : IValueConverter
    {
        // 从枚举转换到颜色
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not FDC12CHVBoxInitState status)
            {
                // 如果值不是预期的枚举类型，可以返回一个默认颜色或抛出异常
                return System.Windows.Media.Brushes.Transparent;
            }

            // 根据枚举值返回对应的颜色
            return status switch
            {
                // 修正后的颜色配置（推荐）
                FDC12CHVBoxInitState.IDLE => System.Windows.Media.Brushes.Gray,        // 空闲状态：灰色（无活动，待命）
                FDC12CHVBoxInitState.ERROR => System.Windows.Media.Brushes.Red,        // 错误状态：红色（警示，异常）
                FDC12CHVBoxInitState.INITALIZING => System.Windows.Media.Brushes.Orange, // 初始化中：橙色（进行中，过渡状态）
                FDC12CHVBoxInitState.RUNNUNG => System.Windows.Media.Brushes.Green,    // 运行中：绿色（正常，活跃状态）
                FDC12CHVBoxInitState.DISCONNECTED => System.Windows.Media.Brushes.DarkRed, // 未连接：深红色（严重异常，连接中断）
                _ => System.Windows.Media.Brushes.Transparent // 默认情况黑色
            };
        }
        

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
