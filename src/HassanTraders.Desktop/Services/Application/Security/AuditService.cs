using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Core.Security;

namespace HassanTraders.Desktop.Services.Application.Security;

public interface IAuditService
{
    /// <summary>Stages an audit entry for the current user (or the given name). The caller saves the unit of work.</summary>
    void Record(AuditAction action, string details, long? userId = null, string? username = null);
}

public sealed class AuditService : IAuditService
{
    private readonly IAuditRepository _repository;
    private readonly IUserSession _session;
    private readonly IClock _clock;

    public AuditService(IAuditRepository repository, IUserSession session, IClock clock)
    {
        _repository = repository;
        _session = session;
        _clock = clock;
    }

    public void Record(AuditAction action, string details, long? userId = null, string? username = null)
    {
        var current = _session.Current;
        _repository.Add(new AuditEntry(
            _clock.UtcNow,
            action,
            userId ?? current?.Id,
            username ?? current?.Username ?? string.Empty,
            details));
    }
}
