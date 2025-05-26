using MediatR;
using Microsoft.EntityFrameworkCore;
using Telerdyoloji_Backend.Domain.Entities;
using Telerdyoloji_Backend.Domain.Repositories;
using TS.Result;

namespace Telerdyoloji_Backend.Application.Features.Roles;
public sealed record GetAllRoleQuery() : IRequest<Result<List<Role>>>;

internal sealed class GetAllRoleQueryHandler(
    IRoleRepository roleRepository) : IRequestHandler<GetAllRoleQuery, Result<List<Role>>>
{
    public async Task<Result<List<Role>>> Handle(GetAllRoleQuery request, CancellationToken cancellationToken)
    {
        List<Role> roles = await roleRepository
            .GetAll()
            .Include(r => r.Logins)
            .OrderBy(p => p.RoleName)
            .ToListAsync();

        return roles;
    }
}