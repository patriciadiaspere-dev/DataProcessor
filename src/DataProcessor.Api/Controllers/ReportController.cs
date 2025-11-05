using System.Security.Claims;
using DataProcessor.Api.Exceptions;
using DataProcessor.Core.Processors;
using DataProcessor.Core.Writers;
using DataProcessor.Data.DTOs.Report;
using DataProcessor.Data.Entities;
using DataProcessor.Data.Repositories;
using DataProcessor.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataProcessor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
<<<<<<< HEAD
[Authorize]
=======
//[Authorize]
>>>>>>> main
public class ReportController : ControllerBase
{
    private readonly IReportProcessor _reportProcessor;
    private readonly IReportExcelWriter _excelWriter;
    private readonly IUserRepository _userRepository;
    private readonly ISubscriptionService _subscriptionService;

    public ReportController(
        IReportProcessor reportProcessor,
        IReportExcelWriter excelWriter,
        IUserRepository userRepository,
        ISubscriptionService subscriptionService)
    {
        _reportProcessor = reportProcessor;
        _excelWriter = excelWriter;
        _userRepository = userRepository;
        _subscriptionService = subscriptionService;
    }

    [HttpPost("amazon-sales")]
    public async Task<ActionResult<ReportResultResponse>> ProcessAmazonSales(
<<<<<<< HEAD
        [FromForm] IFormFile file,
=======
        IFormFile file,
>>>>>>> main
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
