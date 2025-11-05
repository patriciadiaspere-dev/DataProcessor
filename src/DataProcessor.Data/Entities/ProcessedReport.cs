namespace DataProcessor.Data.Entities;

public class ProcessedReport
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
    public string Payload { get; set; } = string.Empty;

    public User? User { get; set; }
}
