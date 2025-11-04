namespace DataProcessor.Core.Models;

public record ProcessedReportSummary(
    int TotalUniqueProducts,
    int TotalUnitsSold,
    decimal TotalRevenue,
    IReadOnlyCollection<ProcessedReportItem> Items
);
