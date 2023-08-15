#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.UserControls.ViewModels
 * 唯一标识：c3350745-2175-4e33-875f-ba869139e4e5
 * 文件名：IPTextBoxViewModel
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/14 18:10:39
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

namespace UtilityTools.UserControls.ViewModels
{
    public class IPTextBoxViewModel :BindableBase
    {
        #region ------------Constructor------------
        public IPTextBoxViewModel()
        {
            
        }
        #endregion

        #region ------------Field------------
        private string _part1;
        private string _part2;
        private string _part3;
        private string _part4;
        private bool _isPart1Focused;
        private bool _isPart2Focused;
        private bool _isPart3Focused;
        private bool _isPart4Focused;
        #endregion

        #region ------------Property------------
        /// <summary>
        /// IP地址
        /// </summary>
        public string AddressText
        {
            get { return $"{Part1 ?? "0"}.{Part2 ?? "0"}.{Part3 ?? "0"}.{Part4 ?? "0"}"; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    return;
                var parts = value.Split('.');

                if (int.TryParse(parts[0], out var num0))
                {
                    Part1 = num0.ToString();
                }

                if (int.TryParse(parts[1], out var num1))
                {
                    Part2 = num1.ToString();
                }

                if (int.TryParse(parts[2], out var num2))
                {
                    Part3 = num2.ToString();
                }

                if (int.TryParse(parts[3], out var num3))
                {
                    Part4 = num3.ToString();
                }
            }
        }

        /// <summary>
        /// IP地址第一部分
        /// </summary>
        public string Part1
        {
            get { return _part1; }
            set
            {
                _part1 = value;
                RaisePropertyChanged();

                SetFocus(true, false, false, false);
                AddressChanged?.Invoke(this, EventArgs.Empty);

                var moveNext = CanMoveNext(ref _part1);
                if (moveNext)
                {
                    SetFocus(false, true, false, false);
                }
            }
        }

        /// <summary>
        /// IP地址第二部分
        /// </summary>
        public string Part2
        {
            get { return _part2; }
            set
            {
                _part2 = value;
                RaisePropertyChanged();

                SetFocus(false, true, false, false);
                AddressChanged?.Invoke(this, EventArgs.Empty);

                var moveNext = CanMoveNext(ref _part2);
                if (moveNext)
                {
                    SetFocus(false, false, true, false);
                }
            }
        }

        /// <summary>
        /// IP地址第三部分
        /// </summary>
        public string Part3
        {
            get { return _part3; }
            set
            {
                _part3 = value;
                RaisePropertyChanged();

                SetFocus(false, false, true, false);
                AddressChanged?.Invoke(this, EventArgs.Empty);

                var moveNext = CanMoveNext(ref _part3);
                if (moveNext)
                {
                    SetFocus(false, false, false, true);
                }
            }
        }

        /// <summary>
        /// IP地址第四部分
        /// </summary>
        public string Part4
        {
            get { return _part4; }
            set
            {
                _part4 = value;
                RaisePropertyChanged();

                SetFocus(false, false, false, true);
                AddressChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 第一部分是否获取焦点
        /// </summary>
        public bool IsPart1Focused
        {
            get { return _isPart1Focused; }
            set
            {
                _isPart1Focused = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 第二部分是否获取焦点
        /// </summary>
        public bool IsPart2Focused
        {
            get { return _isPart2Focused; }
            set
            {
                _isPart2Focused = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 第三部分是否获取焦点
        /// </summary>
        public bool IsPart3Focused
        {
            get { return _isPart3Focused; }
            set
            {
                _isPart3Focused = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 第四部分是否获取焦点
        /// </summary>
        public bool IsPart4Focused
        {
            get { return _isPart4Focused; }
            set
            {
                _isPart4Focused = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region ------------Event------------
        public event EventHandler AddressChanged;
        #endregion

        #region ------------PublicMethod------------
        #endregion

        #region ------------PrivateMethod------------
        private bool CanMoveNext(ref string part)
        {
            bool moveNext = false;

            if (!string.IsNullOrWhiteSpace(part))
            {
                if (part.Length >= 3)
                {
                    moveNext = true;
                }

                if (part.EndsWith("."))
                {
                    moveNext = true;
                    part = part.Replace(".", "");
                }
            }

            return moveNext;
        }

        private void SetFocus(bool part1, bool part2, bool part3, bool part4)
        {
            IsPart1Focused = part1;
            IsPart2Focused = part2;
            IsPart3Focused = part3;
            IsPart4Focused = part4;
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
