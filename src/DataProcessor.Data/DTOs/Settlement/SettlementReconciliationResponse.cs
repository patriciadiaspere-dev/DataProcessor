using System.Collections.Generic;

namespace DataProcessor.Data.DTOs.Settlement;

public class SettlementReconciliationResponse
{
    public string AccountType { get; set; } = string.Empty;
    public string? SalesFileName { get; set; }
    public IList<SettlementReconciliationEntryResponse> Entries { get; set; } = new List<SettlementReconciliationEntryResponse>();
}

public class SettlementReconciliationEntryResponse
{
    public string AmazonOrderId { get; set; } = string.Empty;
    public string? ProductName { get; set; }
    public bool Paid { get; set; }
    public string? SettlementId { get; set; }
    public string AccountType { get; set; } = string.Empty;
    public DateTime? DepositDate { get; set; }
    public SettlementOrderResponse? Order { get; set; }
    public IList<SettlementOrderItemResponse> Items { get; set; } = new List<SettlementOrderItemResponse>();
}

public class SettlementOrderResponse
{
    public string MerchantOrderId { get; set; } = string.Empty;
    public string ShipmentId { get; set; } = string.Empty;
    public string MarketplaceName { get; set; } = string.Empty;
    public string MerchantFulfillmentId { get; set; } = string.Empty;
    public DateTime? PostedDate { get; set; }
    public int? PostedYear { get; set; }
    public int? PostedMonth { get; set; }
}

public class SettlementOrderItemResponse
{
    public string AmazonOrderItemCode { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal FeeTotal { get; set; }
    public IList<SettlementAmountComponentResponse> PriceComponents { get; set; } = new List<SettlementAmountComponentResponse>();
    public IList<SettlementAmountComponentResponse> FeeComponents { get; set; } = new List<SettlementAmountComponentResponse>();
}

public class SettlementAmountComponentResponse
{
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
};
