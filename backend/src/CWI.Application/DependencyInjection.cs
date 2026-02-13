using CWI.Application.Common.Behaviors;
using CWI.Application.Common.Caching;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CWI.Application;

/// <summary>
/// Application katmanı için Dependency Injection kayıtları
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Application servislerini kaydeder
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.Configure<CachingOptions>(configuration.GetSection("Caching"));

        // MediatR kaydı
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
            cfg.AddOpenBehavior(typeof(CacheInvalidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(UnhandledExceptionLoggingBehavior<,>));
        });

        // FluentValidation kaydı
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
