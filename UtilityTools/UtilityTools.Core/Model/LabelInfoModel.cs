#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Model
 * 唯一标识：492fa448-53f8-4645-abe5-a0865850183c
 * 文件名：LabelInfoModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/11 13:50:55
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
using System.Windows;

namespace UtilityTools.Core.Model
{
    public class LabelInfoModel: BindableBase
    {
        #region ------------Constructor------------
        #endregion

        #region ------------Field------------
        private string _name;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(); }
        }

        private string _tip;
        /// <summary>
        /// 提示信息
        /// </summary>
        public string Tip
        {
            get { return _tip; }
            set { _tip = value; RaisePropertyChanged(); }
        }

        private int _channel;
        /// <summary>
        /// 参数通道
        /// </summary>
        public int Channel
        {
            get { return _channel; }
            set { _channel = value; RaisePropertyChanged(); }
        }

        private string _type;
        /// <summary>
        /// 控制类型
        /// </summary>
        public string Type
        {
            get { return _type; }
            set { _type = value; RaisePropertyChanged(); }
        }

        private string _value;
        /// <summary>
        /// 数值
        /// </summary>
        public string Value
        {
            get { return _value; }
            set { _value = value; RaisePropertyChanged(); }
        }

        #endregion

        #region ------------Property------------
        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
