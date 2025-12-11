using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DataProcessor.Core.Processors;

public class AmazonSalesOrderLookupBuilder
{
    private static readonly string[] RequiredColumns =
    {
        "amazon-order-id",
        "product-name",
        "order-status",
        "item-status"
    };

    public IDictionary<string, string> Build(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, true, leaveOpen: true);
        var headerLine = reader.ReadLine() ?? throw new InvalidOperationException("Arquivo vazio ou sem cabeçalho.");
        var headers = headerLine.Split('\t');
        ValidateHeaders(headers);

        var orderIdIndex = Array.IndexOf(headers, "amazon-order-id");
        var productNameIndex = Array.IndexOf(headers, "product-name");
        var orderStatusIndex = Array.IndexOf(headers, "order-status");
        var itemStatusIndex = Array.IndexOf(headers, "item-status");

        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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

            var orderId = columns[orderIdIndex];
            var productName = columns[productNameIndex];
            if (string.IsNullOrWhiteSpace(orderId))
            {
                continue;
            }

            map[orderId] = productName;
        }

        return map;
    }

    private static void ValidateHeaders(IReadOnlyCollection<string> headers)
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
