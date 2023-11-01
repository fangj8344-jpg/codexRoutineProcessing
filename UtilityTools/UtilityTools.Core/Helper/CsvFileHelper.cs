using CsvHelper;
using System.Collections;
using System.Globalization;
using System.IO;

namespace UtilityTools.Core.Helper
{
    public static class CsvFileHelper
    {
        public static void Write(string output, IEnumerable records, IEnumerable headers)
        {
            new PathHelper(output).parent.MakeDir();

            //using (var writer = new StreamWriter(output, false, Encoding.GetEncoding("GB2312")))
            using (var writer = new StreamWriter(output))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                //csv.WriteRecords(records);
                foreach (var header in headers)
                {
                    csv.WriteField(header);
                }

                csv.NextRecord();
                foreach (var record in records)
                {
                    csv.WriteRecord(record);
                    csv.NextRecord();
                }
            }
        }
    }
}
