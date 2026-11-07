using System.Security.Cryptography;
using HassanTraders.Desktop.Services.Application.Abstractions;

namespace HassanTraders.Desktop.Data.Security;

/// <summary>PBKDF2-HMAC-SHA256. Stored as <c>pbkdf2-sha256$iterations$salt$hash</c> (base64 parts).</summary>
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    public const int DefaultIterations = 600_000;
    private const string Prefix = "pbkdf2-sha256";
    private const int SaltBytes = 16;
    private const int HashBytes = 32;

    private readonly int _iterations;

    public Pbkdf2PasswordHasher()
        : this(DefaultIterations)
    {
    }

    /// <summary>Lower iteration counts exist for tests only.</summary>
    public Pbkdf2PasswordHasher(int iterations)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(iterations, 1);
        _iterations = iterations;
    }

    public string Hash(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        var salt = RandomNumberGenerator.GetBytes(SaltBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, _iterations, HashAlgorithmName.SHA256, HashBytes);
        return $"{Prefix}${_iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string hash)
    {
        ArgumentNullException.ThrowIfNull(password);
        if (string.IsNullOrEmpty(hash))
        {
            return false;
        }

        var parts = hash.Split('$');
        if (parts.Length != 4 || parts[0] != Prefix || !int.TryParse(parts[1], out var iterations) || iterations < 1)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
