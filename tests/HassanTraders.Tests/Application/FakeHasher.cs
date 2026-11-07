using HassanTraders.Desktop.Services.Application.Abstractions;

namespace HassanTraders.Desktop.Services.Application.Tests;

/// <summary>Deterministic stand-in: "hash:" + password. Fast, and never used outside tests.</summary>
internal sealed class FakeHasher : IPasswordHasher
{
    public string Hash(string password) => "hash:" + password;

    public bool Verify(string password, string hash) => hash == "hash:" + password;
}
