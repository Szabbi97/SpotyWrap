using Microsoft.AspNetCore.Components;
using SpotyWrap.Services;

namespace SpotyWrap.Components.Layout
{
    public partial class NavMenu : IDisposable
    {
        [Inject] private AuthStateService AuthStateService { get; set; }
        
        private bool isAuthenticated => AuthStateService.IsAuthenticated;
        
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

        public void Dispose()
        {
            AuthStateService.OnChange -= OnAuthStateChanged;
        }
    }
}
