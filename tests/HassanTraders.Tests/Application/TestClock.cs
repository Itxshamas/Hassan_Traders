using HassanTraders.Desktop.Services.Application.Abstractions;

namespace HassanTraders.Desktop.Services.Application.Tests;

internal sealed class TestClock : IClock
{
    public DateTime UtcNow { get; set; } = new(2026, 3, 1, 9, 30, 0, DateTimeKind.Utc);
}
