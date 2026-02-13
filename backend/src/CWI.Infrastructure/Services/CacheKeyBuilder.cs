using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CWI.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace CWI.Infrastructure.Services;

public class CacheKeyBuilder : ICacheKeyBuilder
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CacheKeyBuilder(ICurrentUserService currentUserService, IHttpContextAccessor httpContextAccessor)
    {
        _currentUserService = currentUserService;
        _httpContextAccessor = httpContextAccessor;
    }

    public string BuildScopedKey(string featureKey, object request, bool isUserScoped = true)
    {
        var userId = isUserScoped ? _currentUserService.UserId?.ToString() ?? "anon" : "global";
        var projectType = isUserScoped ? _currentUserService.ProjectType?.ToString() ?? "none" : "global";
        var role = isUserScoped ? GetRole() : "global";
        var requestHash = ComputeRequestHash(request);

        return $"cwi:{featureKey}:v1:{projectType}:{role}:{userId}:{requestHash}";
    }

    private string GetRole()
    {
        if (_currentUserService.IsAdministrator)
        {
            return "admin";
        }

        var roleClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("Role")?.Value;
        return string.IsNullOrWhiteSpace(roleClaim) ? "user" : roleClaim.Trim().ToLowerInvariant();
    }

    private static string ComputeRequestHash(object request)
    {
        var normalized = JsonSerializer.Serialize(request);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
