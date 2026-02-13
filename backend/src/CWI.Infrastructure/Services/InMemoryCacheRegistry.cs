using System.Collections.Concurrent;
using CWI.Application.Interfaces.Services;

namespace CWI.Infrastructure.Services;

public class InMemoryCacheRegistry : ICacheRegistry
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _prefixToKeys = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _keyToPrefixes = new(StringComparer.Ordinal);

    public void Register(string key, IReadOnlyCollection<string> prefixes)
    {
        if (prefixes.Count == 0)
        {
            return;
        }

        var keyPrefixes = _keyToPrefixes.GetOrAdd(key, _ => new ConcurrentDictionary<string, byte>(StringComparer.Ordinal));
        foreach (var prefix in prefixes)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                continue;
            }

            var normalizedPrefix = prefix.Trim();
            var keys = _prefixToKeys.GetOrAdd(normalizedPrefix, _ => new ConcurrentDictionary<string, byte>(StringComparer.Ordinal));
            keys[key] = 0;
            keyPrefixes[normalizedPrefix] = 0;
        }
    }

    public void Unregister(string key)
    {
        if (!_keyToPrefixes.TryRemove(key, out var prefixes))
        {
            return;
        }

        foreach (var prefix in prefixes.Keys)
        {
            if (_prefixToKeys.TryGetValue(prefix, out var keys))
            {
                keys.TryRemove(key, out _);
                if (keys.IsEmpty)
                {
                    _prefixToKeys.TryRemove(prefix, out _);
                }
            }
        }
    }

    public IReadOnlyCollection<string> GetKeysByPrefix(string prefix)
    {
        if (!_prefixToKeys.TryGetValue(prefix, out var keys))
        {
            return [];
        }

        return keys.Keys.ToList();
    }
}
