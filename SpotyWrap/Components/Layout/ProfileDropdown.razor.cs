using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
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

        private bool isAuthenticated => AuthStateService.IsAuthenticated;
        private UserData? userData => AuthStateService.UserData;
        private bool isProfileDropdownOpen = false;

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
            var authUrl = await AuthStateService.LoginAsync(SpotifyOptions.Value.ClientId);
            NavigationManager.NavigateTo(authUrl, true);
        }

        public async Task Logout()
        {
            await AuthStateService.ClearAuthenticationAsync();
            isProfileDropdownOpen = false;
            NavigationManager.NavigateTo("/", true);
            StateHasChanged();
        }

        public void Dispose()
        {
            AuthStateService.OnChange -= OnAuthStateChanged;
        }
    }
}
