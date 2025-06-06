#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2025   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.InstrumentDataRetriever.Entity
 * 唯一标识：6e16514b-f273-4109-becc-16e34d60b871
 * 文件名：InstrumentInfo
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2025/5/16 14:03:30
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
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Modules.InstrumentDataRetriever.Entity
{
    public class InstrumentInfo : BindableBase
    {
        #region ------------Constructor------------
        public InstrumentInfo()
        {

        }
        #endregion

        #region ------------PrivateMethod------------
        private double DoubleValidValue(double value)
        {
            if (double.IsNaN(value))
                return 0f;
            return value;
        }

        private double FloatValidValue(float value)
        {
            if (float.IsNaN(value))
                return 0f;
            return value;
        }
        #endregion

        #region ------------Property------------
        private int _id;
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id
        {
            get { return _id; }
            set { _id = value; RaisePropertyChanged(); }
        }

        private DateTime _time;
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time
        {
            get { return _time; }
            set { _time = value; RaisePropertyChanged(); }
        }

        private double _gunVacuum;
        /// <summary>
        /// 枪头真空
        /// </summary>
        public double GunVacuum
        {
            get { return _gunVacuum; }
            set
            {
                _gunVacuum = value;
                RaisePropertyChanged();
            }
        }

        private double _samVacuum;
        /// <summary>
        /// 样品仓真空
        /// </summary>
        public double SamVacuum
        {
            get { return _samVacuum; }
            set { _samVacuum = value; RaisePropertyChanged(); }
        }

        private bool _isOpenGun;
        /// <summary>
        /// 是否开枪
        /// </summary>
        public bool IsOpenGun
        {
            get { return _isOpenGun; }
            set { _isOpenGun = value; RaisePropertyChanged(); }
        }

        private double _accVol;
        /// <summary>
        /// 实际加速电压
        /// </summary>
        public double AccVol
        {
            get { return _accVol; }
            set { _accVol = value; RaisePropertyChanged(); }
        }

        private double _emissCur;
        /// <summary>
        /// 实际发射电流
        /// </summary>
        public double EmissCur
        {
            get { return _emissCur; }
            set { _emissCur = value; RaisePropertyChanged(); }
        }

        private double _filaR;
        /// <summary>
        /// 实际灯丝电阻
        /// </summary>
        public double FilaR
        {
            get { return _filaR; }
            set { _filaR = value; RaisePropertyChanged(); }
        }

        private double _gridVol;
        /// <summary>
        /// 实际栅极电压
        /// </summary>
        public double GridVol
        {
            get { return _gridVol; }
            set { _gridVol = value; RaisePropertyChanged(); }
        }

        private string _hvState;
        /// <summary>
        /// 高压状态
        /// </summary>
        public string HvState
        {
            get { return _hvState; }
            set { _hvState = value; RaisePropertyChanged(); }
        }

        private int _seGain;
        /// <summary>
        /// SE增益
        /// </summary>
        public int SeGain
        {
            get { return _seGain; }
            set { _seGain = value; RaisePropertyChanged(); }
        }

        private bool _seHvEnable;
        /// <summary>
        /// SE高压输出使能
        /// </summary>
        public bool SeHvEnable
        {
            get { return _seHvEnable; }
            set { _seHvEnable = value; RaisePropertyChanged(); }
        }

        private double _collectVol;
        /// <summary>
        /// 采集电压
        /// </summary>
        public double CollectVol
        {
            get { return _collectVol; }
            set { _collectVol = value; RaisePropertyChanged(); }
        }

        private double _scinVol;
        /// <summary>
        /// 倍增体电压
        /// </summary>
        public double ScinVol
        {
            get { return _scinVol; }
            set { _scinVol = value; RaisePropertyChanged(); }
        }

        private double _decVol;
        /// <summary>
        /// 减速电压
        /// </summary>
        public double DecVol
        {
            get { return _decVol; }
            set { _decVol = value; RaisePropertyChanged(); }
        }

        private double _collectCur;
        /// <summary>
        /// 采集电流反馈
        /// </summary>
        public double CollectCur
        {
            get { return _collectCur; }
            set { _collectCur = value; RaisePropertyChanged(); }
        }

        private double _scinCur;
        /// <summary>
        /// 倍增体电流反馈
        /// </summary>
        public double ScinCur
        {
            get { return _scinCur; }
            set { _scinCur = value; RaisePropertyChanged(); }
        }

        private double _decCur;
        /// <summary>
        /// 减速电流反馈
        /// </summary>
        public double DecCur
        {
            get { return _decCur; }
            set { _decCur = value; RaisePropertyChanged(); }
        }

        private double _temperature1;
        /// <summary>
        /// 温度反馈
        /// </summary>
        public double Temperature1
        {
            get { return _temperature1; }
            set { _temperature1 = value; RaisePropertyChanged(); }
        }

        private double _temperature2;
        /// <summary>
        /// 温度反馈
        /// </summary>
        public double Temperature2
        {
            get { return _temperature2; }
            set { _temperature2 = value; RaisePropertyChanged(); }
        }

        #endregion

    }
}
