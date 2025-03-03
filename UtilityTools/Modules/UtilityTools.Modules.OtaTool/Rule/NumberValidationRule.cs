using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UtilityTools.Modules.OtaTool.Rule
{
    internal class NumberValidationRule : ValidationRule
    {
        
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                var number = Convert.ToInt32((string)value);
                if (number < 0)
                {
                    return new ValidationResult(false, "请输入正确的端口号");
                }
                else
                {
                    return new ValidationResult(true,"");
                }
            }
            catch 
            {
                return new ValidationResult(false,"请输入正确的端口号");
            }

        }
    }
}
