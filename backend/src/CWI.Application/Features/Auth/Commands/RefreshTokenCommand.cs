using CWI.Application.Common.Models;
using CWI.Application.DTOs.Auth;
using CWI.Application.Interfaces.Services;
using MediatR;

namespace CWI.Application.Features.Auth.Commands;

/// <summary>
/// Token yenileme komutu
/// </summary>
public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<LoginResponse>>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    private readonly IAuthService _authService;

    public RefreshTokenCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(command.RefreshToken);
            return Result<LoginResponse>.Succeed(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result<LoginResponse>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<LoginResponse>.Failure("Token yenileme sırasında bir hata oluştu: " + ex.Message);
        }
    }
}
