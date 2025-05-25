using GenericRepository;
using Telerdyoloji_Backend.Domain.Entities;

namespace Telerdyoloji_Backend.Domain.Repositories;

public interface IUserRepository : IRepository<User>
{
    public Task<User?> GetByEmailOrUserNameAsync(string emailOrUsername, CancellationToken cancellationToken = default);
}
