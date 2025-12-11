using System;
using System.Collections.Generic;

namespace DataProcessor.Data.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Cnpj { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime TrialExpiresAt { get; set; }

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<SettlementUpload> SettlementUploads { get; set; } = new List<SettlementUpload>();
}
