using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CWI.Application.DTOs.Auth;
using CWI.Application.Interfaces.Repositories;
using CWI.Application.Interfaces.Services;
using CWI.Domain.Entities.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;


namespace CWI.Infrastructure.Auth;

/// <summary>
/// Kimlik doğrulama servis uygulaması
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }


    /// <inheritdoc/>
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // Şifre hash'ini oluştur
        var passwordHash = HashPasswordInternal(request.Password);

        // Kullanıcıyı bul (UserName veya UserCode olarak arıyoruz)
        // Include(u => u.Role) INNER JOIN yaptığı için eğer RoleId veritabanında karşılığı yoksa kullanıcıyı filtreliyordu.
        // Bu yüzden önce kullanıcıyı çekip sonra rolü yüklemek daha güvenli.
        var user = await _unitOfWork.Repository<User>().AsQueryable()
            .FirstOrDefaultAsync(u => u.UserName == request.UserCode && u.IsActive);
        
        // GEÇİCİ: berksimsek için şifre kontrolünü bypass et
        bool isPasswordCorrect = user != null && (user.PasswordHash == passwordHash || (user.UserName == "berksimsek" && request.Password == "Bs12345"));

        if (user == null || !isPasswordCorrect)
            throw new UnauthorizedAccessException("Geçersiz kullanıcı adı veya şifre.");

        // Rol ve yetkilerini ayrıca yükle (Explicit Loading)
        if (user.RoleId > 0)
        {
            var role = await _unitOfWork.Repository<Role>().AsQueryable()
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == user.RoleId && r.IsActive);
            
            if (role != null)
            {
                user.Role = role;
            }
        }

        // Token oluştur
        var expireMinutes = Convert.ToInt32(_configuration["Jwt:ExpireHours"] ?? "8") * 60;
        var tokenResponse = GenerateJwtToken(user, expireMinutes);
        var refreshToken = GenerateRefreshToken();

        // Refresh token bilgilerini güncelle
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        
        _unitOfWork.Repository<User>().Update(user);
        
        // Login geçmişini kaydet
        var httpContext = _httpContextAccessor.HttpContext;
        var loginHistory = new UserLoginHistory
        {
            UserId = user.Id,
            LoginAt = DateTime.UtcNow,
            IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString(),
            IsSuccessful = true
        };
        _unitOfWork.Repository<UserLoginHistory, long>().Add(loginHistory);

        await _unitOfWork.SaveChangesAsync();
        
        return new LoginResponse
        {
            Token = tokenResponse.Token,
            RefreshToken = refreshToken,
            ExpiresIn = expireMinutes,
            User = MapToUserDto(user)
        };
    }


    /// <inheritdoc/>
    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
    {
        var user = await _unitOfWork.Repository<User>().AsQueryable()
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.IsActive);

        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Geçersiz veya süresi dolmuş yenileme token'ı.");

        // Rol ve yetkilerini ayrıca yükle
        if (user.RoleId > 0)
        {
            var role = await _unitOfWork.Repository<Role>().AsQueryable()
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == user.RoleId && r.IsActive);
            
            if (role != null)
            {
                user.Role = role;
            }
        }

        // Yeni token'ları oluştur
        var expireMinutes = Convert.ToInt32(_configuration["Jwt:ExpireHours"] ?? "8") * 60;
        var tokenResponse = GenerateJwtToken(user, expireMinutes);
        var newRefreshToken = GenerateRefreshToken();

        // Refresh token bilgilerini güncelle
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        
        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync();

        return new LoginResponse
        {
            Token = tokenResponse.Token,
            RefreshToken = newRefreshToken,
            ExpiresIn = expireMinutes,
            User = MapToUserDto(user)
        };
    }

    /// <inheritdoc/>
    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null) return false;

        // Mevcut şifreyi doğrula
        var currentPasswordHash = HashPasswordInternal(request.CurrentPassword);
        if (user.PasswordHash != currentPasswordHash)
            throw new Exception("Mevcut şifre hatalı.");

        if (request.NewPassword != request.ConfirmPassword)
            throw new Exception("Şifreler uyuşmuyor.");

        // Yeni şifreyi kaydet
        user.PasswordHash = HashPasswordInternal(request.NewPassword);
        _unitOfWork.Repository<User>().Update(user);
        return await _unitOfWork.SaveChangesAsync() > 0;
    }

    /// <inheritdoc/>
    public string HashPassword(string password)
    {
        return HashPasswordInternal(password);
    }

    /// <inheritdoc/>
    public async Task<UserDto> GetCurrentUserAsync(int userId)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null) throw new Exception("Kullanıcı bulunamadı.");

        return MapToUserDto(user);
    }

    /// <inheritdoc/>
    public async Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null) throw new Exception("Kullanıcı bulunamadı.");

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        if (!string.IsNullOrEmpty(request.Email))
        {
            user.Email = request.Email;
        }
        
        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync();

        return MapToUserDto(user);
    }

    #region Private Methods

    private UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            UserCode = user.UserName,
            UserName = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            ProjectType = user.ProjectType.ToString(),
            RoleName = user.Role?.Name,
            LinkedCustomerId = user.LinkedCustomerId,
            IsAdministrator = user.IsAdministrator,
            Permissions = user.Role?.RolePermissions?.Select(p => p.PermissionKey).ToList() ?? new List<string>()
        };
    }

    private (string Token, DateTime Expiration) GenerateJwtToken(User user, int expireMinutes)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var keyArray = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "CWI_Secret_Key_At_Least_32_Chars_Long");
        
        var expiration = DateTime.UtcNow.AddMinutes(expireMinutes);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("FullName", user.FullName),
            new Claim("ProjectType", user.ProjectType.ToString()),
            new Claim("Role", user.Role?.Name ?? ""),
            new Claim("LinkedCustomerId", user.LinkedCustomerId?.ToString() ?? ""),
            new Claim("IsAdministrator", (user.IsAdministrator || (user.Role?.IsAdmin ?? false)).ToString())
        };

        // Yetkileri claim'lere ekle
        if (user.Role?.RolePermissions != null)
        {
            foreach (var permission in user.Role.RolePermissions)
            {
                claims.Add(new Claim("Permission", permission.PermissionKey));
            }
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyArray), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return (tokenHandler.WriteToken(token), expiration);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private static string HashPasswordInternal(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        var builder = new StringBuilder();
        foreach (var b in bytes)
        {
            builder.Append(b.ToString("X2"));
        }
        return builder.ToString();
    }

    #endregion
}
