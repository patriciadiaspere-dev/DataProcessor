using System.Security.Claims;
using DataProcessor.Data.DTOs.User;
using DataProcessor.Data.Repositories;
using DataProcessor.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataProcessor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ISubscriptionService _subscriptionService;

    public UserController(IUserRepository userRepository, ISubscriptionService subscriptionService)
    {
        _userRepository = userRepository;
        _subscriptionService = subscriptionService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileResponse>> GetProfile(CancellationToken cancellationToken)
    {
        var user = await ResolveUserAsync(cancellationToken);
        var response = new UserProfileResponse
        {
            Id = user.Id,
            Cnpj = user.Cnpj,
            Email = user.Email,
            CompanyName = user.CompanyName,
            CreatedAt = user.CreatedAt,
            TrialExpiresAt = user.TrialExpiresAt
        };

        return Ok(response);
    }

    [HttpGet("status")]
    public async Task<ActionResult<UserStatusResponse>> GetStatus(CancellationToken cancellationToken)
    {
        var user = await ResolveUserAsync(cancellationToken);
        var status = await _subscriptionService.GetUserStatusAsync(user, cancellationToken);
        var response = new UserStatusResponse
        {
            Status = status,
            TrialDaysRemaining = _subscriptionService.GetTrialDaysRemaining(user)
        };

        return Ok(response);
    }

    private async Task<DataProcessor.Data.Entities.User> ResolveUserAsync(CancellationToken cancellationToken)
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
}
