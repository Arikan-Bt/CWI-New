namespace CWI.Application.Common.Caching;

public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan SlidingExpiration { get; }
    TimeSpan? AbsoluteExpirationRelativeToNow { get; }
    bool BypassCache { get; }
    bool IsUserScoped => true;
}
