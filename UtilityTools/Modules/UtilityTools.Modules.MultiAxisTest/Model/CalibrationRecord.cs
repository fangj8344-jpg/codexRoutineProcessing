using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UtilityTools.Modules.MultiAxisTest.Model
{
    public class CalibrationRecord
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("device_id")]
        public string DeviceId { get; set; }

        [JsonPropertyName("sample_stage_id")]
        public string SampleStageId { get; set; }

        // 🚨 注意看你的文档，这里返回的是一个 OSS 链接！
        [JsonPropertyName("content")]
        public string ContentUrl { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
