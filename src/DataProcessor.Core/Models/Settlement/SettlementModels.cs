using System;
using System.Collections.Generic;

namespace DataProcessor.Core.Models.Settlement;

public record SettlementAmountComponent(string Type, decimal Amount, string Currency);

public record SettlementItemData(
    string AmazonOrderItemCode,
    string Sku,
    int Quantity,
    decimal PrincipalAmount,
    decimal FeeTotal,
    IReadOnlyList<SettlementAmountComponent> PriceComponents,
    IReadOnlyList<SettlementAmountComponent> FeeComponents);

public record SettlementOrderData(
    string AmazonOrderId,
    string MerchantOrderId,
    string ShipmentId,
    string MarketplaceName,
    string MerchantFulfillmentId,
    DateTime? PostedDate,
    IReadOnlyList<SettlementItemData> Items);

public record SettlementReportData(
    string SettlementId,
    decimal TotalAmount,
    DateTime? StartDate,
    DateTime? EndDate,
    DateTime? DepositDate,
    IReadOnlyList<SettlementOrderData> Orders);
