namespace HassanTraders.Desktop.Services.Application.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}
