using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Services.Application.Settings;
using NSubstitute;

namespace HassanTraders.Desktop.Services.Application.Tests;

public class AppSettingsServiceTests
{
    private readonly ISettingsRepository _repo = Substitute.For<ISettingsRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly AppSettingsService _sut;

    public AppSettingsServiceTests() => _sut = new AppSettingsService(_repo, _uow);

    [Theory]
    [InlineData(null, "en")]
    [InlineData("ur", "ur")]
    [InlineData("fr", "en")]
    public async Task GetLanguage_FallsBackToEnglish(string? stored, string expected)
    {
        _repo.GetAsync(AppSettingsService.LanguageKey, Arg.Any<CancellationToken>()).Returns(stored);
        Assert.Equal(expected, await _sut.GetLanguageAsync());
    }

    [Theory]
    [InlineData(null, "light")]
    [InlineData("dark", "dark")]
    [InlineData("pink", "light")]
    public async Task GetTheme_FallsBackToLight(string? stored, string expected)
    {
        _repo.GetAsync(AppSettingsService.ThemeKey, Arg.Any<CancellationToken>()).Returns(stored);
        Assert.Equal(expected, await _sut.GetThemeAsync());
    }

    [Fact]
    public async Task SetLanguage_WithValidCode_SavesOnce()
    {
        var result = await _sut.SetLanguageAsync("ur");

        Assert.True(result.IsSuccess);
        await _repo.Received(1).SetAsync(AppSettingsService.LanguageKey, "ur", Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetLanguage_WithUnknownCode_IsRejectedAndNothingSaved()
    {
        var result = await _sut.SetLanguageAsync("xx");

        Assert.Equal("Settings_InvalidLanguage", result.Error);
        await _repo.DidNotReceiveWithAnyArgs().SetAsync(default!, default!, default);
    }

    [Fact]
    public async Task SetTheme_WithUnknownValue_IsRejected()
    {
        var result = await _sut.SetThemeAsync("neon");
        Assert.Equal("Settings_InvalidTheme", result.Error);
    }
}
