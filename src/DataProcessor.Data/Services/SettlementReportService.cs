using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DataProcessor.Core.Models.Settlement;
using DataProcessor.Core.Processors;
using DataProcessor.Data.DTOs.Settlement;
using DataProcessor.Data.Entities;
using DataProcessor.Data;
using Microsoft.EntityFrameworkCore;

namespace DataProcessor.Data.Services;

public class SettlementReportService : ISettlementReportService
{
    private readonly DataContext _context;
    private readonly AmazonSalesOrderLookupBuilder _salesLookupBuilder;

    public SettlementReportService(DataContext context, AmazonSalesOrderLookupBuilder salesLookupBuilder)
    {
        _context = context;
        _salesLookupBuilder = salesLookupBuilder;
    }

    public async Task<SettlementUpload> SaveAsync(User user, string accountType, string fileName, SettlementReportData data, CancellationToken cancellationToken = default)
    {
        var upload = new SettlementUpload
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            UserCnpj = user.Cnpj,
            AccountType = accountType,
            FileName = fileName,
            SettlementId = data.SettlementId,
            TotalAmount = data.TotalAmount,
            UploadedAt = DateTime.UtcNow,
            StartDate = data.StartDate,
            EndDate = data.EndDate,
            DepositDate = data.DepositDate,
            PeriodYear = (data.DepositDate ?? data.StartDate ?? data.EndDate)?.Year,
            PeriodMonth = (data.DepositDate ?? data.StartDate ?? data.EndDate)?.Month
        };

        foreach (var order in data.Orders)
        {
            var orderEntity = new SettlementOrder
            {
                Id = Guid.NewGuid(),
                SettlementUploadId = upload.Id,
                AmazonOrderId = order.AmazonOrderId,
                MerchantOrderId = order.MerchantOrderId,
                ShipmentId = order.ShipmentId,
                MarketplaceName = order.MarketplaceName,
                MerchantFulfillmentId = order.MerchantFulfillmentId,
                PostedDate = order.PostedDate,
                PostedYear = order.PostedDate?.Year,
                PostedMonth = order.PostedDate?.Month
            };

            foreach (var item in order.Items)
            {
                var itemEntity = new SettlementOrderItem
                {
                    Id = Guid.NewGuid(),
                    SettlementOrderId = orderEntity.Id,
                    AmazonOrderItemCode = item.AmazonOrderItemCode,
                    Sku = item.Sku,
                    Quantity = item.Quantity,
                    PrincipalAmount = item.PrincipalAmount,
                    FeeTotal = item.FeeTotal,
                    PriceComponents = JsonSerializer.Serialize(item.PriceComponents),
                    FeeComponents = JsonSerializer.Serialize(item.FeeComponents)
                };

                orderEntity.Items.Add(itemEntity);
            }

            upload.Orders.Add(orderEntity);
        }

        _context.SettlementUploads.Add(upload);
        await _context.SaveChangesAsync(cancellationToken);
        return upload;
    }

    public async Task<SettlementReconciliationResponse> BuildReconciliationAsync(User user, string accountType, string salesFileName, Stream salesStream, CancellationToken cancellationToken = default)
    {
        var ordersLookup = _salesLookupBuilder.Build(salesStream);
        var uploads = await _context.SettlementUploads
            .Include(u => u.Orders)
            .ThenInclude(o => o.Items)
            .Where(u => u.UserId == user.Id && u.AccountType == accountType)
            .ToListAsync(cancellationToken);

        var response = new SettlementReconciliationResponse
        {
            AccountType = accountType,
            SalesFileName = salesFileName
        };

        var settlementOrders = uploads
            .SelectMany(u => u.Orders.Select(o => new { Upload = u, Order = o }))
            .ToList();

        var paidOrderIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in settlementOrders)
        {
            var productName = ordersLookup.TryGetValue(entry.Order.AmazonOrderId, out var name) ? name : null;
            paidOrderIds.Add(entry.Order.AmazonOrderId);

            response.Entries.Add(new SettlementReconciliationEntryResponse
            {
                AmazonOrderId = entry.Order.AmazonOrderId,
                ProductName = productName,
                Paid = true,
                SettlementId = entry.Upload.SettlementId,
                AccountType = entry.Upload.AccountType,
                DepositDate = entry.Upload.DepositDate,
                Order = new SettlementOrderResponse
                {
                    MerchantOrderId = entry.Order.MerchantOrderId,
                    ShipmentId = entry.Order.ShipmentId,
                    MarketplaceName = entry.Order.MarketplaceName,
                    MerchantFulfillmentId = entry.Order.MerchantFulfillmentId,
                    PostedDate = entry.Order.PostedDate,
                    PostedYear = entry.Order.PostedYear,
                    PostedMonth = entry.Order.PostedMonth
                },
                Items = entry.Order.Items.Select(MapItem).ToList()
            });
        }

        foreach (var saleOrder in ordersLookup)
        {
            if (paidOrderIds.Contains(saleOrder.Key))
            {
                continue;
            }

            response.Entries.Add(new SettlementReconciliationEntryResponse
            {
                AmazonOrderId = saleOrder.Key,
                ProductName = saleOrder.Value,
                Paid = false,
                AccountType = accountType,
                Items = new List<SettlementOrderItemResponse>()
            });
        }

        return response;
    }

    private static SettlementOrderItemResponse MapItem(SettlementOrderItem item)
    {
        var priceComponents = DeserializeComponents(item.PriceComponents);
        var feeComponents = DeserializeComponents(item.FeeComponents);

        return new SettlementOrderItemResponse
        {
            AmazonOrderItemCode = item.AmazonOrderItemCode,
            Sku = item.Sku,
            Quantity = item.Quantity,
            PrincipalAmount = item.PrincipalAmount,
            FeeTotal = item.FeeTotal,
            PriceComponents = priceComponents,
            FeeComponents = feeComponents
        };
    }

    private static List<SettlementAmountComponentResponse> DeserializeComponents(string value)
    {
        var components = string.IsNullOrWhiteSpace(value)
            ? Array.Empty<SettlementAmountComponent>()
            : JsonSerializer.Deserialize<List<SettlementAmountComponent>>(value) ?? new List<SettlementAmountComponent>();

        return components
            .Select(c => new SettlementAmountComponentResponse
            {
                Type = c.Type,
                Amount = c.Amount,
                Currency = c.Currency
            })
            .ToList();
    }
}
