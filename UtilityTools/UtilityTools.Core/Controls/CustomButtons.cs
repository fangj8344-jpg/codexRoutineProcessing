using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using SharpVectors.Converters;
namespace UtilityTools.Core.Controls
{
    #region ButtonClass

    /// <summary>
    /// 圆角按钮
    /// </summary>
    public class CornerButton : Button
    {
        static CornerButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CornerButton), new FrameworkPropertyMetadata(typeof(CornerButton)));
        }

        private System.Timers.Timer? _timer;

        private void StartTimer()
        {
            StopTimer();
            _timer = new System.Timers.Timer();
            _timer.Interval = 1000;
            _timer.AutoReset = false;
            _timer.Elapsed += (s, e) =>
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    this.SetValue(IsHoldingProperty, true);
                });
            };
            _timer.Start();
        }

        private void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            StartTimer();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            StopTimer();
        }

        /// <summary>
        /// 外框圆弧半径
        /// </summary>
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(CornerButton), new PropertyMetadata(null));

        /// <summary>
        /// 是否长时间holding
        /// </summary>
        public bool IsHolding
        {
            get { return (bool)GetValue(IsHoldingProperty); }
            set { SetValue(IsHoldingProperty, value); }
        }
        public static readonly DependencyProperty IsHoldingProperty =
            DependencyProperty.Register("IsHolding", typeof(bool), typeof(CornerButton), new PropertyMetadata(false));
    }

    /// <summary>
    /// 带图标的按钮
    /// </summary>
    public class IconButton : CornerButton
    {
        static IconButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconButton), new FrameworkPropertyMetadata(typeof(IconButton)));
        }

        /// <summary>
        /// 图标路径
        /// </summary>
        public Uri IconSource
        {
            get { return (Uri)GetValue(IconSourceProperty); }
            set { SetValue(IconSourceProperty, value); }
        }
        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register("IconSource", typeof(Uri), typeof(IconButton), new PropertyMetadata(null));


        /// <summary>
        /// 图标宽度
        /// </summary>
        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }
        public static readonly DependencyProperty IconWidthProperty =
            DependencyProperty.Register("IconWidth", typeof(double), typeof(IconButton), new PropertyMetadata(12.0));


        /// <summary>
        /// 图标高度
        /// </summary>
        public double IconHeight
        {
            get { return (double)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }
        public static readonly DependencyProperty IconHeightProperty =
            DependencyProperty.Register("IconHeight", typeof(double), typeof(IconButton), new PropertyMetadata(12.0));

    }

    /// <summary>
    /// 带路径图标按钮
    /// </summary>
    public class GeometryButton : CornerButton
    {
        static GeometryButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GeometryButton), new FrameworkPropertyMetadata(typeof(GeometryButton)));
        }

        /// <summary>
        /// 图标
        /// </summary>
        public Geometry IconGeometry
        {
            get { return (Geometry)GetValue(IconGeometryProperty); }
            set { SetValue(IconGeometryProperty, value); }
        }
        public static readonly DependencyProperty IconGeometryProperty =
            DependencyProperty.Register("IconGeometry", typeof(Geometry), typeof(GeometryButton), new PropertyMetadata(null));


        /// <summary>
        /// 图标宽度
        /// </summary>
        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }
        public static readonly DependencyProperty IconWidthProperty =
            DependencyProperty.Register("IconWidth", typeof(double), typeof(GeometryButton), new PropertyMetadata(12.0));


        /// <summary>
        /// 图标高度
        /// </summary>
        public double IconHeight
        {
            get { return (double)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }
        public static readonly DependencyProperty IconHeightProperty =
            DependencyProperty.Register("IconHeight", typeof(double), typeof(GeometryButton), new PropertyMetadata(12.0));

    }
    #endregion

    #region RepeatButtonClass
    /// <summary>
    /// 圆角长按按钮
    /// </summary>
    public class CornerRepeatButton : RepeatButton
    {
        static CornerRepeatButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CornerRepeatButton), new FrameworkPropertyMetadata(typeof(CornerRepeatButton)));
        }

        /// <summary>
        /// 外框圆弧半径
        /// </summary>
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(CornerRepeatButton), new PropertyMetadata(null));

    }

    /// <summary>
    /// 带图标的长按按钮
    /// </summary>
    public class IconRepeatButton : CornerRepeatButton
    {
        static IconRepeatButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconRepeatButton), new FrameworkPropertyMetadata(typeof(IconRepeatButton)));
        }

        /// <summary>
        /// 图标路径
        /// </summary>
        public Uri IconSource
        {
            get { return (Uri)GetValue(IconSourceProperty); }
            set { SetValue(IconSourceProperty, value); }
        }
        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register("IconSource", typeof(Uri), typeof(IconRepeatButton), new PropertyMetadata(null));


        /// <summary>
        /// 图标宽度
        /// </summary>
        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }
        public static readonly DependencyProperty IconWidthProperty =
            DependencyProperty.Register("IconWidth", typeof(double), typeof(IconRepeatButton), new PropertyMetadata(12.0));


        /// <summary>
        /// 图标高度
        /// </summary>
        public double IconHeight
        {
            get { return (double)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }
        public static readonly DependencyProperty IconHeightProperty =
            DependencyProperty.Register("IconHeight", typeof(double), typeof(IconRepeatButton), new PropertyMetadata(12.0));

    }

    /// <summary>
    /// 带路径图标的长按按钮
    /// </summary>
    public class GeometryRepeatButton : CornerRepeatButton
    {
        static GeometryRepeatButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GeometryRepeatButton), new FrameworkPropertyMetadata(typeof(GeometryRepeatButton)));
        }

        /// <summary>
        /// 外框圆弧半径
        /// </summary>
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(GeometryRepeatButton), new PropertyMetadata(null));


        /// <summary>
        /// 图标
        /// </summary>
        public Geometry IconGeometry
        {
            get { return (Geometry)GetValue(IconGeometryProperty); }
            set { SetValue(IconGeometryProperty, value); }
        }
        public static readonly DependencyProperty IconGeometryProperty =
            DependencyProperty.Register("IconGeometry", typeof(Geometry), typeof(GeometryRepeatButton), new PropertyMetadata(null));


        /// <summary>
        /// 图标宽度
        /// </summary>
        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }
        public static readonly DependencyProperty IconWidthProperty =
            DependencyProperty.Register("IconWidth", typeof(double), typeof(GeometryRepeatButton), new PropertyMetadata(12.0));


        /// <summary>
        /// 图标高度
        /// </summary>
        public double IconHeight
        {
            get { return (double)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }
        public static readonly DependencyProperty IconHeightProperty =
            DependencyProperty.Register("IconHeight", typeof(double), typeof(GeometryRepeatButton), new PropertyMetadata(12.0));

    }

    #endregion

    #region ToggleButtonClass
    /// <summary>
    /// 带圆角的切换按钮
    /// </summary>
    public class CornerToggleButton : ToggleButton
    {
        static CornerToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CornerToggleButton), new FrameworkPropertyMetadata(typeof(CornerToggleButton)));
        }


        private System.Timers.Timer? _timer;

        private void StartTimer()
        {
            StopTimer();
            _timer = new System.Timers.Timer();
            _timer.Interval = 1000;
            _timer.AutoReset = false;
            _timer.Elapsed += (s, e) =>
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    this.SetValue(IsHoldingProperty, true);
                });
            };
            _timer.Start();
        }

        private void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            StartTimer();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            StopTimer();
        }


        /// <summary>
        /// 是否长时间holding
        /// </summary>
        public bool IsHolding
        {
            get { return (bool)GetValue(IsHoldingProperty); }
            set { SetValue(IsHoldingProperty, value); }
        }
        public static readonly DependencyProperty IsHoldingProperty =
            DependencyProperty.Register("IsHolding", typeof(bool), typeof(CornerToggleButton), new PropertyMetadata(false));

        /// <summary>
        /// 外框圆弧半径
        /// </summary>
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(CornerToggleButton), new PropertyMetadata(null));
    }

    /// <summary>
    /// 带图标切换按钮
    /// </summary>
    public class IconToggleButton : CornerToggleButton
    {
        static IconToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconToggleButton), new FrameworkPropertyMetadata(typeof(IconToggleButton)));
        }

        /// <summary>
        /// 图标路径
        /// </summary>
        public Uri IconSource
        {
            get { return (Uri)GetValue(IconSourceProperty); }
            set { SetValue(IconSourceProperty, value); }
        }
        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register("IconSource", typeof(Uri), typeof(IconToggleButton), new PropertyMetadata(null));


        /// <summary>
        /// 图标宽度
        /// </summary>
        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }
        public static readonly DependencyProperty IconWidthProperty =
            DependencyProperty.Register("IconWidth", typeof(double), typeof(IconToggleButton), new PropertyMetadata(12.0));


        /// <summary>
        /// 图标高度
        /// </summary>
        public double IconHeight
        {
            get { return (double)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }
        public static readonly DependencyProperty IconHeightProperty =
            DependencyProperty.Register("IconHeight", typeof(double), typeof(IconToggleButton), new PropertyMetadata(12.0));

    }

    /// <summary>
    /// 带路径图标切换按钮
    /// </summary>
    public class GeometryToggleButton : CornerToggleButton
    {
        static GeometryToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GeometryToggleButton), new FrameworkPropertyMetadata(typeof(GeometryToggleButton)));
        }

        /// <summary>
        /// 图标
        /// </summary>
        public Geometry IconGeometry
        {
            get { return (Geometry)GetValue(IconGeometryProperty); }
            set { SetValue(IconGeometryProperty, value); }
        }
        public static readonly DependencyProperty IconGeometryProperty =
            DependencyProperty.Register("IconGeometry", typeof(Geometry), typeof(GeometryToggleButton), new PropertyMetadata(null));


        /// <summary>
        /// 图标宽度
        /// </summary>
        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }
        public static readonly DependencyProperty IconWidthProperty =
            DependencyProperty.Register("IconWidth", typeof(double), typeof(GeometryToggleButton), new PropertyMetadata(12.0));


        /// <summary>
        /// 图标高度
        /// </summary>
        public double IconHeight
        {
            get { return (double)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }
        public static readonly DependencyProperty IconHeightProperty =
            DependencyProperty.Register("IconHeight", typeof(double), typeof(GeometryToggleButton), new PropertyMetadata(12.0));

    }

    /// <summary>
    /// 双重文字切换按钮
    /// </summary>
    public class DiContentToggleButton : CornerToggleButton
    {
        static DiContentToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DiContentToggleButton), new FrameworkPropertyMetadata(typeof(DiContentToggleButton)));
        }

        public DiContentToggleButton()
        {
            this.Loaded += DiContentToggleButton_Loaded;
        }

        protected override void OnChecked(RoutedEventArgs e)
        {
            base.OnChecked(e);
            Content = CheckedContent;
        }

        protected override void OnUnchecked(RoutedEventArgs e)
        {
            base.OnUnchecked(e);
            Content = UnCheckedContent;
        }

        private void DiContentToggleButton_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsChecked.GetValueOrDefault())
            {
                Content = CheckedContent;
            }
            else
            {
                Content = UnCheckedContent;
            }
        }

        /// <summary>
        /// 未选中内容
        /// </summary>
        public Object UnCheckedContent
        {
            get { return (Object)GetValue(UnCheckedContentProperty); }
            set { SetValue(UnCheckedContentProperty, value); }
        }
        public static readonly DependencyProperty UnCheckedContentProperty =
            DependencyProperty.Register("UnCheckedContent", typeof(Object), typeof(DiContentToggleButton), new PropertyMetadata(null));

        /// <summary>
        /// 选中内容
        /// </summary>
        public Object CheckedContent
        {
            get { return (Object)GetValue(CheckedContentProperty); }
            set { SetValue(CheckedContentProperty, value); }
        }
        public static readonly DependencyProperty CheckedContentProperty =
            DependencyProperty.Register("CheckedContent", typeof(Object), typeof(DiContentToggleButton), new PropertyMetadata(null));
    }

    /// <summary>
    /// 双重图标切换按钮
    /// </summary>
    public class DiIconToggleButton : IconToggleButton
    {
        static DiIconToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DiIconToggleButton), new FrameworkPropertyMetadata(typeof(DiIconToggleButton)));
        }

        public DiIconToggleButton()
        {
            this.Loaded += DiIconToggleButton_Loaded;
        }


        protected override void OnChecked(RoutedEventArgs e)
        {
            base.OnChecked(e);
            IconSource = CheckedIconSource;
            Content = CheckedContent;
        }

        protected override void OnUnchecked(RoutedEventArgs e)
        {
            base.OnUnchecked(e);
            IconSource = UnCheckedIconSource;
            Content = UnCheckedContent;
        }

        private void DiIconToggleButton_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsChecked.GetValueOrDefault())
            {
                IconSource = CheckedIconSource;
                Content = CheckedContent;
            }
            else
            {
                IconSource = UnCheckedIconSource;
                Content = UnCheckedContent;
            }
        }

        /// <summary>
        /// 未选中图标路径
        /// </summary>
        public Uri UnCheckedIconSource
        {
            get { return (Uri)GetValue(UnCheckedIconSourceProperty); }
            set { SetValue(UnCheckedIconSourceProperty, value); }
        }
        public static readonly DependencyProperty UnCheckedIconSourceProperty =
            DependencyProperty.Register("UnCheckedIconSource", typeof(Uri), typeof(DiIconToggleButton), new PropertyMetadata(null));

        /// <summary>
        /// 未选中内容
        /// </summary>
        public Object UnCheckedContent
        {
            get { return (Object)GetValue(UnCheckedContentProperty); }
            set { SetValue(UnCheckedContentProperty, value); }
        }
        public static readonly DependencyProperty UnCheckedContentProperty =
            DependencyProperty.Register("UnCheckedContent", typeof(Object), typeof(DiIconToggleButton), new PropertyMetadata(null));

        /// <summary>
        /// 选中图标路径
        /// </summary>
        public Uri CheckedIconSource
        {
            get { return (Uri)GetValue(CheckedIconSourceProperty); }
            set { SetValue(CheckedIconSourceProperty, value); }
        }
        public static readonly DependencyProperty CheckedIconSourceProperty =
            DependencyProperty.Register("CheckedIconSource", typeof(Uri), typeof(DiIconToggleButton), new PropertyMetadata(null));

        /// <summary>
        /// 选中内容
        /// </summary>
        public Object CheckedContent
        {
            get { return (Object)GetValue(CheckedContentProperty); }
            set { SetValue(CheckedContentProperty, value); }
        }
        public static readonly DependencyProperty CheckedContentProperty =
            DependencyProperty.Register("CheckedContent", typeof(Object), typeof(DiIconToggleButton), new PropertyMetadata(null));
    }

    /// <summary>
    /// 移位寄存器特有的
    /// </summary>
    public class RelayToggleButton : CornerToggleButton
    {
        static RelayToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RelayToggleButton), new FrameworkPropertyMetadata(typeof(RelayToggleButton)));
        }

        public RelayToggleButton()
        {
            this.Checked += RelayToggleButton_Checked;
            this.Unchecked += RelayToggleButton_Checked;
        }

        /// <summary>
        /// 是否选中变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RelayToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            RelayToggleButton? button = sender as RelayToggleButton;
            if (button != null)
            {
                if (button.IsChecked == true)
                {
                    int mask = 0x1 << button.RelayPos;
                    button.RelayValue = (button.RelayValue | mask);
                }
                else
                {
                    int mask = ~(0x1 << button.RelayPos);
                    button.RelayValue = (button.RelayValue & mask);
                }
            }
        }

        /// <summary>
        /// 继电器数值
        /// </summary>
        public int RelayValue
        {
            get { return (int)GetValue(RelayValueProperty); }
            set { SetValue(RelayValueProperty, value); }
        }
        public static readonly DependencyProperty RelayValueProperty =
            DependencyProperty.Register("RelayValue", typeof(int), typeof(RelayToggleButton), new PropertyMetadata(0, RelayValueChanged));

        private static void RelayValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RelayToggleButton? button = d as RelayToggleButton;
            if (button != null)
            {
                int newValue = (int)e.NewValue;
                int oldValue = (int)e.OldValue;

                if (newValue != oldValue)
                {
                    button.RelayValue = newValue;
                    var value = (newValue & (0x1 << button.RelayPos));
                    button.IsChecked = (value != 0);
                }

            }
        }

        /// <summary>
        /// 继电器所在的位数，从低位到高位依次+1，最低位为0
        /// </summary>
        public int RelayPos
        {
            get { return (int)GetValue(RelayPosProperty); }
            set { SetValue(RelayPosProperty, value); }
        }
        public static readonly DependencyProperty RelayPosProperty =
            DependencyProperty.Register("RelayPos", typeof(int), typeof(RelayToggleButton), new PropertyMetadata(0, RelayPosChanged));

        private static void RelayPosChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RelayToggleButton? button = d as RelayToggleButton;
            if (button != null)
            {
                int newValue = (int)e.NewValue;
                int oldValue = (int)e.OldValue;

                if (newValue != oldValue)
                {
                    var value = (button.RelayValue & (0x1 << newValue));
                    button.IsChecked = (value != 0);
                }

            }
        }
    }

    #endregion

    #region RadioButtonClass
    /// <summary>
    /// 圆角
    /// </summary>
    public class CornerRadioButton : RadioButton
    {
        static CornerRadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CornerRadioButton), new FrameworkPropertyMetadata(typeof(CornerRadioButton)));
        }

        private System.Timers.Timer? _timer;

        private void StartTimer()
        {
            StopTimer();
            _timer = new System.Timers.Timer();
            _timer.Interval = 1000;
            _timer.AutoReset = false;
            _timer.Elapsed += (s, e) =>
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    this.SetValue(IsHoldingProperty, true);
                });
            };
            _timer.Start();
        }

        private void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            StartTimer();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            StopTimer();
        }


        /// <summary>
        /// 是否长时间holding
        /// </summary>
        public bool IsHolding
        {
            get { return (bool)GetValue(IsHoldingProperty); }
            set { SetValue(IsHoldingProperty, value); }
        }
        public static readonly DependencyProperty IsHoldingProperty =
            DependencyProperty.Register("IsHolding", typeof(bool), typeof(CornerRadioButton), new PropertyMetadata(false));


        /// <summary>
        /// 外框圆弧半径
        /// </summary>
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(CornerRadioButton), new PropertyMetadata(null));
    }

    /// <summary>
    /// 带图标切换按钮
    /// </summary>
    public class IconRadioButton : CornerRadioButton
    {
        static IconRadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconRadioButton), new FrameworkPropertyMetadata(typeof(IconRadioButton)));
        }

        /// <summary>
        /// 图标路径
        /// </summary>
        public Uri IconSource
        {
            get { return (Uri)GetValue(IconSourceProperty); }
            set { SetValue(IconSourceProperty, value); }
        }
        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register("IconSource", typeof(Uri), typeof(IconRadioButton), new PropertyMetadata(null));


        /// <summary>
        /// 图标宽度
        /// </summary>
        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }
        public static readonly DependencyProperty IconWidthProperty =
            DependencyProperty.Register("IconWidth", typeof(double), typeof(IconRadioButton), new PropertyMetadata(12.0));


        /// <summary>
        /// 图标高度
        /// </summary>
        public double IconHeight
        {
            get { return (double)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }
        public static readonly DependencyProperty IconHeightProperty =
            DependencyProperty.Register("IconHeight", typeof(double), typeof(IconRadioButton), new PropertyMetadata(12.0));

    }

    /// <summary>
    /// 带路径图标切换按钮
    /// </summary>
    public class GeometryRadioButton : CornerRadioButton
    {
        static GeometryRadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GeometryRadioButton), new FrameworkPropertyMetadata(typeof(GeometryRadioButton)));
        }

        /// <summary>
        /// 图标
        /// </summary>
        public Geometry IconGeometry
        {
            get { return (Geometry)GetValue(IconGeometryProperty); }
            set { SetValue(IconGeometryProperty, value); }
        }
        public static readonly DependencyProperty IconGeometryProperty =
            DependencyProperty.Register("IconGeometry", typeof(Geometry), typeof(GeometryRadioButton), new PropertyMetadata(null));


        /// <summary>
        /// 图标宽度
        /// </summary>
        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }
        public static readonly DependencyProperty IconWidthProperty =
            DependencyProperty.Register("IconWidth", typeof(double), typeof(GeometryRadioButton), new PropertyMetadata(12.0));


        /// <summary>
        /// 图标高度
        /// </summary>
        public double IconHeight
        {
            get { return (double)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }
        public static readonly DependencyProperty IconHeightProperty =
            DependencyProperty.Register("IconHeight", typeof(double), typeof(GeometryRadioButton), new PropertyMetadata(12.0));

    }

    /// <summary>
    /// 双图标切换按钮
    /// </summary>
    [TemplatePart(Name = "PART_SvgViewbox", Type = typeof(SvgViewbox))]
    [TemplatePart(Name = "PART_TextBlock", Type = typeof(TextBlock))]
    public class DIconRadioButton : ToggleButton
    {
        static DIconRadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DIconRadioButton), new FrameworkPropertyMetadata(typeof(DIconRadioButton)));
        }

        public DIconRadioButton()
        {
            this.Loaded += DIconRadioButton_Loaded; ;
        }

        private void DIconRadioButton_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsChecked.GetValueOrDefault())
            {
                if (_svgViewbox != null)
                {
                    _svgViewbox.Source = CheckedIconSource;
                }

                if (_textBlock != null)
                {
                    _textBlock.Text = CheckedText;
                }
            }
            else
            {
                if (_svgViewbox != null)
                {
                    _svgViewbox.Source = OriginIconSource;
                }

                if (_textBlock != null)
                {
                    _textBlock.Text = OriginText;
                }
            }
        }

        /// <summary>
        /// 原始文本
        /// </summary>
        public string OriginText
        {
            get { return (string)GetValue(OriginTextProperty); }
            set { SetValue(OriginTextProperty, value); }
        }
        public static readonly DependencyProperty OriginTextProperty =
            DependencyProperty.Register("OriginText", typeof(string), typeof(DIconRadioButton), new PropertyMetadata(""));

        /// <summary>
        /// 选中文本
        /// </summary>
        public string CheckedText
        {
            get { return (string)GetValue(CheckedTextProperty); }
            set { SetValue(CheckedTextProperty, value); }
        }
        public static readonly DependencyProperty CheckedTextProperty =
            DependencyProperty.Register("CheckedText", typeof(string), typeof(DIconRadioButton), new PropertyMetadata(""));

        /// <summary>
        /// 原始图标路径
        /// </summary>
        public Uri OriginIconSource
        {
            get { return (Uri)GetValue(OriginIconSourceProperty); }
            set { SetValue(OriginIconSourceProperty, value); }
        }
        public static readonly DependencyProperty OriginIconSourceProperty =
            DependencyProperty.Register("OriginIconSource", typeof(Uri), typeof(DIconRadioButton), new PropertyMetadata(null));

        /// <summary>
        /// 选中图标路径
        /// </summary>
        public Uri CheckedIconSource
        {
            get { return (Uri)GetValue(CheckedIconSourceProperty); }
            set { SetValue(CheckedIconSourceProperty, value); }
        }
        public static readonly DependencyProperty CheckedIconSourceProperty =
            DependencyProperty.Register("CheckedIconSource", typeof(Uri), typeof(DIconRadioButton), new PropertyMetadata(null));

        /// <summary>
        /// 图标宽度
        /// </summary>
        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }
        public static readonly DependencyProperty IconWidthProperty =
            DependencyProperty.Register("IconWidth", typeof(double), typeof(DIconRadioButton), new PropertyMetadata(12.0));


        /// <summary>
        /// 图标高度
        /// </summary>
        public double IconHeight
        {
            get { return (double)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }
        public static readonly DependencyProperty IconHeightProperty =
            DependencyProperty.Register("IconHeight", typeof(double), typeof(DIconRadioButton), new PropertyMetadata(12.0));

        private SvgViewbox _svgViewbox;
        private TextBlock _textBlock;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // 获取模板部件
            _svgViewbox = GetTemplateChild("PART_SvgViewbox") as SvgViewbox;
            _textBlock = GetTemplateChild("PART_TextBlock") as TextBlock;

            if (_svgViewbox != null)
            {
                _svgViewbox.Source = OriginIconSource;
            }

            if (_textBlock != null)
            {
                _textBlock.Text = OriginText;
            }
        }

        protected override void OnChecked(RoutedEventArgs e)
        {
            base.OnChecked(e);

            if (_svgViewbox != null)
            {
                _svgViewbox.Source = CheckedIconSource;
            }

            if (_textBlock != null)
            {
                _textBlock.Text = CheckedText;
            }
        }

        protected override void OnUnchecked(RoutedEventArgs e)
        {
            base.OnUnchecked(e);

            if (_svgViewbox != null)
            {
                _svgViewbox.Source = OriginIconSource;
            }

            if (_textBlock != null)
            {
                _textBlock.Text = OriginText;
            }
        }
    }

    #endregion
}
