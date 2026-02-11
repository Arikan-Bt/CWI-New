namespace CWI.Application.DTOs.Auth;

/// <summary>
/// Giriş isteği için veri transfer nesnesi
/// </summary>
public class LoginRequest
{
    public string UserCode { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Yenileme token'ı isteği
/// </summary>
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Giriş yanıtı için veri transfer nesnesi
/// </summary>
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public UserDto User { get; set; } = null!;
}

/// <summary>
/// Şifre değiştirme isteği
/// </summary>
public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Kullanıcı veri transfer nesnesi
/// </summary>
public class UserDto
{
    public int Id { get; set; }
    public string UserCode { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? ProjectType { get; set; }
    public string? RoleName { get; set; }
    public int? LinkedCustomerId { get; set; }
    public bool IsAdministrator { get; set; }
    public List<string> Permissions { get; set; } = new List<string>();
}

/// <summary>
/// Profil güncelleme isteği
/// </summary>
public class UpdateProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
