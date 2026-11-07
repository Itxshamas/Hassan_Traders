using CommunityToolkit.Mvvm.ComponentModel;
using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Features.Login;

namespace HassanTraders.Desktop.Shell;

/// <summary>Owns the single window's content: login, then forced password change if flagged, then the shell.</summary>
public sealed partial class RootViewModel : ObservableObject, IDisposable
{
    private readonly Func<LoginViewModel> _loginFactory;
    private readonly Func<ChangePasswordViewModel> _changePasswordFactory;
    private readonly Func<ShellViewModel> _shellFactory;

    public RootViewModel(
        Func<LoginViewModel> loginFactory,
        Func<ChangePasswordViewModel> changePasswordFactory,
        Func<ShellViewModel> shellFactory)
    {
        _loginFactory = loginFactory;
        _changePasswordFactory = changePasswordFactory;
        _shellFactory = shellFactory;
    }

    [ObservableProperty]
    private object? _current;

    public void Start() => ShowLogin();

    public void Dispose() => Replace(null);

    private void ShowLogin()
    {
        var login = _loginFactory();
        login.LoggedIn += OnLoggedIn;
        Replace(login);
    }

    private void OnLoggedIn(object? sender, UserInfo user)
    {
        if (user.MustChangePassword)
        {
            var change = _changePasswordFactory();
            change.Completed += (_, _) => ShowShell();
            Replace(change);
        }
        else
        {
            ShowShell();
        }
    }

    private void ShowShell()
    {
        var shell = _shellFactory();
        shell.SignedOut += (_, _) => ShowLogin();
        Replace(shell);
    }

    private void Replace(object? next)
    {
        var previous = Current;
        Current = next;
        (previous as IDisposable)?.Dispose();
    }
}
