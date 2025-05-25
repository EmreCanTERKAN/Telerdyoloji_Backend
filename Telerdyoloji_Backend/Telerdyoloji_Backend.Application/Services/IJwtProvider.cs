using Telerdyoloji_Backend.Application.Features.Auth.Login;
using Telerdyoloji_Backend.Domain.Entities;

namespace Telerdyoloji_Backend.Application.Services
{
    public interface IJwtProvider
    {
        Task<LoginCommandResponse> CreateToken(User user);
        Task<LoginCommandResponse> RefreshToken(string refreshToken);
    }
}
