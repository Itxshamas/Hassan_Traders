namespace HassanTraders.Desktop.Core.Security;

public enum AuditAction
{
    Login = 1,
    LoginFailed = 2,
    Logout = 3,
    PasswordChanged = 4,
    PriceChanged = 5,
    RecordDeleted = 6,
    DiscountOverLimit = 7,
    StockAdjusted = 8,
    SettingChanged = 9,
}
