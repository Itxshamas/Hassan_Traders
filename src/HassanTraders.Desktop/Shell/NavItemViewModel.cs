using HassanTraders.Desktop.Services;
using HassanTraders.Desktop.Core.Security;

namespace HassanTraders.Desktop.Shell;

public sealed class NavItemViewModel : LocalizedViewModel
{
    public NavItemViewModel(ILocalizationService localization, Feature feature, string iconName)
        : base(localization)
    {
        Feature = feature;
        IconName = iconName;
    }

    public Feature Feature { get; }

    /// <summary>Name of a WPF-UI SymbolRegular value; the view converts it to the icon.</summary>
    public string IconName { get; }

    public string Label => Localization[$"Nav_{Feature}"];

    public static string IconFor(Feature feature) => feature switch
    {
        Feature.Dashboard => "Home24",
        Feature.Billing => "ShoppingBag24",
        Feature.Items => "Box24",
        Feature.StockCount => "ClipboardTaskListLtr24",
        Feature.Purchases => "Receipt24",
        Feature.Udhaar => "Money24",
        Feature.Customers => "People24",
        Feature.Reports => "DataBarVertical24",
        Feature.Settings => "Settings24",
        _ => "Circle24",
    };
}
