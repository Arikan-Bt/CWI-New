using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Payments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Reports.Queries;

public class GetPaymentMethodsLookupQuery : IRequest<List<object>>
{
    public class GetPaymentMethodsLookupQueryHandler : IRequestHandler<GetPaymentMethodsLookupQuery, List<object>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPaymentMethodsLookupQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<object>> Handle(GetPaymentMethodsLookupQuery request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.Repository<PaymentMethod, int>();
            return await repo.AsQueryable()
                .Where(x => x.IsActive)
                .OrderBy(x => x.SortOrder)
                .Select(x => new { label = x.Name, value = x.Code })
                .Cast<object>()
                .ToListAsync(cancellationToken);
        }
    }
}
