using GenericRepository;
using Telerdyoloji_Backend.Domain.Entities;
using Telerdyoloji_Backend.Domain.Repositories;
using Telerdyoloji_Backend.Infrastructure.Context;

namespace Telerdyoloji_Backend.Infrastructure.Repositories;
internal class RoleRepository : Repository<Role, PostgresSqlDbContext>, IRoleRepository
{
    public RoleRepository(PostgresSqlDbContext context) : base(context)
    {
    }
}
