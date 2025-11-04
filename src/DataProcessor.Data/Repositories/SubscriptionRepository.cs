using DataProcessor.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataProcessor.Data.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly DataContext _context;

    public SubscriptionRepository(DataContext context)
    {
        _context = context;
    }

    public Task<Subscription?> GetActiveByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return _context.Subscriptions
            .Where(s => s.UserId == userId && s.IsActive && s.StartDate <= now && s.EndDate >= now)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
