using HassanTraders.Desktop.Core.Common;

namespace HassanTraders.Desktop.Core.Security;

public sealed class User : Entity
{
    private User()
    {
        Username = string.Empty;
        DisplayName = string.Empty;
        PasswordHash = string.Empty;
    }

    /// <summary>Usernames are stored lower-case and trimmed, so lookups are case-insensitive.</summary>
    public string Username { get; private set; }

    public string DisplayName { get; private set; }

    public string PasswordHash { get; private set; }

    public Role Role { get; private set; }

    public bool IsActive { get; private set; }

    public bool MustChangePassword { get; private set; }

    public DateTime CreatedUtc { get; private set; }

    public DateTime? LastLoginUtc { get; private set; }

    public static string NormalizeUsername(string? username) => (username ?? string.Empty).Trim().ToLowerInvariant();

    public static User Create(
        string username,
        string displayName,
        string passwordHash,
        Role role,
        bool mustChangePassword,
        DateTime createdUtc)
    {
        var normalized = NormalizeUsername(username);
        if (normalized.Length == 0)
        {
            throw new DomainException("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("A password hash is required.");
        }

        if (!Enum.IsDefined(role))
        {
            throw new DomainException("Unknown role.");
        }

        return new User
        {
            Username = normalized,
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? normalized : displayName.Trim(),
            PasswordHash = passwordHash,
            Role = role,
            IsActive = true,
            MustChangePassword = mustChangePassword,
            CreatedUtc = createdUtc,
        };
    }

    public void SetPassword(string newPasswordHash, bool mustChangePassword = false)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
        {
            throw new DomainException("A password hash is required.");
        }

        PasswordHash = newPasswordHash;
        MustChangePassword = mustChangePassword;
    }

    public void RecordLogin(DateTime nowUtc) => LastLoginUtc = nowUtc;

    public void SetActive(bool isActive) => IsActive = isActive;
}
