using System.Collections.Generic;
using System.Linq;
using CWI.Application.Interfaces.Repositories;
using CWI.Application.Common.Caching;
using CWI.Domain.Entities.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CWI.Application.Interfaces.Services;

namespace CWI.Application.Features.Reports.Queries;

public class GetCustomersLookupQuery : IRequest<List<CustomerLookupDto>>
{
    public bool OnlyVendors { get; set; } = false;
}

public class CustomerLookupDto
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class GetCustomersLookupQueryHandler : IRequestHandler<GetCustomersLookupQuery, List<CustomerLookupDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetCustomersLookupQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<List<CustomerLookupDto>> Handle(GetCustomersLookupQuery request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Customer>();
        var queryable = repo.AsQueryable()
            .Where(c => c.IsActive);

        if (request.OnlyVendors)
        {
            queryable = queryable.Where(c => c.IsVendor);
        }

        // Eğer yönetici değilse ve bağlı müşteri Id'si varsa sadece onu görsün
        if (!_currentUserService.IsAdministrator && _currentUserService.LinkedCustomerId.HasValue)
        {
            queryable = queryable.Where(c => c.Id == _currentUserService.LinkedCustomerId.Value);
        }

        var customers = await queryable
            .Select(c => new CustomerLookupDto
            {
                Label = $"{c.Code} - {c.Name}",
                Value = c.Code
            })
            .ToListAsync(cancellationToken);

        return customers;
    }
}
