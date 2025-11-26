using System.Text.Json.Serialization;

namespace SpotyWrap.Components.Classes
{
    public class SavedTrack
 {
        [JsonPropertyName("added_at")]
  public DateTime AddedAt { get; set; }

      [JsonPropertyName("track")]
        public Track Track { get; set; }
    }
}
