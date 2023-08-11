#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Attach
 * 唯一标识：971e41ad-1e46-4545-8e3f-9cb641f013dc
 * 文件名：TextBoxAttached
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/11 17:30:35
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace UtilityTools.Core.Attach
{
    public class TextBoxAttached : DependencyObject
    {
        #region IsEnterEndEdit
        public static bool GetIsEnterEndEdit(FrameworkElement item)
        {
            return (bool)item.GetValue(IsEnterEndEditProperty);
        }

        public static void SetIsEnterEndEdit(FrameworkElement item, bool value)
        {
            item.SetValue(IsEnterEndEditProperty, value);
        }

        /// <summary>
        /// 是否能取消选中 (启用此功能会占用 Tag 属性)
        /// </summary>
        public static readonly DependencyProperty IsEnterEndEditProperty =
            DependencyProperty.RegisterAttached(
                "IsEnterEndEdit",
                typeof(bool),
                typeof(TextBoxAttached),
                new UIPropertyMetadata(false, OnIsEnterEndEditChanged));


        static void OnIsEnterEndEditChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement item = depObj as FrameworkElement;

            if (item == null)
                return;

            switch (depObj)
            {
                case TextBox textBox:
                    {
                        if ((bool)e.NewValue)
                        {
                            textBox.KeyUp += TextBox_KeyUp;
                        }
                        else
                        {
                            textBox.KeyUp -= TextBox_KeyUp;
                        }


                        break;
                    }
                default:
                    break;
            }
        }

        private static void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (e.Key == Key.Enter)
            {
                BindingExpression exp = textBox.GetBindingExpression(TextBox.TextProperty);
                exp.UpdateSource();
                //textBox.BindingGroup.CommitEdit();
            }
        }

        #endregion

        #region IsOnlyNumbers
        public static bool GetIsOnlyNumbers(FrameworkElement item)
        {
            return (bool)item.GetValue(IsOnlyNumbersProperty);
        }

        public static void SetIsOnlyNumbers(FrameworkElement item, bool value)
        {
            item.SetValue(IsOnlyNumbersProperty, value);
        }

        /// <summary>
        /// 是否能取消选中 (启用此功能会占用 Tag 属性)
        /// </summary>
        public static readonly DependencyProperty IsOnlyNumbersProperty =
            DependencyProperty.RegisterAttached(
                "IsOnlyNumbers",
                typeof(bool),
                typeof(TextBoxAttached),
                new UIPropertyMetadata(false, OnIsOnlyNumbersChanged));


        static void OnIsOnlyNumbersChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement item = depObj as FrameworkElement;

            if (item == null)
                return;

            switch (depObj)
            {
                case TextBox textBox:
                    {
                        if ((bool)e.NewValue)
                        {
                            textBox.PreviewTextInput += TextBox_PreviewTextInput;
                        }
                        else
                        {
                            textBox.PreviewTextInput -= TextBox_PreviewTextInput;
                        }
                        break;
                    }
                default:
                    break;
            }
        }

        private static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Text) || !char.IsDigit(e.Text[0]))
            {
                e.Handled = true;
            }
        }

        #endregion

        #region IsSupportWheel
        public static bool GetIsSupportWheel(FrameworkElement item)
        {
            return (bool)item.GetValue(IsSupportWheelProperty);
        }

        public static void SetIsSupportWheel(FrameworkElement item, bool value)
        {
            item.SetValue(IsSupportWheelProperty, value);
        }

        // Using a DependencyProperty as the backing store for WheelEnable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSupportWheelProperty =
            DependencyProperty.RegisterAttached("IsSupportWheel", typeof(bool), typeof(TextBoxAttached), new UIPropertyMetadata(false, OnIsSupportWheelPropertyChanged));

        private static void OnIsSupportWheelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement item = d as FrameworkElement;

            if (item == null)
                return;

            switch (d)
            {
                case TextBox textBox:
                    {
                        if ((bool)e.NewValue)
                        {
                            textBox.MouseWheel += TextBox_MouseWheel;
                        }
                        else
                        {
                            textBox.MouseWheel -= TextBox_MouseWheel;
                        }

                        break;
                    }
                default:
                    break;
            }
        }

        private static void TextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            TextBox item = sender as TextBox;

            if (item == null)
                return;

            int curIndex = item.CaretIndex;
            if (curIndex == 0)
                curIndex = 1;
            int total = item.Text.Length;
            if (int.TryParse(item.Text, out int value))
            {
                value += (e.Delta / 120) * (int)Math.Pow(10, total - curIndex);
                item.Text = value.ToString();
                item.CaretIndex = curIndex;
                BindingExpression exp = item.GetBindingExpression(TextBox.TextProperty);
                exp.UpdateSource();
            }
        }

        #endregion
    }
}
