using CWI.Application.Common.Models;
using CWI.Application.DTOs.Auth;
using CWI.Application.Interfaces.Services;
using MediatR;

namespace CWI.Application.Features.Auth.Queries;

/// <summary>
/// Mevcut kullanıcı bilgilerini getirme sorgusu
/// </summary>
public record GetCurrentUserQuery(int UserId) : IRequest<Result<UserDto>>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
{
    private readonly IAuthService _authService;

    public GetCurrentUserQueryHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _authService.GetCurrentUserAsync(query.UserId);
            return Result<UserDto>.Succeed(user);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure("Kullanıcı bilgileri alınamadı: " + ex.Message);
        }
    }
}
