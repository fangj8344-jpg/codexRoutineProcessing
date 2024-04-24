#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.FlyDataTool.Model
 * 唯一标识：c92e142f-a109-410c-a834-9db66d1f71a8
 * 文件名：LineTypeModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/3/21 16:20:28
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
using System.Windows.Media;

namespace UtilityTools.Modules.FlyDataTool.Model
{
    public class LineTypeModel : BindableBase
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(); }
        }

        private string _field;

        public string Field
        {
            get { return _field; }
            set { _field = value; RaisePropertyChanged(); }
        }

        private bool _isChecked;

        public bool IsChecked
        {
            get { return _isChecked; }
            set { _isChecked = value; RaisePropertyChanged(); }
        }

        private string _yAxisKey;

        public string YAxisKey
        {
            get { return _yAxisKey; }
            set { _yAxisKey = value; RaisePropertyChanged(); }
        }


        private int _markerType = 0;

        public int MarkerType
        {
            get { return _markerType; }
            set { _markerType = value; RaisePropertyChanged(); }
        }

        private int _axisType = 0;

        public int AxisType
        {
            get { return _axisType; }
            set { _axisType = value; RaisePropertyChanged(); }
        }
    }
}
