using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Services.Application.Security;
using HassanTraders.Desktop.Services;
using HassanTraders.Desktop.Core.Common;

namespace HassanTraders.Desktop.Features.Login;

/// <summary>Shown right after login when the account is flagged "must change password".</summary>
public sealed partial class ChangePasswordViewModel : LocalizedViewModel
{
    private readonly IUseCaseRunner _runner;
    private readonly IUserSession _session;

    public ChangePasswordViewModel(IUseCaseRunner runner, IUserSession session, ILocalizationService localization)
        : base(localization)
    {
        _runner = runner;
        _session = session;
    }

    public event EventHandler? Completed;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _currentPassword = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorCode;

    public bool HasError => !string.IsNullOrEmpty(ErrorCode);

    public string ErrorMessage => ErrorCode is null ? string.Empty : Localization[ErrorCode];

    private bool CanSave() =>
        !IsBusy && CurrentPassword.Length > 0 && NewPassword.Length > 0 && ConfirmPassword.Length > 0;

    partial void OnErrorCodeChanged(string? value) => OnPropertyChanged(nameof(ErrorMessage));

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        var user = _session.Current;
        if (user is null)
        {
            ErrorCode = "User_NotFound";
            return;
        }

        IsBusy = true;
        ErrorCode = null;
        try
        {
            var request = new ChangePasswordRequest(CurrentPassword, NewPassword, ConfirmPassword);
            var result = await _runner.RunAsync<IAuthService, Result>(s => s.ChangePasswordAsync(user.Id, request));
            if (result.IsFailure)
            {
                ErrorCode = result.Error;
                return;
            }

            CurrentPassword = NewPassword = ConfirmPassword = string.Empty;
            Completed?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
