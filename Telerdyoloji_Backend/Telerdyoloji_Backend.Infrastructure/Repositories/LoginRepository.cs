using GenericRepository;
using Telerdyoloji_Backend.Domain.Entities;
using Telerdyoloji_Backend.Domain.Repositories;
using Telerdyoloji_Backend.Infrastructure.Context;

namespace Telerdyoloji_Backend.Infrastructure.Repositories;

internal class LoginRepository : Repository<Login, PostgresSqlDbContext>, ILoginRepository
{
    public LoginRepository(PostgresSqlDbContext context) : base(context)
    {
    }
}
