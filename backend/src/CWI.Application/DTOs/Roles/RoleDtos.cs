namespace CWI.Application.DTOs.Roles;

public record RoleDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public int UserCount { get; init; }
}

public record RoleDetailDto : RoleDto
{
    public List<string> Permissions { get; init; } = new();
    public List<RoleUserDto> Users { get; init; } = new();
}

public record RoleUserDto
{
    public int Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

public record CreateRoleDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public List<string> Permissions { get; init; } = new();
}

public record UpdateRoleDto : CreateRoleDto
{
    public int Id { get; init; }
}

public record PermissionDto
{
    public string Key { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string GroupName { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public record PermissionGroupDto
{
    public string Name { get; init; } = string.Empty;
    public List<PermissionDto> Permissions { get; init; } = new();
}
