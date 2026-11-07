using System.Diagnostics;
using HassanTraders.Desktop.Core.Catalog;
using HassanTraders.Desktop.Core.Common;
using HassanTraders.Desktop.Core.DTOs.Catalog;
using HassanTraders.Desktop.Features.Items;
using HassanTraders.Desktop.Services;
using HassanTraders.Desktop.Services.Application.Catalog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace HassanTraders.Desktop.Tests;

public sealed class ItemsViewModelTests : IDisposable
{
    private readonly ICatalogQueryService _catalog = Substitute.For<ICatalogQueryService>();
    private readonly LocalizationService _loc = TestSupport.NewLocalization();
    private ItemsViewModel? _sut;

    public void Dispose() => _sut?.Dispose();

    private async Task<ItemsViewModel> CreateAsync(CatalogListing? listing = null)
    {
        _catalog.GetListingAsync(Arg.Any<CancellationToken>()).Returns(listing ?? CatalogTestData.Listing);
        var runner = TestSupport.Runner(s => s.AddSingleton(_catalog));
        _sut = new ItemsViewModel(runner, _loc, NullLogger<ItemsViewModel>.Instance);
        await _sut.LoadTask;
        return _sut;
    }

    [Fact]
    public async Task Load_ShowsEveryItem_WithBilingualRows()
    {
        var vm = await CreateAsync();

        Assert.Equal(ViewState.Ready, vm.State);
        Assert.Equal(4, vm.Rows.Count);
        Assert.Equal("Showing 4 of 4 items", vm.Subtitle);
        var lays = vm.Rows.Single(r => r.Id == 1);
        Assert.Equal("Lays Classic", lays.PrimaryName);
        Assert.Equal("لیز کلاسک", lays.SecondaryName);
        Assert.True(lays.SecondaryIsUrdu);
        Assert.Equal("Rs 20", lays.PriceText);
        Assert.Equal("4 pack sizes", lays.VariantsText);
    }

    [Fact]
    public async Task Load_FormatsRiceAsBagsAndLooseItemsPerKilo()
    {
        var vm = await CreateAsync();

        Assert.Equal("25 kg bag", vm.Rows.Single(r => r.Id == 2).PackLabel);
        Assert.Equal("Rs 6,200", vm.Rows.Single(r => r.Id == 2).PriceText);
        Assert.Equal("Loose, per kg", vm.Rows.Single(r => r.Id == 4).PackLabel);
        Assert.Equal("Inactive", vm.Rows.Single(r => r.Id == 4).StatusText);
    }

    [Theory]
    [InlineData("lays", 1)]
    [InlineData("LAYS CLASSIC", 1)]
    [InlineData("لیز", 1)]
    [InlineData("inam", 1)]
    [InlineData("8901", 1)]
    [InlineData("rice premium", 1)]
    [InlineData("چاول", 2)]
    [InlineData("چيني", 1)]
    [InlineData("zzz", 0)]
    public async Task Search_FindsByNameInEitherScriptBrandBarcodeOrCategory(string text, int expected)
    {
        var vm = await CreateAsync();

        vm.SearchText = text;

        Assert.Equal(expected, vm.Rows.Count);
        Assert.Equal(expected == 0 ? ViewState.NoMatches : ViewState.Ready, vm.State);
    }

    [Fact]
    public async Task CategoryThenTypeFilter_NarrowsTheList()
    {
        var vm = await CreateAsync();
        Assert.False(vm.HasTypes);

        vm.SelectedCategory = vm.CategoryOptions.Single(o => o.Id == 2);

        Assert.Equal(2, vm.Rows.Count);
        Assert.True(vm.HasTypes);
        Assert.Equal(["All types", "Premium", "Economical"], vm.TypeOptions.Select(o => o.Label));

        vm.SelectedType = vm.TypeOptions.Single(o => o.Id == 11);

        Assert.Equal([3L], vm.Rows.Select(r => r.Id));
    }

    [Fact]
    public async Task ChangingCategory_ResetsTheType()
    {
        var vm = await CreateAsync();
        vm.SelectedCategory = vm.CategoryOptions.Single(o => o.Id == 2);
        vm.SelectedType = vm.TypeOptions.Single(o => o.Id == 10);

        vm.SelectedCategory = vm.CategoryOptions.Single(o => o.Id == 1);

        Assert.Null(vm.SelectedType!.Id);
        Assert.Single(vm.Rows);
    }

