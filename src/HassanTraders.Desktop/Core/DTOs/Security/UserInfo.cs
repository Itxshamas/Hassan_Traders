using HassanTraders.Desktop.Core.Security;

namespace HassanTraders.Desktop.Services.Application.Abstractions;

public sealed record UserInfo(long Id, string Username, string DisplayName, Role Role, bool MustChangePassword);
