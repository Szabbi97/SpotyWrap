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
            Console.WriteLine("NavMenu - OnInitialized called");
            AuthStateService.OnChange += OnAuthStateChanged;
        }
        
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Console.WriteLine("NavMenu - OnAfterRenderAsync (firstRender)");
                await AuthStateService.InitializeAsync();
                StateHasChanged();
            }
        }

        private void OnAuthStateChanged()
        {
            Console.WriteLine($"NavMenu - OnAuthStateChanged called. IsAuthenticated: {isAuthenticated}");
            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            Console.WriteLine("NavMenu - Disposing");
            AuthStateService.OnChange -= OnAuthStateChanged;
        }
    }
}
