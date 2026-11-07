using HassanTraders.Desktop.Services.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace HassanTraders.Desktop.Services.Application;

public sealed class UseCaseRunner : IUseCaseRunner
{
    private readonly IServiceScopeFactory _scopeFactory;

    public UseCaseRunner(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

    public async Task<TResult> RunAsync<TService, TResult>(Func<TService, Task<TResult>> useCase)
        where TService : notnull
    {
        ArgumentNullException.ThrowIfNull(useCase);
        using var scope = _scopeFactory.CreateScope();
        return await useCase(scope.ServiceProvider.GetRequiredService<TService>());
    }

    public async Task RunAsync<TService>(Func<TService, Task> useCase)
        where TService : notnull
    {
        ArgumentNullException.ThrowIfNull(useCase);
        using var scope = _scopeFactory.CreateScope();
        await useCase(scope.ServiceProvider.GetRequiredService<TService>());
    }
}
