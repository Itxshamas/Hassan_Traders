using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Services.Application.Security;
using HassanTraders.Desktop.Services;
using HassanTraders.Desktop.Core.Common;

namespace HassanTraders.Desktop.Features.Login;

public sealed partial class LoginViewModel : LocalizedViewModel
{
    private readonly IUseCaseRunner _runner;
    private readonly IAppearanceService _appearance;

    public LoginViewModel(IUseCaseRunner runner, IAppearanceService appearance, ILocalizationService localization)
        : base(localization)
    {
        _runner = runner;
        _appearance = appearance;
    }

    public event EventHandler<UserInfo>? LoggedIn;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _username = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _password = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    [NotifyPropertyChangedFor(nameof(LoginButtonText))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorCode;

    public string LoginButtonText => Localization[IsBusy ? "Login_Busy" : "Login_Button"];

    public bool HasError => !string.IsNullOrEmpty(ErrorCode);

    /// <summary>Shown on the language button: the language you would switch to.</summary>
    public string OtherLanguageLabel => Localization[Localization.Language == "ur" ? "Common_English" : "Common_Urdu"];

    public string ErrorMessage => ErrorCode is null ? string.Empty : Localization[ErrorCode];

    private bool CanLogin() => !IsBusy && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrEmpty(Password);

    partial void OnErrorCodeChanged(string? value) => OnPropertyChanged(nameof(ErrorMessage));

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        IsBusy = true;
        ErrorCode = null;
        try
        {
            var request = new LoginRequest(Username, Password);
            var result = await _runner.RunAsync<IAuthService, Result<UserInfo>>(s => s.LoginAsync(request));
            if (result.IsFailure)
            {
                ErrorCode = result.Error;
                Password = string.Empty;
                return;
            }

            Password = string.Empty;
            LoggedIn?.Invoke(this, result.Value);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ToggleLanguageAsync()
    {
        await _appearance.SetLanguageAsync(Localization.Language == "ur" ? "en" : "ur");
        OnPropertyChanged(nameof(OtherLanguageLabel));
    }
}
