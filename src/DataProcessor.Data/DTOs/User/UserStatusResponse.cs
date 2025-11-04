namespace DataProcessor.Data.DTOs.User;

public class UserStatusResponse
{
    public string Status { get; set; } = string.Empty;
    public int TrialDaysRemaining { get; set; }
}
