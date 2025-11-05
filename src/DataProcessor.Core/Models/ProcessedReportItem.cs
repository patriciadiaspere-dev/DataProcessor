namespace DataProcessor.Core.Models;

public record ProcessedReportItem(
    string Asin,
    string ProductName,
    decimal UnitPrice,
    int TotalQuantity,
    decimal TotalValue
);
