using System.Windows;
using System.Windows.Controls;

namespace HassanTraders.Desktop.Common.Controls;

public partial class SearchBox : UserControl
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(SearchBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(SearchBox), new PropertyMetadata(string.Empty));

    public SearchBox()
    {
        InitializeComponent();
        UpdateAffordances();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((SearchBox)d).UpdateAffordances();

    private void UpdateAffordances()
    {
        var empty = string.IsNullOrEmpty(Text);
        Placeholder_.Visibility = empty ? Visibility.Visible : Visibility.Collapsed;
        Clear.Visibility = empty ? Visibility.Collapsed : Visibility.Visible;
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        Text = string.Empty;
        Input.Focus();
    }
}
