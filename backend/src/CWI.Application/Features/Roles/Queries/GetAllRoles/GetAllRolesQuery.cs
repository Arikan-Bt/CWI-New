using CWI.Application.DTOs.Roles;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Roles.Queries.GetAllRoles;

public record GetAllRolesQuery : IRequest<List<RoleDto>>;

public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, List<RoleDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllRolesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _unitOfWork.Repository<Role>().AsQueryable()
            .Include(r => r.Users)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                IsActive = r.IsActive,
                UserCount = r.Users.Count
            })
            .ToListAsync(cancellationToken);

        return roles;
    }
}
