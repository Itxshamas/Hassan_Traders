using System.IO;

namespace HassanTraders.Desktop.Services.Application.Catalog;

public sealed record ItemImportRow(
    int RowNumber,
    string Category,
    string CategoryUrdu,
    string ProductType,
    string ProductTypeUrdu,
    string Name,
    string NameUrdu,
    string Brand,
    string Barcode,
    string Unit,
    string PackSizeBaseUnits,
    string CostPaisa,
    string SalePaisa,
    string ReorderBaseUnits,
    string VariantName,
    string VariantPackSizeBaseUnits,
    string VariantCostPaisa,
    string VariantSalePaisa);

public sealed record ImportRowError(int RowNumber, string Message);

public sealed record ItemImportReport(int ImportedCount, IReadOnlyList<ImportRowError> Errors);

public interface ICatalogFileReader
{
    Task<IReadOnlyList<ItemImportRow>> ReadAsync(Stream file, string fileName, CancellationToken cancellationToken = default);
}

public interface IItemCatalogImportService
{
    Task<ItemImportReport> ImportAsync(Stream file, string fileName, CancellationToken cancellationToken = default);
}
