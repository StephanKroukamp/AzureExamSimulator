using AzureExamSimulator.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<ExamLoaderService>();
builder.Services.AddScoped<ExamService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<AzureExamSimulator.Components.App>()
    .AddInteractiveServerRenderMode();

_ = Task.Run(async () =>
{
    await Task.Delay(2000);
    try
    {
        var addresses = app.Urls;
        var url = addresses.FirstOrDefault() ?? "http://localhost:5000";
        OpenBrowser(url);
    }
    catch
    {
        // ignore
    }
});

app.Run();

static void OpenBrowser(string url)
{
    try
    {
        if (OperatingSystem.IsWindows())
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
        else if (OperatingSystem.IsMacOS())
            System.Diagnostics.Process.Start("open", url);
        else if (OperatingSystem.IsLinux())
            System.Diagnostics.Process.Start("xdg-open", url);
    }
    catch
    {
        // Silently ignore if browser can't open
    }
}
