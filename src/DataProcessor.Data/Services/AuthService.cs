using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DataProcessor.Data.Configuration;
using DataProcessor.Data.DTOs.Auth;
using DataProcessor.Data.Entities;
using DataProcessor.Data.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DataProcessor.Data.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ISubscriptionService _subscriptionService;
    private readonly JwtSettings _jwtSettings;
    private static readonly TimeSpan TrialDuration = TimeSpan.FromDays(3);

    public AuthService(
        IUserRepository userRepository,
        ISubscriptionService subscriptionService,
        IOptions<JwtSettings> jwtOptions)
    {
        _userRepository = userRepository;
        _subscriptionService = subscriptionService;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _userRepository.GetByCnpjAsync(request.Cnpj, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException("CNPJ já cadastrado.");
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Cnpj = request.Cnpj,
            Email = request.Email,
            CompanyName = request.CompanyName,
            PasswordHash = HashPassword(request.Password),
            CreatedAt = now,
            TrialExpiresAt = now.Add(TrialDuration)
        };

        await _userRepository.AddAsync(user, cancellationToken);

        await _subscriptionService.CreateTrialSubscriptionAsync(user, cancellationToken);

        await _userRepository.SaveChangesAsync(cancellationToken);

        return await BuildLoginResponseAsync(user, cancellationToken);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByCnpjAsync(request.Cnpj, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException("CNPJ ou senha inválidos.");
        }

        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new InvalidOperationException("CNPJ ou senha inválidos.");
        }

        return await BuildLoginResponseAsync(user, cancellationToken);
    }

    private async Task<LoginResponse> BuildLoginResponseAsync(User user, CancellationToken cancellationToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("cnpj", user.Cnpj),
            new("company", user.CompanyName)
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(descriptor);
        var status = await _subscriptionService.GetUserStatusAsync(user, cancellationToken);

        return new LoginResponse
        {
            Token = tokenHandler.WriteToken(token),
            ExpiresAt = expires,
            Status = status
        };
    }

    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
        }
        catch (Exception ex)
        {
            // fallback para hash simples ou erro inesperado
            Console.Error.WriteLine($"Erro ao validar senha: {ex.Message}");
            return false;
        }
    }
}
