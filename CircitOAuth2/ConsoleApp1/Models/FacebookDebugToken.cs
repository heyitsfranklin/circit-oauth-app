using Newtonsoft.Json;

namespace ConsoleApp1.Models
{
    /// <summary>
    /// Facebook response when retrieving token info
    /// </summary>
    public class FacebookDebugToken
    {
        public FacebookDebugTokenData Data { get; set; }
    }

    public class FacebookDebugTokenData
    {
        [JsonProperty("app_id")]
        public string ApplicationId { get; set; }
        public string Application { get; set; }
        [JsonProperty("expires_at")]
        public string ExpiresAt { get; set; }
        [JsonProperty("user_id")]
        public string UserId { get; set; }
    }
}
