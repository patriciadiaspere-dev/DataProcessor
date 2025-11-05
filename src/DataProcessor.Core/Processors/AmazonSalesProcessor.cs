using System.Globalization;
using System.Linq;
using System.Text;
using DataProcessor.Core.Models;

namespace DataProcessor.Core.Processors;

public class AmazonSalesProcessor : IReportProcessor
{
    private static readonly string[] RequiredColumns =
    {
        "order-status",
        "item-status",
        "quantity",
        "asin",
        "item-price",
        "product-name"
    };

    public ProcessedReportSummary Process(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, true, leaveOpen: true);
        var headerLine = reader.ReadLine() ?? throw new InvalidOperationException("Arquivo vazio ou sem cabeçalho.");
        var headers = headerLine.Split('\t');
        ValidateHeaders(headers);

        var asinIndex = Array.IndexOf(headers, "asin");
        var productNameIndex = Array.IndexOf(headers, "product-name");
        var quantityIndex = Array.IndexOf(headers, "quantity");
        var orderStatusIndex = Array.IndexOf(headers, "order-status");
        var itemStatusIndex = Array.IndexOf(headers, "item-status");
        var itemPriceIndex = Array.IndexOf(headers, "item-price");

        var grouped = new Dictionary<(string Asin, decimal Price), (string ProductName, int Quantity)>();

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var columns = line.Split('\t');
            if (columns.Length < headers.Length)
            {
                continue;
            }

            var orderStatus = columns[orderStatusIndex];
            var itemStatus = columns[itemStatusIndex];
            if (!string.Equals(orderStatus, "Shipped", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(itemStatus, "Shipped", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!int.TryParse(columns[quantityIndex], NumberStyles.Integer, CultureInfo.InvariantCulture, out var quantity) ||
                quantity <= 0)
            {
                continue;
            }

            if (!decimal.TryParse(columns[itemPriceIndex], NumberStyles.Number, CultureInfo.InvariantCulture, out var price))
            {
                continue;
            }

            var asin = columns[asinIndex];
            var productName = columns[productNameIndex];
            var key = (asin, price);
            if (grouped.TryGetValue(key, out var entry))
            {
                grouped[key] = (entry.ProductName, entry.Quantity + quantity);
            }
            else
            {
                grouped[key] = (productName, quantity);
            }
        }

        var items = grouped
            .Select(pair => new ProcessedReportItem(
                pair.Key.Asin,
                pair.Value.ProductName,
                pair.Key.Price,
                pair.Value.Quantity,
                pair.Key.Price * pair.Value.Quantity))
            .OrderBy(item => item.ProductName)
            .ToList();

        var totalUnits = items.Sum(item => item.TotalQuantity);
        var totalRevenue = items.Sum(item => item.TotalValue);

        return new ProcessedReportSummary(
            TotalUniqueProducts: items.Count,
            TotalUnitsSold: totalUnits,
            TotalRevenue: totalRevenue,
            Items: items
        );
    }

    private static void ValidateHeaders(string[] headers)
    {
        foreach (var required in RequiredColumns)
        {
            if (!headers.Contains(required, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Coluna obrigatória ausente: {required}");
            }
        }
    }
}
