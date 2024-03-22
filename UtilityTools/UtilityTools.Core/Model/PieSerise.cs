#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Model
 * 唯一标识：22a91a28-4e3b-4b77-acc6-d547ac652cb2
 * 文件名：PieSerise
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/3/21 16:10:10
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

namespace UtilityTools.Core.Model
{
    public class PieSerise : BindableBase
    {
        private int _index;

        public int Index
        {
            get { return _index; }
            set { _index = value; RaisePropertyChanged(); }
        }

        private string _title;

        public string Title
        {
            get { return _title; }
            set { _title = value; RaisePropertyChanged(); }
        }

        private Brush _pieColor;

        public Brush PieColor
        {
            get { return _pieColor; }
            set { _pieColor = value; RaisePropertyChanged(); }
        }

        private double _percentage;

        public double Percentage
        {
            get { return _percentage; }
            set { _percentage = value; RaisePropertyChanged(); }
        }

        private DateTime _beginTime;

        public DateTime BeginTime
        {
            get { return _beginTime; }
            set { _beginTime = value; RaisePropertyChanged(); }
        }

        private DateTime _endTime;

        public DateTime EndTime
        {
            get { return _endTime; }
            set { _endTime = value; RaisePropertyChanged(); }
        }


        private double _times;

        public double Times
        {
            get { return _times; }
            set { _times = value; RaisePropertyChanged(); }
        }
    }
}
