using System;

namespace DataProcessor.Data.DTOs.Settlement;

public class SettlementUploadResponse
{
    public Guid Id { get; set; }
    public string AccountType { get; set; } = string.Empty;
    public string SettlementId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime? DepositDate { get; set; }
    public int? Year { get; set; }
    public int? Month { get; set; }
    public int Orders { get; set; }
}
