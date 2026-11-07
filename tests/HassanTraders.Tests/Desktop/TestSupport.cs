using HassanTraders.Desktop.Services.Application;
using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HassanTraders.Desktop.Tests;

internal static class TestSupport
{
    /// <summary>A real runner (fresh scope per call) over whatever fakes the test registers.</summary>
    public static IUseCaseRunner Runner(Action<IServiceCollection> register)
    {
        var services = new ServiceCollection();
        register(services);
        return new UseCaseRunner(services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>());
    }

    public static LocalizationService NewLocalization(string language = "en")
    {
        var loc = new LocalizationService();
        loc.SetLanguage(language);
        return loc;
    }
}
