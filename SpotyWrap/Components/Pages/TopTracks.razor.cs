using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SpotyWrap.Components.Classes;
using SpotyWrap.Services;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SpotyWrap.Components.Pages
{
    public partial class TopTracks
    {
        [Inject] private AuthStateService AuthStateService { get; set; }
        [Inject] private IJSRuntime JSRuntime { get; set; }
        private const string SHORT_TERM = "short_term";
        private const string MEDIUM_TERM = "medium_term";
        private const string LONG_TERM = "long_term";
        private Dictionary<string,string> termOptions = new()
        {
            { "Last 4 Weeks", SHORT_TERM },
            { "Last 6 Months", MEDIUM_TERM },
            { "All Time", LONG_TERM }
        };

        private bool isLoaded = false;
        private bool isDropdownOpen = false;
        private string selectedTimeRange = SHORT_TERM;
        HttpClient client = new HttpClient();
        private List<Track> topTracks = new List<Track>();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await AuthStateService.InitializeAsync();

                if (AuthStateService.IsAuthenticated)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthStateService.AccessToken);
                    topTracks = await GetTopTracks(selectedTimeRange);
                }

                isLoaded = true;
                StateHasChanged();
            }
        }

        private void ToggleDropdown()
        {
            isDropdownOpen = !isDropdownOpen;
        }

        private async Task SelectTimeRange(string timeRange)
        {
            selectedTimeRange = timeRange;
            isDropdownOpen = false;
            topTracks = await GetTopTracks(selectedTimeRange);
            StateHasChanged();
        }

        private async Task<List<Track>> GetTopTracks(string v)
        {
            var response = await client.GetAsync($"https://api.spotify.com/v1/me/top/tracks?time_range={v}&limit=50");
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var topTracksData = JsonSerializer.Deserialize<TopTracksResponse>(responseBody, options);
                if (topTracksData != null)
                {
                    return topTracksData.Items;
                }
            }
            return [];
        }
    }
}
