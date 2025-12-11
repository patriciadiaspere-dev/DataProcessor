using Microsoft.AspNetCore.Http;

namespace DataProcessor.Api.Requests;

public class SettlementReconciliationRequest
{
    public IFormFile? SalesFile { get; set; }
    public string AccountType { get; set; } = string.Empty;
}
