using System.Text.Json.Serialization;

namespace M3UManager.Models.XtreamModels
{
    public class XtreamChannel
    {
        [JsonPropertyName("num")]
        public int Num { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("stream_type")]
        public string StreamType { get; set; } = string.Empty;
        
        [JsonPropertyName("stream_id")]
        public int StreamId { get; set; }
        
        [JsonPropertyName("stream_icon")]
        public string StreamIcon { get; set; } = string.Empty;
        
        [JsonPropertyName("epg_channel_id")]
        public string EpgChannelId { get; set; } = string.Empty;
        
        [JsonPropertyName("added")]
        public string Added { get; set; } = string.Empty;
        
        [JsonPropertyName("category_id")]
        public string CategoryId { get; set; } = string.Empty;
        
        [JsonPropertyName("custom_sid")]
        public string CustomSid { get; set; } = string.Empty;
        
        [JsonPropertyName("tv_archive")]
        public int TvArchive { get; set; }
        
        [JsonPropertyName("direct_source")]
        public string DirectSource { get; set; } = string.Empty;
        
        [JsonPropertyName("tv_archive_duration")]
        public int TvArchiveDuration { get; set; }
    }
}
