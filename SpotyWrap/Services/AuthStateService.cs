namespace SpotyWrap.Services
{
    public class AuthStateService
    {
        public event Action? OnChange;

        public void NotifyStateChanged() => OnChange?.Invoke();
    }
}
