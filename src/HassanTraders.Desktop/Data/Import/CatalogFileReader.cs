using System.Globalization;
using System.IO;
using HassanTraders.Desktop.Services.Application.Catalog;
using ClosedXML.Excel;

namespace HassanTraders.Desktop.Data.Import;

public sealed class CatalogFileReader : ICatalogFileReader
{
    private static readonly string[] RequiredHeaders =
    [
        "Category",
        "CategoryUrdu",
        "ProductType",
        "ProductTypeUrdu",
        "Name",
        "NameUrdu",
        "Brand",
        "Barcode",
        "Unit",
        "PackSizeBaseUnits",
        "CostPaisa",
        "SalePaisa",
        "ReorderBaseUnits",
        "VariantName",
        "VariantPackSizeBaseUnits",
        "VariantCostPaisa",
        "VariantSalePaisa",
    ];

    public async Task<IReadOnlyList<ItemImportRow>> ReadAsync(
        Stream file,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        var extension = Path.GetExtension(fileName);
        if (extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
        {
            using var reader = new StreamReader(file, leaveOpen: true);
            return await ReadCsvAsync(reader, cancellationToken);
        }

        if (extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return ReadExcel(file, cancellationToken);
        }

        throw new InvalidDataException("Only CSV and XLSX item files are supported.");
    }

    private static async Task<IReadOnlyList<ItemImportRow>> ReadCsvAsync(
        TextReader reader,
        CancellationToken cancellationToken)
    {
        var headerLine = await reader.ReadLineAsync(cancellationToken);
        if (headerLine is null)
        {
            throw new InvalidDataException("The import file is empty.");
        }

        var headers = ParseCsvLine(headerLine).Select(x => x.Trim()).ToArray();
        var indexes = GetIndexes(headers);
        var rows = new List<ItemImportRow>();
        var rowNumber = 1;
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            cancellationToken.ThrowIfCancellationRequested();
            rowNumber++;
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var cells = ParseCsvLine(line);
            rows.Add(CreateRow(rowNumber, key => key < cells.Count ? cells[key].Trim() : string.Empty, indexes));
        }

        return rows;
    }

    private static List<ItemImportRow> ReadExcel(Stream file, CancellationToken cancellationToken)
    {
        using var workbook = new XLWorkbook(file);
        var sheet = workbook.Worksheets.FirstOrDefault() ??
            throw new InvalidDataException("The workbook has no worksheets.");
        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 0;
        if (lastRow == 0)
        {
            throw new InvalidDataException("The import worksheet is empty.");
        }

        var lastColumn = sheet.Row(1).LastCellUsed()?.Address.ColumnNumber ?? 0;
        var headers = Enumerable.Range(1, lastColumn)
            .Select(column => sheet.Cell(1, column).GetString().Trim())
            .ToArray();
        var indexes = GetIndexes(headers);
        var rows = new List<ItemImportRow>();
        for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var currentRow = rowNumber;
            if (sheet.Row(rowNumber).CellsUsed().All(cell => cell.IsEmpty()))
            {
                continue;
            }

            rows.Add(CreateRow(
                rowNumber,
                index => index < 0 ? string.Empty : sheet.Cell(currentRow, index + 1).GetFormattedString(CultureInfo.InvariantCulture),
                indexes));
        }

        return rows;
    }

    private static Dictionary<string, int> GetIndexes(IReadOnlyList<string> headers)
    {
        var indexes = headers
            .Select((header, index) => (header, index))
            .ToDictionary(x => x.header, x => x.index, StringComparer.OrdinalIgnoreCase);
        var missing = RequiredHeaders.Where(header => !indexes.ContainsKey(header)).ToArray();
        if (missing.Length > 0)
        {
            throw new InvalidDataException($"Missing import columns: {string.Join(", ", missing)}.");
        }

        return indexes;
    }

    private static ItemImportRow CreateRow(
        int rowNumber,
        Func<int, string> cell,
        Dictionary<string, int> indexes) =>
        new(
            rowNumber,
            cell(indexes["Category"]),
            cell(indexes["CategoryUrdu"]),
            cell(indexes["ProductType"]),
            cell(indexes["ProductTypeUrdu"]),
            cell(indexes["Name"]),
            cell(indexes["NameUrdu"]),
            cell(indexes["Brand"]),
            cell(indexes["Barcode"]),
            cell(indexes["Unit"]),
            cell(indexes["PackSizeBaseUnits"]),
            cell(indexes["CostPaisa"]),
            cell(indexes["SalePaisa"]),
            cell(indexes["ReorderBaseUnits"]),
            cell(indexes["VariantName"]),
            cell(indexes["VariantPackSizeBaseUnits"]),
            cell(indexes["VariantCostPaisa"]),
            cell(indexes["VariantSalePaisa"]));

    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var value = new System.Text.StringBuilder();
        var quoted = false;
        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (character == '"')
            {
                if (quoted && index + 1 < line.Length && line[index + 1] == '"')
                {
                    value.Append('"');
                    index++;
                }
                else
                {
                    quoted = !quoted;
                }
            }
            else if (character == ',' && !quoted)
            {
                fields.Add(value.ToString());
                value.Clear();
            }
            else
            {
                value.Append(character);
            }
        }

        if (quoted)
        {
            throw new InvalidDataException("CSV row contains an unclosed quoted field.");
        }

        fields.Add(value.ToString());
        return fields;
    }
}
