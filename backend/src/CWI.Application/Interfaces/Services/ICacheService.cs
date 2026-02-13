namespace CWI.Application.Interfaces.Services;

public interface ICacheService
{
    bool TryGetValue<T>(string key, out T? value);
    bool CanCache<T>(T value);

    void Set<T>(
        string key,
        T value,
        TimeSpan slidingExpiration,
        TimeSpan? absoluteExpirationRelativeToNow,
        IReadOnlyCollection<string> prefixes);

    void Remove(string key);

    int RemoveByPrefix(string prefix);
}
