using System.Text.Json.Serialization;

namespace SpotyWrap.Components.Classes
{
    public class UserData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("product")]
        public string Product { get; set; }

        [JsonPropertyName("images")]
        public List<SpotifyImage> Images { get; set; }
    }


}
