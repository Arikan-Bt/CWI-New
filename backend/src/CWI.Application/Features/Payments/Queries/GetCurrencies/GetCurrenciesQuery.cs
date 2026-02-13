using CWI.Application.Common.Models;
using CWI.Application.Common.Caching;
using CWI.Application.Interfaces.Repositories;
using CWI.Domain.Entities.Payments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CWI.Application.Features.Payments.Queries.GetCurrencies;

public class GetCurrenciesQuery : IRequest<List<CurrencyDto>>, ICacheableQuery
{
    public string CacheKey => CachePrefixes.LookupCurrencies;
    public TimeSpan SlidingExpiration => TimeSpan.FromMinutes(2);
    public TimeSpan? AbsoluteExpirationRelativeToNow => TimeSpan.FromMinutes(5);
    public bool BypassCache { get; init; }
    public bool IsUserScoped => false;

    public class GetCurrenciesQueryHandler : IRequestHandler<GetCurrenciesQuery, List<CurrencyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCurrenciesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CurrencyDto>> Handle(GetCurrenciesQuery request, CancellationToken cancellationToken)
        {
            var currencies = await _unitOfWork.Repository<Currency, int>()
                .AsQueryable()
                .AsNoTracking()
                .Where(x => x.IsActive)
                .Select(x => new CurrencyDto
                {
                    Id = x.Id,
                    Code = x.Code ?? string.Empty,
                    Symbol = x.Symbol ?? string.Empty,
                    Name = x.Name ?? string.Empty
                })
                .ToListAsync(cancellationToken);

            return currencies;
        }
    }
}

public class CurrencyDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
