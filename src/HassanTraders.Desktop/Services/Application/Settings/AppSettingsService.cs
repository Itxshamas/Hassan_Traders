using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Core.Common;

namespace HassanTraders.Desktop.Services.Application.Settings;

public interface IAppSettingsService
{
    Task<string> GetLanguageAsync(CancellationToken cancellationToken = default);

    Task<Result> SetLanguageAsync(string language, CancellationToken cancellationToken = default);

    Task<string> GetThemeAsync(CancellationToken cancellationToken = default);

    Task<Result> SetThemeAsync(string theme, CancellationToken cancellationToken = default);
}

public sealed class AppSettingsService : IAppSettingsService
{
    public const string LanguageKey = "ui.language";
    public const string ThemeKey = "ui.theme";
    public const string English = "en";
    public const string Urdu = "ur";
    public const string Light = "light";
    public const string Dark = "dark";

    private readonly ISettingsRepository _settings;
    private readonly IUnitOfWork _unitOfWork;

    public AppSettingsService(ISettingsRepository settings, IUnitOfWork unitOfWork)
    {
        _settings = settings;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> GetLanguageAsync(CancellationToken cancellationToken = default)
    {
        var value = await _settings.GetAsync(LanguageKey, cancellationToken);
        return value is English or Urdu ? value : English;
    }

    public Task<Result> SetLanguageAsync(string language, CancellationToken cancellationToken = default) =>
        SaveAsync(LanguageKey, language, language is English or Urdu, "Settings_InvalidLanguage", cancellationToken);

    public async Task<string> GetThemeAsync(CancellationToken cancellationToken = default)
    {
        var value = await _settings.GetAsync(ThemeKey, cancellationToken);
        return value is Light or Dark ? value : Light;
    }

    public Task<Result> SetThemeAsync(string theme, CancellationToken cancellationToken = default) =>
        SaveAsync(ThemeKey, theme, theme is Light or Dark, "Settings_InvalidTheme", cancellationToken);

    private async Task<Result> SaveAsync(string key, string value, bool valid, string error, CancellationToken cancellationToken)
    {
        if (!valid)
        {
            return Result.Failure(error);
        }

        await _settings.SetAsync(key, value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
