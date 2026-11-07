using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace HassanTraders.Desktop.Tests;

/// <summary>
/// Guards docs/design-system.md: one palette, no raw colours on screens, every style key resolvable,
/// readable contrast in both themes, every translated key present. If one of these fails, a screen is
/// breaking the design system.
/// </summary>
public partial class DesignSystemTests
{
    private static readonly string[] Themes = ["Light", "Dark"];

    [GeneratedRegex("#[0-9A-Fa-f]{6}(?:[0-9A-Fa-f]{2})?\\b")]
    private static partial Regex HexColour();

    [GeneratedRegex("(?:Background|Foreground|Fill|Stroke|BorderBrush|Color)=\"(?:White|Black|Red|Green|Blue|Gray|Grey|Orange|Yellow|Silver|Navy|Teal)\"")]
    private static partial Regex NamedColour();

    [GeneratedRegex("x:Key=\"([^\"]+)\"")]
    private static partial Regex DefinedKey();

    [GeneratedRegex("\\{(?:Static|Dynamic)Resource ([^\\s}{]+)\\}")]
    private static partial Regex ReferencedKey();

    [GeneratedRegex("(?:ColumnDefinition|RowDefinition)[^>]*(?:Width|Height)=\"\\{DynamicResource ([^}]+)\\}\"")]
    private static partial Regex SizedByResource();

    [GeneratedRegex("\\{wpf:T (\\w+)\\}")]
    private static partial Regex TranslatedKey();

    private static Dictionary<string, string> Colors(string theme)
    {
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        return XDocument.Load(Path.Combine(RepoPaths.Resources, "Themes", $"Colors.{theme}.xaml"))
            .Root!.Elements()
            .Where(e => e.Name.LocalName == "Color")
            .ToDictionary(e => ((string)e.Attribute(x + "Key")!).Replace("Color.", string.Empty, StringComparison.Ordinal), e => e.Value.Trim());
    }

    [Fact]
    public void BothThemes_DefineExactlyTheSameColourTokens()
    {
        Assert.Equal(Colors("Light").Keys.Order(), Colors("Dark").Keys.Order());
    }

    [Fact]
    public void EveryColourToken_HasABrush()
    {
        var brushes = File.ReadAllText(Path.Combine(RepoPaths.Resources, "Styles", "Brushes.xaml"));
        foreach (var colour in Colors("Light").Keys)
        {
            Assert.Contains($"x:Key=\"Brush.{colour}\"", brushes);
            Assert.Contains($"Color=\"{{DynamicResource Color.{colour}}}\"", brushes);
        }
    }

    [Fact]
    public void BrandPrimary_IsTheApprovedBlueTeal()
    {
        Assert.Equal("#0B6FA4", Colors("Light")["Primary"], ignoreCase: true);
    }

