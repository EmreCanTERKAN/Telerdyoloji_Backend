using GenericRepository;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Telerdyoloji_Backend.Application.Features.Auth.Login;
using Telerdyoloji_Backend.Application.Services;
using Telerdyoloji_Backend.Domain.Entities;
using Telerdyoloji_Backend.Domain.Repositories;
using Telerdyoloji_Backend.Infrastructure.Options;

namespace Telerdyoloji_Backend.Infrastructure.Services;

internal class JwtProvider(
    IUserRepository userRepository,
    ILoginRepository loginRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork,
    IOptions<JwtOptions> jwtOptions) : IJwtProvider
{
    public async Task<LoginCommandResponse> CreateToken(User user)
    {
        Login? loginInfo = null;
        Role? roleInfo = null;

        if (user.Login is null)
        {
            loginInfo = await loginRepository.GetByExpressionWithTrackingAsync(x => x.LoginId == user.LoginId);

            if (loginInfo is not null)
            {
                roleInfo = await roleRepository.FirstOrDefaultAsync(x => x.RoleId == loginInfo.RoleId);
            }
        }
        else
        {
            loginInfo = user.Login;
            if (loginInfo.Role is null)
            {
                roleInfo = await roleRepository.FirstOrDefaultAsync(x => x.RoleId == loginInfo.RoleId);
            }
            else
            {
                roleInfo = loginInfo.Role;
            }
        }

        if (loginInfo is null)
        {
            throw new InvalidOperationException("Login information not found");
        }

        List<Claim> claims = new()
        {
            new Claim("UserId", user.UserId.ToString()),
            new Claim("LoginId", user.LoginId.ToString()),
            new Claim("RoleId", loginInfo.RoleId.ToString()),
            new Claim("Username", loginInfo.Username),
            new Claim("FirstName", user.FirstName),
            new Claim("LastName", user.LastName),
            new Claim("FullName", user.FullName),
            new Claim("Email", user.Email),
            new Claim("UserCode", user.UserCode),
            new Claim("IsActive", user.IsActive.ToString()),
            
            // Standard JWT claims
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, roleInfo?.RoleName ?? "User")
        };

        DateTime expires = DateTime.UtcNow.AddMonths(1);
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.SecretKey));

        JwtSecurityToken jwtSecurityToken = new(
            issuer: jwtOptions.Value.Issuer,
            audience: jwtOptions.Value.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512));

        JwtSecurityTokenHandler handler = new();
        string token = handler.WriteToken(jwtSecurityToken);

        // Refresh token oluştur
        string refreshToken = Guid.NewGuid().ToString();
        DateTime refreshTokenExpires = expires.AddHours(1);

        // Login bilgilerini güncelle
        loginInfo.LastLogin = DateTime.UtcNow;
        loginInfo.RefreshToken = refreshToken;
        loginInfo.RefreshTokenExpires = refreshTokenExpires;

        loginRepository.Update(loginInfo);
        await unitOfWork.SaveChangesAsync(); // Değişiklikleri kaydet

        return new LoginCommandResponse(token, refreshToken, refreshTokenExpires);
    }

    public async Task<LoginCommandResponse> RefreshToken(string refreshToken)
    {
        var login = await loginRepository.GetByExpressionAsync(x => x.RefreshToken == refreshToken && x.RefreshTokenExpires > DateTime.UtcNow && x.IsActive);

        if (login is null)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        }

        var user = await userRepository.GetByExpressionAsync(x => x.LoginId == login.LoginId);

        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("User not found or inactive");
        }
        user.Login = login;

        return await CreateToken(user);
    }
}
