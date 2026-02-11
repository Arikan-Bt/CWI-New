using CWI.Application.DTOs.Auth;

namespace CWI.Application.Interfaces.Services;

/// <summary>
/// Kimlik doğrulama servis arayüzü
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Kullanıcı girişi yapar ve JWT token döner
    /// </summary>
    Task<LoginResponse> LoginAsync(LoginRequest request);
    
    /// <summary>
    /// Token yeniler
    /// </summary>
    Task<LoginResponse> RefreshTokenAsync(string refreshToken);
    
    /// <summary>
    /// Şifre değiştirir
    /// </summary>
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    
    /// <summary>
    /// Şifre hash'ler
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Mevcut kullanıcı bilgilerini getirir
    /// </summary>
    /// <summary>
    /// Mevcut kullanıcı bilgilerini getirir
    /// </summary>
    Task<UserDto> GetCurrentUserAsync(int userId);

    /// <summary>
    /// Profil bilgilerini günceller
    /// </summary>
    Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileRequest request);
}
