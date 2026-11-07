using System.Windows;
using System.Windows.Controls;

namespace HassanTraders.Desktop.Common.Controls;

/// <summary>
/// A labelled input: label above, the input as Content, error message below.
/// <code>&lt;controls:FormField Label="{wpf:T Login_Username}" Error="{Binding UsernameError}"&gt;&lt;TextBox/&gt;&lt;/controls:FormField&gt;</code>
/// Template lives in Resources/Styles/Forms.xaml.
/// </summary>
public class FormField : ContentControl
{
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(FormField), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ErrorProperty =
        DependencyProperty.Register(nameof(Error), typeof(string), typeof(FormField), new PropertyMetadata(string.Empty));

    static FormField() =>
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FormField), new FrameworkPropertyMetadata(typeof(FormField)));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string? Error
    {
        get => (string?)GetValue(ErrorProperty);
        set => SetValue(ErrorProperty, value);
    }
}
