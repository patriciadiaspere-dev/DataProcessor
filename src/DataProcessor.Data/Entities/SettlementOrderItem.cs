using System;

namespace DataProcessor.Data.Entities;

public class SettlementOrderItem
{
    public Guid Id { get; set; }
    public Guid SettlementOrderId { get; set; }
    public string AmazonOrderItemCode { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal FeeTotal { get; set; }
    public string PriceComponents { get; set; } = string.Empty;
    public string FeeComponents { get; set; } = string.Empty;

    public SettlementOrder? Order { get; set; }
}
