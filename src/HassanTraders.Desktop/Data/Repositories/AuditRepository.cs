using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Core.Security;

namespace HassanTraders.Desktop.Data.Persistence.Repositories;

internal sealed class AuditRepository : IAuditRepository
{
    private readonly AppDbContext _db;

    public AuditRepository(AppDbContext db) => _db = db;

    public void Add(AuditEntry entry) => _db.AuditLog.Add(entry);
}
