using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Services.Application.Security;
using HassanTraders.Desktop.Core.DTOs.Catalog;
using HassanTraders.Desktop.Features.Dashboard;
using HassanTraders.Desktop.Features.Items;
using HassanTraders.Desktop.Features.Login;
using HassanTraders.Desktop.Services.Application.Catalog;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using HassanTraders.Desktop.Features.Settings;
using HassanTraders.Desktop.Services;
using HassanTraders.Desktop.Shell;
using HassanTraders.Desktop.Core.Common;
using HassanTraders.Desktop.Core.Security;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace HassanTraders.Desktop.Tests;

public sealed class ShellAndNavigationTests : IDisposable
{
    private static readonly CatalogSummary EmptySummary = new(0, 0, 0, 0, 0, []);
    private readonly IAuthService _auth = Substitute.For<IAuthService>();
    private readonly ICatalogQueryService _catalog = Substitute.For<ICatalogQueryService>();
    private readonly IAppearanceService _appearance = Substitute.For<IAppearanceService>();
    private readonly LocalizationService _loc = TestSupport.NewLocalization();
    private readonly UserSession _session = new();
    private readonly NavigationService _navigation;
    private readonly IUseCaseRunner _runner;
    private readonly ServiceProvider _provider;

    public ShellAndNavigationTests()
    {
        _catalog.GetSummaryAsync(Arg.Any<CancellationToken>()).Returns(EmptySummary);
        _catalog.GetListingAsync(Arg.Any<CancellationToken>()).Returns(new CatalogListing([], []));
        _runner = TestSupport.Runner(s =>
        {
            s.AddSingleton(_auth);
            s.AddSingleton(_catalog);
        });

        var services = new ServiceCollection();
        services.AddSingleton<ILocalizationService>(_loc);
        services.AddSingleton(_appearance);
        services.AddSingleton(_auth);
        services.AddSingleton(_runner);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ItemsViewModel>();
        _provider = services.BuildServiceProvider();
        _navigation = new NavigationService(_provider);
    }

    private ShellViewModel ShellFor(Role role)
    {
        _session.SignIn(new UserInfo(1, "u", "User One", role, false));
        return new ShellViewModel(_session, _navigation, _runner, _appearance, _loc);
    }

    [Fact]
    public void Owner_SeesAllNineScreens_InMenuOrder()
    {
        var shell = ShellFor(Role.Owner);
        Assert.Equal(
            [Feature.Dashboard, Feature.Billing, Feature.Items, Feature.StockCount, Feature.Purchases, Feature.Udhaar, Feature.Customers, Feature.Reports, Feature.Settings],
            shell.NavItems.Select(n => n.Feature));
    }

    [Fact]
    public void Cashier_SeesOnlyBillingUdhaarAndSettings_AndLandsOnBilling()
    {
        var shell = ShellFor(Role.Cashier);

        Assert.Equal([Feature.Billing, Feature.Udhaar, Feature.Settings], shell.NavItems.Select(n => n.Feature));
        Assert.Equal(Feature.Billing, shell.SelectedNavItem!.Feature);
        var page = Assert.IsType<PlaceholderViewModel>(shell.CurrentPage);
        Assert.Equal("Billing", page.Title);
    }

    [Fact]
    public void SelectingSettings_ShowsTheSettingsPage()
    {
        var shell = ShellFor(Role.Owner);
        shell.SelectedNavItem = shell.NavItems.Single(n => n.Feature == Feature.Settings);
        Assert.IsType<SettingsViewModel>(shell.CurrentPage);
    }

    [Fact]
    public void ShellShowsUserAndRole_AndTextFollowsLanguage()
    {
        var shell = ShellFor(Role.StoreKeeper);
        Assert.Equal("User One", shell.UserName);
        Assert.Equal("Store keeper", shell.RoleLabel);
        Assert.Equal("Stock count", shell.NavItems[0].Label);

        _loc.SetLanguage("ur");

        Assert.Equal("اسٹور کیپر", shell.RoleLabel);
        Assert.Equal("اسٹاک کی گنتی", shell.NavItems[0].Label);
    }

