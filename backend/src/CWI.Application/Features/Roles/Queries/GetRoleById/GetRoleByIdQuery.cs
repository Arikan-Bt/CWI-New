using CWI.Application.DTOs.Roles;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Roles.Queries.GetRoleById;

public record GetRoleByIdQuery(int Id) : IRequest<RoleDetailDto>;

public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRoleByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RoleDetailDto> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.Repository<Role>().AsQueryable()
            .Include(r => r.Users)
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (role == null)
            throw new Exception("Role not found");

        return new RoleDetailDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsActive = role.IsActive,
            UserCount = role.Users.Count,
            Permissions = role.RolePermissions.Select(rp => rp.PermissionKey).ToList(),
            Users = role.Users.Select(u => new RoleUserDto
            {
                Id = u.Id,
                FullName = $"{u.FirstName} {u.LastName}",
                Email = u.Email ?? string.Empty,
                IsActive = u.IsActive
            }).ToList()
        };
    }
}
