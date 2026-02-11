using CWI.Application.DTOs.Common;
using CWI.Application.DTOs.Users;
using CWI.Application.Interfaces.Repositories;
using CWI.Application.Interfaces.Services;
using CWI.Domain.Entities.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery : PagedRequest, IRequest<UserPagedResult<UserDto>>
{
    public string? FilterName { get; init; }
    public string? FilterEmail { get; init; }
    public string? FilterRole { get; init; }
    public string? FilterStatus { get; init; }
}

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, UserPagedResult<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetUsersQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<UserPagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<User>();
        var query = repo.AsQueryable()
            .Include(u => u.Role)
            .Include(u => u.LinkedCustomer)
            .Where(u => u.IsActive);
        var isAsc = (request.SortOrder ?? 1) == 1;

        // Eğer kullanıcı yönetici değilse, sadece kendi projesindeki kullanıcıları görmelidir.
        if (!_currentUserService.IsAdministrator && _currentUserService.ProjectType.HasValue)
        {
            query = query.Where(u => u.ProjectType == _currentUserService.ProjectType.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            query = query.Where(u => 
                (u.FirstName != null && u.FirstName.ToLower().Contains(search)) ||
                (u.LastName != null && u.LastName.ToLower().Contains(search)) ||
                (u.Email != null && u.Email.ToLower().Contains(search)) ||
                (u.UserName.ToLower().Contains(search))
            );
        }

        if (!string.IsNullOrWhiteSpace(request.FilterName))
        {
            var filter = request.FilterName.ToLower();
            query = query.Where(u =>
                ((u.FirstName ?? string.Empty) + " " + (u.LastName ?? string.Empty)).ToLower().Contains(filter));
        }

        if (!string.IsNullOrWhiteSpace(request.FilterEmail))
        {
            var filter = request.FilterEmail.ToLower();
            query = query.Where(u => u.Email != null && u.Email.ToLower().Contains(filter));
        }

        if (!string.IsNullOrWhiteSpace(request.FilterRole))
        {
            var filter = request.FilterRole.ToLower();
            query = query.Where(u => u.Role.Name.ToLower().Contains(filter));
        }

        if (!string.IsNullOrWhiteSpace(request.FilterStatus))
        {
            var filter = request.FilterStatus.ToLower();
            query = query.Where(u => (u.IsActive ? "active" : "inactive").Contains(filter));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        query = request.SortField?.ToLowerInvariant() switch
        {
            "name" => isAsc ? query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName) : query.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.LastName),
            "email" => isAsc ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email),
            "role" => isAsc ? query.OrderBy(u => u.Role.Name) : query.OrderByDescending(u => u.Role.Name),
            "status" => isAsc ? query.OrderBy(u => u.IsActive) : query.OrderByDescending(u => u.IsActive),
            _ => query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.FirstName ?? string.Empty,
                Surname = u.LastName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                Role = u.Role.Name,
                RoleId = u.RoleId,
                Status = u.IsActive ? "Active" : "Inactive",
                ClientCode = u.UserName,
                MobilePhone = u.PhoneNumber,
                ProjectType = u.ProjectType,
                CurrentAccount = u.LinkedCustomer != null ? u.LinkedCustomer.Code : null
            })
            .ToListAsync(cancellationToken);

        return new UserPagedResult<UserDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
