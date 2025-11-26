using SpotyWrap.Components.Classes;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.JSInterop;

namespace SpotyWrap.Components.Pages
{
    public partial class AllLiked : IAsyncDisposable
    {
        private string accessToken;
        private LikedSongsResponse likedSongsData;
        private List<SavedTrack> allTracks = new();
        private bool isLoaded = false;
        private bool isLoadingMore = false;
        private string nextUrl = null;
        private DotNetObjectReference<AllLiked> dotNetHelper;
        private IJSObjectReference scrollModule;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                accessToken = await JSRuntime.InvokeAsync<string>("getSpotifyAccessToken");

                if (!string.IsNullOrEmpty(accessToken))
                {
                    await LoadLikedSongs();

                    await SetupInfiniteScroll();
                }

                isLoaded = true;
                StateHasChanged();
            }
        }

        private async Task SetupInfiniteScroll()
        {
            try
            {
                dotNetHelper = DotNetObjectReference.Create(this);
                scrollModule = await JSRuntime.InvokeAsync<IJSObjectReference>("setupInfiniteScroll", dotNetHelper);
            }
            catch (Exception)
            {
                // Error setting up infinite scroll
            }
        }

        [JSInvokable]
        public async Task LoadMoreSongsFromJS()
        {
            await LoadMoreSongs();
        }

        private async Task LoadLikedSongs()
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.GetAsync("https://api.spotify.com/v1/me/tracks?limit=50");
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
                        allTracks.AddRange(likedSongsData.Items);
                        nextUrl = likedSongsData.Next;
                    }
                }
            }
            catch (Exception)
            {
                // Error loading liked songs
            }
        }

        public async Task LoadMoreSongs()
        {
            if (isLoadingMore || string.IsNullOrEmpty(nextUrl))
            {
                return;
            }

            isLoadingMore = true;
            StateHasChanged();

            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.GetAsync(nextUrl);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var moreData = JsonSerializer.Deserialize<LikedSongsResponse>(responseBody, options);

                    if (moreData != null)
                    {
                        allTracks.AddRange(moreData.Items);
                        nextUrl = moreData.Next;
                    }
                }
            }
            catch (Exception)
            {
                // Error loading more songs
            }
            finally
            {
                isLoadingMore = false;
                StateHasChanged();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (scrollModule != null)
            {
                await scrollModule.DisposeAsync();
            }

            dotNetHelper?.Dispose();
        }
    }
}
