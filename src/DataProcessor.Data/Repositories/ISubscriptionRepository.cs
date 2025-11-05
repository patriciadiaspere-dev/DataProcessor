using DataProcessor.Data.Entities;

namespace DataProcessor.Data.Repositories;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetActiveByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
