using ImTools;
using MetadataExtractor;
using Newtonsoft.Json;
using NLog;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace UtilityTools.Modules.ImageAnalyzer.Model
{
    public class ZemMetaData : BindableBase
    {
        private int _accVoltage;
        [JsonProperty("acc_voltage")]
        public int AccVoltage 
        {
            get
            {
                return _accVoltage;
            }
            set
            { 
                _accVoltage = value;
                RaisePropertyChanged();
            }
        }

        private int _avgLength;
        [JsonProperty("avg_length")]
        public int AvgLength
        {
            get
            {
                return _avgLength;
            }
            set
            {
                _avgLength = value;
                RaisePropertyChanged();
            }
        }

        private string _date;
        [JsonProperty("date")]
        public string Date
        {
            get
            {
                return _date;
            }
            set
            {
                _date = value;
                RaisePropertyChanged();
            }
        }

        private string _detector;
        [JsonProperty("detector")]
        public string Detector
        {
            get
            {
                return _detector;
            }
            set
            {
                _detector = value;
                RaisePropertyChanged();
            }
        }

        private float _fieldSize;
        [JsonProperty("field_size")]
        public float FieldSize
        {
            get
            {
                return _fieldSize;
            }
            set
            {
                _fieldSize = value;
                RaisePropertyChanged();
            }
        }

        private string _firmwareVersion;
        [JsonProperty("firmware_version")]
        public string FirmwareVersion
        {
            get
            {
                return _firmwareVersion;
            }
            set
            {
                _firmwareVersion = value;
                RaisePropertyChanged();
            }
        }

        private string _model;
        [JsonProperty("model")]
        public string Model
        {
            get
            {
                return _model;
            }
            set
            {
                _model = value;
                RaisePropertyChanged();
            }
        }

        private string _position;
        [JsonProperty("position")]
        public string Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                RaisePropertyChanged();
            }
        }

        private string _pressure;
        [JsonProperty("pressure")]
        public string Pressure
        {
            get
            {
                return _pressure;
            }
            set
            {
                _pressure = value;
                RaisePropertyChanged();
            }
        }

        private int _zoom;
        [JsonProperty("zoom")]
        public int Zoom
        {
            get
            {
                return _zoom;
            }
            set
            {
                _zoom = value;
                RaisePropertyChanged();
            }
        }

        private int _significantBit;
        [JsonProperty("significant_bit")]
        public int SignificantBit
        {
            get
            {
                return _significantBit;
            }
            set
            {
                _significantBit = value;
                RaisePropertyChanged();
            }
        }

        private string _version;
        [JsonProperty("version")]
        public string Version
        {
            get { return _version; }
            set { _version = value; RaisePropertyChanged(); }
        }

        private float _ob;
        [JsonProperty("Ob")]
        public float Ob
        {
            get { return _ob; }
            set { _ob = value; RaisePropertyChanged(); }
        }

        private float _pixelLength;
        [JsonProperty("PixelLength")]
        public float PixelLength
        {
            get { return _pixelLength; }
            set { _pixelLength = value; RaisePropertyChanged(); }
        }

        private string _title;
        [JsonProperty("Title")]
        public string Title
        {
            get { return _title; }
            set { _title = value; RaisePropertyChanged(); }
        }

        private string _note;
        [JsonProperty("Note")]
        public string Note
        {
            get { return _note; }
            set { _note = value; RaisePropertyChanged(); }
        }

        private int _frequency;
        [JsonProperty("Frequency")]
        public int Frequency
        {
            get { return _frequency; }
            set { _frequency = value; RaisePropertyChanged(); }
        }


        public int FreqADC = 40 * 1000000; // 40MHz


        public bool IsSignificantBitValid()
        {
            return 0 < SignificantBit;
        }
    }

    public class SignificantBitParser
    {
        public SignificantBitParser(int SignificantBit)
        {
            if (SignificantBit < 0) return;

            SignificantHigh = (SignificantBit & 0xf0) >> 4;
            SignificantLow = SignificantBit & 0x0f;
            foreach (int j in Enumerable.Range(SignificantLow, SignificantHigh - SignificantLow + 1))
            {
                SignificantMask |= 1 << j;
            }
        }

        public int SignificantMask = 0;
        public int SignificantHigh = -1;
        public int SignificantLow = -1;
    }

    class ImageMetadata
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();



        public static ZemMetaData ParseMetadataZem15(IReadOnlyList<Directory> directories)
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
            var metadata = JsonConvert.DeserializeObject<ZemMetaData>(json);

            string sbk = "Significant bit";
            metadata.SignificantBit = d.ContainsKey(sbk) ? (int)d[sbk][0] : -1;


            return metadata;
        }

        public static ZemMetaData ParseMetadataZem20(IReadOnlyList<Directory> directories)
        {
            var descriptions = directories.Where(d => d.Name == "PNG-zTXt").Select(d => d.Tags[0].Description).Where(d => d.StartsWith("comment:")).ToList();
            var xdoc = XDocument.Parse(descriptions[0][8..]);
            var content = xdoc.ToString();
            byte[] latin1Bytes = Encoding.Latin1.GetBytes(content); // 使用默认编码格式获取字节数组
            string latin1String = Encoding.UTF8.GetString(latin1Bytes); // 将字节数组转换为目标编码格式的字符串
            xdoc = XDocument.Parse(latin1String);
            var d = xdoc.Root.Elements().ToDictionary(d => d.Name.LocalName, d => d.Value);
            Dictionary<string, string> dict = new Dictionary<string, string>()
            {
                {"model", d["Name"]},
                {"version", d["Version"]},
                {"date", d["Date"]},
                {"detector", d["Detecter"]},
                {"Ob", d["OB"][0..^2]},
                {"zoom", d["Mag"][1..]},
                {"acc_voltage", d["HighVol"][0..^2]},
                {"PixelLength", d["PixelLength"][0..^2]},
                {"Title", d["Title"]},
                {"Note", d["Note"]},
                {"position", d["Position"]},
                {"avg_length", d.TryGetValue("AvLenth", out var result) ? result : "8"}, // 默认给8点平均，后续需要保持该值
                {"Frequency", d["Frequency"][0..^2]},
                {"significant_bit", d["DataFlag"]},
            };

            var json = JsonConvert.SerializeObject(dict, Formatting.Indented);
            var metadata = JsonConvert.DeserializeObject<ZemMetaData>(json);

            //metadata.SignificantBit = -1;
            return metadata;
        }

        public static ZemMetaData ReadMetadata(string imagePath)
        {
            var directories = ImageMetadataReader.ReadMetadata(imagePath);

            try
            {
                return ParseMetadataZem15(directories);
            }
            catch (Exception ex)
            {
                LOGGER.Warn(ex, $"ParseMetadataZem15 failed for {imagePath}");
            }


            try
            {
                return ParseMetadataZem20(directories);
            }
            catch (Exception ex)
            {
                LOGGER.Warn(ex, $"ParseMetadataZem20 failed for {imagePath}");
            }

            throw new Exception("未找到元数据");
        }
    }
}
