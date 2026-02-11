using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Customers;
using MediatR;

namespace CWI.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommand : IRequest<int>
{
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
}

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = new Customer
        {
            Code = request.Code,
            Name = request.Name,
            TaxOfficeName = request.TaxOfficeName,
            TaxNumber = request.TaxNumber,
            AddressLine1 = request.AddressLine1,
            City = request.City,
            PrimaryPhone = request.Phone,
            Email = request.Email,
            IsActive = request.Status == "Active",
            IsVendor = request.IsVendor
        };

        await _unitOfWork.Repository<Customer>().AddAsync(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return customer.Id;
    }
}
