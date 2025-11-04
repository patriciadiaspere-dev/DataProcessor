using DataProcessor.Core.Models;

namespace DataProcessor.Data.DTOs.Report;

public class ReportResultResponse
{
    public int TotalUniqueProducts { get; set; }
    public int TotalUnitsSold { get; set; }
    public decimal TotalRevenue { get; set; }
    public IReadOnlyCollection<ProcessedReportItem> Items { get; set; } = Array.Empty<ProcessedReportItem>();
}
