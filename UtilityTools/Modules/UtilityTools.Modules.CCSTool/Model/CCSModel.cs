#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.CCSTool.Model
 * 唯一标识：d081442d-082a-4e8d-9236-b735ea972651
 * 文件名：CCSModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/5/21 9:42:21
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

namespace UtilityTools.Modules.CCSTool.Model
{
    public class CCSModel : BindableBase
    {
        private string _title = string.Empty;

        public string Title
        {
            get { return _title; }
            set { _title = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<IntSliderInfoModel> _controlItems = new ObservableCollection<IntSliderInfoModel>();

        public ObservableCollection<IntSliderInfoModel>  ControlItems
        {
            get { return _controlItems; }
            set { _controlItems = value; RaisePropertyChanged(); }
        }
    }
}
