using System.Net;
using CWI.Application.Common.Logging;
using CWI.Application.Interfaces.Services;
using FluentValidation;

namespace CWI.API.Middlewares;

/// <summary>
/// Tüm uygulamadaki hatalarý yakalayan merkezi middleware
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
        httpContext.Request.EnableBuffering();

        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bir hata oluþtu: {Message}", ex.Message);
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = MapStatusCode(exception);

        var responseMessage = context.Response.StatusCode == (int)HttpStatusCode.InternalServerError
            ? "Beklenmeyen bir sunucu hatasý oluþtu."
            : exception.Message;

        if (!IsAlreadyLogged(context))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var contextReader = scope.ServiceProvider.GetRequiredService<IRequestContextReader>();
                var errorLogWriter = scope.ServiceProvider.GetRequiredService<IErrorLogWriter>();
                var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

                var requestSnapshot = await contextReader.ReadAsync(context.RequestAborted);

                var model = new ErrorLogCreateModel
                {
                    Message = exception.Message,
                    StackTrace = exception.StackTrace,
                    InnerException = exception.InnerException?.Message,
                    Source = exception.Source,
                    TraceId = requestSnapshot.TraceId,
                    UserId = requestSnapshot.UserId,
                    UserName = requestSnapshot.UserName,
                    IpAddress = requestSnapshot.IpAddress,
                    RequestUrl = requestSnapshot.RequestUrl,
                    HttpMethod = requestSnapshot.HttpMethod,
                    RequestBody = requestSnapshot.RequestBodyMasked,
                    ExceptionType = exception.GetType().FullName ?? exception.GetType().Name,
                    Target = exception.TargetSite?.DeclaringType != null
                        ? $"{exception.TargetSite.DeclaringType.FullName}.{exception.TargetSite.Name}"
                        : exception.TargetSite?.Name,
                    RequestQuery = requestSnapshot.RequestQuery,
                    RequestRouteValues = requestSnapshot.RequestRouteValues,
                    RequestHeaders = requestSnapshot.RequestHeaders,
                    RequestContentType = requestSnapshot.RequestContentType,
                    RequestContentLength = requestSnapshot.RequestContentLength,
                    RequestBodyMasked = requestSnapshot.RequestBodyMasked,
                    ErrorCode = MapErrorCode(exception),
                    ParameterName = MapParameterName(exception),
                    Environment = environment.EnvironmentName,
                    MachineName = Environment.MachineName,
                    OccurredAt = DateTime.UtcNow
                };

                await errorLogWriter.WriteAsync(model, context.RequestAborted);
                MarkAsLogged(context);
            }
            catch (Exception logEx)
            {
                _logger.LogCritical(logEx, "Hata logu veritabanýna yazýlýrken hata oluþtu.");
            }
        }

        var response = new ErrorDetails
        {
            StatusCode = context.Response.StatusCode,
            Message = responseMessage,
            DetailedMessage = exception.Message,
            TraceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(response.ToString());
    }

    private static int MapStatusCode(Exception exception)
    {
        return exception switch
        {
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            ValidationException => (int)HttpStatusCode.BadRequest,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }

    private static string MapErrorCode(Exception exception)
    {
        return exception switch
        {
            InvalidOperationException => "INVALID_OPERATION",
            KeyNotFoundException => "NOT_FOUND",
            UnauthorizedAccessException => "UNAUTHORIZED",
            ValidationException => "VALIDATION_ERROR",
            ArgumentException => "INVALID_ARGUMENT",
            _ => "UNHANDLED_EXCEPTION"
        };
    }

    private static string? MapParameterName(Exception exception)
    {
        return exception switch
        {
            ArgumentException argumentException => argumentException.ParamName,
            ValidationException validationException => validationException.Errors.FirstOrDefault()?.PropertyName,
            _ => null
        };
    }

    private static bool IsAlreadyLogged(HttpContext context)
    {
        return context.Items.TryGetValue(ErrorLogConstants.AlreadyLoggedHttpContextItemKey, out var alreadyLogged) &&
               alreadyLogged is true;
    }

    private static void MarkAsLogged(HttpContext context)
    {
        context.Items[ErrorLogConstants.AlreadyLoggedHttpContextItemKey] = true;
    }
}
