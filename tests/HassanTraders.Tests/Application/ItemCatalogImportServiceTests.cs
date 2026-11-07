using HassanTraders.Desktop.Services.Application.Abstractions;
using HassanTraders.Desktop.Services.Application.Catalog;
using HassanTraders.Desktop.Core.Catalog;
using NSubstitute;

namespace HassanTraders.Desktop.Services.Application.Tests;

public sealed class ItemCatalogImportServiceTests
{
    [Fact]
    public async Task Import_ReportsInvalidRowsAndSavesValidRowsOnce()
    {
        var reader = Substitute.For<ICatalogFileReader>();
        var items = Substitute.For<IItemRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        reader.ReadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(
            [
                new ItemImportRow(2, "Rice", "چاول", "Premium", "پریمیم", "Good Rice", "اچھے چاول", "", "", "Bag", "25000", "100000", "50000", "50000", "", "", "", ""),
                new ItemImportRow(3, "Rice", "چاول", "", "", "Bad Item", "خراب چیز", "", "", "Invalid", "0", "0", "0", "0", "", "", "", ""),
            ]);
        items.FindCategoryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Category?)null);
        items.FindProductTypeAsync(Arg.Any<Category>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ProductType?)null);
        items.BarcodeExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var service = new ItemCatalogImportService(reader, items, unitOfWork);
        var report = await service.ImportAsync(new MemoryStream(), "items.csv");

        Assert.Equal(1, report.ImportedCount);
        Assert.Equal([new ImportRowError(3, "Unit, pack size, prices, or reorder quantity is invalid.")], report.Errors);
        items.Received(1).Add(Arg.Any<Item>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Import_ReportsDuplicateBarcodesWithinTheFile()
    {
        var reader = Substitute.For<ICatalogFileReader>();
        var items = Substitute.For<IItemRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var row = new ItemImportRow(2, "Pantry", "راشن", "", "", "Rice", "چاول", "", "123", "Piece", "1", "100", "120", "5", "", "", "", "");
        reader.ReadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([row, row with { RowNumber = 3, Name = "Another item" }]);
        items.FindCategoryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Category?)null);
        items.BarcodeExistsAsync("123", Arg.Any<CancellationToken>()).Returns(false);
        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var service = new ItemCatalogImportService(reader, items, unitOfWork);
        var report = await service.ImportAsync(new MemoryStream(), "items.csv");

        Assert.Equal(1, report.ImportedCount);
        Assert.Equal(3, Assert.Single(report.Errors).RowNumber);
        Assert.Equal("Duplicate barcode.", report.Errors[0].Message);
    }
}
