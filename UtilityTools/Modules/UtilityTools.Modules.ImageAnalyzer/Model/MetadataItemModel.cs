#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.ImageAnalyzer.Model
 * 唯一标识：4f40a727-fa72-43f5-b51f-6a34012ccbf1
 * 文件名：MetadataItemModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/4/24 17:59:23
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

namespace UtilityTools.Modules.ImageAnalyzer.Model
{
    public class MetadataItemModel : BindableBase
    {
		public MetadataItemModel(string name, string key, string value)
		{
			Name = name;
            Key = key;
			Value = value;
		}

		private string _name;

		public string Name
		{
			get { return _name; }
			set { _name = value; RaisePropertyChanged(); }
		}

		private string _key;

		public string Key
		{
			get { return _key; }
			set { _key = value; RaisePropertyChanged(); }
		}

		private string _value;

		public string Value
		{
			get { return _value; }
			set { _value = value; RaisePropertyChanged(); }
		}
	}
}
