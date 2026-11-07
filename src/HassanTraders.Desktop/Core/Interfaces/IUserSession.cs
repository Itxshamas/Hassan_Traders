namespace HassanTraders.Desktop.Services.Application.Abstractions;

/// <summary>Who is signed in right now (one desktop user at a time).</summary>
public interface IUserSession
{
    UserInfo? Current { get; }

    void SignIn(UserInfo user);

    void SignOut();
}
