using System.Xml.Linq;
using HassanTraders.Desktop.Services.Application.Security;
using HassanTraders.Desktop.Services;
using HassanTraders.Desktop.Core.Security;

namespace HassanTraders.Desktop.Tests;

public class LocalizationTests
{
    private static string ResourceFolder()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src", "HassanTraders.Desktop", "Resources")))
        {
            dir = dir.Parent;
        }

        return Path.Combine(dir?.FullName ?? throw new DirectoryNotFoundException("Repo root not found"), "src", "HassanTraders.Desktop", "Resources");
    }

    private static Dictionary<string, string> ReadResx(string file) =>
        XDocument.Load(Path.Combine(ResourceFolder(), file))
            .Root!.Elements("data")
            .ToDictionary(d => (string)d.Attribute("name")!, d => (string)d.Element("value")!);

    [Fact]
    public void EnglishAndUrduFiles_HaveExactlyTheSameKeys()
    {
        var en = ReadResx("Strings.resx").Keys.OrderBy(k => k).ToList();
        var ur = ReadResx("Strings.ur.resx").Keys.OrderBy(k => k).ToList();
        Assert.Equal(en, ur);
    }

    [Fact]
    public void NoTextIsEmpty_AndUrduIsActuallyTranslated()
    {
        var en = ReadResx("Strings.resx");
        var ur = ReadResx("Strings.ur.resx");
        Assert.All(en, kv => Assert.False(string.IsNullOrWhiteSpace(kv.Value), kv.Key));
        Assert.All(ur, kv => Assert.False(string.IsNullOrWhiteSpace(kv.Value), kv.Key));

        // Only brand-like words may be identical across languages.
        var untranslated = en.Where(kv => ur[kv.Key] == kv.Value).Select(kv => kv.Key).Order().ToList();
        Assert.Equal(["Common_English", "Common_Urdu"], untranslated);
    }

    [Fact]
    public void Urdu_UsesArabicScript()
    {
        var ur = ReadResx("Strings.ur.resx");
        Assert.Matches("[؀-ۿ]", ur["Login_Title"]);
        Assert.Matches("[؀-ۿ]", ur["Nav_Billing"]);
    }

    [Fact]
    public void EveryNavigationFeatureAndRole_HasAKey()
    {
        var keys = ReadResx("Strings.resx").Keys.ToHashSet();
        foreach (var feature in Enum.GetValues<Feature>())
        {
            Assert.Contains($"Nav_{feature}", keys);
        }

        foreach (var role in Enum.GetValues<Role>())
        {
            Assert.Contains($"Role_{role}", keys);
        }
    }

    [Fact]
    public void EveryErrorCodeTheApplicationCanReturn_HasATranslation()
    {
        // Codes from AuthService, AppSettingsService and the validators. Add new codes here when you add them there.
        string[] codes =
        [
            "Login_Invalid", "Login_Inactive", "User_NotFound", "Validation_UsernameRequired", "Validation_PasswordRequired",
            "Password_TooShort", "Password_NeedsLetterAndDigit", "Password_Mismatch", "Password_SameAsOld",
            "Password_CurrentWrong", "Settings_InvalidLanguage", "Settings_InvalidTheme",
        ];
        var keys = ReadResx("Strings.resx").Keys.ToHashSet();
        Assert.All(codes, c => Assert.Contains(c, keys));

        // And the validators really produce codes from that list.
        var validator = new ChangePasswordRequestValidator();
        var produced = validator.Validate(new ChangePasswordRequest("a", "a", "b")).Errors.Select(e => e.ErrorMessage);
        Assert.All(produced, p => Assert.Contains(p, codes));
    }

    [Fact]
    public void Service_StartsEnglish_AndSwitchesToRightToLeftUrdu()
    {
        var loc = new LocalizationService();
        Assert.Equal("en", loc.Language);
        Assert.False(loc.IsRightToLeft);
        Assert.Equal("Billing", loc["Nav_Billing"]);

        var changed = new List<string?>();
        loc.PropertyChanged += (_, e) => changed.Add(e.PropertyName);
        loc.SetLanguage("ur");

        Assert.Equal("ur", loc.Language);
        Assert.True(loc.IsRightToLeft);
        Assert.Equal("بلنگ", loc["Nav_Billing"]);
        Assert.Contains("Item[]", changed);
        Assert.Contains(string.Empty, changed);
    }

    [Fact]
    public void SettingTheSameLanguageAgain_DoesNotRaiseEvents()
    {
        var loc = new LocalizationService();
        var count = 0;
        loc.PropertyChanged += (_, _) => count++;
        loc.SetLanguage("en");
        Assert.Equal(0, count);
    }

    [Fact]
    public void UnsupportedLanguage_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LocalizationService().SetLanguage("fr"));
    }

    [Fact]
    public void MissingKey_IsVisibleNotSilent()
    {
        Assert.Equal("!No_Such_Key!", new LocalizationService()["No_Such_Key"]);
    }
}
