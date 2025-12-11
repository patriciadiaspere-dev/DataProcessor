using System.Security.Claims;
using DataProcessor.Api.Exceptions;
using DataProcessor.Core.Processors;
using DataProcessor.Core.Writers;
using DataProcessor.Data.DTOs.Report;
using DataProcessor.Data.DTOs.Settlement;
using DataProcessor.Api.Requests;
using DataProcessor.Data.Entities;
using DataProcessor.Data.Repositories;
using DataProcessor.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataProcessor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly IReportProcessor _reportProcessor;
    private readonly IReportExcelWriter _excelWriter;
    private readonly ISettlementReportProcessor _settlementProcessor;
    private readonly ISettlementReportService _settlementService;
    private readonly IUserRepository _userRepository;
    private readonly ISubscriptionService _subscriptionService;

    public ReportController(
        IReportProcessor reportProcessor,
        IReportExcelWriter excelWriter,
        ISettlementReportProcessor settlementProcessor,
        ISettlementReportService settlementService,
        IUserRepository userRepository,
        ISubscriptionService subscriptionService)
    {
        _reportProcessor = reportProcessor;
        _excelWriter = excelWriter;
        _settlementProcessor = settlementProcessor;
        _settlementService = settlementService;
        _userRepository = userRepository;
        _subscriptionService = subscriptionService;
    }

    [HttpPost("amazon-sales")]
    public async Task<ActionResult<ReportResultResponse>> ProcessAmazonSales(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(cancellationToken);
        await EnsureActiveStatusAsync(user, cancellationToken);

        if (file is null || file.Length == 0)
        {
            throw new InvalidOperationException("Arquivo inválido.");
        }

        using var stream = file.OpenReadStream();
        var summary = _reportProcessor.Process(stream);

        var response = new ReportResultResponse
        {
            TotalUniqueProducts = summary.TotalUniqueProducts,
            TotalUnitsSold = summary.TotalUnitsSold,
            TotalRevenue = summary.TotalRevenue,
            Items = summary.Items
        };

        return Ok(response);
    }

    [HttpPost("amazon-sales/export")]
    public async Task<IActionResult> ExportAmazonSales(
        [FromForm] IFormFile file,
        CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(cancellationToken);
        await EnsureActiveStatusAsync(user, cancellationToken);

        if (file is null || file.Length == 0)
        {
            throw new InvalidOperationException("Arquivo inválido.");
        }

        using var stream = file.OpenReadStream();
        var summary = _reportProcessor.Process(stream);
        var excelBytes = _excelWriter.Write(summary);

        var fileName = $"amazon-sales-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpPost("settlement/upload")]
    public async Task<ActionResult<IEnumerable<SettlementUploadResponse>>> UploadSettlementReports(
        [FromForm] SettlementUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(cancellationToken);
        await EnsureActiveStatusAsync(user, cancellationToken);

        if (request.Files is null || request.Files.Count == 0)
        {
            throw new InvalidOperationException("Envie pelo menos um arquivo XML.");
        }

        if (request.Files.Count > 10)
        {
            throw new InvalidOperationException("Limite de 10 arquivos por envio.");
        }

        if (string.IsNullOrWhiteSpace(request.AccountType))
        {
            throw new InvalidOperationException("Tipo de conta é obrigatório.");
        }

        var responses = new List<SettlementUploadResponse>();

        foreach (var file in request.Files)
        {
            using var stream = file.OpenReadStream();
            var parsed = _settlementProcessor.Process(stream);
            var saved = await _settlementService.SaveAsync(user, request.AccountType, file.FileName, parsed, cancellationToken);

            responses.Add(new SettlementUploadResponse
            {
                Id = saved.Id,
                AccountType = saved.AccountType,
                SettlementId = saved.SettlementId,
                TotalAmount = saved.TotalAmount,
                UploadedAt = saved.UploadedAt,
                DepositDate = saved.DepositDate,
                Year = saved.PeriodYear,
                Month = saved.PeriodMonth,
                Orders = saved.Orders.Count
            });
        }

        return Ok(responses);
    }

    [HttpPost("settlement/reconciliation")]
    public async Task<ActionResult<SettlementReconciliationResponse>> GenerateSettlementReconciliation(
        [FromForm] SettlementReconciliationRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await ResolveUserAsync(cancellationToken);
        await EnsureActiveStatusAsync(user, cancellationToken);

        if (request.SalesFile is null || request.SalesFile.Length == 0)
        {
            throw new InvalidOperationException("Arquivo de pedidos é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(request.AccountType))
        {
            throw new InvalidOperationException("Tipo de conta é obrigatório.");
        }

        using var stream = request.SalesFile.OpenReadStream();
        var response = await _settlementService.BuildReconciliationAsync(user, request.AccountType, request.SalesFile.FileName, stream, cancellationToken);
        return Ok(response);
    }

    private async Task<User> ResolveUserAsync(CancellationToken cancellationToken)
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(idValue) || !Guid.TryParse(idValue, out var userId))
        {
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new UnauthorizedAccessException("Usuário não encontrado.");
        }

        return user;
    }

    private async Task EnsureActiveStatusAsync(User user, CancellationToken cancellationToken)
    {
        var status = await _subscriptionService.GetUserStatusAsync(user, cancellationToken);
        if (status == "expirado")
        {
            throw new TrialExpiredException("Trial expirado. Faça upgrade para continuar.");
        }
    }
}
