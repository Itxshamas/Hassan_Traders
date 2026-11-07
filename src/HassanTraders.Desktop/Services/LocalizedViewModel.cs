using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace HassanTraders.Desktop.Services;

/// <summary>Base for ViewModels that show localized text: re-reads every property when the language changes.</summary>
public abstract class LocalizedViewModel : ObservableObject, IDisposable
{
    protected LocalizedViewModel(ILocalizationService localization)
    {
        Localization = localization;
        Localization.PropertyChanged += OnLocalizationChanged;
    }

    private bool _disposed;

    protected ILocalizationService Localization { get; }

    public bool IsRightToLeft => Localization.IsRightToLeft;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Localization.PropertyChanged -= OnLocalizationChanged;
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    /// <summary>Called when the UI language changed, before bindings refresh. Rebuild any cached localized text here.</summary>
    protected virtual void OnLanguageChanged()
    {
    }

    private void OnLocalizationChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName))
        {
            OnLanguageChanged();
            OnPropertyChanged(e);
        }
    }
}
