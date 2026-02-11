using CWI.Application.Common.Models;
using CWI.Application.DTOs.Auth;
using CWI.Application.Interfaces.Services;
using MediatR;

namespace CWI.Application.Features.Auth.Commands;

/// <summary>
/// Profil güncelleme komutu
/// </summary>
public record UpdateProfileCommand(int UserId, UpdateProfileRequest Request) : IRequest<Result<UserDto>>;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<UserDto>>
{
    private readonly IAuthService _authService;

    public UpdateProfileCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<UserDto>> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var userDto = await _authService.UpdateProfileAsync(command.UserId, command.Request);
            return Result<UserDto>.Succeed(userDto);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure("Profil güncellenirken bir hata oluştu: " + ex.Message);
        }
    }
}
