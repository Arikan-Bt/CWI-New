using CWI.Application.Common.Caching;
using CWI.Application.DTOs.Roles;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Identity;
using MediatR;

namespace CWI.Application.Features.Roles.Commands.CreateRole;

public record CreateRoleCommand : CreateRoleDto, IRequest<int>, IInvalidatesCache
{
    public IReadOnlyCollection<string> CachePrefixesToInvalidate => [CachePrefixes.LookupRoles];
}

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = new Role
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            Code = request.Name.Replace(" ", "").ToUpper(),
            RolePermissions = request.Permissions.Select(p => new RolePermission
            {
                PermissionKey = p
            }).ToList()
        };

        await _unitOfWork.Repository<Role>().AddAsync(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return role.Id;
    }
}
