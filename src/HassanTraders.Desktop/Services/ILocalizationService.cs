using System.ComponentModel;

namespace HassanTraders.Desktop.Services;

/// <summary>Looks up user-visible text from the .resx files. Contains no WPF types so ViewModels stay testable.</summary>
public interface ILocalizationService : INotifyPropertyChanged
{
    /// <summary>"en" or "ur".</summary>
    string Language { get; }

    bool IsRightToLeft { get; }

    string this[string key] { get; }

    string Format(string key, params object[] args);

    void SetLanguage(string language);
}
