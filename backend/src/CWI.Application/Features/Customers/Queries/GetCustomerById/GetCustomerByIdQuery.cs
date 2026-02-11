using CWI.Application.DTOs.Customers;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Customers.Queries.GetCustomerById;

public record GetCustomerByIdQuery(int Id) : IRequest<CustomerDto>;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCustomerByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Customer>();
        var customer = await repo.AsQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (customer == null)
        {
            throw new Exception($"Customer with id {request.Id} not found");
        }

        return new CustomerDto
        {
            Id = customer.Id,
            Code = customer.Code,
            Name = customer.Name,
            TaxOfficeName = customer.TaxOfficeName,
            TaxNumber = customer.TaxNumber,
            AddressLine1 = customer.AddressLine1,
            City = customer.City,
            Phone = customer.PrimaryPhone,
            Email = customer.Email,
            IsActive = customer.IsActive,
            Status = customer.IsActive ? "Active" : "Inactive",
            IsVendor = customer.IsVendor
        };
    }
}
