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
        
        // For live TV and movies
        [JsonPropertyName("stream_id")]
        public int StreamId { get; set; }
        
        // For TV series - the API uses series_id instead of stream_id
        [JsonPropertyName("series_id")]
        public int SeriesId { get; set; }
        
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
        
        // Helper property to get the correct ID based on content type
        public int GetId()
        {
            // For series, use series_id, otherwise use stream_id
            return SeriesId != 0 ? SeriesId : StreamId;
        }
    }
}