    [Fact]
    public void PageChange_NotifiesAndDisposesThePreviousPage()
    {
        var shell = ShellFor(Role.Owner);
        var firstPage = (LocalizedViewModel)shell.CurrentPage!;
        var changed = new List<string?>();
        shell.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        shell.SelectedNavItem = shell.NavItems.Single(n => n.Feature == Feature.Reports);

        Assert.Contains(nameof(ShellViewModel.CurrentPage), changed);
        Assert.NotSame(firstPage, shell.CurrentPage);
        // A disposed page no longer reacts to language changes.
        var raised = 0;
        firstPage.PropertyChanged += (_, _) => raised++;
        _loc.SetLanguage("ur");
        Assert.Equal(0, raised);
    }

    [Fact]
    public async Task SignOut_LogsOut_ClearsThePage_AndRaisesSignedOut()
    {
        var shell = ShellFor(Role.Owner);
        var signedOut = false;
        shell.SignedOut += (_, _) => signedOut = true;

        await shell.SignOutCommand.ExecuteAsync(null);

        Assert.True(signedOut);
        Assert.Null(_navigation.CurrentPage);
        await _auth.Received(1).LogoutAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Shell_RequiresASignedInUser()
    {
        Assert.Throws<InvalidOperationException>(() => new ShellViewModel(_session, _navigation, _runner, _appearance, _loc));
    }

    [Fact]
    public void NavigationIcons_AreDefinedForEveryFeature()
    {
        foreach (var feature in Enum.GetValues<Feature>())
        {
            Assert.NotEqual("Circle24", NavItemViewModel.IconFor(feature));
        }
    }

    [Fact]
    public async Task Root_FlowsFromLogin_ThroughForcedPasswordChange_ToShell_AndBackOnSignOut()
    {
        var info = new UserInfo(1, "admin", "Administrator", Role.Owner, MustChangePassword: true);
        _auth.LoginAsync(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>()).Returns(ci =>
        {
            _session.SignIn(info);
            return Result.Success(info);
        });
        _auth.ChangePasswordAsync(1, Arg.Any<ChangePasswordRequest>(), Arg.Any<CancellationToken>()).Returns(Result.Success());

        var root = new RootViewModel(
            () => new LoginViewModel(_runner, _appearance, _loc),
            () => new ChangePasswordViewModel(_runner, _session, _loc),
            () => new ShellViewModel(_session, _navigation, _runner, _appearance, _loc));
        root.Start();

        var login = Assert.IsType<LoginViewModel>(root.Current);
        login.Username = "admin";
        login.Password = "Admin@123";
        await login.LoginCommand.ExecuteAsync(null);

        var change = Assert.IsType<ChangePasswordViewModel>(root.Current);
        change.CurrentPassword = "Admin@123";
        change.NewPassword = "Shop2026!x";
        change.ConfirmPassword = "Shop2026!x";
        await change.SaveCommand.ExecuteAsync(null);

        var shell = Assert.IsType<ShellViewModel>(root.Current);
        await shell.SignOutCommand.ExecuteAsync(null);

        Assert.IsType<LoginViewModel>(root.Current);
    }

    [Fact]
    public async Task Root_GoesStraightToTheShell_WhenNoPasswordChangeIsNeeded()
    {
        var info = new UserInfo(2, "ali", "Ali", Role.Cashier, MustChangePassword: false);
        _auth.LoginAsync(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>()).Returns(ci =>
        {
            _session.SignIn(info);
            return Result.Success(info);
        });
        var root = new RootViewModel(
            () => new LoginViewModel(_runner, _appearance, _loc),
            () => new ChangePasswordViewModel(_runner, _session, _loc),
            () => new ShellViewModel(_session, _navigation, _runner, _appearance, _loc));
        root.Start();

        var login = (LoginViewModel)root.Current!;
        login.Username = "ali";
        login.Password = "secret123";
        await login.LoginCommand.ExecuteAsync(null);

        Assert.IsType<ShellViewModel>(root.Current);
    }

    public void Dispose() => _provider.Dispose();
}
