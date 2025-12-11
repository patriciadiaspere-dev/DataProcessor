using System;
using System.Collections.Generic;

namespace DataProcessor.Data.Entities;

public class SettlementUpload
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserCnpj { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string SettlementId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? DepositDate { get; set; }
    public int? PeriodYear { get; set; }
    public int? PeriodMonth { get; set; }

    public User? User { get; set; }
    public ICollection<SettlementOrder> Orders { get; set; } = new List<SettlementOrder>();
}
