#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2023   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Rules
 * 唯一标识：9d4cfd51-00e4-4f3e-a9d1-f3356c41c6ac
 * 文件名：IPRangeValidationRule
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2023/8/14 17:59:16
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UtilityTools.Core.Rules
{
    public class IPRangeValidationRule : ValidationRule
    {
        #region ------------Constructor------------
        public IPRangeValidationRule()
        {

        }
        #endregion

        #region ------------Field------------
        private int _max;
        private int _min;
        #endregion

        #region ------------Property------------
        /// <summary>
        /// 最大值
        /// </summary>
        public int Max
        {
            get { return _max; }
            set { _max = value; }
        }

        /// <summary>
        /// 最小值
        /// </summary>
        public int Min
        {
            get { return _min; }
            set { _min = value; }
        }
        #endregion

        #region ------------PublicMethod------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int val = 0;
            var strVal = (string)value;
            try
            {
                if (strVal.Length > 0)
                {
                    if (strVal.EndsWith("."))
                    {
                        return CheckRanges(strVal.Replace(".", ""));
                    }

                    return CheckRanges(strVal);
                }
            }
            catch (Exception ex)
            {
                return new ValidationResult(false, "Illegal characters or " + ex.Message);
            }

            if ((val < Min) || (val > Max))
            {
                return new ValidationResult(false, "Please enter the value in the range: " + Min + " - " + Max + ".");
            }
            else
            {
                return ValidationResult.ValidResult;
            }
        }
        #endregion

        #region ------------PrivateMethod------------
        private ValidationResult CheckRanges(string strVal)
        {
            if (int.TryParse(strVal, out var res))
            {
                if ((res < Min) || (res > Max))
                {
                    return new ValidationResult(false, "Please enter the value in the range: " + Min + " - " + Max + ".");
                }
                else
                {
                    return ValidationResult.ValidResult;
                }
            }
            else
            {
                return new ValidationResult(false, "Illegal characters entered");
            }
        }
        #endregion

        #region ------------StaticMethod------------
        #endregion
    }
}
