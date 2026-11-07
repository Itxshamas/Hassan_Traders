using System.Windows;
using System.Windows.Media;
using HassanTraders.Desktop.Services.Application.Settings;
using HassanTraders.Desktop.Services;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace HassanTraders.Desktop.Common.Helpers;

/// <summary>
/// Applies the light or dark colour dictionary of the design system and re-colours WPF-UI's own controls
/// with the brand primary, so stock WPF-UI parts (scroll bars, tooltips, menus) match the rest of the app.
/// </summary>
public sealed class WpfThemeApplier : IThemeApplier
{
    private const string ColorsPrefix = "Resources/Themes/Colors.";

    public void Apply(string theme)
    {
        var dark = theme == AppSettingsService.Dark;
        var resources = System.Windows.Application.Current.Resources;

        SwapColorDictionary(resources, dark ? "Dark" : "Light");

        var applicationTheme = dark ? ApplicationTheme.Dark : ApplicationTheme.Light;
        // updateAccent: false keeps the Windows accent colour from replacing the brand colour.
        ApplicationThemeManager.Apply(applicationTheme, WindowBackdropType.None, updateAccent: false);
        if (resources["Color.Primary"] is Color primary)
        {
            ApplicationAccentColorManager.Apply(primary, applicationTheme);
        }
    }

    private static void SwapColorDictionary(ResourceDictionary resources, string name)
    {
        var merged = resources.MergedDictionaries;
        var existing = merged.FirstOrDefault(d =>
            d.Source is { } source && source.OriginalString.Contains(ColorsPrefix, StringComparison.Ordinal));
        var next = new ResourceDictionary { Source = new Uri($"{ColorsPrefix}{name}.xaml", UriKind.Relative) };
        if (existing is null)
        {
            merged.Add(next);
        }
        else
        {
            merged[merged.IndexOf(existing)] = next;
        }
    }
}
