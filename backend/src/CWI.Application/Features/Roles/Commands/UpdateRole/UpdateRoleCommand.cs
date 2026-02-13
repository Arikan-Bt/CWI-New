using CWI.Application.Common.Caching;
using CWI.Application.DTOs.Roles;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Roles.Commands.UpdateRole;

public record UpdateRoleCommand : UpdateRoleDto, IRequest<Unit>, IInvalidatesCache
{
    public IReadOnlyCollection<string> CachePrefixesToInvalidate => [CachePrefixes.LookupRoles, CachePrefixes.LookupUsers];
}

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRoleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.Repository<Role>().AsQueryable()
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (role == null)
            throw new Exception("Role not found");

        role.Name = request.Name;
        role.Description = request.Description;
        role.IsActive = request.IsActive;

        var existingPermissions = role.RolePermissions.ToList();
        if (existingPermissions.Any())
        {
            _unitOfWork.Repository<RolePermission>().DeleteRange(existingPermissions);
        }
        role.RolePermissions.Clear();

        foreach (var permission in request.Permissions)
        {
            role.RolePermissions.Add(new RolePermission
            {
                PermissionKey = permission,
                RoleId = role.Id
            });
        }

        _unitOfWork.Repository<Role>().Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