    [Fact]
    public void RawHexColours_AppearOnlyInTheThemeFiles()
    {
        var offenders = new List<string>();
        foreach (var file in RepoPaths.AllXaml())
        {
            if (Path.GetFileName(file).StartsWith("Colors.", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (Match match in HexColour().Matches(File.ReadAllText(file)))
            {
                offenders.Add($"{Path.GetRelativePath(RepoPaths.Desktop, file)}: {match.Value}");
            }
        }

        Assert.True(offenders.Count == 0, "Use a Brush.* token instead of a raw colour: " + string.Join("; ", offenders));
    }

    [Fact]
    public void NamedColours_AreNotUsedOnScreens()
    {
        var offenders = RepoPaths.AllXaml()
            .Where(f => NamedColour().IsMatch(File.ReadAllText(f)))
            .Select(f => Path.GetRelativePath(RepoPaths.Desktop, f))
            .ToList();
        Assert.True(offenders.Count == 0, "Use a Brush.* token instead of a named colour: " + string.Join("; ", offenders));
    }

    [Fact]
    public void EveryReferencedResourceKey_IsDefinedSomewhere()
    {
        var files = RepoPaths.AllXaml().ToList();
        var defined = files.SelectMany(f => DefinedKey().Matches(File.ReadAllText(f)).Select(m => m.Groups[1].Value)).ToHashSet();
        var references = files.SelectMany(f => ReferencedKey().Matches(File.ReadAllText(f))).Count();
        Assert.True(references > 50, "The reference scan found almost nothing; the regex is broken.");
        var missing = files
            .SelectMany(f => ReferencedKey().Matches(File.ReadAllText(f))
                .Select(m => (File: Path.GetRelativePath(RepoPaths.Desktop, f), Key: m.Groups[1].Value)))
            .Where(r => !defined.Contains(r.Key))
            .Select(r => $"{r.File}: {r.Key}")
            .Distinct()
            .ToList();
        Assert.True(missing.Count == 0, "Undefined resource keys (would crash at runtime): " + string.Join("; ", missing));
    }

    [Fact]
    public void ResourcesUsedAsColumnOrRowSizes_AreGridLengths()
    {
        // A Double resource assigned to ColumnDefinition.Width compiles but throws when the page loads.
        var tokens = File.ReadAllText(Path.Combine(RepoPaths.Resources, "Styles", "Tokens.xaml"));
        var sized = RepoPaths.AllXaml()
            .SelectMany(f => SizedByResource().Matches(File.ReadAllText(f)).Select(m => m.Groups[1].Value))
            .Distinct()
            .ToList();
        Assert.NotEmpty(sized);
        Assert.All(sized, key => Assert.Matches($"<GridLength x:Key=\"{Regex.Escape(key)}\"", tokens));
    }

    [Fact]
    public void DesignSystem_MergesEveryStyleFile()
    {
        var entry = File.ReadAllText(Path.Combine(RepoPaths.Resources, "DesignSystem.xaml"));
        foreach (var file in Directory.EnumerateFiles(Path.Combine(RepoPaths.Resources, "Styles"), "*.xaml"))
        {
            Assert.Contains($"Source=\"Styles/{Path.GetFileName(file)}\"", entry);
        }
    }

    [Fact]
    public void EveryTranslatedKeyUsedInXaml_ExistsInBothLanguages()
    {
        var english = ResourceKeys("Strings.resx");
        var urdu = ResourceKeys("Strings.ur.resx");
        var used = RepoPaths.AllXaml()
            .SelectMany(f => TranslatedKey().Matches(File.ReadAllText(f)).Select(m => m.Groups[1].Value))
            .ToHashSet();
        Assert.NotEmpty(used);
        Assert.All(used, key =>
        {
            Assert.Contains(key, english);
            Assert.Contains(key, urdu);
        });
    }

    [Theory]
    [InlineData("TextPrimary", "Surface")]
    [InlineData("TextPrimary", "AppBackground")]
    [InlineData("TextSecondary", "Surface")]
    [InlineData("TextSecondary", "SurfaceAlt")]
    [InlineData("TextMuted", "Surface")]
    [InlineData("TextMuted", "AppBackground")]
    [InlineData("OnPrimary", "Primary")]
    [InlineData("OnPrimary", "PrimaryHover")]
    [InlineData("OnDanger", "Danger")]
    [InlineData("Primary", "Surface")]
    [InlineData("Primary", "PrimaryTint")]
    [InlineData("Danger", "Surface")]
    [InlineData("Success", "SuccessBg")]
    [InlineData("Warning", "WarningBg")]
    [InlineData("Danger", "DangerBg")]
    [InlineData("Info", "InfoBg")]
    [InlineData("SidebarText", "Sidebar")]
    [InlineData("SidebarText", "SidebarHover")]
    [InlineData("SidebarTextActive", "SidebarActive")]
    public void TextColours_MeetWcagAaContrast_InBothThemes(string foreground, string background)
    {
        foreach (var theme in Themes)
        {
            var colours = Colors(theme);
            var ratio = Contrast(colours[foreground], colours[background]);
            Assert.True(ratio >= 4.5, $"{theme}: {foreground} on {background} is {ratio:0.00}:1, needs 4.5:1");
        }
    }

    private static HashSet<string> ResourceKeys(string file) =>
        XDocument.Load(Path.Combine(RepoPaths.Resources, file)).Root!.Elements("data")
            .Select(d => (string)d.Attribute("name")!).ToHashSet();

    private static double Contrast(string a, string b)
    {
        var (high, low) = (Luminance(a), Luminance(b));
        if (high < low)
        {
            (high, low) = (low, high);
        }

        return (high + 0.05) / (low + 0.05);
    }

    private static double Luminance(string hex)
    {
        var rgb = hex.TrimStart('#')[^6..];
        double Channel(int start)
        {
            var c = Convert.ToInt32(rgb.Substring(start, 2), 16) / 255.0;
            return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }

        return (0.2126 * Channel(0)) + (0.7152 * Channel(2)) + (0.0722 * Channel(4));
    }
}
