using GenericRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scrutor;
using System.Reflection;
using Telerdyoloji_Backend.Infrastructure.Context;
using Telerdyoloji_Backend.Infrastructure.Options;

namespace Telerdyoloji_Backend.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PostgresSqlDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("PostgreSql"));
        });

        services.AddScoped<IUnitOfWork>(srv => srv.GetRequiredService<PostgresSqlDbContext>());

        services.Configure<JwtOptions>(configuration.GetSection("JwtOptions"));


        services.Scan(action =>
        {
            action
            .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(publicOnly: false)
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsMatchingInterface()
            .AsImplementedInterfaces()
            .WithScopedLifetime();
        });

        services.AddHealthChecks()
        .AddCheck("health-check", () => HealthCheckResult.Healthy())
        .AddDbContextCheck<PostgresSqlDbContext>();

        services.AddAuthorization();

        return services;
    }
}
