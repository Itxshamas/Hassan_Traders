using System.Windows;
using System.Windows.Controls;
using SymbolRegular = Wpf.Ui.Controls.SymbolRegular;

namespace HassanTraders.Desktop.Common.Controls;

public partial class StateView : UserControl
{
    public static readonly DependencyProperty KindProperty =
        DependencyProperty.Register(nameof(Kind), typeof(StateKind), typeof(StateView), new PropertyMetadata(StateKind.Empty, OnAppearanceChanged));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(StateView), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(nameof(Message), typeof(string), typeof(StateView), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IconNameProperty =
        DependencyProperty.Register(nameof(IconName), typeof(string), typeof(StateView), new PropertyMetadata(string.Empty, OnAppearanceChanged));

    public static readonly DependencyProperty ActionProperty =
        DependencyProperty.Register(nameof(Action), typeof(object), typeof(StateView));

    public StateView()
    {
        InitializeComponent();
        ApplyAppearance();
    }

    public StateKind Kind
    {
        get => (StateKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>Optional WPF-UI symbol name; defaults depend on <see cref="Kind"/>.</summary>
    public string IconName
    {
        get => (string)GetValue(IconNameProperty);
        set => SetValue(IconNameProperty, value);
    }

    public object? Action
    {
        get => GetValue(ActionProperty);
        set => SetValue(ActionProperty, value);
    }

    private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((StateView)d).ApplyAppearance();

    private void ApplyAppearance()
    {
        var tone = Kind == StateKind.Error ? Tone.Danger : Tone.Info;
        IconHolder.SetResourceReference(Border.BackgroundProperty, Kind == StateKind.Error ? "Brush.DangerBg" : "Brush.PrimaryTint");
        Glyph.SetResourceReference(Control.ForegroundProperty, Kind == StateKind.Error ? ToneKeys.Foreground(tone) : "Brush.Primary");

        var loading = Kind == StateKind.Loading;
        Ring.Visibility = loading ? Visibility.Visible : Visibility.Collapsed;
        Glyph.Visibility = loading ? Visibility.Collapsed : Visibility.Visible;

        var fallback = Kind == StateKind.Error ? SymbolRegular.ErrorCircle24 : SymbolRegular.Box24;
        Glyph.Symbol = Enum.TryParse<SymbolRegular>(IconName, out var named) ? named : fallback;
    }
}
