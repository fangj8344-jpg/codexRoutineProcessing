#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Model
 * 唯一标识：606b52f6-8351-4318-b340-220ee71c878b
 * 文件名：PidModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/10/24 8:35:00
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
    public class PidModel : BindableBase
    {
		private float _kp;

		public float Kp
		{
			get { return _kp; }
			set { _kp = value; RaisePropertyChanged(); }
		}

		private float _ki;

		public float Ki
		{
			get { return _ki; }
			set { _ki = value; RaisePropertyChanged(); }
		}

		private float _kd;

		public float Kd
		{
			get { return _kd; }
			set { _kd = value; RaisePropertyChanged(); }
		}

	}
}
