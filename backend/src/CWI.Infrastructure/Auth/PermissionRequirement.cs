using Microsoft.AspNetCore.Authorization;

namespace CWI.Infrastructure.Auth;

/// <summary>
/// Yetki gereksinimi
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}
