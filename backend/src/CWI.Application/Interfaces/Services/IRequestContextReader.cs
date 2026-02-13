using CWI.Application.Common.Logging;

namespace CWI.Application.Interfaces.Services;

public interface IRequestContextReader
{
    Task<RequestContextSnapshot> ReadAsync(CancellationToken cancellationToken = default);
}
