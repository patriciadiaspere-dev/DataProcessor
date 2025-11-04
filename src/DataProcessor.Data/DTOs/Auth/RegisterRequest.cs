namespace DataProcessor.Data.DTOs.Auth;

public class RegisterRequest
{
    public string Cnpj { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
