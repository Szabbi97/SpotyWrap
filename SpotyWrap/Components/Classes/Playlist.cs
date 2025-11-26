using System.Text.Json.Serialization;

namespace SpotyWrap.Components.Classes
{
    public class Playlist
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("public")]
        public bool Public { get; set; }

        [JsonPropertyName("collaborative")]
        public bool Collaborative { get; set; }

        [JsonPropertyName("images")]
        public List<SpotifyImage> Images { get; set; } = new();

        [JsonPropertyName("owner")]
        public PlaylistOwner Owner { get; set; }

        [JsonPropertyName("tracks")]
        public PlaylistTracks Tracks { get; set; }

        [JsonPropertyName("uri")]
        public string Uri { get; set; }

        [JsonPropertyName("external_urls")]
        public ExternalUrls ExternalUrls { get; set; }

        [JsonPropertyName("snapshot_id")]
        public string SnapshotId { get; set; }
    }
}