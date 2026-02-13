using CWI.Application.Common.Caching;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Roles.Commands.DeleteRole;

public record DeleteRoleCommand(int Id) : IRequest<Unit>, IInvalidatesCache
{
    public IReadOnlyCollection<string> CachePrefixesToInvalidate => [CachePrefixes.LookupRoles, CachePrefixes.LookupUsers];
}

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRoleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.Repository<Role>().AsQueryable()
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (role == null)
            throw new Exception("Role not found");

        if (role.Users.Any())
            throw new Exception("Cannot delete role with assigned users");

        _unitOfWork.Repository<Role>().Delete(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
