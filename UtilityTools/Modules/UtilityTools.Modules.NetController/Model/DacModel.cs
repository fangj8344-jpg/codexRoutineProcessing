#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.NetController.Model
 * 唯一标识：d2eac1d0-b3e6-4bdd-951b-f9577c8e50fe
 * 文件名：DacModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/11 15:45:38
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
using System.Windows.Data;
using UtilityTools.Core.Model;
using UtilityTools.Core.Mvvm;

namespace UtilityTools.Modules.NetController.Model
{
    public class DacModel : BindableBase
    {
        #region ------------Constructor------------
        public DacModel()
        {
            Dacs = new ObservableCollection<IntSliderInfoModel>();
            Dacs.Add(new IntSliderInfoModel() { Title = "DAC1", Tip="", Type="DAC", Channel = 0xA, Value = 0, MinValue = 0, MaxValue = 1023, Interval = 500 });
            Dacs.Add(new IntSliderInfoModel() { Title = "DAC2", Tip="", Type="DAC", Channel = 0xB, Value = 0, MinValue = 0, MaxValue = 1023, Interval = 500 });
            Dacs.Add(new IntSliderInfoModel() { Title = "DAC3", Tip="", Type="DAC", Channel = 0xC, Value = 0, MinValue = 0, MaxValue = 1023, Interval = 500 });
            Dacs.Add(new IntSliderInfoModel() { Title = "DAC4", Tip="", Type="DAC", Channel = 0xD, Value = 0, MinValue = 0, MaxValue = 1023, Interval = 500 });
        }
        #endregion

        #region ------------Field------------
        private ObservableCollection<IntSliderInfoModel> _dacs;
        #endregion

        #region ------------Property------------
        /// <summary>
        /// 可变速风扇
        /// </summary>
        public ObservableCollection<IntSliderInfoModel> Dacs
        {
            get { return _dacs; }
            set { _dacs = value; RaisePropertyChanged(); }
        }
        #endregion

        #region ------------PublicMethod------------
        /// <summary>
        /// 设置属性变更回调函数
        /// </summary>
        /// <param name="handler"></param>
        public void SetPropertyChangedHandle(PropertyChangedEventHandler handler)
        {
            foreach (IntSliderInfoModel sliderItem in Dacs)
            {
                sliderItem.PropertyChanged += handler;
            }
        }
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
