using GenericRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Scrutor;
using System.Reflection;
using System.Text;
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

        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var jwtOptions = configuration.GetSection("JwtOptions").Get<JwtOptions>()!;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,

                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });


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
