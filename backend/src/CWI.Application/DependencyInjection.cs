using FluentValidation;
using CWI.Application.Common.Behaviors;
using MediatR;
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
    /// <returns>Service collection</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        // MediatR kaydı
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(UnhandledExceptionLoggingBehavior<,>));
        });
        
        // FluentValidation kaydı
        services.AddValidatorsFromAssembly(assembly);
        
        return services;
    }
}
