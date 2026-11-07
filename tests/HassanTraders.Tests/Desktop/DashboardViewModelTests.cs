using HassanTraders.Desktop.Core.DTOs.Catalog;
using HassanTraders.Desktop.Features.Dashboard;
using HassanTraders.Desktop.Services;
using HassanTraders.Desktop.Services.Application.Catalog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace HassanTraders.Desktop.Tests;

public sealed class DashboardViewModelTests : IDisposable
{
    private readonly ICatalogQueryService _catalog = Substitute.For<ICatalogQueryService>();
    private readonly LocalizationService _loc = TestSupport.NewLocalization();
    private DashboardViewModel? _sut;

    public void Dispose() => _sut?.Dispose();

    private async Task<DashboardViewModel> CreateAsync(CatalogSummary summary)
    {
        _catalog.GetSummaryAsync(Arg.Any<CancellationToken>()).Returns(summary);
        _sut = new DashboardViewModel(TestSupport.Runner(s => s.AddSingleton(_catalog)), _loc, NullLogger<DashboardViewModel>.Instance);
        await _sut.LoadTask;
        return _sut;
    }

    private static CatalogSummary RealSummary() => CatalogQueryService.Summarize(CatalogTestData.Listing);

    [Fact]
    public async Task ShowsOnlyRealCatalogueNumbers()
    {
        var vm = await CreateAsync(RealSummary());

        Assert.Equal(ViewState.Ready, vm.State);
        Assert.Equal("3", vm.ActiveItemsText);
        Assert.Equal("1 inactive", vm.ActiveItemsHint);
        Assert.Equal("3", vm.CategoriesText);
        Assert.Equal("2", vm.TypesText);
        Assert.Equal("4", vm.VariantsText);
    }

    [Fact]
    public async Task AllActiveCatalogue_SaysSoInsteadOfZeroInactive()
    {
        var vm = await CreateAsync(new CatalogSummary(5, 0, 1, 0, 0, []));
        Assert.Equal("All items are active", vm.ActiveItemsHint);
    }

    [Fact]
    public async Task CategoryBars_UseTheLargestCategoryAsTheScale()
    {
        var vm = await CreateAsync(RealSummary());

        var rice = vm.CategoryBars.Single(b => b.Label == "Rice");
        Assert.Equal(2, rice.Count);
        Assert.All(vm.CategoryBars, b => Assert.Equal(2, b.Max));
    }

    [Fact]
    public async Task RangeCard_ShowsTheCategoryThatHasTypes_WithItemsPerType()
    {
        var vm = await CreateAsync(RealSummary());

        Assert.True(vm.HasRange);
        Assert.Equal("Rice by type", vm.RangeTitle);
        Assert.Equal(["Premium", "Economical"], vm.RangeBars.Select(b => b.Label));
        Assert.Equal([1, 1], vm.RangeBars.Select(b => b.Count));
    }

    [Fact]
    public async Task NoCategoryWithTypes_MeansNoRangeCard()
    {
        var vm = await CreateAsync(new CatalogSummary(1, 0, 1, 0, 0, [new CategoryBreakdown("Snacks", "نمکو", 1, [])]));
        Assert.False(vm.HasRange);
        Assert.Empty(vm.RangeBars);
    }

    [Fact]
    public async Task EmptyCatalogue_ShowsZeros_NotInventedNumbers()
    {
        var vm = await CreateAsync(new CatalogSummary(0, 0, 0, 0, 0, []));
        Assert.Equal("0", vm.ActiveItemsText);
        Assert.Empty(vm.CategoryBars);
        Assert.True(vm.ShowContent);
    }

    [Fact]
    public async Task SwitchingToUrdu_RelabelsTheBars()
    {
        var vm = await CreateAsync(RealSummary());

        _loc.SetLanguage("ur");

        Assert.Contains(vm.CategoryBars, b => b.Label == "چاول");
        Assert.Equal("چاول ورائٹی کے لحاظ سے", vm.RangeTitle);
        Assert.Equal("پریمیم", vm.RangeBars[0].Label);
        Assert.Equal("1 غیر فعال", vm.ActiveItemsHint);
    }

    [Fact]
    public async Task LoadFailure_ShowsErrorState_AndRetryRecovers()
    {
        var calls = 0;
        _catalog.GetSummaryAsync(Arg.Any<CancellationToken>()).Returns(_ =>
            ++calls == 1 ? Task.FromException<CatalogSummary>(new InvalidOperationException("boom")) : Task.FromResult(RealSummary()));
        _sut = new DashboardViewModel(TestSupport.Runner(s => s.AddSingleton(_catalog)), _loc, NullLogger<DashboardViewModel>.Instance);
        await _sut.LoadTask;
        Assert.True(_sut.ShowError);
        Assert.False(_sut.ShowContent);

        await _sut.RetryCommand.ExecuteAsync(null);

        Assert.True(_sut.ShowContent);
        Assert.Equal("3", _sut.ActiveItemsText);
    }
}
