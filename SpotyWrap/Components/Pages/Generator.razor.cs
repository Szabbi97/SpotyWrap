using Microsoft.AspNetCore.Components;
using SpotyWrap.Components.Classes;
using SpotyWrap.Services;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SpotyWrap.Components.Pages
{
    public partial class Generator
    {
        [Inject] private AuthStateService AuthStateService { get; set; }

        private bool isLoaded = false;
        private LikedSongsResponse? likedSongsData;
        private List<SavedTrack> allTracks = new();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await AuthStateService.InitializeAsync();
                isLoaded = true;
                StateHasChanged();
            }
        }

        private async Task LoadLikedSongs(DateTime? dateTime = null)
        {
            if (!AuthStateService.IsAuthenticated)
                return;

            try
            {
                allTracks = new List<SavedTrack>();
                likedSongsData = null;
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthStateService.AccessToken);

                var Url = "https://api.spotify.com/v1/me/tracks?limit=50";

                do
                {
                    var response = await client.GetAsync(Url);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        likedSongsData = JsonSerializer.Deserialize<LikedSongsResponse>(responseBody, options);

                        if (likedSongsData != null)
                        {
                            if (dateTime.HasValue)
                            {
                                var filteredItems = likedSongsData.Items
                                    .Where(item => item.AddedAt.Month == dateTime.Value.Month)
                                    .ToList();
                                allTracks.AddRange(filteredItems);
                                if (filteredItems.Count < likedSongsData.Items.Count)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                allTracks.AddRange(likedSongsData.Items);
                                Url = likedSongsData.Next;
                            }
                        }
                    }
                } while (Url != null);
            }
            catch (Exception)
            {
                // Error loading liked songs
            }
        }

        private async Task GenerateThis()
        {
            await LoadLikedSongs(DateTime.Now);
            await GeneratePlaylist($"{DateTime.Now.Year}.{DateTime.Now.Month}", [.. allTracks.Select(t => t.Track)]);
        }

        private async Task GenerateAll()
        {
            await LoadLikedSongs();

            var tracksByMonth = allTracks
                 .GroupBy(savedTrack => new
                 {
                     Year = savedTrack.AddedAt.Year,
                     Month = savedTrack.AddedAt.Month
                 })
                          .Select(group => new
                          {
                              Name = $"{group.Key.Year}.{group.Key.Month}",
                              Tracks = group.Select(st => st.Track).ToList()
                          })
                .OrderBy(x => x.Name)
             .ToList();

            foreach (var pl in tracksByMonth)
            {
                await GeneratePlaylist(pl.Name, pl.Tracks);
            }
        }

        private async Task GeneratePlaylist(string name, List<Track> tracks)
        {
            if (!AuthStateService.IsAuthenticated)
                return;

            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthStateService.AccessToken);
                
                var userResponse = await client.GetAsync("https://api.spotify.com/v1/me");
                var userResponseBody = await userResponse.Content.ReadAsStringAsync();
                
                if (userResponse.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var userData = JsonSerializer.Deserialize<UserData>(userResponseBody, options);
                    
                    if (userData != null)
                    {
                        var playlistsResponse = await client.GetAsync($"https://api.spotify.com/v1/users/{userData.Id}/playlists");
                        var playlistsBody = await playlistsResponse.Content.ReadAsStringAsync();
                        
                        if (playlistsResponse.IsSuccessStatusCode)
                        {
                            var playlists = JsonSerializer.Deserialize<PlaylistResponse>(playlistsBody, options);
                            if (playlists != null)
                            {
                                var existingPlaylist = playlists.Items.FirstOrDefault(p => p.Name == name);
                                if (existingPlaylist != null)
                                {
                                    return;
                                }
                            }
                        }

                        var playlistData = new
                        {
                            name = name,
                            description = "Generated by SpotyWrap",
                            @public = false
                        };
                        var playlistContent = new StringContent(JsonSerializer.Serialize(playlistData), System.Text.Encoding.UTF8, "application/json");
                        var playlistResponse = await client.PostAsync($"https://api.spotify.com/v1/users/{userData.Id}/playlists", playlistContent);
                        var playlistResponseBody = await playlistResponse.Content.ReadAsStringAsync();
                        
                        if (playlistResponse.IsSuccessStatusCode)
                        {
                            var playlistInfo = JsonSerializer.Deserialize<Playlist>(playlistResponseBody, options);
                            if (playlistInfo != null)
                            {
                                var trackUris = tracks.Select(t => t.Uri).ToList();
                                var addTracksData = new
                                {
                                    uris = trackUris
                                };
                                var addTracksContent = new StringContent(JsonSerializer.Serialize(addTracksData), System.Text.Encoding.UTF8, "application/json");
                                await client.PostAsync($"https://api.spotify.com/v1/playlists/{playlistInfo.Id}/tracks", addTracksContent);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Error generating playlist
            }
        }
    }
}
