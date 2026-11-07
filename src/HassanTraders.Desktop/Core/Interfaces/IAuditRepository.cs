using HassanTraders.Desktop.Core.Security;

namespace HassanTraders.Desktop.Services.Application.Abstractions;

public interface IAuditRepository
{
    /// <summary>Stages the entry; it is written when the unit of work saves.</summary>
    void Add(AuditEntry entry);
}
