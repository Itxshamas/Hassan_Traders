using HassanTraders.Desktop.Core.Catalog;
using HassanTraders.Desktop.Data.Import;
using HassanTraders.Desktop.Data.Persistence.Repositories;
using HassanTraders.Desktop.Services.Application.Catalog;

namespace HassanTraders.Desktop.Data.Tests;

public sealed class CatalogReaderTests : IDisposable
{
    private readonly SqliteTestDb _db = new();

    public void Dispose() => _db.Dispose();

    private async Task ImportSampleAsync()
    {
        await using var context = _db.CreateContext();
        await using var stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "TestData", "items.csv"));
        var report = await new ItemCatalogImportService(new CatalogFileReader(), new ItemRepository(context), context)
            .ImportAsync(stream, "items.csv");
        Assert.Empty(report.Errors);
    }

    [Fact]
    public async Task Read_ReturnsEverySampleItemWithBothLanguagesCategoryAndType()
    {
        await ImportSampleAsync();
        await using var context = _db.CreateContext();

        var listing = await new CatalogReader(context).ReadAsync();

        Assert.Equal(38, listing.Items.Count);
        Assert.Equal(8, listing.Categories.Count);
        var supreme = listing.Items.Single(i => i.Name == "Inam Supreme Rice");
        Assert.Equal("انعام سپریم چاول", supreme.UrduName);
        Assert.Equal("Rice", supreme.CategoryName);
        Assert.Equal("چاول", supreme.CategoryUrduName);
        Assert.Equal("Premium", supreme.ProductTypeName);
        Assert.Equal(UnitKind.Bag, supreme.Unit);
        Assert.Equal(25_000, supreme.PackSize);
        Assert.Equal(620_000, supreme.SalePrice.Paisa);
        Assert.Equal(580_000, supreme.CostPrice.Paisa);
        Assert.Equal(50_000, supreme.ReorderLevel.BaseUnits);
        Assert.True(supreme.IsActive);
    }

    [Fact]
    public async Task Read_CountsVariantsPerItem_AndKeepsItemsWithoutTypeOrVariants()
    {
        await ImportSampleAsync();
        await using var context = _db.CreateContext();

        var listing = await new CatalogReader(context).ReadAsync();

        Assert.Equal(4, listing.Items.Single(i => i.Name == "Lays Classic").VariantCount);
        var cheetos = listing.Items.Single(i => i.Name.StartsWith("Cheetos", StringComparison.Ordinal));
        Assert.Equal(0, cheetos.VariantCount);
        Assert.Null(cheetos.ProductTypeId);
        Assert.Null(cheetos.ProductTypeName);
    }

    [Fact]
    public async Task Read_GivesRiceItsFourTypesInDisplayOrder()
    {
        await ImportSampleAsync();
        await using var context = _db.CreateContext();

        var listing = await new CatalogReader(context).ReadAsync();

        var rice = listing.Categories.Single(c => c.Name == "Rice");
        Assert.Equal(["Premium", "Superior", "Middle order", "Economical"], rice.Types.Select(t => t.Name));
        Assert.Empty(listing.Categories.Single(c => c.Name == "Snacks").Types);
    }

    [Fact]
    public async Task Summary_OverTheSampleCatalogue_MatchesTheDatabase()
    {
        await ImportSampleAsync();
        await using var context = _db.CreateContext();

        var summary = await new CatalogQueryService(new CatalogReader(context)).GetSummaryAsync();

        Assert.Equal(38, summary.ActiveItems);
        Assert.Equal(0, summary.InactiveItems);
        Assert.Equal(8, summary.CategoryCount);
        Assert.Equal(4, summary.TypeCount);
        Assert.Equal(4, summary.VariantCount);
        Assert.Equal(11, summary.Categories.Single(c => c.Name == "Rice").ItemCount);
    }

    [Fact]
    public async Task Read_OnAnEmptyDatabase_ReturnsEmptyLists()
    {
        await using var context = _db.CreateContext();

        var listing = await new CatalogReader(context).ReadAsync();

        Assert.Empty(listing.Items);
        Assert.Empty(listing.Categories);
    }
}
