using GenericRepository;
using Microsoft.EntityFrameworkCore;
using Telerdyoloji_Backend.Domain.Entities;
using Telerdyoloji_Backend.Domain.Repositories;
using Telerdyoloji_Backend.Infrastructure.Context;

namespace Telerdyoloji_Backend.Infrastructure.Repositories;

internal class UserRepository : Repository<User, PostgresSqlDbContext>, IUserRepository
{
    public UserRepository(PostgresSqlDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Kullanıcının Email veya Username vererek User bilgisine erişir.
    /// </summary>
    /// <param name="emailOrUsername">Email veya Username</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>User nesnesi (yoksa null)</returns>
    public async Task<User?> GetByEmailOrUserNameAsync(string emailOrUsername, CancellationToken cancellationToken = default)
    {
        // Burada kullanıcı Email'e veya Login.Username değerine göre arama yapılır.
        // 'Login' navigation propertysi üzerinden Username'e erişiyoruz.
        return await Where(u =>
                u.Email == emailOrUsername ||
                (u.Login != null && u.Login.Username == emailOrUsername))
            .Include(u => u.Login) // Gerekirse Login entity'sini de dahil ediyoruz
            .FirstOrDefaultAsync(cancellationToken);
    }
}