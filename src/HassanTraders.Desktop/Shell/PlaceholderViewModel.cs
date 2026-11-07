using HassanTraders.Desktop.Services;

namespace HassanTraders.Desktop.Shell;

/// <summary>Stand-in page for screens that arrive in later phases.</summary>
public sealed class PlaceholderViewModel : LocalizedViewModel
{
    private readonly string _titleKey;

    public PlaceholderViewModel(ILocalizationService localization, string titleKey)
        : base(localization)
    {
        _titleKey = titleKey;
    }

    public string Title => Localization[_titleKey];

    public string Message => Localization["Common_ComingSoon"];
}
