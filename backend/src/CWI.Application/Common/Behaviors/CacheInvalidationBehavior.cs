using CWI.Application.Common.Caching;
using CWI.Application.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CWI.Application.Common.Behaviors;

public class CacheInvalidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheInvalidationBehavior<TRequest, TResponse>> _logger;
    private readonly CachingOptions _cachingOptions;

    public CacheInvalidationBehavior(
        ICacheService cacheService,
        IOptions<CachingOptions> cachingOptions,
        ILogger<CacheInvalidationBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
        _cachingOptions = cachingOptions.Value;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        if (!_cachingOptions.Enabled || request is not IInvalidatesCache invalidatesCache)
        {
            return response;
        }

        foreach (var prefix in invalidatesCache.CachePrefixesToInvalidate.Distinct(StringComparer.Ordinal))
        {
            var removedCount = _cacheService.RemoveByPrefix(prefix);
            _logger.LogInformation("cache_invalidate_total prefix:{Prefix} removed:{RemovedCount}", prefix, removedCount);
        }

        return response;
    }
}
