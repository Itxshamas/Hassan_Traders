using HassanTraders.Desktop.Services.Application.Catalog;
using HassanTraders.Desktop.Core.Catalog;
using ClosedXML.Excel;
using HassanTraders.Desktop.Data.Import;
using HassanTraders.Desktop.Data.Persistence;
using HassanTraders.Desktop.Data.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HassanTraders.Desktop.Data.Tests;

public sealed class CatalogPersistenceTests : IDisposable
{
    private readonly SqliteTestDb _db = new();

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task ItemCsv_ImportsCategoriesTypesAndAllSampleItems()
    {
        await using var context = _db.CreateContext();
        await using var stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "TestData", "items.csv"));
        var service = new ItemCatalogImportService(new CatalogFileReader(), new ItemRepository(context), context);

        var report = await service.ImportAsync(stream, "items.csv");

        Assert.Empty(report.Errors);
        Assert.Equal(41, report.ImportedCount);
        Assert.Equal(8, await context.Categories.CountAsync());
        Assert.Equal(4, await context.ProductTypes.CountAsync());
        Assert.Equal(38, await context.Items.CountAsync());
        Assert.Equal(4, await context.Variants.CountAsync());
        Assert.Equal(
            25_000,
            await context.Items.Where(item => item.Name == "Inam Supreme Rice").Select(item => item.PackSize).SingleAsync());
        Assert.Equal(UnitKind.Bag, await context.Items.Where(item => item.Name == "Inam Supreme Rice").Select(item => item.Unit).SingleAsync());
    }

    [Fact]
    public async Task ImportReader_ReportsInvalidNumericCellsAsRowErrors()
    {
        const string csv = """
            Category,CategoryUrdu,ProductType,ProductTypeUrdu,Name,NameUrdu,Brand,Barcode,Unit,PackSizeBaseUnits,CostPaisa,SalePaisa,ReorderBaseUnits,VariantName,VariantPackSizeBaseUnits,VariantCostPaisa,VariantSalePaisa
            Snacks,نمکو,,,Bad item,خراب چیز,,,Piece,1,not-a-price,100,1,,,,
            """;
        await using var context = _db.CreateContext();
        await using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csv));
        var service = new ItemCatalogImportService(new CatalogFileReader(), new ItemRepository(context), context);

        var report = await service.ImportAsync(stream, "items.csv");

        Assert.Equal(0, report.ImportedCount);
        Assert.Equal(2, Assert.Single(report.Errors).RowNumber);
        Assert.Empty(await context.Items.ToListAsync());
    }

    [Fact]
    public async Task ExcelReader_ImportsXlsxCatalogueRows()
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Items");
        var headers = "Category,CategoryUrdu,ProductType,ProductTypeUrdu,Name,NameUrdu,Brand,Barcode,Unit,PackSizeBaseUnits,CostPaisa,SalePaisa,ReorderBaseUnits,VariantName,VariantPackSizeBaseUnits,VariantCostPaisa,VariantSalePaisa"
            .Split(',');
        var values = new[] { "Pantry", "راشن", "", "", "Sample item", "نمونہ چیز", "", "", "Piece", "1", "100", "120", "5", "", "", "", "" };
        for (var index = 0; index < headers.Length; index++)
        {
            sheet.Cell(1, index + 1).Value = headers[index];
            sheet.Cell(2, index + 1).Value = values[index];
        }

        await using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        await using var context = _db.CreateContext();
        var service = new ItemCatalogImportService(new CatalogFileReader(), new ItemRepository(context), context);

        var report = await service.ImportAsync(stream, "items.xlsx");

        Assert.Empty(report.Errors);
        Assert.Equal(1, report.ImportedCount);
        Assert.Equal("Sample item", await context.Items.Select(item => item.Name).SingleAsync());
    }
}
