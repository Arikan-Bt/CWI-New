namespace CWI.Application.Interfaces.Services;

public interface ICacheRegistry
{
    void Register(string key, IReadOnlyCollection<string> prefixes);
    void Unregister(string key);
    IReadOnlyCollection<string> GetKeysByPrefix(string prefix);
}
