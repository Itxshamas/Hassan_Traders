using FluentValidation;
using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Core.Common;
using HassanTraders.Desktop.Core.Security;

namespace HassanTraders.Desktop.Services.Application.Security;

public interface IAuthService
{
    Task<Result<UserInfo>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<Result> ChangePasswordAsync(long userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);
}

public sealed class AuthService : IAuthService
{
    // Verified against when the username is unknown, so both failure paths cost the same time.
    private readonly Lazy<string> _dummyHash;

    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _audit;
    private readonly IUserSession _session;
    private readonly IClock _clock;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<ChangePasswordRequest> _changeValidator;

    public AuthService(
        IUserRepository users,
        IPasswordHasher hasher,
        IUnitOfWork unitOfWork,
        IAuditService audit,
        IUserSession session,
        IClock clock,
        IValidator<LoginRequest> loginValidator,
        IValidator<ChangePasswordRequest> changeValidator)
    {
        _users = users;
        _hasher = hasher;
        _unitOfWork = unitOfWork;
        _audit = audit;
        _session = session;
        _clock = clock;
        _loginValidator = loginValidator;
        _changeValidator = changeValidator;
        _dummyHash = new Lazy<string>(() => _hasher.Hash("not-a-real-password"));
    }

    public async Task<Result<UserInfo>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<UserInfo>(validation.Errors[0].ErrorMessage);
        }

        var username = User.NormalizeUsername(request.Username);
        var user = await _users.FindByUsernameAsync(username, cancellationToken);

        var passwordOk = _hasher.Verify(request.Password, user?.PasswordHash ?? _dummyHash.Value);
        if (user is null || !passwordOk)
        {
            _audit.Record(AuditAction.LoginFailed, "Wrong username or password", user?.Id, username);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<UserInfo>("Login_Invalid");
        }

        if (!user.IsActive)
        {
            _audit.Record(AuditAction.LoginFailed, "Account is inactive", user.Id, user.Username);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<UserInfo>("Login_Inactive");
        }

        user.RecordLogin(_clock.UtcNow);
        _audit.Record(AuditAction.Login, "Signed in", user.Id, user.Username);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var info = ToInfo(user);
        _session.SignIn(info);
        return Result.Success(info);
    }

    public async Task<Result> ChangePasswordAsync(
        long userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await _changeValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure(validation.Errors[0].ErrorMessage);
        }

        var user = await _users.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result.Failure("User_NotFound");
        }

        if (!_hasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return Result.Failure("Password_CurrentWrong");
        }

        user.SetPassword(_hasher.Hash(request.NewPassword));
        _audit.Record(AuditAction.PasswordChanged, "Password changed", user.Id, user.Username);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _session.SignIn(ToInfo(user));
        return Result.Success();
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        if (_session.Current is not null)
        {
            _audit.Record(AuditAction.Logout, "Signed out");
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        _session.SignOut();
    }

    private static UserInfo ToInfo(User user) =>
        new(user.Id, user.Username, user.DisplayName, user.Role, user.MustChangePassword);
}
