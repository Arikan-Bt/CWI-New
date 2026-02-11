using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Lookups;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Reports.Queries;

public class GetShipmentTermsLookupQuery : IRequest<List<object>>
{
    public class GetShipmentTermsLookupQueryHandler : IRequestHandler<GetShipmentTermsLookupQuery, List<object>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetShipmentTermsLookupQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<object>> Handle(GetShipmentTermsLookupQuery request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.Repository<ShipmentTerm, int>();
            return await repo.AsQueryable()
                .Where(x => x.IsActive)
                .OrderBy(x => x.SortOrder)
                .Select(x => new { label = x.Name, value = x.Code })
                .Cast<object>()
                .ToListAsync(cancellationToken);
        }
    }
}
