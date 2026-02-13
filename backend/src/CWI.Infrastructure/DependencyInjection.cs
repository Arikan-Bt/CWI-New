using CWI.Application.Interfaces.Repositories;
using CWI.Infrastructure.Persistence;
using CWI.Infrastructure.Repositories;
using CWI.Infrastructure.Services;
using CWI.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using CWI.Infrastructure.Auth;

namespace CWI.Infrastructure;

/// <summary>
/// Infrastructure katmanı için Dependency Injection kayıtları
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Infrastructure servislerini kaydeder
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext kaydı
        services.AddDbContext<CWIDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(CWIDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                    // Karmaşık raporlama sorguları için timeout süresini artır
                    sqlOptions.CommandTimeout(120);
                });
        });
        
        // Repository ve UnitOfWork kayıtları
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Auth servisleri
        services.AddScoped<Application.Interfaces.Services.IAuthService, Auth.AuthService>();
        services.AddScoped<Application.Interfaces.Services.ICurrentUserService, Services.CurrentUserService>();
        services.AddScoped<ISensitiveDataMasker, SensitiveDataMasker>();
        services.AddScoped<IRequestContextReader, HttpRequestContextReader>();
        services.AddScoped<IErrorLogWriter, ErrorLogWriter>();
        services.AddScoped<ICacheKeyBuilder, CacheKeyBuilder>();
        services.AddSingleton<ICacheRegistry, InMemoryCacheRegistry>();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        
        // Authorization
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddHostedService<ErrorLogRetentionBackgroundService>();
        
        return services;
    }
}
