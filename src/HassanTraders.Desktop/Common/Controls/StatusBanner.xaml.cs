using System.Windows;
using System.Windows.Controls;
using SymbolRegular = Wpf.Ui.Controls.SymbolRegular;

namespace HassanTraders.Desktop.Common.Controls;

public partial class StatusBanner : UserControl
{
    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(nameof(Message), typeof(string), typeof(StatusBanner), new PropertyMetadata(string.Empty, OnMessageChanged));

    public static readonly DependencyProperty ToneProperty =
        DependencyProperty.Register(nameof(Tone), typeof(Tone), typeof(StatusBanner), new PropertyMetadata(Tone.Info, OnToneChanged));

    public StatusBanner()
    {
        InitializeComponent();
        ApplyTone();
        ApplyVisibility();
    }

    public string? Message
    {
        get => (string?)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public Tone Tone
    {
        get => (Tone)GetValue(ToneProperty);
        set => SetValue(ToneProperty, value);
    }

    private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((StatusBanner)d).ApplyVisibility();

    private static void OnToneChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((StatusBanner)d).ApplyTone();

    private void ApplyVisibility() =>
        Visibility = string.IsNullOrEmpty(Message) ? Visibility.Collapsed : Visibility.Visible;

    private void ApplyTone()
    {
        Bar.SetResourceReference(Border.BackgroundProperty, ToneKeys.Background(Tone));
        Bar.SetResourceReference(Border.BorderBrushProperty, ToneKeys.Foreground(Tone));
        Glyph.SetResourceReference(Control.ForegroundProperty, ToneKeys.Foreground(Tone));
        Body.SetResourceReference(TextBlock.ForegroundProperty, "Brush.TextPrimary");
        Glyph.Symbol = Tone switch
        {
            Tone.Success => SymbolRegular.CheckmarkCircle24,
            Tone.Warning => SymbolRegular.Warning24,
            Tone.Danger => SymbolRegular.ErrorCircle24,
            _ => SymbolRegular.Info24,
        };
    }
}
