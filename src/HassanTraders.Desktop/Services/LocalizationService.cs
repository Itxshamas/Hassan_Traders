using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace HassanTraders.Desktop.Services;

public sealed class LocalizationService : ILocalizationService
{
    public const string English = "en";
    public const string Urdu = "ur";

    private static readonly ResourceManager Resources =
        new("HassanTraders.Desktop.Resources.Strings", typeof(LocalizationService).Assembly);

    private CultureInfo _culture = new(English);

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>XAML markup extensions bind to this one instance; the DI container registers the same object.</summary>
    public static LocalizationService Instance { get; } = new();

    public string Language => _culture.TwoLetterISOLanguageName;

    public bool IsRightToLeft => _culture.TextInfo.IsRightToLeft;

    public string this[string key]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(key);
            return Resources.GetString(key, _culture) ?? $"!{key}!";
        }
    }

    public string Format(string key, params object[] args) =>
        string.Format(_culture, this[key], args);

    public void SetLanguage(string language)
    {
        var culture = language switch
        {
            Urdu => new CultureInfo("ur-PK"),
            English => new CultureInfo(English),
            _ => throw new ArgumentOutOfRangeException(nameof(language), language, "Unsupported language."),
        };

        if (culture.Name == _culture.Name)
        {
            return;
        }

        _culture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        // "Item[]" refreshes indexer bindings ({infra:T Key}); empty name refreshes every property.
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
    }
}
