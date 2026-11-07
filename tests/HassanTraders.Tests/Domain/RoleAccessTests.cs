using HassanTraders.Desktop.Core.Security;

namespace HassanTraders.Desktop.Core.Tests;

public class RoleAccessTests
{
    [Fact]
    public void Owner_CanOpenEverything()
    {
        foreach (var feature in Enum.GetValues<Feature>())
        {
            Assert.True(RoleAccess.CanAccess(Role.Owner, feature));
        }
    }

    [Theory]
    [InlineData(Role.Cashier, Feature.Billing, true)]
    [InlineData(Role.Cashier, Feature.Udhaar, true)]
    [InlineData(Role.Cashier, Feature.Reports, false)]
    [InlineData(Role.Cashier, Feature.Items, false)]
    [InlineData(Role.StoreKeeper, Feature.StockCount, true)]
    [InlineData(Role.StoreKeeper, Feature.Billing, false)]
    [InlineData(Role.StoreKeeper, Feature.Reports, false)]
    [InlineData(Role.Manager, Feature.Purchases, true)]
    [InlineData(Role.Manager, Feature.Reports, true)]
    public void Matrix_FollowsRoleTable(Role role, Feature feature, bool expected)
    {
        Assert.Equal(expected, RoleAccess.CanAccess(role, feature));
    }

    [Fact]
    public void EveryRole_CanOpenSettingsForLanguageAndTheme()
    {
        foreach (var role in Enum.GetValues<Role>())
        {
            Assert.True(RoleAccess.CanAccess(role, Feature.Settings));
        }
    }

    [Fact]
    public void FeaturesFor_KeepsNavigationOrder()
    {
        var cashier = RoleAccess.FeaturesFor(Role.Cashier);
        Assert.Equal([Feature.Billing, Feature.Udhaar, Feature.Settings], cashier);
    }
}
