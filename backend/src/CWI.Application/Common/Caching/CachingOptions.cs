namespace CWI.Application.Common.Caching;

public class CachingOptions
{
    public bool Enabled { get; set; } = false;
    public int DefaultSlidingSeconds { get; set; } = 120;
    public int DefaultAbsoluteSeconds { get; set; } = 300;
    public long MemorySizeLimitMb { get; set; } = 64;
    public long MaxEntrySizeKb { get; set; } = 128;
    public double CompactionPercentage { get; set; } = 0.2;
}
