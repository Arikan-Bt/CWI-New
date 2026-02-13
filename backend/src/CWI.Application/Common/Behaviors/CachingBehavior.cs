using CWI.Application.Common.Caching;
using CWI.Application.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CWI.Application.Common.Behaviors;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICacheService _cacheService;
    private readonly ICacheKeyBuilder _cacheKeyBuilder;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
    private readonly CachingOptions _cachingOptions;

    public CachingBehavior(
        ICacheService cacheService,
        ICacheKeyBuilder cacheKeyBuilder,
        IOptions<CachingOptions> cachingOptions,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _cacheKeyBuilder = cacheKeyBuilder;
        _logger = logger;
        _cachingOptions = cachingOptions.Value;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_cachingOptions.Enabled || request is not ICacheableQuery cacheableQuery || cacheableQuery.BypassCache)
        {
            return await next();
        }

        var cacheKey = _cacheKeyBuilder.BuildScopedKey(cacheableQuery.CacheKey, request, cacheableQuery.IsUserScoped);
        if (_cacheService.TryGetValue<TResponse>(cacheKey, out var cachedResponse) && cachedResponse is not null)
        {
            _logger.LogInformation("cache_hit_total key:{CacheKey}", cacheKey);
            return cachedResponse;
        }

        _logger.LogInformation("cache_miss_total key:{CacheKey}", cacheKey);
        var response = await next();

        if (response is null)
        {
            return response;
        }

        if (!_cacheService.CanCache(response))
        {
            _logger.LogInformation("cache_skip_large_entry key:{CacheKey}", cacheKey);
            return response;
        }

        var slidingExpiration = cacheableQuery.SlidingExpiration == default
            ? TimeSpan.FromSeconds(_cachingOptions.DefaultSlidingSeconds)
            : cacheableQuery.SlidingExpiration;

        var absoluteExpiration = cacheableQuery.AbsoluteExpirationRelativeToNow ??
                                 TimeSpan.FromSeconds(_cachingOptions.DefaultAbsoluteSeconds);

        _cacheService.Set(cacheKey, response, slidingExpiration, absoluteExpiration, [cacheableQuery.CacheKey]);
        return response;
    }
}
