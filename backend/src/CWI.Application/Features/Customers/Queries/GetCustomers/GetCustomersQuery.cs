using CWI.Application.DTOs.Common;
using CWI.Application.DTOs.Customers;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Customers.Queries.GetCustomers;

public record GetCustomersQuery : PagedRequest, IRequest<PagedResult<CustomerDto>>
{
    public string? FilterCode { get; init; }
    public string? FilterName { get; init; }
    public string? FilterCity { get; init; }
    public string? FilterPhone { get; init; }
    public string? FilterEmail { get; init; }
    public string? FilterType { get; init; }
    public string? FilterStatus { get; init; }
}

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, PagedResult<CustomerDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCustomersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Customer>();
        var query = repo.AsQueryable().AsNoTracking();
        var isAsc = (request.SortOrder ?? 1) == 1;

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(search) || 
                                     x.Code.ToLower().Contains(search) || 
                                     (x.TaxNumber != null && x.TaxNumber.Contains(search)) ||
                                     (x.Email != null && x.Email.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.FilterCode))
        {
            var filter = request.FilterCode.ToLower();
            query = query.Where(x => x.Code.ToLower().Contains(filter));
        }

        if (!string.IsNullOrWhiteSpace(request.FilterName))
        {
            var filter = request.FilterName.ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(filter));
        }

        if (!string.IsNullOrWhiteSpace(request.FilterCity))
        {
            var filter = request.FilterCity.ToLower();
            query = query.Where(x => x.City != null && x.City.ToLower().Contains(filter));
        }

        if (!string.IsNullOrWhiteSpace(request.FilterPhone))
        {
            var filter = request.FilterPhone.ToLower();
            query = query.Where(x => x.PrimaryPhone != null && x.PrimaryPhone.ToLower().Contains(filter));
        }

        if (!string.IsNullOrWhiteSpace(request.FilterEmail))
        {
            var filter = request.FilterEmail.ToLower();
            query = query.Where(x => x.Email != null && x.Email.ToLower().Contains(filter));
        }

        if (!string.IsNullOrWhiteSpace(request.FilterType))
        {
            var filter = request.FilterType.ToLower();
            query = query.Where(x => (x.IsVendor ? "vendor" : "customer").Contains(filter));
        }

        if (!string.IsNullOrWhiteSpace(request.FilterStatus))
        {
            var filter = request.FilterStatus.ToLower();
            query = query.Where(x => (x.IsActive ? "active" : "inactive").Contains(filter));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.SortField?.ToLowerInvariant() switch
        {
            "code" => isAsc ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
            "name" => isAsc ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name),
            "city" => isAsc ? query.OrderBy(x => x.City) : query.OrderByDescending(x => x.City),
            "phone" => isAsc ? query.OrderBy(x => x.PrimaryPhone) : query.OrderByDescending(x => x.PrimaryPhone),
            "email" => isAsc ? query.OrderBy(x => x.Email) : query.OrderByDescending(x => x.Email),
            "isvendor" => isAsc ? query.OrderBy(x => x.IsVendor) : query.OrderByDescending(x => x.IsVendor),
            "status" => isAsc ? query.OrderBy(x => x.IsActive) : query.OrderByDescending(x => x.IsActive),
            _ => query.OrderBy(x => x.Name)
        };

        var customers = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = customers.Select(x => new CustomerDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            TaxOfficeName = x.TaxOfficeName,
            TaxNumber = x.TaxNumber,
            AddressLine1 = x.AddressLine1,
            City = x.City,
            Phone = x.PrimaryPhone,
            Email = x.Email,
            IsActive = x.IsActive,
            Status = x.IsActive ? "Active" : "Inactive",
            IsVendor = x.IsVendor
        }).ToList();

        return new PagedResult<CustomerDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
