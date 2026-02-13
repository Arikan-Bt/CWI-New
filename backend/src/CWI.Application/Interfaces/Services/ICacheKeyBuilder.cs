namespace CWI.Application.Interfaces.Services;

public interface ICacheKeyBuilder
{
    string BuildScopedKey(string featureKey, object request, bool isUserScoped = true);
}
