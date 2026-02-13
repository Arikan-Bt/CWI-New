using CWI.Application.Common.Logging;

namespace CWI.Application.Interfaces.Services;

public interface IErrorLogWriter
{
    Task<long> WriteAsync(ErrorLogCreateModel model, CancellationToken cancellationToken = default);
    Task MarkResolvedAsync(long id, int resolvedByUserId, string? note, CancellationToken cancellationToken = default);
}
