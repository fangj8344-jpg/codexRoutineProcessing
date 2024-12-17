using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UtilityTools.Modules.HvController.ValidationResult
{
    public class StepValueValidationResult : ValidationRule
    {
        public override System.Windows.Controls.ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is ushort)
            {
                var stepValue = (ushort)value;
                if (stepValue > 16 || stepValue < 1)
                {
                    return new System.Windows.Controls.ValidationResult(false, "请输入1-16的整数");
                }
                else 
                {
                    return new System.Windows.Controls.ValidationResult(true, null);
                }
            }
            else
            {
                return new System.Windows.Controls.ValidationResult(false, "请输入1-16的整数");
            }
        
        }
    }
}
