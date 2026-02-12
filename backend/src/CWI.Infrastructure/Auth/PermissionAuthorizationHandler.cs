using Microsoft.AspNetCore.Authorization;

namespace CWI.Infrastructure.Auth;

/// <summary>
/// Yetki gereksinimi kontrolcüsü
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User == null)
        {
            return Task.CompletedTask;
        }

        // Kullanıcının "IsAdministrator" claim'i varsa tüm yetkilere sahiptir
        var isAdminClaim = context.User.FindFirst("IsAdministrator");
        if (isAdminClaim != null && bool.TryParse(isAdminClaim.Value, out bool isAdmin) && isAdmin)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // "Permission" claim'leri arasında gereksinim duyulan yetki var mı kontrol et
        var permissions = context.User.FindAll("Permission").Select(x => x.Value);

        if (permissions.Any(x => x == requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
