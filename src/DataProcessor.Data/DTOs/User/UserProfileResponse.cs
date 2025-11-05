namespace DataProcessor.Data.DTOs.User;

public class UserProfileResponse
{
    public Guid Id { get; set; }
    public string Cnpj { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime TrialExpiresAt { get; set; }
}
