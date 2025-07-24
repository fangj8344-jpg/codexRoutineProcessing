#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Modules.CCSTool.Model
 * 唯一标识：d081442d-082a-4e8d-9236-b735ea972651
 * 文件名：CCSModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/5/21 9:42:21
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

using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityTools.Core.Model;

namespace UtilityTools.Modules.CCSTool.Model
{
    public class CCSModel : BindableBase
    {
        public CCSModel() 
        {
            WobbleCommand = new DelegateCommand(Wobble);
        }   
        private bool _isWobble = false;
        private int _maxValue;
        private int _minValue;

        
        private System.Timers.Timer _timer;
        
        private int _scale = 1;
        private int _coarseValue;
        private string _title = string.Empty;
        private int _oldValue;
        private IntSliderInfoModel _ch7;
        public string Title
        {
            get { return _title; }
            set { _title = value; RaisePropertyChanged(); }
        }
        private int _width = 1000;
        public int Width
        {
            get { return _width; }
            set { _width = value; RaisePropertyChanged(); }
        }
        private int _interval = 100;
        public int Interval
        {
            get { return _interval; }
            set { _interval = value; RaisePropertyChanged(); }
        }
        private int _step = 100;
        public int Step
        {
            get { return _step; }
            set { _step = value; RaisePropertyChanged(); }
        }
        
        private ObservableCollection<IntSliderInfoModel> _controlItems = new ObservableCollection<IntSliderInfoModel>();
  

        public ObservableCollection<IntSliderInfoModel>  ControlItems
        {
            get { return _controlItems; }
            set { _controlItems = value; RaisePropertyChanged(); }
        }
        
        public DelegateCommand WobbleCommand { get; set; }
        private void Wobble()
        {
            if (_isWobble == false)
            {
                _isWobble = true;
            }
            else
            {
                _isWobble = false;
            }

            if (_isWobble == false)
            {
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer.Dispose();
                    _timer = null;
                }
                if (_ch7.Value != null)
                {
                    _ch7.Value = _oldValue;
                }
            }
            else
            {
               
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer.Dispose();
                    _timer = null;
                }
                _timer = new System.Timers.Timer(Interval);
                _timer.Elapsed += Timer_Elapsed;
                _timer.AutoReset = true;
                _timer.Enabled = true;
                _timer.Start();
                foreach(var item in ControlItems)
                {
                    if (item.Title == "CH7")
                    {
                        _ch7 = item;
                        _oldValue = item.Value;
                    }
                }
                _maxValue = _ch7.Value + Width;
                _minValue = _ch7.Value - Width;
                if (_maxValue > UInt16.MaxValue)
                    _maxValue = UInt16.MaxValue;
                if (_minValue < 0)
                    _minValue = 0;
            }
        }
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var value = _ch7.Value + (_scale * Step);
            if (value >= _maxValue)
            {
                value = _maxValue;
                _scale *= -1;
            }
            if (value <= _minValue)
            {
                value = _minValue;
                _scale *= -1;
            }
            _ch7.Value = value;
        }
    }
}
