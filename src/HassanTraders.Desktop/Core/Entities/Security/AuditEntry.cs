using HassanTraders.Desktop.Core.Common;

namespace HassanTraders.Desktop.Core.Security;

public sealed class AuditEntry : Entity
{
    private AuditEntry()
    {
        Username = string.Empty;
        Details = string.Empty;
    }

    public AuditEntry(DateTime timestampUtc, AuditAction action, long? userId, string username, string details)
    {
        TimestampUtc = timestampUtc;
        Action = action;
        UserId = userId;
        Username = username ?? string.Empty;
        Details = details ?? string.Empty;
    }

    public DateTime TimestampUtc { get; private set; }

    public AuditAction Action { get; private set; }

    public long? UserId { get; private set; }

    /// <summary>Kept as text so the entry survives user deletion and records failed attempts for unknown names.</summary>
    public string Username { get; private set; }

    public string Details { get; private set; }
}
