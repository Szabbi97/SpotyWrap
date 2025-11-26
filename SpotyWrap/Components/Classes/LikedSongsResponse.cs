using System.Text.Json.Serialization;

namespace SpotyWrap.Components.Classes
{
    public class LikedSongsResponse
    {
        [JsonPropertyName("href")]
        public string Href { get; set; }

        [JsonPropertyName("items")]
        public List<SavedTrack> Items { get; set; } = new();

        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        [JsonPropertyName("next")]
        public string Next { get; set; }

        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        [JsonPropertyName("previous")]
        public string Previous { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
}