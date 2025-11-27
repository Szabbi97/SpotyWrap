using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using SpotyWrap.Components.Classes;
using SpotyWrap.Configuration;
using SpotyWrap.Services;

namespace SpotyWrap.Components.Layout
{
    public partial class ProfileDropdown : IDisposable
    {
        [Parameter] public EventCallback OnDropdownStateChanged { get; set; }

        [Inject] private AuthStateService AuthStateService { get; set; }
        [Inject] private IOptions<SpotifySettings> SpotifyOptions { get; set; }
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private IJSRuntime JSRuntime { get; set; }

        private bool isAuthenticated => AuthStateService.IsAuthenticated;
        private UserData? userData => AuthStateService.UserData;
        private bool isProfileDropdownOpen = false;
        private string codeVerifier;

        protected override void OnInitialized()
        {
            AuthStateService.OnChange += OnAuthStateChanged;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await AuthStateService.InitializeAsync();
                StateHasChanged();
            }
        }

        private void OnAuthStateChanged()
        {
            InvokeAsync(StateHasChanged);
        }

        private async Task ToggleProfileDropdown()
        {
            isProfileDropdownOpen = !isProfileDropdownOpen;
            await OnDropdownStateChanged.InvokeAsync();
        }

        private async Task HandleFocusOut(FocusEventArgs args)
        {
            await Task.Delay(100);
            isProfileDropdownOpen = false;
            StateHasChanged();
        }

        public async Task Login()
        {
            var clientId = SpotifyOptions.Value.ClientId;

            var uri = new Uri(NavigationManager.Uri);

            var host = uri.Host == "localhost" ? "127.0.0.1" : uri.Host;
            var redirectUri = $"{uri.Scheme}://{host}:{uri.Port}/";

            codeVerifier = await JSRuntime.InvokeAsync<string>("generateCodeVerifier");
            var codeChallenge = await JSRuntime.InvokeAsync<string>("generateCodeChallenge", codeVerifier);

            await JSRuntime.InvokeVoidAsync("sessionStorage.setItem", "code_verifier", codeVerifier);

            var scopes = "user-read-private user-read-email user-top-read user-library-read playlist-modify-public playlist-modify-private";
            var authUrl = $"https://accounts.spotify.com/authorize?" +
                $"response_type=code&" +
                $"client_id={clientId}&" +
                $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                $"scope={Uri.EscapeDataString(scopes)}&" +
                $"code_challenge_method=S256&" +
                $"code_challenge={codeChallenge}";

            NavigationManager.NavigateTo(authUrl, true);
        }

        public async Task Logout()
        {
            await AuthStateService.ClearAuthenticationAsync();
            isProfileDropdownOpen = false;
            NavigationManager.NavigateTo("/", true);
        }

        public void Dispose()
        {
            AuthStateService.OnChange -= OnAuthStateChanged;
        }
    }
}
