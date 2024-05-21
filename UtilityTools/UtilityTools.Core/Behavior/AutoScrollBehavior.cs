#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Behavior
 * 唯一标识：1e64adba-6c1d-4db9-a252-ca815f91f9d0
 * 文件名：AutoScrollBehavior
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/5/21 14:10:04
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

using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UtilityTools.Core.Behavior
{
    public class AutoScrollBehavior : Behavior<ListView>
    {
        ScrollViewer _scrollViewer = null;
        bool _autoScroll = true;

        protected override void OnAttached()
        {
            base.OnAttached();
            var listView = this.AssociatedObject;

            if (listView != null)
            {
                //listView.SelectionChanged += new SelectionChangedEventHandler(AssociatedObject_SelectionChanged);
                listView.Loaded += ListView_Loaded;
            }
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            var listView = sender as ListView;

            if (_scrollViewer == null)
            {
                _scrollViewer = FindSimpleVisualChild<ScrollViewer>(listView);
                if (_scrollViewer != null)
                {
                    _scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                }
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollView = sender as ScrollViewer;

            if(scrollView == null) 
            {
                return;
            }

            double dVer = scrollView.VerticalOffset;
            double vViewport = scrollView.ViewportHeight;
            double eextent = scrollView.ExtentHeight;

            if(eextent == 0) 
            {
                return;
            }

            _autoScroll = eextent - dVer - vViewport < 3;

            if(_autoScroll) 
            {
                scrollView.Dispatcher.BeginInvoke((Action)delegate
                {
                    scrollView.ScrollToBottom();
                });
            }
        }

        void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = sender as ListView;

            if (listView != null)
            {
                if (listView.SelectedIndex >= 0)
                {
                    listView.Dispatcher.BeginInvoke((Action)delegate
                    {
                        var item = listView.Items.GetItemAt(listView.SelectedIndex);
                        //listBox.ScrollIntoView(listBox.SelectedItem);//在这里使用一的方法
                        listView.ScrollIntoView(item);//在这里使用一的方法
                        listView.UpdateLayout();
                    });
                }
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            var listView = this.AssociatedObject;

            if (listView != null)
            {
                //listView.SelectionChanged -= new SelectionChangedEventHandler(AssociatedObject_SelectionChanged);
                var scrollViewer = FindSimpleVisualChild<ScrollViewer>(listView);
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
                }
            }
        }

        T FindSimpleVisualChild<T>(DependencyObject element) where T : class
        {
            if(element is T t)
                return t;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            { 
                var child = VisualTreeHelper.GetChild(element, i);
                var result = FindSimpleVisualChild<T>(child);
                if(result != null)
                    return result;
            }

            return null;
        }

    }
}
