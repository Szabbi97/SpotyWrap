using System.Text.Json.Serialization;

namespace SpotyWrap.Components.Classes
{
    public class Artist
    {
      [JsonPropertyName("id")]
public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("uri")]
        public string Uri { get; set; }

        [JsonPropertyName("external_urls")]
        public ExternalUrls ExternalUrls { get; set; }
    }
}
