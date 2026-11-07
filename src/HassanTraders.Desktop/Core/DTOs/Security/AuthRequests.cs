using FluentValidation;

namespace HassanTraders.Desktop.Services.Application.Security;

public sealed record LoginRequest(string Username, string Password);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);

/// <summary>Validation messages are resource codes (Err_*), translated in the Desktop project.</summary>
public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("Validation_UsernameRequired");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Validation_PasswordRequired");
    }
}

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public const int MinimumLength = 8;

    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Validation_PasswordRequired");
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Validation_PasswordRequired")
            .MinimumLength(MinimumLength).WithMessage("Password_TooShort")
            .Must(HasLetterAndDigit).WithMessage("Password_NeedsLetterAndDigit");
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword).WithMessage("Password_Mismatch");
        RuleFor(x => x.NewPassword)
            .NotEqual(x => x.CurrentPassword).WithMessage("Password_SameAsOld");
    }

    private static bool HasLetterAndDigit(string password) =>
        password.Any(char.IsLetter) && password.Any(char.IsDigit);
}
