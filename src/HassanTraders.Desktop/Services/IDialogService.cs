namespace HassanTraders.Desktop.Services;

public interface IDialogService
{
    void ShowError(string title, string message);

    void ShowInfo(string title, string message);

    /// <summary>Asks a yes/no question. Use <paramref name="danger"/> for destructive actions (delete, discard).</summary>
    bool Confirm(string title, string message, string confirmText, string cancelText, bool danger = false);
}
