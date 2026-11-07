using HassanTraders.Desktop.Core.Catalog;
using HassanTraders.Desktop.Core.Common;
using HassanTraders.Desktop.Core.DTOs.Catalog;
using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Services.Application.Catalog;
using NSubstitute;

namespace HassanTraders.Desktop.Application.Tests;

public class CatalogQueryServiceTests
{
    private static CatalogItemRow Item(long id, long categoryId, long? typeId, bool active = true, int variants = 0) => new(
        id, $"Item {id}", $"شے {id}", string.Empty, null, categoryId, "Cat", "قسم", typeId, null, null,
        UnitKind.Piece, 1, Money.Zero, Money.Zero, Quantity.Zero, active, variants);

    [Fact]
    public async Task Summary_CountsOnlyActiveItemsPerCategoryAndType()
    {
        var reader = Substitute.For<ICatalogReader>();
        reader.ReadAsync(Arg.Any<CancellationToken>()).Returns(new CatalogListing(
            [Item(1, 1, 10), Item(2, 1, 10), Item(3, 1, 11), Item(4, 1, 11, active: false, variants: 3), Item(5, 2, null, variants: 2)],
            [
                new CatalogCategoryRow(1, "Rice", "چاول", [new CatalogTypeRow(10, "Premium", "پریمیم"), new CatalogTypeRow(11, "Economical", "اقتصادی")]),
                new CatalogCategoryRow(2, "Snacks", "نمکو", []),
            ]));
        var sut = new CatalogQueryService(reader);

        var summary = await sut.GetSummaryAsync();

        Assert.Equal(4, summary.ActiveItems);
        Assert.Equal(1, summary.InactiveItems);
        Assert.Equal(2, summary.CategoryCount);
        Assert.Equal(2, summary.TypeCount);
        Assert.Equal(5, summary.VariantCount);
        var rice = summary.Categories.Single(c => c.Name == "Rice");
        Assert.Equal(3, rice.ItemCount);
        Assert.Equal([2, 1], rice.Types.Select(t => t.ItemCount));
    }

    [Fact]
    public async Task Summary_OfAnEmptyCatalogue_IsAllZeros()
    {
        var reader = Substitute.For<ICatalogReader>();
        reader.ReadAsync(Arg.Any<CancellationToken>()).Returns(new CatalogListing([], []));

        var summary = await new CatalogQueryService(reader).GetSummaryAsync();

        Assert.Equal(new CatalogSummary(0, 0, 0, 0, 0, []).ActiveItems, summary.ActiveItems);
        Assert.Empty(summary.Categories);
    }

    [Fact]
    public async Task Listing_ComesStraightFromTheReader()
    {
        var listing = new CatalogListing([Item(1, 1, null)], []);
        var reader = Substitute.For<ICatalogReader>();
        reader.ReadAsync(Arg.Any<CancellationToken>()).Returns(listing);

        Assert.Same(listing, await new CatalogQueryService(reader).GetListingAsync());
    }
}
