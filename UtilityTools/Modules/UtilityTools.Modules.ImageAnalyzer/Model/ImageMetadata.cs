using MetadataExtractor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace UtilityTools.Modules.ImageAnalyzer.Model
{
    public class Zem15MetaData
    {
        [JsonProperty("acc_voltage")]
        public int AccVoltage { get; set; }

        [JsonProperty("avg_length")]
        public int AvgLength { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("detector")]
        public string Detector { get; set; }

        [JsonProperty("field_size")]
        public float FieldSize { get; set; }

        [JsonProperty("firmware_version")]
        public string FirmwareVersion { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("position")]
        public string Position { get; set; }

        [JsonProperty("pressure")]
        public string Pressure { get; set; }

        [JsonProperty("zoom")]
        public int Zoom { get; set; }
    }


    class ImageMetadata
    {

        public static Zem15MetaData ParseMetadataZem15(IReadOnlyList<Directory> directories)
        {
            var descriptions = directories.Where(d => d.Name == "PNG-tEXt").Select(d => d.Tags[0].Description).ToList();
            var d = descriptions.ToDictionary(d => d.Split(':', 2)[0], d => d.Split(':', 2)[1].Trim());
            Dictionary<string, string> dict = new Dictionary<string, string>()
            {
                {"acc_voltage", d["ACC Voltage"]},
                {"avg_length", d["AvLenth"]},
                {"date", d["Date"]},
                {"detector", d["Detector"]},
                {"field_size", d["Field Size(um)"]},
                {"firmware_version", d.TryGetValue("FirmwareVersion", out var result) ? result : "UNK" },
                {"model", d["Model"]},
                {"zoom", d["Zoom"]},
                // {"position", d.text["Position"].split(","))},
                // {"pressure", d if im.text["Pressure"] == "***" else im.text["Pressure"]},
            };

            var json = JsonConvert.SerializeObject(dict, Formatting.Indented);
            var metadata = JsonConvert.DeserializeObject<Zem15MetaData>(json);
            return metadata;
        }

        public static Zem15MetaData ParseMetadataZem20(IReadOnlyList<Directory> directories)
        {
            var descriptions = directories.Where(d => d.Name == "PNG-zTXt").Select(d => d.Tags[0].Description).Where(d => d.StartsWith("comment:")).ToList();
            var xdoc = XDocument.Parse(descriptions[0][8..]);
            var d = xdoc.Root.Elements().ToDictionary(d => d.Name.LocalName, d => d.Value);
            Dictionary<string, string> dict = new Dictionary<string, string>()
            {
                {"model", d["Name"]},
                {"detector", d["Detecter"]},
                {"acc_voltage", d["HighVol"][0..^2]},
                {"avg_length", d.TryGetValue("AvLenth", out var result) ? result : "8"}, // 默认给8点平均，后续需要保持该值
            };

            var json = JsonConvert.SerializeObject(dict, Formatting.Indented);
            var metadata = JsonConvert.DeserializeObject<Zem15MetaData>(json);
            return metadata;
        }

        public static Zem15MetaData ReadMetadata(string imagePath)
        {
            var directories = ImageMetadataReader.ReadMetadata(imagePath);

            try
            {
                return ParseMetadataZem15(directories);
            }
            catch
            {

            }


            try
            {
                return ParseMetadataZem20(directories);
            }
            catch
            {

            }

            throw new Exception("未找到元数据");
        }
    }
}
