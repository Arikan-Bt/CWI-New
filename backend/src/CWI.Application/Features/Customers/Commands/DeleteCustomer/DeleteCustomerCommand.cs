using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Customers.Commands.DeleteCustomer;

public record DeleteCustomerCommand(int Id) : IRequest;

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCustomerCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Customer>();
        var customer = await repo.AsQueryable()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (customer == null)
        {
            throw new Exception($"Customer with id {request.Id} not found");
        }

        customer.IsActive = false;
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
