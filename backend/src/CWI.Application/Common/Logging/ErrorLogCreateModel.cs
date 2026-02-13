namespace CWI.Application.Common.Logging;

public sealed record ErrorLogCreateModel
{
    public string Message { get; init; } = string.Empty;
    public string? StackTrace { get; init; }
    public string? InnerException { get; init; }
    public string? Source { get; init; }
    public string TraceId { get; init; } = string.Empty;
    public int? UserId { get; init; }
    public string? UserName { get; init; }
    public string? IpAddress { get; init; }
    public string? RequestUrl { get; init; }
    public string? HttpMethod { get; init; }
    public string? RequestBody { get; init; }
    public string ExceptionType { get; init; } = string.Empty;
    public string? Target { get; init; }
    public string? RequestQuery { get; init; }
    public string? RequestRouteValues { get; init; }
    public string? RequestHeaders { get; init; }
    public string? RequestContentType { get; init; }
    public long? RequestContentLength { get; init; }
    public string? RequestBodyMasked { get; init; }
    public string? ErrorCode { get; init; }
    public string? ParameterName { get; init; }
    public string? Environment { get; init; }
    public string? MachineName { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
