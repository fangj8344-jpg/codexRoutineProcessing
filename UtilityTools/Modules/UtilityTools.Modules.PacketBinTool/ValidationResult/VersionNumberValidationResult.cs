using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UtilityTools.Modules.PacketBinTool.ValidationResult
{
    public class VersionNumberValidationResult : ValidationRule
    {
        public override System.Windows.Controls.ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || value.ToString().Length == 0)
            {
                return new System.Windows.Controls.ValidationResult(false, "版本号不能为空");
            }
            else
            {
                string[] vn = value.ToString().Split(".");
                if (vn.Length == 2)
                {
                    if (int.TryParse(vn[0], out int majorVN) && int.TryParse(vn[1], out int minorVN))
                    {
                        if (majorVN > 0 && minorVN >= 0)
                        {
                            return new System.Windows.Controls.ValidationResult(true, null);
                        }
                        return new System.Windows.Controls.ValidationResult(false, "请输入正确的版本号");

                    }
                    else
                    {
                        return new System.Windows.Controls.ValidationResult(false, "请按规定的格式输入:主版本号(正整数).次版本号(非负正数)");
                    }
                }
                else
                {
                    return new System.Windows.Controls.ValidationResult(false, "请按规定的格式输入:主版本号.次版本号");
                }  
            }
            
        }
    }
}
