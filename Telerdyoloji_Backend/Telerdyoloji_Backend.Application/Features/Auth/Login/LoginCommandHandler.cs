using GenericRepository;
using MediatR;
using Telerdyoloji_Backend.Application.Services;
using Telerdyoloji_Backend.Domain.Entities;
using Telerdyoloji_Backend.Domain.Repositories;
using TS.Result;

namespace Telerdyoloji_Backend.Application.Features.Auth.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginCommandResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly ILoginRepository _loginRepository;
    private readonly IJwtProvider _jwtProvider;
    private const int MaxFailedAttempts = 3;
    private const int LockoutDurationMinutes = 5;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IUserRepository userRepository,
        ILoginRepository loginRepository,
        IJwtProvider jwtProvider)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _loginRepository = loginRepository;
        _jwtProvider = jwtProvider;
    }

    public async Task<Result<LoginCommandResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1) Kullanıcı var mı? (Email veya Username)
        var user = await _userRepository.GetByEmailOrUserNameAsync(request.EmailOrUserName, cancellationToken);
        if (user is null || user.Login is null)
        {
            return Result<LoginCommandResponse>.Failure(500, "Kullanıcı bulunamadı");
        }

        // 2) Önceki lockout kontrolü.
        if (user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            var remaining = user.LockoutEnd.Value - DateTime.UtcNow;
            var minutesBlocked = Math.Ceiling(remaining.TotalMinutes);
            return Result<LoginCommandResponse>.Failure(500, $"Şifrenizi 3 defa yanlış girdiğiniz için kullanıcı {minutesBlocked} dakika süreyle bloke edilmiştir.");
        }

        // 3) Email onaylı mı?
        if (!user.IsEmailVerified)
        {
            return Result<LoginCommandResponse>.Failure(500, "E-posta adresiniz henüz onaylı değil.");
        }

        // 4) Kullanıcı aktif mi?
        if (!user.IsActive || !user.Login.IsActive)
        {
            return Result<LoginCommandResponse>.Failure(500, "Kullanıcı pasif durumda, giriş yapılamaz.");
        }

        // 5) Şifre kontrolü (hash ve salt gibi yöntemlerle doğrulama yapılmalı).
        var isPasswordValid = VerifyPassword(user.Login.Password, request.Password);
        if (!isPasswordValid)
        {
            // Burada hatalı giriş sayısı kontrolü yapalım:
            await HandleFailedLoginAttemptAsync(user, cancellationToken);
            return Result<LoginCommandResponse>.Failure(500, "Şifreniz yanlış.");
        }

        // 6) Şifre doğru ise - başarılı giriş işlemleri
        HandleSuccessfulLogin(user, cancellationToken);

        // 7) JWT token oluşturma.
        var token = await _jwtProvider.CreateToken(user);


        // 8) Refresh token'ı Login entity'sine kaydet
        user.Login.RefreshToken = token.RefreshToken;
        user.Login.RefreshTokenExpires = token.RefreshTokenExpires;
        user.Login.LastLogin = DateTime.UtcNow;

        _loginRepository.Update(user.Login);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 9) Başarılı sonuç dön
        return token;
    }

    private bool VerifyPassword(string storedPasswordHash, string providedPassword)
    {
        // TODO: Gerçek projede BCrypt, PBKDF2 veya Argon2 kullanın
        // Örnek: return BCrypt.Net.BCrypt.Verify(providedPassword, storedPasswordHash);
        return storedPasswordHash == providedPassword;
    }

    private async Task HandleFailedLoginAttemptAsync(User user, CancellationToken cancellationToken)
    {
        // Failed attempts sayısını artır
        user.FailedLoginAttempts++;

        // Maksimum deneme sayısına ulaştı mı?
        if (user.FailedLoginAttempts >= MaxFailedAttempts)
        {
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutDurationMinutes);
        }

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private void HandleSuccessfulLogin(User user, CancellationToken cancellationToken)
    {
        // Başarılı giriş sonrası lockout ve failed attempts'i sıfırla
        user.LockoutEnabled = false;
        user.LockoutEnd = null;
        user.FailedLoginAttempts = 0;
        user.UpdatedAt = DateTime.UtcNow;

        _userRepository.Update(user);
        // SaveChanges'i ana metodda yapacağız, burada yapmıyoruz
    }
}
