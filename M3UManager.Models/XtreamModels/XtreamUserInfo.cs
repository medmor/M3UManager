using System.Text.Json.Serialization;

namespace M3UManager.Models.XtreamModels
{
    public class XtreamUserInfo
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
        
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
        
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
        
        [JsonPropertyName("auth")]
        public int Auth { get; set; }
        
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
        
        [JsonPropertyName("exp_date")]
        public string ExpDate { get; set; } = string.Empty;
        
        [JsonPropertyName("is_trial")]
        public string IsTrial { get; set; } = string.Empty;
        
        [JsonPropertyName("active_cons")]
        public string ActiveCons { get; set; } = string.Empty;
        
        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = string.Empty;
        
        [JsonPropertyName("max_connections")]
        public string MaxConnections { get; set; } = string.Empty;
        
        [JsonPropertyName("allowed_output_formats")]
        public List<string> AllowedOutputFormats { get; set; } = new List<string>();
    }
}
