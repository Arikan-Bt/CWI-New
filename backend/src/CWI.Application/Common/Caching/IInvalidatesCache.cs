namespace CWI.Application.Common.Caching;

public interface IInvalidatesCache
{
    IReadOnlyCollection<string> CachePrefixesToInvalidate { get; }
}
