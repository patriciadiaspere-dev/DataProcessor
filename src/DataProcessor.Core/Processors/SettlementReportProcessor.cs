using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using DataProcessor.Core.Models.Settlement;

namespace DataProcessor.Core.Processors;

public class SettlementReportProcessor : ISettlementReportProcessor
{
    public SettlementReportData Process(Stream stream)
    {
        var document = XDocument.Load(stream);
        var settlementReport = document.Descendants("SettlementReport").FirstOrDefault()
            ?? throw new InvalidOperationException("Elemento SettlementReport não encontrado no XML.");

        var settlementData = settlementReport.Element("SettlementData")
            ?? throw new InvalidOperationException("Elemento SettlementData não encontrado no XML.");

        var settlementId = settlementData.Element("AmazonSettlementID")?.Value.Trim('\'', '"')
            ?? throw new InvalidOperationException("AmazonSettlementID não encontrado.");

        var totalAmount = ParseDecimal(settlementData.Element("TotalAmount"));
        var startDate = ParseDate(settlementData.Element("StartDate"));
        var endDate = ParseDate(settlementData.Element("EndDate"));
        var depositDate = ParseDate(settlementData.Element("DepositDate"));

        var orders = settlementReport.Elements("Order")
            .Select(ParseOrder)
            .ToList();

        return new SettlementReportData(
            settlementId,
            totalAmount,
            startDate,
            endDate,
            depositDate,
            orders);
    }

    private static SettlementOrderData ParseOrder(XElement orderElement)
    {
        var amazonOrderId = orderElement.Element("AmazonOrderID")?.Value.Trim('\'', '"')
            ?? throw new InvalidOperationException("AmazonOrderID ausente em Order.");
        var merchantOrderId = orderElement.Element("MerchantOrderID")?.Value.Trim('\'', '"') ?? string.Empty;
        var shipmentId = orderElement.Element("ShipmentID")?.Value.Trim('\'', '"') ?? string.Empty;
        var marketplaceName = orderElement.Element("MarketplaceName")?.Value.Trim('\'', '"') ?? string.Empty;

        var fulfillment = orderElement.Element("Fulfillment")
            ?? throw new InvalidOperationException("Elemento Fulfillment ausente no Order.");

        var merchantFulfillmentId = fulfillment.Element("MerchantFulfillmentID")?.Value.Trim('\'', '"') ?? string.Empty;
        var postedDate = ParseDate(fulfillment.Element("PostedDate"));

        var items = fulfillment.Elements("Item")
            .Select(ParseItem)
            .ToList();

        return new SettlementOrderData(
            amazonOrderId,
            merchantOrderId,
            shipmentId,
            marketplaceName,
            merchantFulfillmentId,
            postedDate,
            items);
    }

    private static SettlementItemData ParseItem(XElement itemElement)
    {
        var amazonOrderItemCode = itemElement.Element("AmazonOrderItemCode")?.Value.Trim('\'', '"') ?? string.Empty;
        var sku = itemElement.Element("SKU")?.Value.Trim('\'', '"') ?? string.Empty;
        var quantity = ParseInt(itemElement.Element("Quantity"));

        var priceComponents = itemElement.Element("ItemPrice")?
            .Elements("Component")
            .Select(ParseComponent)
            .ToList() ?? new List<SettlementAmountComponent>();

        var feeComponents = itemElement.Element("ItemFees")?
            .Elements("Fee")
            .Select(ParseComponent)
            .ToList() ?? new List<SettlementAmountComponent>();

        var principalAmount = priceComponents.FirstOrDefault(c => string.Equals(c.Type, "Principal", StringComparison.OrdinalIgnoreCase))?.Amount ?? 0m;
        var feeTotal = feeComponents.Sum(f => f.Amount);

        return new SettlementItemData(
            amazonOrderItemCode,
            sku,
            quantity,
            principalAmount,
            feeTotal,
            priceComponents,
            feeComponents);
    }

    private static SettlementAmountComponent ParseComponent(XElement componentElement)
    {
        var type = componentElement.Element("Type")?.Value.Trim('\'', '"') ?? string.Empty;
        var amountElement = componentElement.Element("Amount")
            ?? throw new InvalidOperationException("Elemento Amount ausente.");

        var currency = amountElement.Attribute("currency")?.Value ?? string.Empty;
        var amount = ParseDecimal(amountElement);

        return new SettlementAmountComponent(type, amount, currency);
    }

    private static decimal ParseDecimal(XElement? element)
    {
        if (element is null)
        {
            return 0m;
        }

        var value = element.Value.Trim('\'', '"');
        return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0m;
    }

    private static DateTime? ParseDate(XElement? element)
    {
        if (element is null)
        {
            return null;
        }

        var value = element.Value.Trim('\'', '"');
        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var parsed)
            ? parsed
            : null;
    }

    private static int ParseInt(XElement? element)
    {
        var value = element?.Value.Trim('\'', '"');
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0;
    }
}
