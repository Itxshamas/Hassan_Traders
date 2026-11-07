using System.Windows.Data;
using System.Windows.Markup;
using HassanTraders.Desktop.Services;

namespace HassanTraders.Desktop.Common.Helpers;

/// <summary>
/// XAML translation: <c>Text="{infra:T Nav_Dashboard}"</c>. Binds to the localization indexer,
/// so the text updates live when the language changes.
/// </summary>
[MarkupExtensionReturnType(typeof(object))]
public sealed class TExtension : MarkupExtension
{
    public TExtension()
    {
        Key = string.Empty;
    }

    public TExtension(string key)
    {
        Key = key;
    }

    [ConstructorArgument("key")]
    public string Key { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new Binding($"[{Key}]")
        {
            Source = LocalizationService.Instance,
            Mode = BindingMode.OneWay,
        };
        return binding.ProvideValue(serviceProvider);
    }
}
