using CWI.Application.DTOs.Roles;
using CWI.Domain.Constants;
using MediatR;
using System.Reflection;

namespace CWI.Application.Features.Roles.Queries.GetPermissions;

public record GetPermissionsQuery : IRequest<List<PermissionGroupDto>>;

public class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, List<PermissionGroupDto>>
{
    public Task<List<PermissionGroupDto>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        var groups = new List<PermissionGroupDto>();

        // Permissions sınıfındaki iç sınıfları dolaşarak yetkileri topla
        var nestedTypes = typeof(Permissions).GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

        foreach (var type in nestedTypes)
        {
            var group = new PermissionGroupDto
            {
                Name = type.Name,
                Permissions = new List<PermissionDto>()
            };

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            foreach (var field in fields)
            {
                if (field.IsLiteral && !field.IsInitOnly)
                {
                    var value = field.GetValue(null)?.ToString() ?? string.Empty;
                    var rawAction = field.Name;
                    var action = rawAction.Replace("_", " "); // Underscore'ları boşluğa çevir
                    var module = type.Name;
                    
                    var description = module == "Menus" 
                        ? $"Allows access to the {action} menu."
                        : $"Allows {action.ToLower()} access for the {module} module.";

                    group.Permissions.Add(new PermissionDto
                    {
                        Key = value,
                        Name = action,
                        GroupName = module,
                        Description = description
                    });
                }
            }

            groups.Add(group);
        }

        return Task.FromResult(groups);
    }
}
