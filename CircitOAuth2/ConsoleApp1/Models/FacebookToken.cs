using Newtonsoft.Json;

namespace ConsoleApp1.Models
{
    public class FacebookToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        /// <summary>
        /// The number of seconds remaining until the token expires
        /// </summary>
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
