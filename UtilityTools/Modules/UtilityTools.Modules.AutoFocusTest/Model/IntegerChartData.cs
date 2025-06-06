#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2025   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.AutoFocusTest.Model
 * 唯一标识：abf33837-5748-42d3-b416-8b4a1d38169e
 * 文件名：IntegerChartData
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2025/5/6 10:37:43
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

namespace UtilityTools.Modules.AutoFocusTest.Model
{
    public class IntegerChartData : BindableBase
    {
        public IntegerChartData()
        {

        }

        public IntegerChartData(int key, double value)
        {
            Key = key;
            Value = value;
        }

        public IntegerChartData DeepCopy()
        {
            var model = new IntegerChartData();
            model.Key = Key;
            model.Value = Value;
            return model;
        }

        private int _key;
        /// <summary>
        /// 整数Key
        /// </summary>
        public int Key
        {
            get { return _key; }
            set { _key = value; RaisePropertyChanged(); }
        }

        private double _value;
        /// <summary>
        /// 数据
        /// </summary>
        public double Value
        {
            get { return _value; }
            set { _value = value; RaisePropertyChanged(); }
        }
    }
}
