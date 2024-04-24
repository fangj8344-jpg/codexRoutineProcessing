#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.FlyDataTool.Model
 * 唯一标识：450e20cd-2207-423c-8c8a-75647411c8c5
 * 文件名：FlyDataModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/3/21 16:19:58
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
    public class FlyDataModel : BindableBase
    {
        private int _index;
        /// <summary>
        /// 当前索引
        /// </summary>
        public int Index
        {
            get { return _index; }
            set { _index = value; RaisePropertyChanged(); }
        }


        private DateTime _time;

        public DateTime Time
        {
            get { return _time; }
            set { _time = value; RaisePropertyChanged(); }
        }

        private double _accVol;

        public double AccVol
        {
            get { return _accVol; }
            set { _accVol = value; RaisePropertyChanged(); }
        }

        private double _filaCur;

        public double FilaCur
        {
            get { return _filaCur; }
            set { _filaCur = value; RaisePropertyChanged(); }
        }

        private double _filaR;

        public double FilaR
        {
            get { return _filaR; }
            set { _filaR = value; RaisePropertyChanged(); }
        }

        private double _gridVol;

        public double GridVol
        {
            get { return _gridVol; }
            set { _gridVol = value; RaisePropertyChanged(); }
        }

        private Brush _brush = new SolidColorBrush(Colors.Black);

        public Brush Brush
        {
            get { return _brush; }
            set { _brush = value; RaisePropertyChanged(); }
        }

    }
}
