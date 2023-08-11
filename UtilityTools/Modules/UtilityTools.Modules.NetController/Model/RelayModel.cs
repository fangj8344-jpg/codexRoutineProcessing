#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.NetController.Model
 * 唯一标识：bfebdd4f-77eb-4853-b608-59c40d1d96bb
 * 文件名：RelayModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/11 15:31:08
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Model;

namespace UtilityTools.Modules.NetController.Model
{
    public class RelayModel : BindableBase
    {
        #region ------------Constructor------------
        public RelayModel()
        {
            Enables = new ObservableCollection<ToggleInfoModel>();
            Enables.Add(new ToggleInfoModel { Name = "CH0", Tip = "", Index = 0, Enable = false });
            Enables.Add(new ToggleInfoModel { Name = "CH1", Tip = "", Index = 1, Enable = false });
            Enables.Add(new ToggleInfoModel { Name = "CH2", Tip = "", Index = 2, Enable = false });
            Enables.Add(new ToggleInfoModel { Name = "CH3", Tip = "", Index = 3, Enable = false });
            Enables.Add(new ToggleInfoModel { Name = "CH4", Tip = "", Index = 4, Enable = false });
            Enables.Add(new ToggleInfoModel { Name = "CH5", Tip = "", Index = 5, Enable = false });
            Enables.Add(new ToggleInfoModel { Name = "CH6", Tip = "", Index = 6, Enable = false });
            Enables.Add(new ToggleInfoModel { Name = "CH7", Tip = "", Index = 7, Enable = false });
            Enables.Add(new ToggleInfoModel { Name = "CH8", Tip = "", Index = 8, Enable = false });
            Enables.Add(new ToggleInfoModel { Name = "CH9", Tip = "", Index = 9, Enable = false });
            Enables.Add(new ToggleInfoModel { Name = "CH10", Tip = "", Index = 10, Enable = false });
            Enables.Add(new ToggleInfoModel { Name = "CH11", Tip = "", Index = 11, Enable = false });
            Enables.Add(new ToggleInfoModel { Name = "CH12", Tip = "", Index = 12, Enable = false });
            Enables.Add(new ToggleInfoModel { Name = "CH13", Tip = "", Index = 13, Enable = false });
            Enables.Add(new ToggleInfoModel { Name = "CH14", Tip = "", Index = 14, Enable = false });
            Enables.Add(new ToggleInfoModel { Name = "CH15", Tip = "", Index = 15, Enable = false });
        }
        #endregion

        #region ------------Field------------
        private ObservableCollection<ToggleInfoModel> _enables;
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
        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
