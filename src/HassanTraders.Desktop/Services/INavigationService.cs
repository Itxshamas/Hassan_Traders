using HassanTraders.Desktop.Core.Security;
using Microsoft.Extensions.DependencyInjection;

namespace HassanTraders.Desktop.Services;

public interface INavigationService
{
    event EventHandler? PageChanged;

    object? CurrentPage { get; }

    void Navigate(Feature feature);

    void Clear();
}

public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _services;

    public NavigationService(IServiceProvider services) => _services = services;

    public event EventHandler? PageChanged;

    public object? CurrentPage { get; private set; }

    public void Navigate(Feature feature)
    {
        var next = Create(feature);
        Replace(next);
    }

    public void Clear() => Replace(null);

    private object Create(Feature feature) => feature switch
    {
        // ActivatorUtilities, not GetRequiredService: the container would otherwise keep every page it ever
        // created alive until the app exits (it tracks disposable transients).
        Feature.Dashboard => ActivatorUtilities.CreateInstance<Desktop.Features.Dashboard.DashboardViewModel>(_services),
        Feature.Items => ActivatorUtilities.CreateInstance<Desktop.Features.Items.ItemsViewModel>(_services),
        Feature.Settings => ActivatorUtilities.CreateInstance<Desktop.Features.Settings.SettingsViewModel>(_services),
        _ => ActivatorUtilities.CreateInstance<Desktop.Shell.PlaceholderViewModel>(_services, $"Nav_{feature}"),
    };

    private void Replace(object? next)
    {
        var previous = CurrentPage;
        CurrentPage = next;
        (previous as IDisposable)?.Dispose();
        PageChanged?.Invoke(this, EventArgs.Empty);
    }
}
