using System.Text.Json;
using CWI.Application.Common.Caching;
using CWI.Application.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CWI.Infrastructure.Services;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ICacheRegistry _cacheRegistry;
    private readonly CachingOptions _cachingOptions;

    public MemoryCacheService(
        IMemoryCache memoryCache,
        ICacheRegistry cacheRegistry,
        IOptions<CachingOptions> cachingOptions)
    {
        _memoryCache = memoryCache;
        _cacheRegistry = cacheRegistry;
        _cachingOptions = cachingOptions.Value;
    }

    public bool TryGetValue<T>(string key, out T? value)
    {
        if (_memoryCache.TryGetValue(key, out var cachedValue) && cachedValue is T typedValue)
        {
            value = typedValue;
            return true;
        }

        value = default;
        return false;
    }

    public bool CanCache<T>(T value)
    {
        var entrySize = CalculateEntrySize(value);
        var maxEntryBytes = Math.Max(1, _cachingOptions.MaxEntrySizeKb) * 1024;
        return entrySize <= maxEntryBytes;
    }

    public void Set<T>(
        string key,
        T value,
        TimeSpan slidingExpiration,
        TimeSpan? absoluteExpirationRelativeToNow,
        IReadOnlyCollection<string> prefixes)
    {
        var entrySize = CalculateEntrySize(value);
        var entryOptions = new MemoryCacheEntryOptions
        {
            SlidingExpiration = slidingExpiration
        };

        if (absoluteExpirationRelativeToNow.HasValue)
        {
            entryOptions.AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow;
        }

        entryOptions.SetSize(entrySize);
        entryOptions.RegisterPostEvictionCallback((evictedKey, _, _, _) =>
        {
            if (evictedKey is string keyString)
            {
                _cacheRegistry.Unregister(keyString);
            }
        });

        _memoryCache.Set(key, value, entryOptions);
        _cacheRegistry.Register(key, prefixes);
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
        _cacheRegistry.Unregister(key);
    }

    public int RemoveByPrefix(string prefix)
    {
        var keys = _cacheRegistry.GetKeysByPrefix(prefix);
        foreach (var key in keys)
        {
            _memoryCache.Remove(key);
            _cacheRegistry.Unregister(key);
        }

        return keys.Count;
    }

    private static long CalculateEntrySize<T>(T value)
    {
        if (value == null)
        {
            return 1;
        }

        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
            return Math.Max(1, bytes.LongLength);
        }
        catch
        {
            return 1024;
        }
    }
}
