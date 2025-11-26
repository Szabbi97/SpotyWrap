using System.Text.Json.Serialization;

namespace SpotyWrap.Components.Classes
{
    public class SpotifyImage
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("height")]
        public int? Height { get; set; }

        [JsonPropertyName("width")]
        public int? Width { get; set; }
    }
}
