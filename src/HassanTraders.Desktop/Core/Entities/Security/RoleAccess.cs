namespace HassanTraders.Desktop.Core.Security;

/// <summary>Which role may open which screen (see CLAUDE.md section 2 and docs/decisions.md D-08).</summary>
public static class RoleAccess
{
    private static readonly Dictionary<Role, Feature[]> Map = new()
    {
        [Role.Owner] = Enum.GetValues<Feature>(),
        [Role.Manager] =
        [
            Feature.Dashboard, Feature.Billing, Feature.Items, Feature.StockCount, Feature.Purchases,
            Feature.Udhaar, Feature.Customers, Feature.Reports, Feature.Settings,
        ],
        [Role.Cashier] = [Feature.Billing, Feature.Udhaar, Feature.Settings],
        [Role.StoreKeeper] = [Feature.StockCount, Feature.Purchases, Feature.Settings],
    };

    public static bool CanAccess(Role role, Feature feature) =>
        Map.TryGetValue(role, out var features) && Array.IndexOf(features, feature) >= 0;

    public static IReadOnlyList<Feature> FeaturesFor(Role role) =>
        Enum.GetValues<Feature>().Where(f => CanAccess(role, f)).ToArray();
}
