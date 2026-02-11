using System.Net;
using CWI.Domain.Entities.System;
using CWI.Application.Interfaces.Repositories;

namespace CWI.API.Middlewares;

/// <summary>
/// Tüm uygulamadaki hataları yakalayan merkezi middleware
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IServiceProvider serviceProvider)
    {
        _next = next;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bir hata oluştu: {Message}", ex.Message);
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        // Özel hata tiplerine göre status code belirlenebilir (örn. NotFoundException -> 404)
        var message = "An unexpected error occurred on the server.";
        
        // Hatayı veritabanına logla
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            
            var errorLog = new ErrorLog
            {
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                InnerException = exception.InnerException?.Message,
                Source = exception.Source,
                RequestUrl = context.Request.Path,
                HttpMethod = context.Request.Method,
                OccurredAt = DateTime.UtcNow,
                IsResolved = false
            };
            
            await unitOfWork.Repository<ErrorLog, long>().AddAsync(errorLog);
            await unitOfWork.SaveChangesAsync();
        }
        catch (Exception logEx)
        {
            // Loglama sırasında hata oluşursa konsola yaz
            _logger.LogCritical(logEx, "An error occurred while logging the error to the database!");
        }

        var response = new ErrorDetails
        {
            StatusCode = context.Response.StatusCode,
            Message = message,
            DetailedMessage = exception.Message // Production ortamında gizlenmelidir
        };

        await context.Response.WriteAsync(response.ToString());
    }
}
