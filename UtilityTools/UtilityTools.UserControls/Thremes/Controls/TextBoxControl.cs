#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.UserControls.Thremes.Controls
 * 唯一标识：e7f6ac12-9a5d-4419-9578-14fc2e7b726c
 * 文件名：TextBoxControl
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/1/9 15:17:04
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
using System.Windows;

namespace UtilityTools.UserControls.Thremes.Controls
{
    public class PlaceHoldTextBox : TextBox
    {
        static PlaceHoldTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PlaceHoldTextBox), new FrameworkPropertyMetadata(typeof(PlaceHoldTextBox)));
        }

        /// <summary>
        /// 为空时的占位提示语句
        /// </summary>
        public string PlaceHold
        {
            get { return (string)GetValue(PlaceHoldProperty); }
            set { SetValue(PlaceHoldProperty, value); }
        }
        public static readonly DependencyProperty PlaceHoldProperty =
            DependencyProperty.Register("PlaceHold", typeof(string), typeof(PlaceHoldTextBox), new PropertyMetadata(null));

        /// <summary>
        /// 外框圆弧半径
        /// </summary>
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(PlaceHoldTextBox), new PropertyMetadata(null));
    }
}
