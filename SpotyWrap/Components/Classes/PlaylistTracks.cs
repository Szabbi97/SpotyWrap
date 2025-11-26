using System.Text.Json.Serialization;

namespace SpotyWrap.Components.Classes
{
    public class PlaylistTracks
    {
        [JsonPropertyName("href")]
        public string Href { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
}
