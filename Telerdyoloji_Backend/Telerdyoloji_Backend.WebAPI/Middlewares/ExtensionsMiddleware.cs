using Telerdyoloji_Backend.Domain.Entities;
using Telerdyoloji_Backend.Domain.Repositories;
using Telerdyoloji_Backend.Infrastructure.Context;

namespace Telerdyoloji_Backend.WebAPI.Middlewares;

public static class ExtensionsMiddleware
{
    public async static void CreateFirstUser(WebApplication app)
    {
        using (var scoped = app.Services.CreateScope())
        {
            var context = scoped.ServiceProvider.GetRequiredService<PostgresSqlDbContext>();
            var userRepository = scoped.ServiceProvider.GetRequiredService<IUserRepository>();
            var loginRepository = scoped.ServiceProvider.GetRequiredService<ILoginRepository>();
            var roleRepository = scoped.ServiceProvider.GetRequiredService<IRoleRepository>();

            var adminRole = await roleRepository.GetByExpressionAsync(x => x.RoleName == "Admin");

            if (adminRole is null)
            {
                adminRole = new Role
                {
                    RoleName = "Admin",
                    Description = "Sistem Yöneticisi",
                    CreatedAt = DateTime.UtcNow
                };

                roleRepository.Add(adminRole);
                context.SaveChanges();
            }

            var existingUser = userRepository.GetByExpression(x => x.Email == "admin@netcare.com");

            if (existingUser is null)
            {
                var loginId = Guid.NewGuid();

                //Login nesnesi oluştur
                var login = new Login
                {
                    LoginId = loginId,
                    RoleId = adminRole.RoleId,
                    Username = "admin",
                    Password = "1", // TODO: haslenmş bir şifre kullanılmalı
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UserCode = "ADMIN001"
                };

                var user = new User
                {
                    LoginId = loginId,
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@admin.com",
                    Phone = "",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true,
                    UserCode = "ADMIN001",
                    IsEmailVerified = true, // Admin kullanıcısı için email onaylı
                    LockoutEnabled = false,
                    LockoutEnd = null,
                    FailedLoginAttempts = 0
                };

                loginRepository.Add(login);
                userRepository.Add(user);

                context.SaveChanges();


                Console.WriteLine("Admin kullanıcısı oluşturuldu:");
                Console.WriteLine("Email: admin@netcare.com");
                Console.WriteLine("Şifre: 1");
            }


        }
    }
}
