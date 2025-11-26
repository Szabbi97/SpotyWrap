using System.Text.Json.Serialization;

namespace SpotyWrap.Components.Classes
{
    public class ExternalUrls
    {
     [JsonPropertyName("spotify")]
  public string Spotify { get; set; }
    }
}
