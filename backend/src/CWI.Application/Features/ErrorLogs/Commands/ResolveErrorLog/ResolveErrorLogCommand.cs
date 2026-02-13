using CWI.Application.Interfaces.Services;
using MediatR;

namespace CWI.Application.Features.ErrorLogs.Commands.ResolveErrorLog;

public record ResolveErrorLogCommand(long Id, int ResolvedByUserId, string? ResolutionNote) : IRequest<bool>;

public class ResolveErrorLogCommandHandler : IRequestHandler<ResolveErrorLogCommand, bool>
{
    private readonly IErrorLogWriter _errorLogWriter;

    public ResolveErrorLogCommandHandler(IErrorLogWriter errorLogWriter)
    {
        _errorLogWriter = errorLogWriter;
    }

    public async Task<bool> Handle(ResolveErrorLogCommand request, CancellationToken cancellationToken)
    {
        await _errorLogWriter.MarkResolvedAsync(
            request.Id,
            request.ResolvedByUserId,
            request.ResolutionNote,
            cancellationToken);

        return true;
    }
}
