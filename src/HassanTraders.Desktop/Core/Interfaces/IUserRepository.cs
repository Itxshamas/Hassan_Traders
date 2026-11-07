using HassanTraders.Desktop.Core.Security;

namespace HassanTraders.Desktop.Services.Application.Abstractions;

public interface IUserRepository
{
    /// <summary>Finds by normalized (lower-case, trimmed) username.</summary>
    Task<User?> FindByUsernameAsync(string normalizedUsername, CancellationToken cancellationToken = default);

    Task<User?> FindByIdAsync(long id, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(CancellationToken cancellationToken = default);
}
