#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Model
 * 唯一标识：4e7feba7-d9bc-42f6-b660-189104c2e153
 * 文件名：SliderInfoModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/11 10:56:14
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

using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Model
{
    public class IntSliderInfoModel : SliderInfoModel<int> { }

    public class DoubleSliderInfoModel : SliderInfoModel<double> { }

    public class FloatSliderInfoModel : SliderInfoModel<float> { }

    public class SliderInfoModel<T> : BindableBase where T : struct
    {
        #region ------------Constructor------------
        #endregion

        #region ------------Field------------
        private string _title;
        private string _tip;
        private int _channel;
        private T _value;
        private T _minValue;
        private T _maxValue;
        private double _interval;
        #endregion

        #region ------------Property------------
        /// <summary>
        /// 滑块标题
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { _title = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 提示信息
        /// </summary>
        public string Tip
        {
            get { return _tip; }
            set { _tip = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 通道编码
        /// </summary>
        public int Channel
        {
            get { return _channel; }
            set { _channel = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 滑块数值
        /// </summary>
        public T Value
        {
            get { return _value; }
            set
            {
                _value = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 滑块最小值
        /// </summary>
        public T MinValue
        {
            get { return _minValue; }
            set { _minValue = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 滑块最大值
        /// </summary>
        public T MaxValue
        {
            get { return _maxValue; }
            set { _maxValue = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 滑块数值变化触发事件的时间间隔
        /// </summary>
        public double Interval
        {
            get { return _interval; }
            set { _interval = value; RaisePropertyChanged(); }
        }
        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
