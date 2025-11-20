using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UtilityTools.Modules.FDC12CHVBox.Rule
{
    public class RangeValidationRule : ValidationRule
    {
        public int Min { get; set; } = 5;
        public int Max { get; set; } = 500;
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!int.TryParse(value ? .ToString(),out int inputValue))
            {
                return new ValidationResult(false ,"请输入有效的整数");
            }
            if (inputValue < Min || inputValue > Max)
            {
                return new ValidationResult(false,$"值必须在{Min}到{Max}之间");
            }
            return ValidationResult.ValidResult;
        }
    }
}
