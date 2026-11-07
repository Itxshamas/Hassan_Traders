using Microsoft.Extensions.DependencyInjection;

namespace HassanTraders.Desktop.Services.Application.Tests;

public class UseCaseRunnerTests
{
    private sealed class ScopedThing
    {
        public Guid Id { get; } = Guid.NewGuid();
    }

    [Fact]
    public async Task RunAsync_UsesAFreshScopePerCall()
    {
        var services = new ServiceCollection();
        services.AddScoped<ScopedThing>();
        await using var provider = services.BuildServiceProvider();
        var sut = new UseCaseRunner(provider.GetRequiredService<IServiceScopeFactory>());

        var first = await sut.RunAsync<ScopedThing, Guid>(s => Task.FromResult(s.Id));
        var second = await sut.RunAsync<ScopedThing, Guid>(s => Task.FromResult(s.Id));

        Assert.NotEqual(first, second);
    }

    [Fact]
    public async Task AddApplication_RegistersValidatorsAndServices()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        await using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<FluentValidation.IValidator<Security.LoginRequest>>());
        Assert.NotNull(provider.GetService<Abstractions.IUseCaseRunner>());
    }
}
