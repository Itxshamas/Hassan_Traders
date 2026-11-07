namespace HassanTraders.Desktop.Services;

/// <summary>Applies "light" or "dark" to the running UI. The only WPF-specific part of theming.</summary>
public interface IThemeApplier
{
    void Apply(string theme);
}
