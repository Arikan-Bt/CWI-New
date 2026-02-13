namespace CWI.Application.DTOs.System;

public sealed record ErrorLogFilterRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public bool? IsResolved { get; init; }
    public int? UserId { get; init; }
    public string? RequestUrl { get; init; }
    public string? ExceptionType { get; init; }
    public string? SearchTerm { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public sealed record ResolveErrorLogRequest
{
    public string? ResolutionNote { get; init; }
}

public sealed record ErrorLogListItemDto
{
    public long Id { get; init; }
    public DateTime OccurredAt { get; init; }
    public bool IsResolved { get; init; }
    public string Message { get; init; } = string.Empty;
    public string ExceptionType { get; init; } = string.Empty;
    public string? ErrorCode { get; init; }
    public string TraceId { get; init; } = string.Empty;
    public int? UserId { get; init; }
    public string? UserName { get; init; }
    public string? HttpMethod { get; init; }
    public string? RequestUrl { get; init; }
}

public sealed record ErrorLogDetailDto
{
    public long Id { get; init; }
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
    public DateTime OccurredAt { get; init; }
    public bool IsResolved { get; init; }
    public string? ResolutionNote { get; init; }
    public DateTime? ResolvedAt { get; init; }
    public int? ResolvedByUserId { get; init; }
}
