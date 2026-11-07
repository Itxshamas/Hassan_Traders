using HassanTraders.Desktop.Services.Application.Abstractions;

namespace HassanTraders.Desktop.Services.Application.Security;

public sealed class UserSession : IUserSession
{
    public UserInfo? Current { get; private set; }

    public void SignIn(UserInfo user)
    {
        ArgumentNullException.ThrowIfNull(user);
        Current = user;
    }

    public void SignOut() => Current = null;
}
