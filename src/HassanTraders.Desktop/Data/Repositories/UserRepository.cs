using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Core.Security;
using Microsoft.EntityFrameworkCore;

namespace HassanTraders.Desktop.Data.Persistence.Repositories;

internal sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db) => _db = db;

    public Task<User?> FindByUsernameAsync(string normalizedUsername, CancellationToken cancellationToken = default) =>
        _db.Users.FirstOrDefaultAsync(u => u.Username == normalizedUsername, cancellationToken);

    public Task<User?> FindByIdAsync(long id, CancellationToken cancellationToken = default) =>
        _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await _db.Users.AddAsync(user, cancellationToken);

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default) =>
        _db.Users.AnyAsync(cancellationToken);
}
