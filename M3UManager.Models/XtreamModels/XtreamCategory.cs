using System.Text.Json.Serialization;

namespace M3UManager.Models.XtreamModels
{
    public class XtreamCategory
    {
        [JsonPropertyName("category_id")]
        public string CategoryId { get; set; } = string.Empty;
        
        [JsonPropertyName("category_name")]
        public string CategoryName { get; set; } = string.Empty;
        
        [JsonPropertyName("parent_id")]
        public int ParentId { get; set; }
    }
}
