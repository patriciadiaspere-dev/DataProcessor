using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace DataProcessor.Api.Requests;

public class SettlementUploadRequest
{
    public List<IFormFile> Files { get; set; } = new();
    public string AccountType { get; set; } = string.Empty;
}
