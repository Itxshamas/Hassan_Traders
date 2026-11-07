using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Services.Application.Security;
using HassanTraders.Desktop.Core.Security;
using NSubstitute;

namespace HassanTraders.Desktop.Services.Application.Tests;

public class AuthServiceTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IAuditRepository _auditRepo = Substitute.For<IAuditRepository>();
    private readonly UserSession _session = new();
    private readonly TestClock _clock = new();
    private readonly FakeHasher _hasher = new();
    private readonly AuthService _sut;
    private readonly User _admin;

    public AuthServiceTests()
    {
        _admin = User.Create("admin", "Administrator", _hasher.Hash("Admin@123"), Role.Owner, true, _clock.UtcNow);
        _users.FindByUsernameAsync("admin", Arg.Any<CancellationToken>()).Returns(_admin);
        _users.FindByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(_admin);
        var audit = new AuditService(_auditRepo, _session, _clock);
        _sut = new AuthService(
            _users, _hasher, _uow, audit, _session, _clock,
            new LoginRequestValidator(), new ChangePasswordRequestValidator());
    }

    [Fact]
    public async Task Login_WithCorrectPassword_SignsInAndAudits()
    {
        var result = await _sut.LoginAsync(new LoginRequest("  ADMIN ", "Admin@123"));

        Assert.True(result.IsSuccess);
        Assert.Equal("admin", result.Value.Username);
        Assert.True(result.Value.MustChangePassword);
        Assert.Equal(result.Value, _session.Current);
        Assert.Equal(_clock.UtcNow, _admin.LastLoginUtc);
        _auditRepo.Received(1).Add(Arg.Is<AuditEntry>(e => e.Action == AuditAction.Login && e.Username == "admin"));
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Login_WithWrongPassword_FailsAuditsAndDoesNotSignIn()
    {
        var result = await _sut.LoginAsync(new LoginRequest("admin", "nope"));

        Assert.Equal("Login_Invalid", result.Error);
        Assert.Null(_session.Current);
        Assert.Null(_admin.LastLoginUtc);
        _auditRepo.Received(1).Add(Arg.Is<AuditEntry>(e => e.Action == AuditAction.LoginFailed));
    }

    [Fact]
    public async Task Login_WithUnknownUser_GivesSameErrorAsWrongPassword()
    {
        _users.FindByUsernameAsync("ghost", Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _sut.LoginAsync(new LoginRequest("ghost", "whatever"));

        Assert.Equal("Login_Invalid", result.Error);
        _auditRepo.Received(1).Add(Arg.Is<AuditEntry>(e => e.Action == AuditAction.LoginFailed && e.Username == "ghost"));
    }

    [Fact]
    public async Task Login_WithInactiveUser_IsRejected()
    {
        _admin.SetActive(false);

        var result = await _sut.LoginAsync(new LoginRequest("admin", "Admin@123"));

        Assert.Equal("Login_Inactive", result.Error);
        Assert.Null(_session.Current);
    }

    [Theory]
    [InlineData("", "x", "Validation_UsernameRequired")]
    [InlineData("admin", "", "Validation_PasswordRequired")]
    public async Task Login_WithMissingFields_FailsValidationWithoutTouchingTheDatabase(string user, string pass, string error)
    {
        var result = await _sut.LoginAsync(new LoginRequest(user, pass));

        Assert.Equal(error, result.Error);
        await _users.DidNotReceiveWithAnyArgs().FindByUsernameAsync(default!, default);
    }

    [Fact]
    public async Task ChangePassword_WithValidRequest_UpdatesHashClearsFlagAndAudits()
    {
        var result = await _sut.ChangePasswordAsync(0, new ChangePasswordRequest("Admin@123", "NewPass123", "NewPass123"));

        Assert.True(result.IsSuccess);
        Assert.Equal("hash:NewPass123", _admin.PasswordHash);
        Assert.False(_admin.MustChangePassword);
        Assert.False(_session.Current!.MustChangePassword);
        _auditRepo.Received(1).Add(Arg.Is<AuditEntry>(e => e.Action == AuditAction.PasswordChanged));
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("Admin@123", "short1", "short1", "Password_TooShort")]
    [InlineData("Admin@123", "onlyletters", "onlyletters", "Password_NeedsLetterAndDigit")]
    [InlineData("Admin@123", "12345678", "12345678", "Password_NeedsLetterAndDigit")]
    [InlineData("Admin@123", "NewPass123", "Different123", "Password_Mismatch")]
    [InlineData("Admin@123", "Admin@123", "Admin@123", "Password_SameAsOld")]
    [InlineData("", "NewPass123", "NewPass123", "Validation_PasswordRequired")]
    public async Task ChangePassword_RejectsInvalidRequests(string current, string next, string confirm, string error)
    {
        var result = await _sut.ChangePasswordAsync(1, new ChangePasswordRequest(current, next, confirm));

        Assert.Equal(error, result.Error);
        Assert.Equal("hash:Admin@123", _admin.PasswordHash);
        await _uow.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task ChangePassword_WithWrongCurrentPassword_Fails()
    {
        var result = await _sut.ChangePasswordAsync(1, new ChangePasswordRequest("wrong-one1", "NewPass123", "NewPass123"));

        Assert.Equal("Password_CurrentWrong", result.Error);
        Assert.Equal("hash:Admin@123", _admin.PasswordHash);
    }

    [Fact]
    public async Task ChangePassword_ForMissingUser_Fails()
    {
        _users.FindByIdAsync(99, Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _sut.ChangePasswordAsync(99, new ChangePasswordRequest("Admin@123", "NewPass123", "NewPass123"));

        Assert.Equal("User_NotFound", result.Error);
    }

    [Fact]
    public async Task Logout_ClearsSessionAndAuditsOnce()
    {
        await _sut.LoginAsync(new LoginRequest("admin", "Admin@123"));
        _auditRepo.ClearReceivedCalls();

        await _sut.LogoutAsync();
        await _sut.LogoutAsync();

        Assert.Null(_session.Current);
        _auditRepo.Received(1).Add(Arg.Is<AuditEntry>(e => e.Action == AuditAction.Logout && e.Username == "admin"));
    }
}
