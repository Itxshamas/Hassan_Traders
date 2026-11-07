using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HassanTraders.Desktop.Services.Application.Settings;
using HassanTraders.Desktop.Services;

namespace HassanTraders.Desktop.Features.Settings;

public sealed partial class SettingsViewModel : LocalizedViewModel
{
    private readonly IAppearanceService _appearance;

    public SettingsViewModel(IAppearanceService appearance, ILocalizationService localization)
        : base(localization)
    {
        _appearance = appearance;
    }

    public string Title => Localization["Settings_Title"];

    public bool IsEnglish => Localization.Language == "en";

    public bool IsUrdu => Localization.Language == "ur";

    public bool IsLight => _appearance.Theme == AppSettingsService.Light;

    public bool IsDark => _appearance.Theme == AppSettingsService.Dark;

    [ObservableProperty]
    private string? _errorCode;

    public string ErrorMessage => ErrorCode is null ? string.Empty : Localization[ErrorCode];

    partial void OnErrorCodeChanged(string? value) => OnPropertyChanged(nameof(ErrorMessage));

    [RelayCommand]
    private async Task SetLanguageAsync(string language)
    {
        var result = await _appearance.SetLanguageAsync(language);
        ErrorCode = result.Error;
        OnPropertyChanged(string.Empty);
    }

    [RelayCommand]
    private async Task SetThemeAsync(string theme)
    {
        var result = await _appearance.SetThemeAsync(theme);
        ErrorCode = result.Error;
        OnPropertyChanged(string.Empty);
    }
}
