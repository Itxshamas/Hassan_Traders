using System.Windows;
using System.Windows.Controls;
using HassanTraders.Desktop.Common.Converters;
using SymbolRegular = Wpf.Ui.Controls.SymbolRegular;

namespace HassanTraders.Desktop.Common.Controls;

/// <summary>Design-system dialog: info, error or confirm. Replaces MessageBox. Only presentation logic lives here.</summary>
public partial class DialogWindow : Window
{
    public DialogWindow(
        string title,
        string message,
        DialogKind kind,
        string confirmText,
        string? cancelText,
        bool danger,
        bool isRightToLeft)
    {
        InitializeComponent();
        FlowDirection = isRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        FontFamily = BoolToFontFamilyConverter.For(isRightToLeft);
        TextBlock.SetLineHeight(this, isRightToLeft ? 40 : double.NaN);

        TitleText.Text = title;
        MessageText.Text = message;
        ConfirmButton.Content = confirmText;
        if (cancelText is null)
        {
            CancelButton.Visibility = Visibility.Collapsed;
            ConfirmButton.IsCancel = true;
        }
        else
        {
            CancelButton.Content = cancelText;
        }

        var tone = kind switch
        {
            DialogKind.Error => Tone.Danger,
            DialogKind.Confirm when danger => Tone.Warning,
            _ => Tone.Info,
        };
        IconHolder.SetResourceReference(Border.BackgroundProperty, ToneKeys.Background(tone));
        Glyph.SetResourceReference(Control.ForegroundProperty, ToneKeys.Foreground(tone));
        Glyph.Symbol = kind switch
        {
            DialogKind.Error => SymbolRegular.ErrorCircle24,
            DialogKind.Confirm => SymbolRegular.QuestionCircle24,
            _ => SymbolRegular.Info24,
        };
        ConfirmButton.SetResourceReference(
            FrameworkElement.StyleProperty,
            kind == DialogKind.Error ? "Button.Primary" : danger ? "Button.Danger" : "Button.Primary");
    }

    private void OnConfirm(object sender, RoutedEventArgs e) => DialogResult = true;

    private void OnCancel(object sender, RoutedEventArgs e) => DialogResult = false;
}
