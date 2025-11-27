using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SpotyWrap.Services;

namespace SpotyWrap.Components.Layout
{
    public partial class NavMenu : IDisposable
    {
        [Inject] private IJSRuntime JSRuntime { get; set; }
        [Inject] private AuthStateService AuthStateService { get; set; }
        private string accessToken;
        
        protected override void OnInitialized()
        {
            Console.WriteLine("NavMenu - OnInitialized called");
            AuthStateService.OnChange += OnAuthStateChanged;
        }
        
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Console.WriteLine("NavMenu - OnAfterRenderAsync (firstRender)");
                await RefreshAccessToken();
            }
        }

        private async void OnAuthStateChanged()
        {
            Console.WriteLine("NavMenu - OnAuthStateChanged called");
            await RefreshAccessToken();
        }

        private async Task RefreshAccessToken()
        {
            var oldToken = accessToken;
            accessToken = await JSRuntime.InvokeAsync<string>("getSpotifyAccessToken");
            Console.WriteLine($"NavMenu - Token refreshed. Old: '{oldToken}', New: '{accessToken}'");
            Console.WriteLine($"NavMenu - Is null or empty: {string.IsNullOrEmpty(accessToken)}");
            await InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            Console.WriteLine("NavMenu - Disposing");
            AuthStateService.OnChange -= OnAuthStateChanged;
        }
    }
}
