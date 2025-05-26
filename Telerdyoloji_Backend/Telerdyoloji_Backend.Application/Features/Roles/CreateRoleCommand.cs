using FluentValidation;
using GenericRepository;
using MediatR;
using Telerdyoloji_Backend.Domain.Entities;
using Telerdyoloji_Backend.Domain.Repositories;
using TS.Result;

namespace Telerdyoloji_Backend.Application.Features.Roles;
public sealed record CreateRoleCommand(
    string RoleName,
    string Description) : IRequest<Result<string>>;


public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("Rol adı boş olamaz.")
            .MaximumLength(50).WithMessage("Rol adı 50 karakterden uzun olamaz.");

        RuleFor(x => x.Description)
            .MaximumLength(100).WithMessage("Açıklama 100 karakterden uzun olamaz.");
    }
}

internal sealed class CreateRoleCommandHandler(
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateRoleCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        bool isRoleNameExists = await roleRepository.AnyAsync(p => p.RoleName == request.RoleName, cancellationToken);

        if (isRoleNameExists)
            return Result<string>.Failure("Bu rol adı ile daha önce kayıt oluşturulmuş");

        Role role = new Role
        {
            RoleName = request.RoleName,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        roleRepository.Add(role);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Role başarıyla kaydedildi";
    }
}



