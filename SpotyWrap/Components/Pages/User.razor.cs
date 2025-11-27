using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using SpotyWrap.Components.Classes;
using SpotyWrap.Configuration;
using SpotyWrap.Services;

namespace SpotyWrap.Components.Pages
{
    public partial class User
    {
        [Inject] private IOptions<SpotifySettings> SpotifyOptions { get; set; }
        [Inject] private AuthStateService AuthStateService { get; set; }
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private IJSRuntime JSRuntime { get; set; }

        private UserData? userData => AuthStateService.UserData;
        private bool isLoaded = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("checkForSpotifyCallback");

                var authCode = await JSRuntime.InvokeAsync<string>("getSpotifyAuthCode");

                if (!string.IsNullOrEmpty(authCode))
                {
                    await AuthStateService.ExchangeCodeForTokenAsync(authCode, SpotifyOptions.Value.ClientId, "/user");
                }
                else
                {
                    await AuthStateService.InitializeAsync();
                }

                isLoaded = true;
                StateHasChanged();
            }
        }

        public async Task Login()
        {
            var authUrl = await AuthStateService.LoginAsync(SpotifyOptions.Value.ClientId, "/user");
            NavigationManager.NavigateTo(authUrl, true);
        }

        public async Task Logout()
        {
            await AuthStateService.ClearAuthenticationAsync();
            StateHasChanged();
        }
    }
}
