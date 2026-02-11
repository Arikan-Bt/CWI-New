using CWI.Application.Common.Models;
using CWI.Application.DTOs.Auth;
using CWI.Application.Interfaces.Services;
using MediatR;

namespace CWI.Application.Features.Auth.Commands;

/// <summary>
/// Şifre değiştirme komutu
/// </summary>
public record ChangePasswordCommand(int UserId, ChangePasswordRequest Request) : IRequest<Result>;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IAuthService _authService;

    public ChangePasswordCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var success = await _authService.ChangePasswordAsync(command.UserId, command.Request);
            return success ? Result.Succeed() : Result.Failure("Password could not be changed.");
        }
        catch (Exception ex)
        {
            return Result.Failure("An error occurred while changing the password: " + ex.Message);
        }
    }
}
