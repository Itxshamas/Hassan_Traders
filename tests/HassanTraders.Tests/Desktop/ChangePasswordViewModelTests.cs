using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Services.Application.Security;
using HassanTraders.Desktop.Features.Login;
using HassanTraders.Desktop.Core.Common;
using HassanTraders.Desktop.Core.Security;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace HassanTraders.Desktop.Tests;

public sealed class ChangePasswordViewModelTests : IDisposable
{
    private readonly IAuthService _auth = Substitute.For<IAuthService>();
    private readonly UserSession _session = new();
    private readonly ChangePasswordViewModel _sut;

    public ChangePasswordViewModelTests()
    {
        _session.SignIn(new UserInfo(7, "admin", "Administrator", Role.Owner, true));
        _sut = new ChangePasswordViewModel(
            TestSupport.Runner(s => s.AddSingleton(_auth)), _session, TestSupport.NewLocalization());
        _sut.CurrentPassword = "Admin@123";
        _sut.NewPassword = "Shop2026!x";
        _sut.ConfirmPassword = "Shop2026!x";
    }

    [Fact]
    public void SaveCommand_NeedsAllThreeFields()
    {
        Assert.True(_sut.SaveCommand.CanExecute(null));
        _sut.ConfirmPassword = string.Empty;
        Assert.False(_sut.SaveCommand.CanExecute(null));
    }

    [Fact]
    public async Task Save_Success_SendsTheRightUserAndRaisesCompleted()
    {
        _auth.ChangePasswordAsync(7, Arg.Any<ChangePasswordRequest>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        var completed = false;
        _sut.Completed += (_, _) => completed = true;

        await _sut.SaveCommand.ExecuteAsync(null);

        Assert.True(completed);
        Assert.Equal(string.Empty, _sut.NewPassword);
        await _auth.Received(1).ChangePasswordAsync(
            7,
            Arg.Is<ChangePasswordRequest>(r => r.CurrentPassword == "Admin@123" && r.NewPassword == "Shop2026!x" && r.ConfirmPassword == "Shop2026!x"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Save_Failure_ShowsTheLocalizedReason_AndKeepsTheFields()
    {
        _auth.ChangePasswordAsync(Arg.Any<long>(), Arg.Any<ChangePasswordRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Password_CurrentWrong"));
        var completed = false;
        _sut.Completed += (_, _) => completed = true;

        await _sut.SaveCommand.ExecuteAsync(null);

        Assert.False(completed);
        Assert.Equal("The current password is not correct.", _sut.ErrorMessage);
        Assert.Equal("Shop2026!x", _sut.NewPassword);
    }

    [Fact]
    public async Task Save_WithNobodySignedIn_Fails()
    {
        _session.SignOut();
        await _sut.SaveCommand.ExecuteAsync(null);
        Assert.Equal("User_NotFound", _sut.ErrorCode);
        await _auth.DidNotReceiveWithAnyArgs().ChangePasswordAsync(default, default!, default);
    }

    public void Dispose() => _sut.Dispose();
}
