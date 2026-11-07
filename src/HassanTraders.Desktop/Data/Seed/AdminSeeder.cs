using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Core.Security;

namespace HassanTraders.Desktop.Data.Persistence.Seed;

internal sealed class AdminSeeder
{
    /// <summary>Known first-run password. The account is flagged so it must be changed at first login.</summary>
    public const string DefaultUsername = "admin";
    public const string DefaultPassword = "Admin@123";

    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IClock _clock;

    public AdminSeeder(IUserRepository users, IPasswordHasher hasher, IClock clock)
    {
        _users = users;
        _hasher = hasher;
        _clock = clock;
    }

    /// <returns>True if the admin user was created.</returns>
    public async Task<bool> SeedAsync(CancellationToken cancellationToken)
    {
        if (await _users.AnyAsync(cancellationToken))
        {
            return false;
        }

        var admin = User.Create(
            DefaultUsername,
            "Administrator",
            _hasher.Hash(DefaultPassword),
            Role.Owner,
            mustChangePassword: true,
            _clock.UtcNow);
        await _users.AddAsync(admin, cancellationToken);
        return true;
    }
}
