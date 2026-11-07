using System.Windows;
using System.Windows.Controls;

namespace HassanTraders.Desktop.Common.Controls;

/// <summary>Rounded status label (OK / Low / Out, Active / Inactive). Colour comes from <see cref="Tone"/>.</summary>
public partial class StatusPill : UserControl
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(StatusPill), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ToneProperty =
        DependencyProperty.Register(nameof(Tone), typeof(Tone), typeof(StatusPill), new PropertyMetadata(Tone.Neutral, OnToneChanged));

    public StatusPill()
    {
        InitializeComponent();
        ApplyTone();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Tone Tone
    {
        get => (Tone)GetValue(ToneProperty);
        set => SetValue(ToneProperty, value);
    }

    private static void OnToneChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((StatusPill)d).ApplyTone();

    private void ApplyTone()
    {
        // Resource references (not copies) so the pill follows light/dark theme switches.
        Pill.SetResourceReference(Border.BackgroundProperty, ToneKeys.Background(Tone));
        Caption.SetResourceReference(TextBlock.ForegroundProperty, ToneKeys.Foreground(Tone));
    }
}
