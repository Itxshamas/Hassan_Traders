using System.Windows;
using HassanTraders.Desktop.Common.Controls;
using HassanTraders.Desktop.Services;

namespace HassanTraders.Desktop.Common.Helpers;

public sealed class WpfDialogService : IDialogService
{
    private readonly ILocalizationService _localization;

    public WpfDialogService(ILocalizationService localization) => _localization = localization;

    public void ShowError(string title, string message) =>
        Show(title, message, DialogKind.Error, _localization["Common_Ok"], null, danger: false);

    public void ShowInfo(string title, string message) =>
        Show(title, message, DialogKind.Info, _localization["Common_Ok"], null, danger: false);

    public bool Confirm(string title, string message, string confirmText, string cancelText, bool danger = false) =>
        Show(title, message, DialogKind.Confirm, confirmText, cancelText, danger);

    private bool Show(string title, string message, DialogKind kind, string confirmText, string? cancelText, bool danger)
    {
        var dialog = new DialogWindow(title, message, kind, confirmText, cancelText, danger, _localization.IsRightToLeft);
        var owner = System.Windows.Application.Current?.MainWindow;
        if (owner is { IsVisible: true })
        {
            dialog.Owner = owner;
        }
        else
        {
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        return dialog.ShowDialog() == true;
    }
}
