using Microsoft.JSInterop;
using SpotyWrap.Components.Classes;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SpotyWrap.Services
{
    public class AuthStateService
    {
        private readonly IJSRuntime _jsRuntime;
        private string? _accessToken;
        private UserData? _userData;
        private bool _isInitialized = false;

        public event Action? OnChange;

        public AuthStateService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
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
            Console.WriteLine($"AuthStateService - Initialized. IsAuthenticated: {IsAuthenticated}");
        }

        public async Task SetTokenAsync(string token)
        {
            _accessToken = token;
            await LoadUserDataAsync();
            NotifyStateChanged();
            Console.WriteLine("AuthStateService - Token set and state notified");
        }

        public async Task ClearAuthenticationAsync()
        {
            await _jsRuntime.InvokeVoidAsync("clearSpotifyAccessToken");
            _accessToken = null;
            _userData = null;
            NotifyStateChanged();
            Console.WriteLine("AuthStateService - Authentication cleared");
        }

        public async Task RefreshTokenAsync()
        {
            _accessToken = await _jsRuntime.InvokeAsync<string>("getSpotifyAccessToken");
            
            if (!string.IsNullOrEmpty(_accessToken) && _userData == null)
            {
                await LoadUserDataAsync();
            }
            
            NotifyStateChanged();
            Console.WriteLine($"AuthStateService - Token refreshed. IsAuthenticated: {IsAuthenticated}");
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
                    Console.WriteLine($"AuthStateService - User data loaded: {_userData?.DisplayName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AuthStateService - Error loading user data: {ex.Message}");
            }
        }

        public void NotifyStateChanged() => OnChange?.Invoke();
    }
}
