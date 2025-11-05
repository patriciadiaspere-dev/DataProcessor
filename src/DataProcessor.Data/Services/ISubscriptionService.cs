using DataProcessor.Data.Entities;

namespace DataProcessor.Data.Services;

public interface ISubscriptionService
{
    Task<string> GetUserStatusAsync(User user, CancellationToken cancellationToken = default);
    int GetTrialDaysRemaining(User user);
}
