using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SpotyWrap.Components.Classes;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SpotyWrap.Services
{
    public class AuthStateService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly NavigationManager _navigationManager;
        private string? _accessToken;
        private UserData? _userData;
        private bool _isInitialized = false;

        public event Action? OnChange;

        public AuthStateService(IJSRuntime jsRuntime, NavigationManager navigationManager)
        {
            _jsRuntime = jsRuntime;
            _navigationManager = navigationManager;
        }

        public string? AccessToken => _accessToken;
        public UserData? UserData => _userData;
        public bool IsAuthenticated => !string.IsNullOrEmpty(_accessToken);

        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            _accessToken = await _jsRuntime.InvokeAsync<string>("getSpotifyAccessToken");
            
            if (!string.IsNullOrEmpty(_accessToken))
            {
                await LoadUserDataAsync();
            }

            _isInitialized = true;
        }

        public async Task<string> LoginAsync(string clientId, string? path = null)
        {
            var uri = new Uri(_navigationManager.Uri);
            var redirectPath = string.IsNullOrEmpty(path) ? "/" : path;
            var redirectUri = $"{uri.Scheme}://127.0.0.1:{uri.Port}{redirectPath}";

            var codeVerifier = await _jsRuntime.InvokeAsync<string>("generateCodeVerifier");
            var codeChallenge = await _jsRuntime.InvokeAsync<string>("generateCodeChallenge", codeVerifier);

            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "code_verifier", codeVerifier);

            var scopes = "user-read-private user-read-email user-top-read user-library-read playlist-modify-public playlist-modify-private";
            var authUrl = $"https://accounts.spotify.com/authorize?" +
                $"response_type=code&" +
                $"client_id={clientId}&" +
                $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                $"scope={Uri.EscapeDataString(scopes)}&" +
                $"code_challenge_method=S256&" +
                $"code_challenge={codeChallenge}";

            return authUrl;
        }

        public async Task<bool> ExchangeCodeForTokenAsync(string code, string clientId, string? path = null)
        {
            var uri = new Uri(_navigationManager.Uri);
            var redirectPath = string.IsNullOrEmpty(path) ? "/" : path;
            var redirectHost = uri.Host == "localhost" ? "127.0.0.1" : uri.Host;
            var redirectUri = $"{uri.Scheme}://{redirectHost}:{uri.Port}{redirectPath}";

            var codeVerifier = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "code_verifier");

            if (string.IsNullOrEmpty(codeVerifier))
            {
                return false;
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
                        await _jsRuntime.InvokeVoidAsync("eval",
                            $"document.cookie = 'spotify_access_token={tokenResponse.access_token}; max-age={expiresIn}; path=/; SameSite=Lax'");

                        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "code_verifier");

                        await SetTokenAsync(tokenResponse.access_token);

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if needed
            }

            return false;
        }

        public async Task SetTokenAsync(string token)
        {
            _accessToken = token;
            await LoadUserDataAsync();
            NotifyStateChanged();
        }

        public async Task ClearAuthenticationAsync()
        {
            await _jsRuntime.InvokeVoidAsync("clearSpotifyAccessToken");
            _accessToken = null;
            _userData = null;
            NotifyStateChanged();
        }

        public async Task RefreshTokenAsync()
        {
            _accessToken = await _jsRuntime.InvokeAsync<string>("getSpotifyAccessToken");
            
            if (!string.IsNullOrEmpty(_accessToken) && _userData == null)
            {
                await LoadUserDataAsync();
            }
            
            NotifyStateChanged();
        }

        private async Task LoadUserDataAsync()
        {
            if (string.IsNullOrEmpty(_accessToken))
                return;

            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                var response = await client.GetAsync("https://api.spotify.com/v1/me");
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    _userData = JsonSerializer.Deserialize<UserData>(responseBody, options);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void NotifyStateChanged() => OnChange?.Invoke();

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
