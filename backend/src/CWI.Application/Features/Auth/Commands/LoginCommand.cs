using CWI.Application.Common.Models;
using CWI.Application.DTOs.Auth;
using CWI.Application.Interfaces.Services;
using MediatR;

namespace CWI.Application.Features.Auth.Commands;

/// <summary>
/// Kullanıcı girişi komutu
/// </summary>
public record LoginCommand(LoginRequest Request) : IRequest<Result<LoginResponse>>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.LoginAsync(command.Request);
            return Result<LoginResponse>.Succeed(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result<LoginResponse>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<LoginResponse>.Failure("Giriş işlemi sırasında bir hata oluştu: " + ex.Message);
        }
    }
}
