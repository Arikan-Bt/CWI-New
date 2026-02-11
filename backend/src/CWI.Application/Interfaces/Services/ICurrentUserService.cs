using CWI.Domain.Enums;

namespace CWI.Application.Interfaces.Services;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? UserName { get; }
    int? LinkedCustomerId { get; }
    ProjectType? ProjectType { get; }
    bool IsAdministrator { get; }
}
