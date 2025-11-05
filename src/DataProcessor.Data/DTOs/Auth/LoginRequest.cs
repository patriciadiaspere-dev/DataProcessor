namespace DataProcessor.Data.DTOs.Auth;

public class LoginRequest
{
    public string Cnpj { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
