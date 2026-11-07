using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Services.Application.Security;
using HassanTraders.Desktop.Features.Login;
using HassanTraders.Desktop.Services;
using HassanTraders.Desktop.Core.Common;
using HassanTraders.Desktop.Core.Security;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace HassanTraders.Desktop.Tests;

public sealed class LoginViewModelTests : IDisposable
{
    private static readonly UserInfo Admin = new(1, "admin", "Administrator", Role.Owner, true);
    private readonly IAuthService _auth = Substitute.For<IAuthService>();
    private readonly IAppearanceService _appearance = Substitute.For<IAppearanceService>();
    private readonly LocalizationService _loc = TestSupport.NewLocalization();
    private readonly LoginViewModel _sut;

    public LoginViewModelTests()
    {
        var runner = TestSupport.Runner(s => s.AddSingleton(_auth));
        _sut = new LoginViewModel(runner, _appearance, _loc);
    }

    [Fact]
    public void LoginCommand_NeedsUsernameAndPassword()
    {
        Assert.False(_sut.LoginCommand.CanExecute(null));
        _sut.Username = "admin";
        Assert.False(_sut.LoginCommand.CanExecute(null));
        _sut.Password = "x";
        Assert.True(_sut.LoginCommand.CanExecute(null));
    }

    [Fact]
    public async Task Login_Success_RaisesLoggedInAndClearsThePassword()
    {
        _auth.LoginAsync(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>()).Returns(Result.Success(Admin));
        UserInfo? received = null;
        _sut.LoggedIn += (_, u) => received = u;
        _sut.Username = "admin";
        _sut.Password = "Admin@123";

        await _sut.LoginCommand.ExecuteAsync(null);

        Assert.Equal(Admin, received);
        Assert.Equal(string.Empty, _sut.Password);
        Assert.False(_sut.HasError);
        Assert.False(_sut.IsBusy);
        await _auth.Received(1).LoginAsync(
            Arg.Is<LoginRequest>(r => r.Username == "admin" && r.Password == "Admin@123"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Login_Failure_ShowsLocalizedMessage_AndClearsThePassword()
    {
        _auth.LoginAsync(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>()).Returns(Result.Failure<UserInfo>("Login_Invalid"));
        var raised = false;
        _sut.LoggedIn += (_, _) => raised = true;
        _sut.Username = "admin";
        _sut.Password = "bad";

        await _sut.LoginCommand.ExecuteAsync(null);

        Assert.False(raised);
        Assert.True(_sut.HasError);
        Assert.Equal("Wrong username or password.", _sut.ErrorMessage);
        Assert.Equal(string.Empty, _sut.Password);
    }

    [Fact]
    public async Task ErrorMessage_FollowsTheLanguage()
    {
        _auth.LoginAsync(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>()).Returns(Result.Failure<UserInfo>("Login_Invalid"));
        _sut.Username = "admin";
        _sut.Password = "bad";
        await _sut.LoginCommand.ExecuteAsync(null);
        var changed = new List<string?>();
        _sut.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        _loc.SetLanguage("ur");

        Assert.Contains(string.Empty, changed);
        Assert.Equal("صارف کا نام یا پاس ورڈ غلط ہے۔", _sut.ErrorMessage);
        Assert.True(_sut.IsRightToLeft);
    }

    [Fact]
    public async Task LoginIsBusyWhileTheCallRuns_SoItCannotBeDoubleSubmitted()
    {
        var gate = new TaskCompletionSource<Result<UserInfo>>();
        _auth.LoginAsync(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>()).Returns(gate.Task);
        _sut.Username = "admin";
        _sut.Password = "Admin@123";

        var running = _sut.LoginCommand.ExecuteAsync(null);

        Assert.True(_sut.IsBusy);
        Assert.False(_sut.LoginCommand.CanExecute(null));
        Assert.Equal("Signing in...", _sut.LoginButtonText);

        gate.SetResult(Result.Success(Admin));
        await running;
        Assert.False(_sut.IsBusy);
    }

    [Fact]
    public async Task ToggleLanguage_SwitchesToTheOtherLanguage()
    {
        await _sut.ToggleLanguageCommand.ExecuteAsync(null);
        await _appearance.Received(1).SetLanguageAsync("ur");
        Assert.Equal("اردو", _sut.OtherLanguageLabel);
    }

    public void Dispose() => _sut.Dispose();
}
