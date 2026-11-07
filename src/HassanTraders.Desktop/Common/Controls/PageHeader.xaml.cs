using System.Windows;
using System.Windows.Controls;

namespace HassanTraders.Desktop.Common.Controls;

public partial class PageHeader : UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(PageHeader), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(PageHeader), new PropertyMetadata(string.Empty, OnSubtitleChanged));

    public static readonly DependencyProperty ActionsProperty =
        DependencyProperty.Register(nameof(Actions), typeof(object), typeof(PageHeader));

    private static readonly DependencyPropertyKey HasSubtitlePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasSubtitle), typeof(bool), typeof(PageHeader), new PropertyMetadata(false));

    public static readonly DependencyProperty HasSubtitleProperty = HasSubtitlePropertyKey.DependencyProperty;

    public PageHeader() => InitializeComponent();

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public object? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    public bool HasSubtitle => (bool)GetValue(HasSubtitleProperty);

    private static void OnSubtitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        d.SetValue(HasSubtitlePropertyKey, !string.IsNullOrEmpty(e.NewValue as string));
}
