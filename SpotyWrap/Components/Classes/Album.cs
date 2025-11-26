using System.Text.Json.Serialization;

namespace SpotyWrap.Components.Classes
{
    public class Album
    {
        [JsonPropertyName("id")]
     public string Id { get; set; }

        [JsonPropertyName("name")]
 public string Name { get; set; }

        [JsonPropertyName("images")]
   public List<SpotifyImage> Images { get; set; } = new();

        [JsonPropertyName("release_date")]
 public string ReleaseDate { get; set; }

   [JsonPropertyName("total_tracks")]
        public int TotalTracks { get; set; }

        [JsonPropertyName("uri")]
        public string Uri { get; set; }

    [JsonPropertyName("external_urls")]
public ExternalUrls ExternalUrls { get; set; }
    }
}
