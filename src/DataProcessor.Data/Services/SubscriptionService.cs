using DataProcessor.Data.Entities;
using DataProcessor.Data.Repositories;

namespace DataProcessor.Data.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public SubscriptionService(ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task CreateTrialSubscriptionAsync(User user, CancellationToken cancellationToken = default)
    {
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            PlanType = "trial",
            StartDate = user.CreatedAt,
            EndDate = user.TrialExpiresAt,
            IsActive = true
        };

        await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        await _subscriptionRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> GetUserStatusAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user.TrialExpiresAt > DateTime.UtcNow)
        {
            return "trial";
        }

        var subscription = await _subscriptionRepository.GetActiveByUserAsync(user.Id, cancellationToken);
        if (subscription is not null)
        {
            return "assinado";
        }

        return "expirado";
    }

    public int GetTrialDaysRemaining(User user)
    {
        var remaining = (int)Math.Ceiling((user.TrialExpiresAt - DateTime.UtcNow).TotalDays);
        return remaining < 0 ? 0 : remaining;
    }
}
