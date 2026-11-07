using HassanTraders.Desktop.Services.Application.Abstractions;

namespace HassanTraders.Desktop.Data;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
