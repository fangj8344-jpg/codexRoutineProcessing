using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Model
{
    /// <summary>
    /// 导航菜单数据模型
    /// </summary>
    public class MenuBarModel : BindableBase
    {
        #region 字段
        private string _title;
        private string _icon;
        private string _nameSpace;
        private string _tip;
        #endregion

        #region Property
        /// <summary>
        /// 菜单标题
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { _title = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 菜单图标
        /// </summary>
        public string Icon
        {
            get { return _icon; }
            set { _icon = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 菜单映射的页面名称空间
        /// </summary>
        public string NameSpace
        {
            get { return _nameSpace; }
            set { _nameSpace = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 菜单提示
        /// </summary>
        public string Tip
        {
            get { return _tip; }
            set { _tip = value; RaisePropertyChanged(); }
        }
        #endregion
    }
}
