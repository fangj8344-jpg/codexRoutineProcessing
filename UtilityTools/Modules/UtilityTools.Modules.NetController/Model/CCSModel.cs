#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.NetController.Model
 * 唯一标识：cbcd8d13-0e38-44d6-b785-60f7128b3c2c
 * 文件名：CCSModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/10 17:38:03
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
    public class CCSModel : BindableBase
    {
        #region ------------Constructor------------
        public CCSModel()
        {
            Enables = new ObservableCollection<ToggleInfoModel>();
            Enables.Add(new ToggleInfoModel() { Name = "对中Y1", Type = "CCS", Channel = 0, Enable = false, Tip = "Enable0（CHB5）" });
            Enables.Add(new ToggleInfoModel() { Name = "对中Y2", Type = "CCS", Channel = 1, Enable = false, Tip = "Enable1（CHB8）" });
            Enables.Add(new ToggleInfoModel() { Name = "对中X1", Type = "CCS", Channel = 2, Enable = false, Tip = "Enable2（CHA0）" });
            Enables.Add(new ToggleInfoModel() { Name = "对中X2", Type = "CCS", Channel = 3, Enable = false, Tip = "Enable3（CHA1）" });
            Enables.Add(new ToggleInfoModel() { Name = "像散D", Type = "CCS", Channel = 4, Enable = false, Tip = "Enable4（CHB4）" });
            Enables.Add(new ToggleInfoModel() { Name = "像散C", Type = "CCS", Channel = 5, Enable = false, Tip = "Enable5（CHB3）" });
            Enables.Add(new ToggleInfoModel() { Name = "像散A", Type = "CCS", Channel = 6, Enable = false, Tip = "Enable6（CHB1）" });
            Enables.Add(new ToggleInfoModel() { Name = "像散B", Type = "CCS", Channel = 7, Enable = false, Tip = "Enable7（CHB2）" });

            LensModels = new ObservableCollection<IntSliderInfoModel>();
            LensModels.Add(new IntSliderInfoModel() { Title = "物镜值", Type = "Focus", Channel = 0, Value = 45101, Interval = 500, MinValue = 0, MaxValue = 0xFFFF });
            LensModels.Add(new IntSliderInfoModel() { Title = "压缩镜A", Type = "Compress", Channel = 0xA, Value = 1430, Interval = 500, MinValue = 0, MaxValue = 0xFFF });
            LensModels.Add(new IntSliderInfoModel() { Title = "压缩镜B", Type = "Compress", Channel = 0xB, Value = 1430, Interval = 500, MinValue = 0, MaxValue = 0xFFF });

            AligModels = new ObservableCollection<IntSliderInfoModel>();
            AligModels.Add(new IntSliderInfoModel() { Title = "对中X1", Type = "Center", Channel = 0, Value = 261, Interval = 500, MinValue = 0, MaxValue = 0xFFF });
            AligModels.Add(new IntSliderInfoModel() { Title = "对中X2", Type = "Center", Channel = 1, Value = 110, Interval = 500, MinValue = 0, MaxValue = 0xFFF });
            AligModels.Add(new IntSliderInfoModel() { Title = "对中Y1", Type = "Center", Channel = 2, Value = 482, Interval = 500, MinValue = 0, MaxValue = 0xFFF });
            AligModels.Add(new IntSliderInfoModel() { Title = "对中Y2", Type = "Center", Channel = 3, Value = 11, Interval = 500, MinValue = 0, MaxValue = 0xFFF });

            AstigModels = new ObservableCollection<IntSliderInfoModel>();
            AstigModels.Add(new IntSliderInfoModel() { Title = "像散A", Type = "Astig", Channel = 0xA, Value = 0, Interval = 500, MinValue = 0, MaxValue = 0xFFF });
            AstigModels.Add(new IntSliderInfoModel() { Title = "像散B", Type = "Astig", Channel = 0xB, Value = 0, Interval = 500, MinValue = 0, MaxValue = 0xFFF });
            AstigModels.Add(new IntSliderInfoModel() { Title = "像散C", Type = "Astig", Channel = 0xC, Value = 0, Interval = 500, MinValue = 0, MaxValue = 0xFFF });
            AstigModels.Add(new IntSliderInfoModel() { Title = "像散D", Type = "Astig", Channel = 0xD, Value = 248, Interval = 500, MinValue = 0, MaxValue = 0xFFF });
        }
        #endregion

        #region ------------Field------------
        private ObservableCollection<ToggleInfoModel> _enables;
        private ObservableCollection<IntSliderInfoModel> _lensModels;
        private ObservableCollection<IntSliderInfoModel> _aligModels;
        private ObservableCollection<IntSliderInfoModel> _astigModels;

        #endregion

        #region ------------Property------------
        /// <summary>
        /// 继电器使能开关
        /// </summary>
        public ObservableCollection<ToggleInfoModel> Enables
        {
            get { return _enables; }
            set { _enables = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 镜头数据集合
        /// </summary>
        public ObservableCollection<IntSliderInfoModel> LensModels
        {
            get { return _lensModels; }
            set { _lensModels = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 对中线圈数据集合
        /// </summary>
        public ObservableCollection<IntSliderInfoModel> AligModels
        {
            get { return _aligModels; }
            set { _aligModels = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 像散线圈数据集合
        /// </summary>
        public ObservableCollection<IntSliderInfoModel> AstigModels
        {
            get { return _astigModels; }
            set { _astigModels = value; RaisePropertyChanged(); }
        }
        #endregion

        #region ------------PublicMethod------------
        /// <summary>
        /// 设置属性变更回调函数
        /// </summary>
        /// <param name="handler"></param>
        public void SetPropertyChangedHandle(PropertyChangedEventHandler handler)
        {
            foreach (ToggleInfoModel enableItem in Enables)
            {
                enableItem.PropertyChanged += handler;
            }

            foreach (IntSliderInfoModel sliderItem in LensModels)
            {
                sliderItem.PropertyChanged += handler;
            }

            foreach (IntSliderInfoModel sliderItem in AligModels)
            {
                sliderItem.PropertyChanged += handler;
            }

            foreach (IntSliderInfoModel sliderItem in AstigModels)
            {
                sliderItem.PropertyChanged += handler;
            }
        }

        private void ToggleInfoModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
