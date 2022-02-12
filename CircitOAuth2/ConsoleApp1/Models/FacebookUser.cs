using Newtonsoft.Json;
using System;

namespace ConsoleApp1.Models
{
    /// <summary>
    /// Facebook user response
    /// </summary>
    public class FacebookUser
    {
        public string Id { get; set; }
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("last_name")]
        public string LastName { get; set; }
        [JsonProperty("middle_name")]
        public string MiddleName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
