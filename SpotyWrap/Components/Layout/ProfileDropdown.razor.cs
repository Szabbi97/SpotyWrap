using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using SpotyWrap.Components.Classes;
using SpotyWrap.Configuration;
using SpotyWrap.Services;

namespace SpotyWrap.Components.Layout
{
    public partial class ProfileDropdown : IAsyncDisposable
    {
        [Parameter] public EventCallback OnDropdownStateChanged { get; set; }

        [Inject] private AuthStateService AuthStateService { get; set; }
        [Inject] private IOptions<SpotifySettings> SpotifyOptions { get; set; }
        [Inject] private NavigationManager NavigationManager { get; set; }

        private bool isAuthenticated => AuthStateService.IsAuthenticated;
        private UserData? userData => AuthStateService.UserData;
        private bool isProfileDropdownOpen = false;
        private ElementReference profileContainerRef;
        private IJSObjectReference? _jsModule;
        private DotNetObjectReference<ProfileDropdown>? _dotNetRef;

        protected override void OnInitialized()
        {
            AuthStateService.OnChange += OnAuthStateChanged;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await AuthStateService.InitializeAsync();
                _dotNetRef = DotNetObjectReference.Create(this);
                
                try
                {
                    _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Components/Layout/ProfileDropdown.razor.js");
                    await _jsModule.InvokeVoidAsync("initialize", profileContainerRef, _dotNetRef);
                }
                catch (Exception)
                {
                }
                
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

        public async Task Login()
        {
            var authUrl = await AuthStateService.LoginAsync(SpotifyOptions.Value.ClientId);
            NavigationManager.NavigateTo(authUrl, true);
        }

        public async Task Logout()
        {
            isProfileDropdownOpen = false;
            await AuthStateService.ClearAuthenticationAsync();
            NavigationManager.NavigateTo("/", true);
        }

        [JSInvokable]
        public void CloseDropdown()
        {
            if (isProfileDropdownOpen)
            {
                isProfileDropdownOpen = false;
                InvokeAsync(StateHasChanged);
            }
        }

        public async ValueTask DisposeAsync()
        {
            AuthStateService.OnChange -= OnAuthStateChanged;
            
            if (_jsModule != null)
            {
                try
                {
                    await _jsModule.InvokeVoidAsync("dispose");
                    await _jsModule.DisposeAsync();
                }
                catch (Exception)
                {
                }
            }

            _dotNetRef?.Dispose();
        }
    }
}
