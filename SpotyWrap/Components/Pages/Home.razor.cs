using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using SpotyWrap.Configuration;
using SpotyWrap.Services;

namespace SpotyWrap.Components.Pages
{
    public partial class Home
    {
        [Inject] private IOptions<SpotifySettings> SpotifyOptions { get; set; }
        [Inject] private AuthStateService AuthStateService { get; set; }
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private IJSRuntime JSRuntime { get; set; }

        private bool isLoaded = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("checkForSpotifyCallback");

                var authCode = await JSRuntime.InvokeAsync<string>("getSpotifyAuthCode");

                if (!string.IsNullOrEmpty(authCode))
                {
                    var success = await AuthStateService.ExchangeCodeForTokenAsync(authCode, SpotifyOptions.Value.ClientId);
                    
                    if (success)
                    {
                        NavigationManager.NavigateTo("/", true);
                    }
                }
                else
                {
                    await AuthStateService.InitializeAsync();
                }

                isLoaded = true;
                StateHasChanged();
            }
        }

        private async Task Login()
        {
            var authUrl = await AuthStateService.LoginAsync(SpotifyOptions.Value.ClientId);
            NavigationManager.NavigateTo(authUrl, true);
        }
    }
}
