using HassanTraders.Desktop.Core.Common;

namespace HassanTraders.Desktop.Core.Tests;

public class ResultTests
{
    [Fact]
    public void Success_HasNoError()
    {
        var result = Result.Success(42);
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Failure_CarriesCodeAndHasNoValue()
    {
        var result = Result.Failure<int>("Login_Invalid");
        Assert.True(result.IsFailure);
        Assert.Equal("Login_Invalid", result.Error);
        Assert.Throws<DomainException>(() => result.Value);
    }

    [Fact]
    public void Failure_RequiresAnErrorCode()
    {
        Assert.Throws<DomainException>(() => Result.Failure(string.Empty));
    }
}
