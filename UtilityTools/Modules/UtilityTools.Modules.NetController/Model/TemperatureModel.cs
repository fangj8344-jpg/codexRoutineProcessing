#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.NetController.Model
 * 唯一标识：3bbb8da8-db0c-4969-a3a4-1f41e8ed6470
 * 文件名：TemparatureModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/11 14:00:16
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Model;
using UtilityTools.Core.Mvvm;

namespace UtilityTools.Modules.NetController.Model
{
    public class TemperatureModel : CustomBindableBase
    {
        #region ------------Constructor------------
        public TemperatureModel()
        {
            Temperatures = new ObservableCollection<LabelInfoModel>();
            Temperatures.Add(new LabelInfoModel() { Name = "温度读数1", Channel = 1, Tip = "", Value = "未知" });
            Temperatures.Add(new LabelInfoModel() { Name = "温度读数2", Channel = 2, Tip = "", Value = "未知" });

            Fans = new ObservableCollection<IntSliderInfoModel>();
            Fans.Add(new IntSliderInfoModel() { Title = "风扇1", Type="Fans", Tip="可调速风扇", Value = 0, Channel = 1, MinValue = 0, MaxValue = 100, Interval = 500 });
            Fans.Add(new IntSliderInfoModel() { Title = "风扇2", Type="Fans", Tip="可调速风扇", Value = 0, Channel = 2, MinValue = 0, MaxValue = 100, Interval = 500 });
            Fans.Add(new IntSliderInfoModel() { Title = "风扇3", Type="Fans", Tip="可调速风扇", Value = 0, Channel = 3, MinValue = 0, MaxValue = 100, Interval = 500 });
        }
        #endregion

        #region ------------Field------------
        private ObservableCollection<LabelInfoModel> _temperatures;
        private ObservableCollection<IntSliderInfoModel> _fans;
        #endregion

        #region ------------Property------------
        /// <summary>
        /// 温度读数
        /// </summary>
        public ObservableCollection<LabelInfoModel> Temperatures
        {
            get { return _temperatures; }
            set { _temperatures = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 可变速风扇
        /// </summary>
        public ObservableCollection<IntSliderInfoModel> Fans
        {
            get { return _fans; }
            set { _fans = value; RaisePropertyChanged(); }
        }

        #endregion

        #region ------------PublicMethod------------
        /// <summary>
        /// 设置属性变更回调函数
        /// </summary>
        /// <param name="handler"></param>
        public void SetPropertyChangedHandle(EventHandler handler)
        {
            foreach (LabelInfoModel valueItem in Temperatures)
            {
                DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(LabelInfoModel.ValueProperty, typeof(LabelInfoModel));
                dpd.AddValueChanged(valueItem, handler);
            }

            foreach (IntSliderInfoModel sliderItem in Fans)
            {
                DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(IntSliderInfoModel.ValueProperty, typeof(IntSliderInfoModel));
                dpd.AddValueChanged(sliderItem, handler);
            }
        }
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
