using Microsoft.JSInterop;

namespace AzureExamSimulator.Services;

public class ThemeService
{
    private readonly IJSRuntime _js;
    private string _theme = "dark";
    private bool _initialized;

    public ThemeService(IJSRuntime js)
    {
        _js = js;
    }

    public string Theme => _theme;
    public bool IsDark => _theme == "dark";

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;

        try
        {
            var stored = await _js.InvokeAsync<string?>("localStorage.getItem", "theme");
            if (stored is "light" or "dark")
                _theme = stored;
        }
        catch
        {
            // JS interop not available yet, use default
        }
    }

    public async Task ToggleAsync()
    {
        _theme = _theme == "dark" ? "light" : "dark";

        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", "theme", _theme);
            await _js.InvokeVoidAsync("eval", $"document.documentElement.setAttribute('data-theme', '{_theme}')");
        }
        catch
        {
            // Fallback: theme will still be applied via Blazor re-render
        }
    }
}
