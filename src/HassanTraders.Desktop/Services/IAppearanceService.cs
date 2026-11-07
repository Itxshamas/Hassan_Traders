using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Services.Application.Settings;
using HassanTraders.Desktop.Core.Common;

namespace HassanTraders.Desktop.Services;

/// <summary>Language and theme: saves the choice in the database and applies it to the UI.</summary>
public interface IAppearanceService
{
    string Theme { get; }

    /// <summary>Reads the saved language and theme (English and light if nothing is saved) and applies them.</summary>
    Task InitializeAsync();

    Task<Result> SetLanguageAsync(string language);

    Task<Result> SetThemeAsync(string theme);
}

public sealed class AppearanceService : IAppearanceService
{
    private readonly IUseCaseRunner _runner;
    private readonly ILocalizationService _localization;
    private readonly IThemeApplier _themeApplier;

    public AppearanceService(IUseCaseRunner runner, ILocalizationService localization, IThemeApplier themeApplier)
    {
        _runner = runner;
        _localization = localization;
        _themeApplier = themeApplier;
        Theme = AppSettingsService.Light;
    }

    public string Theme { get; private set; }

    public async Task InitializeAsync()
    {
        var language = await _runner.RunAsync<IAppSettingsService, string>(s => s.GetLanguageAsync());
        var theme = await _runner.RunAsync<IAppSettingsService, string>(s => s.GetThemeAsync());
        _localization.SetLanguage(language);
        Theme = theme;
        _themeApplier.Apply(theme);
    }

    public async Task<Result> SetLanguageAsync(string language)
    {
        var result = await _runner.RunAsync<IAppSettingsService, Result>(s => s.SetLanguageAsync(language));
        if (result.IsSuccess)
        {
            _localization.SetLanguage(language);
        }

        return result;
    }

    public async Task<Result> SetThemeAsync(string theme)
    {
        var result = await _runner.RunAsync<IAppSettingsService, Result>(s => s.SetThemeAsync(theme));
        if (result.IsSuccess)
        {
            Theme = theme;
            _themeApplier.Apply(theme);
        }

        return result;
    }
}
