using System.Text.Json;
using CWI.Application.Common.Logging;
using CWI.Application.Interfaces.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CWI.Application.Common.Behaviors;

public class UnhandledExceptionLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IErrorLogWriter _errorLogWriter;
    private readonly IRequestContextReader _requestContextReader;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISensitiveDataMasker _sensitiveDataMasker;

    public UnhandledExceptionLoggingBehavior(
        IErrorLogWriter errorLogWriter,
        IRequestContextReader requestContextReader,
        IHttpContextAccessor httpContextAccessor,
        ISensitiveDataMasker sensitiveDataMasker)
    {
        _errorLogWriter = errorLogWriter;
        _requestContextReader = requestContextReader;
        _httpContextAccessor = httpContextAccessor;
        _sensitiveDataMasker = sensitiveDataMasker;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null &&
                httpContext.Items.TryGetValue(ErrorLogConstants.AlreadyLoggedHttpContextItemKey, out var alreadyLogged) &&
                alreadyLogged is true)
            {
                throw;
            }

            var snapshot = await _requestContextReader.ReadAsync(cancellationToken);
            var requestPayload = SerializeRequest(request);

            var model = new ErrorLogCreateModel
            {
                Message = ex.Message,
                StackTrace = ex.StackTrace,
                InnerException = ex.InnerException?.Message,
                Source = ex.Source,
                TraceId = snapshot.TraceId,
                UserId = snapshot.UserId,
                UserName = snapshot.UserName,
                IpAddress = snapshot.IpAddress,
                RequestUrl = snapshot.RequestUrl,
                HttpMethod = snapshot.HttpMethod,
                RequestBody = requestPayload,
                ExceptionType = ex.GetType().FullName ?? ex.GetType().Name,
                Target = ex.TargetSite?.DeclaringType != null
                    ? $"{ex.TargetSite.DeclaringType.FullName}.{ex.TargetSite.Name}"
                    : ex.TargetSite?.Name,
                RequestQuery = snapshot.RequestQuery,
                RequestRouteValues = snapshot.RequestRouteValues,
                RequestHeaders = snapshot.RequestHeaders,
                RequestContentType = snapshot.RequestContentType,
                RequestContentLength = snapshot.RequestContentLength,
                RequestBodyMasked = snapshot.RequestBodyMasked ?? requestPayload,
                ErrorCode = MapErrorCode(ex),
                ParameterName = MapParameterName(ex),
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                MachineName = Environment.MachineName,
                OccurredAt = DateTime.UtcNow
            };

            await _errorLogWriter.WriteAsync(model, cancellationToken);

            if (httpContext != null)
            {
                httpContext.Items[ErrorLogConstants.AlreadyLoggedHttpContextItemKey] = true;
            }

            throw;
        }
    }

    private string? SerializeRequest(TRequest request)
    {
        try
        {
            var raw = JsonSerializer.Serialize(request);
            return _sensitiveDataMasker.MaskJson(raw);
        }
        catch
        {
            return null;
        }
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
}
