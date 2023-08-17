#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.NetController.Model
 * 唯一标识：50b2043b-c832-4891-8e71-f979e4b6ba99
 * 文件名：VacuumModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/10 17:37:10
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
using System.Windows.Controls.Primitives;
using UtilityTools.Core.Model;
using UtilityTools.Core.Mvvm;

namespace UtilityTools.Modules.NetController.Model
{
    public class VacuumModel : BindableBase
    {
        #region ------------Constructor------------
        public VacuumModel()
        {
            Vacuums = new ObservableCollection<LabelInfoModel>();
            Vacuums.Add(new LabelInfoModel() { Name = "真空读数1", Type="Vacuum", Channel = 1, Tip = "通道1真空读数", Value = "未知"});
            Vacuums.Add(new LabelInfoModel() { Name = "真空读数2", Type="Vacuum", Channel = 2, Tip = "通道2真空读数", Value = "未知" });
            Vacuums.Add(new LabelInfoModel() { Name = "真空读数3", Type="Vacuum", Channel = 3, Tip = "通道3真空读数", Value = "未知" });
            Vacuums.Add(new LabelInfoModel() { Name = "真空读数4", Type="Vacuum", Channel = 4, Tip = "通道4真空读数", Value = "未知" });

            Motor = new ToggleInfoModel() { Name = "Motor", Type="Motor", Tip="电机", Channel = 0, Enable = false};
        }
        #endregion

        #region ------------Field------------
        private ObservableCollection<LabelInfoModel> _vacuums;
        private ToggleInfoModel _motor;
        #endregion

        #region ------------Property------------
        /// <summary>
        /// 真空读数
        /// </summary>
        public ObservableCollection<LabelInfoModel> Vacuums
        {
            get { return _vacuums; }
            set { _vacuums = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 电机
        /// </summary>
        public ToggleInfoModel Motor
        {
            get { return _motor; }
            set { _motor = value; RaisePropertyChanged(); }
        }

        #endregion

        #region ------------PublicMethod------------
        /// <summary>
        /// 设置属性变更回调函数
        /// </summary>
        /// <param name="handler"></param>
        public void SetPropertyChangedHandle(PropertyChangedEventHandler handler)
        {
            foreach (LabelInfoModel valueItem in Vacuums)
            {
                valueItem.PropertyChanged += handler;
            }

            {
                Motor.PropertyChanged += handler;
            }
        }
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
