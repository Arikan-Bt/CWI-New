using System.Text;
using System.Text.Json;
using CWI.Application.Common.Logging;
using CWI.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace CWI.Infrastructure.Services;

public class HttpRequestContextReader : IRequestContextReader
{
    private static readonly string[] HeaderWhitelist =
    {
        "User-Agent",
        "Authorization",
        "X-Forwarded-For",
        "X-Request-Id",
        "Correlation-Id"
    };

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISensitiveDataMasker _sensitiveDataMasker;

    public HttpRequestContextReader(
        IHttpContextAccessor httpContextAccessor,
        ICurrentUserService currentUserService,
        ISensitiveDataMasker sensitiveDataMasker)
    {
        _httpContextAccessor = httpContextAccessor;
        _currentUserService = currentUserService;
        _sensitiveDataMasker = sensitiveDataMasker;
    }

    public async Task<RequestContextSnapshot> ReadAsync(CancellationToken cancellationToken = default)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            return new RequestContextSnapshot();
        }

        var request = context.Request;
        var queryData = request.Query.ToDictionary(
            pair => pair.Key,
            pair => (string?)pair.Value.ToString(),
            StringComparer.OrdinalIgnoreCase);
        var routeData = request.RouteValues.ToDictionary(
            pair => pair.Key,
            pair => pair.Value?.ToString(),
            StringComparer.OrdinalIgnoreCase);
        var selectedHeaders = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var headerName in HeaderWhitelist)
        {
            if (request.Headers.TryGetValue(headerName, out var headerValue))
            {
                selectedHeaders[headerName] = headerValue.ToString();
            }
        }

        var maskedQuery = _sensitiveDataMasker.MaskKeyValuePairs(queryData);
        var maskedRoute = _sensitiveDataMasker.MaskKeyValuePairs(routeData);
        var maskedHeaders = _sensitiveDataMasker.MaskKeyValuePairs(selectedHeaders);

        return new RequestContextSnapshot
        {
            TraceId = context.TraceIdentifier,
            UserId = _currentUserService.UserId,
            UserName = _currentUserService.UserName,
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            RequestUrl = $"{request.PathBase}{request.Path}",
            HttpMethod = request.Method,
            RequestQuery = JsonSerializer.Serialize(maskedQuery),
            RequestRouteValues = JsonSerializer.Serialize(maskedRoute),
            RequestHeaders = JsonSerializer.Serialize(maskedHeaders),
            RequestContentType = request.ContentType,
            RequestContentLength = request.ContentLength,
            RequestBodyMasked = await ReadRequestBodyAsync(request, cancellationToken)
        };
    }

    private async Task<string?> ReadRequestBodyAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        if (request.ContentLength == null || request.ContentLength <= 0)
        {
            return null;
        }

        request.EnableBuffering();

        if (!request.Body.CanSeek)
        {
            return null;
        }

        request.Body.Position = 0;

        using var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);
        request.Body.Position = 0;

        if (string.IsNullOrWhiteSpace(rawBody))
        {
            return null;
        }

        if (rawBody.Length > ErrorLogConstants.MaxRequestBodyLength)
        {
            rawBody = rawBody[..ErrorLogConstants.MaxRequestBodyLength] + "...[TRUNCATED]";
        }

        return _sensitiveDataMasker.MaskJson(rawBody);
    }
}
