using FluentValidation;
using GenericRepository;
using MediatR;
using Telerdyoloji_Backend.Domain.Entities;
using Telerdyoloji_Backend.Domain.Repositories;
using TS.Result;

namespace Telerdyoloji_Backend.Application.Features.Roles;
public sealed record UpdateRoleCommand(
    int RoleId,
    string RoleName,
    string Description) : IRequest<Result<string>>;

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("Rol adı boş olamaz.")
            .MaximumLength(100).WithMessage("Rol adı 100 karakterden uzun olamaz.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Rol açıklaması boş olamaz.")
            .MaximumLength(500).WithMessage("Açıklama 500 karakterden uzun olamaz.");
    }
}

internal sealed class UpdateRoleCommandHandler(
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateRoleCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        Role role = await roleRepository.GetByExpressionWithTrackingAsync(p => p.RoleId == request.RoleId, cancellationToken);

        if (role is null)
            return Result<string>.Failure("Bu idye uygun kayıt bulunamadı");

        if (role.RoleName != request.RoleName)
        {
            bool isRoleNameExists = await roleRepository.AnyAsync(p => p.RoleName == request.RoleName, cancellationToken);

            if (isRoleNameExists)
                return Result<string>.Failure("Bu rol adı ile daha önce kayıt oluşturulmuş");
        }

        role.RoleName = request.RoleName;
        role.Description = request.Description;
        

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Role başarıyla güncellendi";
    }
}
