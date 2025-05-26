using GenericRepository;
using MediatR;
using Telerdyoloji_Backend.Domain.Repositories;
using TS.Result;

namespace Telerdyoloji_Backend.Application.Features.Roles;
public sealed record DeleteRoleByIdCommand(
    int RoleId) : IRequest<Result<string>>;

internal sealed class DeleteRoleCommandHandler(
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteRoleByIdCommand, Result<string>>
{
    public async Task<Result<string>> Handle(DeleteRoleByIdCommand request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.FirstOrDefaultAsync(p => p.RoleId == request.RoleId, cancellationToken);

        if (role is null)
            return Result<string>.Failure("Bu idye uygun kayıt bulunamadı");

        roleRepository.Delete(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return "Role başarıyla silindi.";
    }
}
