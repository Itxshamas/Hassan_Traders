namespace HassanTraders.Desktop.Services.Application.Abstractions;

/// <summary>
/// Runs one use case in its own DI scope (and so its own DbContext). A desktop app has no request scope,
/// so ViewModels call services through this instead of holding a long-lived DbContext.
/// </summary>
public interface IUseCaseRunner
{
    Task<TResult> RunAsync<TService, TResult>(Func<TService, Task<TResult>> useCase)
        where TService : notnull;

    Task RunAsync<TService>(Func<TService, Task> useCase)
        where TService : notnull;
}
