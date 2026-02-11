using CWI.Domain.Enums;

namespace CWI.Application.DTOs.Users;

public record UserDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Surname { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? ClientCode { get; init; }
    public string? MobilePhone { get; init; }
    public string? CurrentAccount { get; init; }
    public int RoleId { get; init; }
    public ProjectType ProjectType { get; init; }
    public List<int> AllowedBrands { get; init; } = new();
    public List<int> RestrictedBrands { get; init; } = new();
    public List<string> AllowedProducts { get; init; } = new();
    public List<string> BlockedProducts { get; init; } = new();
}

public record CreateUserRequest
{
    public string Name { get; init; } = string.Empty;
    public string Surname { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public int RoleId { get; init; }
    public string Status { get; init; } = "Active";
    public string? ClientCode { get; init; }
    public string? MobilePhone { get; init; }
    public string? CurrentAccount { get; init; }
    public List<int> AllowedBrands { get; init; } = new();
    public List<int> RestrictedBrands { get; init; } = new();
    public List<string> AllowedProducts { get; init; } = new();
    public List<string> BlockedProducts { get; init; } = new();
}

public record UpdateUserRequest : CreateUserRequest
{
    public int Id { get; init; }
    public new string? Password { get; init; } // Optional on update
}

public record UserPagedResult<T>
{
    public List<T> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public record UserActivityDto
{
    public DateTime Date { get; init; }
    public string Operation { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}

