using DataProcessor.Data.Entities;

namespace DataProcessor.Data.Repositories;

public interface IUserRepository
{
    Task<User?> GetByCnpjAsync(string cnpj, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
