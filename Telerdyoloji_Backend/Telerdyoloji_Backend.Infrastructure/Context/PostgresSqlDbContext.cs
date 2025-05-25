using GenericRepository;
using Microsoft.EntityFrameworkCore;
using Telerdyoloji_Backend.Domain.Entities;

namespace Telerdyoloji_Backend.Infrastructure.Context;
public class PostgresSqlDbContext : DbContext, IUnitOfWork
{
    public PostgresSqlDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Role> Roles { get; set; }
    public DbSet<Login> Logins { get; set; }
    public DbSet<User> Users { get; set; }

    protected PostgresSqlDbContext()
    {
    }
}