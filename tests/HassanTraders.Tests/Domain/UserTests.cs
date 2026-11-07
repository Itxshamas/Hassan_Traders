using HassanTraders.Desktop.Core.Common;
using HassanTraders.Desktop.Core.Security;

namespace HassanTraders.Desktop.Core.Tests;

public class UserTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_NormalizesUsernameAndDefaultsDisplayName()
    {
        var user = User.Create("  Ali  ", " ", "hash", Role.Cashier, false, Now);
        Assert.Equal("ali", user.Username);
        Assert.Equal("ali", user.DisplayName);
        Assert.True(user.IsActive);
    }

    [Theory]
    [InlineData("", "hash")]
    [InlineData("   ", "hash")]
    [InlineData("ali", "")]
    public void Create_RejectsMissingUsernameOrHash(string username, string hash)
    {
        Assert.Throws<DomainException>(() => User.Create(username, "x", hash, Role.Owner, false, Now));
    }

    [Fact]
    public void Create_RejectsUnknownRole()
    {
        Assert.Throws<DomainException>(() => User.Create("ali", "Ali", "hash", (Role)99, false, Now));
    }

    [Fact]
    public void SetPassword_ClearsMustChangeFlag()
    {
        var user = User.Create("admin", "Admin", "old", Role.Owner, true, Now);
        user.SetPassword("new");
        Assert.Equal("new", user.PasswordHash);
        Assert.False(user.MustChangePassword);
    }

    [Fact]
    public void RecordLogin_StoresTime()
    {
        var user = User.Create("admin", "Admin", "h", Role.Owner, false, Now);
        user.RecordLogin(Now.AddHours(1));
        Assert.Equal(Now.AddHours(1), user.LastLoginUtc);
    }
}
