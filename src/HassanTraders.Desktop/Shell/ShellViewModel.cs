using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Services.Application.Security;
using HassanTraders.Desktop.Services;
using HassanTraders.Desktop.Core.Security;

namespace HassanTraders.Desktop.Shell;

/// <summary>The signed-in main screen: left navigation (filtered by role) and the current page.</summary>
public sealed partial class ShellViewModel : LocalizedViewModel
{
    private readonly INavigationService _navigation;
    private readonly IUseCaseRunner _runner;
    private readonly IAppearanceService _appearance;
    private readonly UserInfo _user;

    public ShellViewModel(
        IUserSession session,
        INavigationService navigation,
        IUseCaseRunner runner,
        IAppearanceService appearance,
        ILocalizationService localization)
        : base(localization)
    {
        _user = session.Current ?? throw new InvalidOperationException("Nobody is signed in.");
        _navigation = navigation;
        _runner = runner;
        _appearance = appearance;

        foreach (var feature in RoleAccess.FeaturesFor(_user.Role))
        {
            NavItems.Add(new NavItemViewModel(localization, feature, NavItemViewModel.IconFor(feature)));
        }

        _navigation.PageChanged += OnPageChanged;
        SelectedNavItem = NavItems.FirstOrDefault();
    }

    public event EventHandler? SignedOut;

    public ObservableCollection<NavItemViewModel> NavItems { get; } = [];

    [ObservableProperty]
    private NavItemViewModel? _selectedNavItem;

    public object? CurrentPage => _navigation.CurrentPage;

    public string UserName => _user.DisplayName;

    public string RoleLabel => Localization[$"Role_{_user.Role}"];

    public string OtherLanguageLabel => Localization[Localization.Language == "ur" ? "Common_English" : "Common_Urdu"];

    partial void OnSelectedNavItemChanged(NavItemViewModel? value)
    {
        if (value is not null)
        {
            _navigation.Navigate(value.Feature);
        }
    }

    [RelayCommand]
    private async Task ToggleLanguageAsync() =>
        await _appearance.SetLanguageAsync(Localization.Language == "ur" ? "en" : "ur");

    [RelayCommand]
    private async Task SignOutAsync()
    {
        await _runner.RunAsync<IAuthService>(s => s.LogoutAsync());
        _navigation.Clear();
        SignedOut?.Invoke(this, EventArgs.Empty);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _navigation.PageChanged -= OnPageChanged;
            foreach (var item in NavItems)
            {
                item.Dispose();
            }
        }
    }

    private void OnPageChanged(object? sender, EventArgs e) => OnPropertyChanged(nameof(CurrentPage));
}
