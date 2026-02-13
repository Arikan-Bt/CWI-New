using CWI.Application.Common.Caching;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Customers.Commands.UpdateCustomer;

public class UpdateCustomerCommand : IRequest, IInvalidatesCache
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TaxOfficeName { get; set; }
    public string? TaxNumber { get; set; }
    public string? AddressLine1 { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsVendor { get; set; }

    public IReadOnlyCollection<string> CachePrefixesToInvalidate => [CachePrefixes.LookupCustomers];
}

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomerCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Customer>();
        var customer = await repo.AsQueryable()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (customer == null)
        {
            throw new Exception($"Customer with id {request.Id} not found");
        }

        customer.Code = request.Code;
        customer.Name = request.Name;
        customer.TaxOfficeName = request.TaxOfficeName;
        customer.TaxNumber = request.TaxNumber;
        customer.AddressLine1 = request.AddressLine1;
        customer.City = request.City;
        customer.PrimaryPhone = request.Phone;
        customer.Email = request.Email;
        customer.IsActive = request.Status == "Active";
        customer.IsVendor = request.IsVendor;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
