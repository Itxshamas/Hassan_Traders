using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace HassanTraders.Desktop.Common.Converters;

public sealed class BoolToFlowDirectionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Urdu (right-to-left) uses the bundled Noto Nastaliq Urdu; English uses Segoe UI.</summary>
public sealed class BoolToFontFamilyConverter : IValueConverter
{
    private static readonly FontFamily Urdu = new(new Uri("pack://application:,,,/Assets/Fonts/"), "./#Noto Nastaliq Urdu");
    private static readonly FontFamily English = new("Segoe UI Variable, Segoe UI");

    public static FontFamily For(bool isRightToLeft) => isRightToLeft ? Urdu : English;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        For(value is true);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Nastaliq script has tall ascenders and descenders and is clipped at the default line height.</summary>
public sealed class BoolToLineHeightConverter : IValueConverter
{
    private const double UrduLineHeight = 40;

    /// <summary>An optional converter parameter overrides the Urdu line height (for small captions).</summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not true)
        {
            return double.NaN;
        }

        return parameter is not null && double.TryParse(parameter.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var height)
            ? height
            : UrduLineHeight;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Body text is two points larger in Urdu so Nastaliq stays readable.</summary>
public sealed class BoolToFontSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? 16d : 14d;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class SymbolNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is string name && Enum.TryParse<SymbolRegular>(name, out var symbol) ? symbol : SymbolRegular.Circle24;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
