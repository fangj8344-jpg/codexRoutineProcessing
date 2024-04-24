#region ------------Infomation------------
/*----------------------------------------------------------------
 * 版权所有 (c) 2024   保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：XJC
 * 公司名称：
 * 命名空间：UtilityTools.Core.Helper
 * 唯一标识：9b060d72-6eac-4f4a-8cae-3e21cd7f999c
 * 文件名：ExcelHelper
 * 当前用户域：XJC
 * 
 * 创建者：xjc
 * 电子邮箱：xxxx@hotmail.com
 * 创建时间：2024/3/21 16:35:33
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

using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityTools.Core.Helper
{
    public static class ExcelHelper
    {
        public static DataTable GetDataGrid(string path)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            try
            {
                using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    string ext = Path.GetExtension(path);
                    switch (ext)
                    {
                        case ".csv":
                            {
                                var reader = ExcelReaderFactory.CreateCsvReader(stream);
                                var result = reader.AsDataSet();
                                return result.Tables[0];
                            }
                        case ".xlsx":
                            {
                                var reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                                var result = reader.AsDataSet();
                                return result.Tables[0];
                            }
                        default:
                            return null;
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
