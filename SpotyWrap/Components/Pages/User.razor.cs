using SpotyWrap.Components.Classes;
using SpotyWrap.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.JSInterop;
using System.Text;
using Microsoft.Extensions.Options;

namespace SpotyWrap.Components.Pages
{
    public partial class User
    {
        private string accessToken;
        private UserData userData;
        private bool isLoaded = false;
        private string codeVerifier;
        private readonly SpotifySettings _spotifySettings;

        [Microsoft.AspNetCore.Components.Inject]
        private IOptions<SpotifySettings> SpotifyOptions { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("checkForSpotifyCallback");

                // Check for authorization code first
                var authCode = await JSRuntime.InvokeAsync<string>("getSpotifyAuthCode");

                if (!string.IsNullOrEmpty(authCode))
                {
                    // Exchange code for token
                    await ExchangeCodeForToken(authCode);
                }
                else
                {
                    // Try to get access token from cookie
                    accessToken = await JSRuntime.InvokeAsync<string>("getSpotifyAccessToken");
                }

                if (!string.IsNullOrEmpty(accessToken))
                {
                    await LoadUserDataAsync();
                }

                isLoaded = true;
                StateHasChanged();
            }
        }

        public void SetAccessToken(string token)
        {
            accessToken = token;
        }

        public async Task Login()
        {
            var clientId = SpotifyOptions.Value.ClientId;

            // Get the full current URL and construct redirect URI
            var uri = new Uri(NavigationManager.Uri);

            // Replace localhost with 127.0.0.1 for Spotify compatibility
            var host = uri.Host == "localhost" ? "127.0.0.1" : uri.Host;
            var redirectUri = $"{uri.Scheme}://{host}:{uri.Port}/user";

            // Generate PKCE code verifier and challenge
            codeVerifier = await JSRuntime.InvokeAsync<string>("generateCodeVerifier");
            var codeChallenge = await JSRuntime.InvokeAsync<string>("generateCodeChallenge", codeVerifier);

            // Store code verifier for later use
            await JSRuntime.InvokeVoidAsync("sessionStorage.setItem", "code_verifier", codeVerifier);

            var scopes = "user-read-private user-read-email user-top-read user-library-read";
            var authUrl = $"https://accounts.spotify.com/authorize?" +
       $"response_type=code&" +
      $"client_id={clientId}&" +
      $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
 $"scope={Uri.EscapeDataString(scopes)}&" +
                $"code_challenge_method=S256&" +
     $"code_challenge={codeChallenge}";

     Console.WriteLine($"Redirect URI: {redirectUri}");
      await JSRuntime.InvokeVoidAsync("console.log", $"Auth URL: {authUrl}");

       NavigationManager.NavigateTo(authUrl, true);
 }

        private async Task ExchangeCodeForToken(string code)
  {
     var clientId = SpotifyOptions.Value.ClientId;

   // Get the redirect URI again
            var uri = new Uri(NavigationManager.Uri);
   var host = uri.Host == "localhost" ? "127.0.0.1" : uri.Host;
      var redirectUri = $"{uri.Scheme}://{host}:{uri.Port}/user";

  // Get code verifier from session storage
     codeVerifier = await JSRuntime.InvokeAsync<string>("sessionStorage.getItem", "code_verifier");

if (string.IsNullOrEmpty(codeVerifier))
  {
     Console.WriteLine("Code verifier not found");
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
        // Store token in cookie
         var expiresIn = tokenResponse.expires_in;
         await JSRuntime.InvokeVoidAsync("eval",
      $"document.cookie = 'spotify_access_token={tokenResponse.access_token}; max-age={expiresIn}; path=/; SameSite=Lax'");

      accessToken = tokenResponse.access_token;

// Clear code verifier
       await JSRuntime.InvokeVoidAsync("sessionStorage.removeItem", "code_verifier");
   }
      }
    else
{
     var error = await response.Content.ReadAsStringAsync();
   Console.WriteLine($"Token exchange failed: {error}");
  }
  }
 catch (Exception ex)
  {
     Console.WriteLine($"Error exchanging code for token: {ex.Message}");
   }
        }

        public async Task Logout()
        {
  await JSRuntime.InvokeVoidAsync("clearSpotifyAccessToken");
   accessToken = null;
     userData = null;
            StateHasChanged();
        }

      private async Task LoadUserDataAsync()
        {
     try
            {
   var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
     var response = await client.GetAsync("https://api.spotify.com/v1/me");

   var responseBody = await response.Content.ReadAsStringAsync();
          Console.WriteLine($"Spotify API Response: {responseBody}");

  if (response.IsSuccessStatusCode)
 {
var options = new JsonSerializerOptions
         {
           PropertyNameCaseInsensitive = true
          };

   userData = JsonSerializer.Deserialize<UserData>(responseBody, options);

   if (userData != null)
     {
            Console.WriteLine($"User loaded: {userData.DisplayName}");
          }
          else
         {
              Console.WriteLine("UserData deserialization returned null");
        }
        }
        else
    {
    Console.WriteLine($"API Error: {response.StatusCode} - {responseBody}");
      }
   }
       catch (Exception ex)
   {
          Console.WriteLine($"Error loading user data: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
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