    [Fact]
    public async Task ClearFilters_RestoresEverything()
    {
        var vm = await CreateAsync();
        vm.SearchText = "rice";
        vm.SelectedCategory = vm.CategoryOptions.Single(o => o.Id == 2);
        Assert.True(vm.HasActiveFilter);

        vm.ClearFiltersCommand.Execute(null);

        Assert.False(vm.HasActiveFilter);
        Assert.Equal(4, vm.Rows.Count);
        Assert.Equal(string.Empty, vm.SearchText);
    }

    [Fact]
    public async Task EmptyCatalogue_ShowsTheEmptyState_NotTheNoMatchState()
    {
        var vm = await CreateAsync(new CatalogListing([], []));

        Assert.Equal(ViewState.Empty, vm.State);
        Assert.True(vm.ShowEmpty);
        Assert.False(vm.ShowGrid);
    }

    [Fact]
    public async Task LoadFailure_ShowsTheErrorState_AndRetryRecovers()
    {
        var calls = 0;
        _catalog.GetListingAsync(Arg.Any<CancellationToken>()).Returns(_ =>
            ++calls == 1 ? Task.FromException<CatalogListing>(new InvalidOperationException("db locked")) : Task.FromResult(CatalogTestData.Listing));
        var runner = TestSupport.Runner(s => s.AddSingleton(_catalog));
        _sut = new ItemsViewModel(runner, _loc, NullLogger<ItemsViewModel>.Instance);
        await _sut.LoadTask;

        Assert.Equal(ViewState.Error, _sut.State);
        Assert.True(_sut.ShowError);

        await _sut.RetryCommand.ExecuteAsync(null);

        Assert.Equal(ViewState.Ready, _sut.State);
        Assert.Equal(4, _sut.Rows.Count);
    }

    [Fact]
    public async Task SwitchingToUrdu_SwapsLanguageOnRowsAndFilters_KeepingTheSelection()
    {
        var vm = await CreateAsync();
        vm.SelectedCategory = vm.CategoryOptions.Single(o => o.Id == 2);
        vm.SelectedType = vm.TypeOptions.Single(o => o.Id == 10);

        _loc.SetLanguage("ur");

        var row = vm.Rows.Single();
        Assert.Equal("انعام سپریم چاول", row.PrimaryName);
        Assert.Equal("Inam Supreme Rice", row.SecondaryName);
        Assert.False(row.SecondaryIsUrdu);
        Assert.Equal("چاول", row.CategoryLabel);
        Assert.Equal("پریمیم", row.TypeLabel);
        Assert.Equal("25 کلو بوری", row.PackLabel);
        Assert.Equal("تمام اقسام", vm.CategoryOptions[0].Label);
        Assert.Equal(2, vm.SelectedCategory!.Id);
        Assert.Equal(10, vm.SelectedType!.Id);
        Assert.Equal("4 میں سے 1 اشیاء", vm.Subtitle);
    }

    [Fact]
    public async Task Search_Over10000Items_IsFarFasterThanHalfASecond()
    {
        var items = Enumerable.Range(1, 10_000)
            .Select(i => CatalogTestData.Lays with
            {
                Id = i,
                Name = $"Item {i} Premium",
                UrduName = $"شے {i}",
                Barcode = $"89{i:D8}",
            })
            .ToList();
        var vm = await CreateAsync(new CatalogListing(items, CatalogTestData.Listing.Categories));

        var watch = Stopwatch.StartNew();
        vm.SearchText = "premium 9999";
        watch.Stop();

        Assert.Single(vm.Rows);
        Assert.True(watch.ElapsedMilliseconds < 500, $"Search took {watch.ElapsedMilliseconds} ms");
    }

    [Fact]
    public async Task DisposingThePage_StopsItReactingToLanguageChanges()
    {
        var vm = await CreateAsync();
        var row = vm.Rows[0];
        vm.Dispose();

        _loc.SetLanguage("ur");

        Assert.Equal("Lays Classic", row.PrimaryName);
    }
}
