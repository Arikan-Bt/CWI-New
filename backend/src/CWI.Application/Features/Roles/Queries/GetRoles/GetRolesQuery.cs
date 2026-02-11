using CWI.Application.DTOs.Common;
using CWI.Application.DTOs.Roles;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Roles.Queries.GetRoles;

public record GetRolesQuery : PagedRequest, IRequest<PagedResult<RoleDto>>
{
    public string? FilterName { get; init; }
    public string? FilterDescription { get; init; }
    public string? FilterStatus { get; init; }
}

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, PagedResult<RoleDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRolesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Role>();
        var query = repo.AsQueryable().Include(r => r.Users).AsNoTracking();
        var isAsc = (request.SortOrder ?? 1) == 1;

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            query = query.Where(r => r.Name.ToLower().Contains(search) || 
                                     (r.Description != null && r.Description.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.FilterName))
        {
            var filter = request.FilterName.ToLower();
            query = query.Where(r => r.Name.ToLower().Contains(filter));
        }

        if (!string.IsNullOrWhiteSpace(request.FilterDescription))
        {
            var filter = request.FilterDescription.ToLower();
            query = query.Where(r => r.Description != null && r.Description.ToLower().Contains(filter));
        }

        if (!string.IsNullOrWhiteSpace(request.FilterStatus))
        {
            var filter = request.FilterStatus.ToLower();
            query = query.Where(r => (r.IsActive ? "active" : "inactive").Contains(filter));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        query = request.SortField?.ToLowerInvariant() switch
        {
            "name" => isAsc ? query.OrderBy(r => r.Name) : query.OrderByDescending(r => r.Name),
            "description" => isAsc ? query.OrderBy(r => r.Description) : query.OrderByDescending(r => r.Description),
            "usercount" => isAsc ? query.OrderBy(r => r.Users.Count) : query.OrderByDescending(r => r.Users.Count),
            "isactive" => isAsc ? query.OrderBy(r => r.IsActive) : query.OrderByDescending(r => r.IsActive),
            "status" => isAsc ? query.OrderBy(r => r.IsActive) : query.OrderByDescending(r => r.IsActive),
            _ => query.OrderBy(r => r.Name)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                IsActive = r.IsActive,
                UserCount = r.Users.Count
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<RoleDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
