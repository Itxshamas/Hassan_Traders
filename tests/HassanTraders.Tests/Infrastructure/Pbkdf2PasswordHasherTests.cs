using HassanTraders.Desktop.Data.Security;

namespace HassanTraders.Desktop.Data.Tests;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _sut = new(iterations: 1_000);

    [Fact]
    public void Hash_ThenVerify_Succeeds()
    {
        var hash = _sut.Hash("Admin@123");
        Assert.True(_sut.Verify("Admin@123", hash));
        Assert.False(_sut.Verify("admin@123", hash));
    }

    [Fact]
    public void Hash_NeverContainsThePassword_AndUsesAFreshSalt()
    {
        var a = _sut.Hash("Admin@123");
        var b = _sut.Hash("Admin@123");
        Assert.DoesNotContain("Admin@123", a);
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void DefaultIterations_AreSlow()
    {
        Assert.True(Pbkdf2PasswordHasher.DefaultIterations >= 600_000);
        Assert.StartsWith("pbkdf2-sha256$600000$", new Pbkdf2PasswordHasher().Hash("x"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("plain-text")]
    [InlineData("pbkdf2-sha256$abc$AAAA$AAAA")]
    [InlineData("pbkdf2-sha256$1000$not base64!$AAAA")]
    [InlineData("other-algo$1000$AAAA$AAAA")]
    public void Verify_RejectsMalformedHashesWithoutThrowing(string hash)
    {
        Assert.False(_sut.Verify("anything", hash));
    }

    [Fact]
    public void Verify_HonoursTheIterationCountStoredInTheHash()
    {
        var hash = new Pbkdf2PasswordHasher(2_000).Hash("secret1A");
        Assert.True(_sut.Verify("secret1A", hash));
    }
}
