using System.Windows;
using System.Windows.Controls;

namespace HassanTraders.Desktop.Common.Controls;

/// <summary>PasswordBox.Password cannot be bound by default. This attached property adds a two-way binding.</summary>
public static class PasswordBoxBinder
{
    // Default is null (not "") so a bound empty string still counts as a change and attaches the handler.
    public static readonly DependencyProperty BoundPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BoundPassword",
            typeof(string),
            typeof(PasswordBoxBinder),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundPasswordChanged));

    public static string? GetBoundPassword(DependencyObject d) => (string?)d.GetValue(BoundPasswordProperty);

    public static void SetBoundPassword(DependencyObject d, string? value) => d.SetValue(BoundPasswordProperty, value);

    private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox box)
        {
            return;
        }

        box.PasswordChanged -= OnPasswordChanged;
        var value = (string?)e.NewValue ?? string.Empty;
        if (box.Password != value)
        {
            box.Password = value;
        }

        box.PasswordChanged += OnPasswordChanged;
    }

    private static void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        var box = (PasswordBox)sender;
        SetBoundPassword(box, box.Password);
    }
}
