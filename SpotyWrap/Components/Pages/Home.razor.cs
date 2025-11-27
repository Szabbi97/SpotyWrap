using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using SpotyWrap.Configuration;
using SpotyWrap.Services;
using System.Text.Json;

namespace SpotyWrap.Components.Pages
{
    public partial class Home
    {
        [Inject] private IOptions<SpotifySettings> SpotifyOptions { get; set; }
        [Inject] private AuthStateService AuthStateService { get; set; }
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private IJSRuntime JSRuntime { get; set; }

        private string codeVerifier;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("checkForSpotifyCallback");

                var authCode = await JSRuntime.InvokeAsync<string>("getSpotifyAuthCode");

                if (!string.IsNullOrEmpty(authCode))
                {
                    await ExchangeCodeForToken(authCode);
                }
            }
        }

        private async Task ExchangeCodeForToken(string code)
        {
            var clientId = SpotifyOptions.Value.ClientId;

            var uri = new Uri(NavigationManager.Uri);
            var host = uri.Host == "localhost" ? "127.0.0.1" : uri.Host;
            var redirectUri = $"{uri.Scheme}://{host}:{uri.Port}/";

            codeVerifier = await JSRuntime.InvokeAsync<string>("sessionStorage.getItem", "code_verifier");

            if (string.IsNullOrEmpty(codeVerifier))
            {
                return;
            }

            var client = new HttpClient();
            var requestBody = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", redirectUri },
                { "client_id", clientId },
                { "code_verifier", codeVerifier }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token")
            {
                Content = new FormUrlEncodedContent(requestBody)
            };

            try
            {
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<SpotifyTokenResponse>(json);

                    if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.access_token))
                    {
                        var expiresIn = tokenResponse.expires_in;
                        await JSRuntime.InvokeVoidAsync("eval",
                     $"document.cookie = 'spotify_access_token={tokenResponse.access_token}; max-age={expiresIn}; path=/; SameSite=Lax'");


                        await JSRuntime.InvokeVoidAsync("sessionStorage.removeItem", "code_verifier");

                        await AuthStateService.SetTokenAsync(tokenResponse.access_token);

                        NavigationManager.NavigateTo("/", true);
                    }
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        private class SpotifyTokenResponse
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string refresh_token { get; set; }
            public string scope { get; set; }
        }
    }
}
