namespace M3UManager.Models.XtreamModels
{
    public class XtreamAuthInfo
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ServerUrl { get; set; } = string.Empty;
        
        public static XtreamAuthInfo Parse(string xtreamUrl)
        {
            // Expected format: http://server:port/username/password
            // or http://server:port/get.php?username=user&password=pass&type=m3u_plus
            
            var authInfo = new XtreamAuthInfo();
            
            if (string.IsNullOrWhiteSpace(xtreamUrl))
                return authInfo;
                
            var uri = new Uri(xtreamUrl);
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            
            // Check if it's a query string format
            if (xtreamUrl.Contains("username=") && xtreamUrl.Contains("password="))
            {
                var query = ParseQueryString(uri.Query);
                authInfo.Username = query.TryGetValue("username", out var user) ? user : string.Empty;
                authInfo.Password = query.TryGetValue("password", out var pass) ? pass : string.Empty;
                authInfo.ServerUrl = $"{uri.Scheme}://{uri.Authority}";
            }
            // Check if it's path format
            else if (segments.Length >= 2)
            {
                authInfo.Username = segments[0];
                authInfo.Password = segments[1];
                authInfo.ServerUrl = $"{uri.Scheme}://{uri.Authority}";
            }
            
            return authInfo;
        }

        private static Dictionary<string, string> ParseQueryString(string query)
        {
            var result = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(query))
                return result;

            query = query.TrimStart('?');
            var pairs = query.Split('&');
            
            foreach (var pair in pairs)
            {
                var parts = pair.Split('=');
                if (parts.Length == 2)
                {
                    result[parts[0]] = Uri.UnescapeDataString(parts[1]);
                }
            }
            
            return result;
        }
    }
}
