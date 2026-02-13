namespace CWI.Application.Common.Logging;

public sealed record RequestContextSnapshot
{
    public string TraceId { get; init; } = string.Empty;
    public int? UserId { get; init; }
    public string? UserName { get; init; }
    public string? IpAddress { get; init; }
    public string? RequestUrl { get; init; }
    public string? HttpMethod { get; init; }
    public string? RequestQuery { get; init; }
    public string? RequestRouteValues { get; init; }
    public string? RequestHeaders { get; init; }
    public string? RequestContentType { get; init; }
    public long? RequestContentLength { get; init; }
    public string? RequestBodyMasked { get; init; }
}
