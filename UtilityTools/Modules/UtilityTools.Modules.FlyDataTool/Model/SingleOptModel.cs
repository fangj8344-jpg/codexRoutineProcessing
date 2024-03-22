#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.FlyDataTool.Model
 * 唯一标识：247cc593-9944-4661-9780-ca6715991caa
 * 文件名：SingleOptModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/3/21 16:53:55
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

namespace UtilityTools.Modules.FlyDataTool.Model
{
    /// <summary>
    /// 单次开枪模型
    /// </summary>
    public class SingleOptModel : BindableBase
    {
        private ObservableCollection<FlyDataModel> _flyDataModels = new();
        /// <summary>
        /// 飞行数据集合
        /// </summary>
        public ObservableCollection<FlyDataModel> FlyDataModels
        {
            get { return _flyDataModels; }
            set { _flyDataModels = value; RaisePropertyChanged(); }
        }

        private int _flyIndex;
        /// <summary>
        /// 飞行索引
        /// </summary>
        public int FlyIndex
        {
            get { return _flyIndex; }
            set { _flyIndex = value; RaisePropertyChanged(); }
        }

        private string _title = "";
        /// <summary>
        /// 飞行标题
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { _title = value; RaisePropertyChanged(); }
        }


        private DateTime _beginTime;
        /// <summary>
        /// 起始飞行时间
        /// </summary>
        public DateTime BeginTime
        {
            get { return _beginTime; }
            set { _beginTime = value; RaisePropertyChanged(); }
        }

        private DateTime _endTime;
        /// <summary>
        /// 结束飞行时间
        /// </summary>
        public DateTime EndTime
        {
            get { return _endTime; }
            set { _endTime = value; RaisePropertyChanged(); }
        }

        private double _flyTime;
        /// <summary>
        /// 飞行时长，单位s
        /// </summary>
        public double FlyTime
        {
            get { return _flyTime; }
            set { _flyTime = value; RaisePropertyChanged(); }
        }

        private double _aveFilaR;
        /// <summary>
        /// 平均灯丝电阻
        /// </summary>
        public double AveFilaR
        {
            get { return _aveFilaR; }
            set { _aveFilaR = value; RaisePropertyChanged(); }
        }

    }
}
