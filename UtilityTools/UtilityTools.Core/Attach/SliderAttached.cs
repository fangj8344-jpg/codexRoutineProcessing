#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Attach
 * 唯一标识：46020209-ccf8-44d6-95ca-868eafb4dd8b
 * 文件名：SliderAttached
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/5/21 9:59:05
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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace UtilityTools.Core.Attach
{
    public class SliderAttached : DependencyObject
    {
        public static int GetWheelScale(FrameworkElement item)
        {
            return (int)item.GetValue(WheelScaleProperty);
        }

        public static void SetWheelScale(FrameworkElement item, int value)
        {
            item.SetValue(WheelScaleProperty, value);
        }


        /// <summary>
        /// 是否能取消选中 (启用此功能会占用 Tag 属性)
        /// </summary>
        public static readonly DependencyProperty WheelScaleProperty =
            DependencyProperty.RegisterAttached(
                "WheelScale",
                typeof(int),
                typeof(SliderAttached),
                new UIPropertyMetadata(5));

        #region IsSupportWheel
        public static bool GetIsSupportWheel(FrameworkElement item)
        {
            return (bool)item.GetValue(IsSupportWheelProperty);
        }

        public static void SetIsSupportWheel(FrameworkElement item, bool value)
        {
            item.SetValue(IsSupportWheelProperty, value);
        }


        /// <summary>
        /// 是否能取消选中 (启用此功能会占用 Tag 属性)
        /// </summary>
        public static readonly DependencyProperty IsSupportWheelProperty =
            DependencyProperty.RegisterAttached(
                "IsSupportWheel",
                typeof(bool),
                typeof(SliderAttached),
                new UIPropertyMetadata(false, OnIsSupportWheelChanged));


        static void OnIsSupportWheelChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement item = depObj as FrameworkElement;

            if (item == null)
                return;

            switch (depObj)
            {
                case Slider slider:
                    {
                        if ((bool)e.NewValue)
                        {
                            slider.PreviewMouseWheel += Slider_MouseWheel;
                        }
                        else
                        {
                            slider.PreviewMouseWheel -= Slider_MouseWheel;
                        }

                        break;
                    }
                default:
                    break;
            }
        }

        private static void Slider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                int factor = GetWheelScale(slider);
                slider.Value += (e.Delta * factor / 120);
            }

            e.Handled = true;
        }

        #endregion

        #region ValueChangedCommand
        public static ICommand GetValueChangedCommand(FrameworkElement item)
        {
            return (ICommand)item.GetValue(ValueChangedCommandProperty);
        }

        public static void SetValueChangedCommand(FrameworkElement item, ICommand value)
        {
            item.SetValue(ValueChangedCommandProperty, value);
        }

        /// <summary>
        /// 是否能取消选中 (启用此功能会占用 Tag 属性)
        /// </summary>
        public static readonly DependencyProperty ValueChangedCommandProperty =
            DependencyProperty.RegisterAttached(
                "ValueChangedCommand",
                typeof(ICommand),
                typeof(SliderAttached),
                new UIPropertyMetadata(null, OnValueChangedCommandPropertyChanged));

        private static void OnValueChangedCommandPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement item = depObj as FrameworkElement;

            if (item == null)
                return;

            switch (depObj)
            {
                case Slider slider:
                    {
                        slider.AddHandler(Slider.ValueChangedEvent, new RoutedEventHandler((sender, args) =>
                        {
                            var command = GetValueChangedCommand(slider);
                            if (command != null && command.CanExecute(slider.Value))
                            {
                                command.Execute(slider.Value);
                            }
                        }));
                        break;
                    }
                default:
                    break;
            }
        }

        #endregion
    }
}
