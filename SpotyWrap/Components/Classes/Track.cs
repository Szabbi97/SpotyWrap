using System.Text.Json.Serialization;

namespace SpotyWrap.Components.Classes
{
    public class Track
    {
    [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
  public string Name { get; set; }

      [JsonPropertyName("artists")]
        public List<Artist> Artists { get; set; } = new();

 [JsonPropertyName("album")]
        public Album Album { get; set; }

   [JsonPropertyName("duration_ms")]
        public int DurationMs { get; set; }

        [JsonPropertyName("explicit")]
     public bool Explicit { get; set; }

        [JsonPropertyName("popularity")]
        public int Popularity { get; set; }

   [JsonPropertyName("preview_url")]
        public string PreviewUrl { get; set; }

 [JsonPropertyName("uri")]
        public string Uri { get; set; }

        [JsonPropertyName("external_urls")]
        public ExternalUrls ExternalUrls { get; set; }
    }
}
