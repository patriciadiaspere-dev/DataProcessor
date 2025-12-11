using System;
using System.Collections.Generic;

namespace DataProcessor.Data.Entities;

public class SettlementOrder
{
    public Guid Id { get; set; }
    public Guid SettlementUploadId { get; set; }
    public string AmazonOrderId { get; set; } = string.Empty;
    public string MerchantOrderId { get; set; } = string.Empty;
    public string ShipmentId { get; set; } = string.Empty;
    public string MarketplaceName { get; set; } = string.Empty;
    public string MerchantFulfillmentId { get; set; } = string.Empty;
    public DateTime? PostedDate { get; set; }
    public int? PostedYear { get; set; }
    public int? PostedMonth { get; set; }

    public SettlementUpload? SettlementUpload { get; set; }
    public ICollection<SettlementOrderItem> Items { get; set; } = new List<SettlementOrderItem>();
}
