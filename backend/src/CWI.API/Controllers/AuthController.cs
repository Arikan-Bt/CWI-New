using System.Security.Claims;
using CWI.Application.Common.Models;
using CWI.Application.DTOs.Auth;
using CWI.Application.Features.Auth.Commands;
using CWI.Application.Features.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CWI.API.Controllers;

/// <summary>
/// Kimlik doğrulama API'leri
/// </summary>
[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Kullanıcı girişi yapar ve JWT token döndürür
    /// </summary>
    /// <param name="request">Giriş isteği</param>
    /// <returns>Başarılıysa token ve kullanıcı bilgileri</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(Result<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<LoginResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _mediator.Send(new LoginCommand(request));
        
        if (!result.Success)
            return Unauthorized(result);
            
        return Ok(result);
    }

    /// <summary>
    /// Yenileme token'ı kullanarak yeni bir JWT token alır
    /// </summary>
    /// <param name="request">Yenileme token isteği</param>
    /// <returns>Yeni token ve kullanıcı bilgileri</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(Result<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<LoginResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken));
        
        if (!result.Success)
            return Unauthorized(result);
            
        return Ok(result);
    }

    /// <summary>
    /// Mevcut kullanıcının şifresini değiştirir
    /// </summary>
    /// <param name="request">Şifre değiştirme bilgileri</param>
    /// <returns>İşlem sonucu</returns>
    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            return Unauthorized(Result.Failure("User ID not found."));

        var result = await _mediator.Send(new ChangePasswordCommand(userId, request));
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }

    /// <summary>
    /// Mevcut giriş yapmış kullanıcının bilgilerini getirir
    /// </summary>
    /// <returns>Kullanıcı bilgileri</returns>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(Result<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<UserDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            return Unauthorized(Result<UserDto>.Failure("User ID not found."));

        var result = await _mediator.Send(new GetCurrentUserQuery(userId));
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }


    /// <summary>
    /// Profil bilgilerini günceller
    /// </summary>
    /// <param name="request">Profil güncelleme bilgileri</param>
    /// <returns>Güncel kullanıcı bilgileri</returns>
    [Authorize]
    [HttpPut("profile")]
    [ProducesResponseType(typeof(Result<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<UserDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            return Unauthorized(Result<UserDto>.Failure("User ID not found."));

        var result = await _mediator.Send(new UpdateProfileCommand(userId, request));
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }
}
