using System.Security.Claims;
using CWI.Application.Interfaces.Services;
using CWI.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace CWI.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }
    }

    public string? UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name;

    public int? LinkedCustomerId
    {
        get
        {
            var claimValue = _httpContextAccessor.HttpContext?.User?.FindFirstValue("LinkedCustomerId");
            if (int.TryParse(claimValue, out var customerId))
            {
                return customerId;
            }
            return null;
        }
    }

    public ProjectType? ProjectType
    {
        get
        {
            var claimValue = _httpContextAccessor.HttpContext?.User?.FindFirstValue("ProjectType");
            if (Enum.TryParse<ProjectType>(claimValue, out var projectType))
            {
                return projectType;
            }
            return null;
        }
    }

    public bool IsAdministrator
    {
        get
        {
            var roleClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("Role");
            var isAdminClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("IsAdministrator");

            if (roleClaim != null && (roleClaim.Equals("Administrators", StringComparison.OrdinalIgnoreCase) || 
                                     roleClaim.Equals("Administrator", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            if (isAdminClaim != null && isAdminClaim.Equals("True", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
