using CWI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CWI.Infrastructure.Services;

public class ErrorLogRetentionBackgroundService : BackgroundService
{
    private static readonly TimeSpan RunInterval = TimeSpan.FromHours(24);
    private static readonly TimeSpan InitialDelay = TimeSpan.FromMinutes(2);
    private const int RetentionDays = 180;

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ErrorLogRetentionBackgroundService> _logger;

    public ErrorLogRetentionBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ErrorLogRetentionBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(InitialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<CWIDbContext>();
                var cutoffDate = DateTime.UtcNow.AddDays(-RetentionDays);

                var deletedCount = await dbContext.ErrorLogs
                    .Where(x => x.OccurredAt < cutoffDate)
                    .ExecuteDeleteAsync(stoppingToken);

                _logger.LogInformation("ErrorLog retention çalıştı. Silinen kayıt sayısı: {DeletedCount}", deletedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ErrorLog retention çalışırken hata oluştu.");
            }

            await Task.Delay(RunInterval, stoppingToken);
        }
    }
}
