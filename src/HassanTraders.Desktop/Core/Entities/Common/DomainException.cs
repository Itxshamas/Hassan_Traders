namespace HassanTraders.Desktop.Core.Common;

/// <summary>Thrown when a domain invariant would be broken. The message is for logs, not for end users.</summary>
public sealed class DomainException : Exception
{
    public DomainException()
    {
    }

    public DomainException(string message)
        : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
