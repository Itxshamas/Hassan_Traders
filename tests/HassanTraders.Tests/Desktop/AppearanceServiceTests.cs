using HassanTraders.Desktop.Services.Application.Settings;
using HassanTraders.Desktop.Services;
using HassanTraders.Desktop.Core.Common;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace HassanTraders.Desktop.Tests;

public class AppearanceServiceTests
{
    private readonly IAppSettingsService _settings = Substitute.For<IAppSettingsService>();
    private readonly IThemeApplier _theme = Substitute.For<IThemeApplier>();
    private readonly LocalizationService _loc = new();
    private readonly AppearanceService _sut;

    public AppearanceServiceTests()
    {
        _sut = new AppearanceService(TestSupport.Runner(s => s.AddSingleton(_settings)), _loc, _theme);
    }

    [Fact]
    public async Task Initialize_AppliesTheSavedLanguageAndTheme()
    {
        _settings.GetLanguageAsync(Arg.Any<CancellationToken>()).Returns("ur");
        _settings.GetThemeAsync(Arg.Any<CancellationToken>()).Returns("dark");

        await _sut.InitializeAsync();

        Assert.True(_loc.IsRightToLeft);
        Assert.Equal("dark", _sut.Theme);
        _theme.Received(1).Apply("dark");
    }

    [Fact]
    public async Task SetLanguage_SavesThenSwitches()
    {
        _settings.SetLanguageAsync("ur", Arg.Any<CancellationToken>()).Returns(Result.Success());

        var result = await _sut.SetLanguageAsync("ur");

        Assert.True(result.IsSuccess);
        Assert.Equal("ur", _loc.Language);
    }

    [Fact]
    public async Task SetLanguage_WhenSaveIsRejected_LeavesTheUiAlone()
    {
        _settings.SetLanguageAsync("xx", Arg.Any<CancellationToken>()).Returns(Result.Failure("Settings_InvalidLanguage"));

        var result = await _sut.SetLanguageAsync("xx");

        Assert.True(result.IsFailure);
        Assert.Equal("en", _loc.Language);
    }

    [Fact]
    public async Task SetTheme_SavesThenApplies()
    {
        _settings.SetThemeAsync("dark", Arg.Any<CancellationToken>()).Returns(Result.Success());

        await _sut.SetThemeAsync("dark");

        Assert.Equal("dark", _sut.Theme);
        _theme.Received(1).Apply("dark");
    }

    [Fact]
    public async Task SetTheme_WhenRejected_DoesNotApply()
    {
        _settings.SetThemeAsync("neon", Arg.Any<CancellationToken>()).Returns(Result.Failure("Settings_InvalidTheme"));

        await _sut.SetThemeAsync("neon");

        Assert.Equal("light", _sut.Theme);
        _theme.DidNotReceiveWithAnyArgs().Apply(default!);
    }
}
