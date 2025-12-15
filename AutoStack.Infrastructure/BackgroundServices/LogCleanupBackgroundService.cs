using AutoStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoStack.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that runs daily to delete audit logs older than configured retention period (GDPR compliance)
/// Runs at 2 AM UTC every day
/// </summary>
public class LogCleanupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LogCleanupBackgroundService> _logger;
    private readonly IConfiguration _configuration;

    public LogCleanupBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<LogCleanupBackgroundService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Log cleanup background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 2 AM UTC
                var now = DateTime.UtcNow;
                var next2Am = now.Date.AddDays(now.Hour >= 2 ? 1 : 0).AddHours(2);
                var delay = next2Am - now;

                _logger.LogInformation(
                    "Next log cleanup scheduled for {NextRun} UTC (in {Delay})",
                    next2Am,
                    delay);

                // Wait until 2 AM
                await Task.Delay(delay, stoppingToken);

                if (stoppingToken.IsCancellationRequested)
                    break;

                await PerformCleanupAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error in log cleanup background service");

                // Wait 1 hour before retrying to avoid tight error loop
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        _logger.LogInformation("Log cleanup background service stopped");
    }

    private async Task PerformCleanupAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var retentionDays = _configuration.GetValue("LogCleanup:RetentionDays", 30);
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

        _logger.LogInformation(
            "Starting cleanup of audit logs older than {CutoffDate}",
            cutoffDate);

        var totalDeleted = 0;
        var batchSize = _configuration.GetValue("LogCleanup:BatchSize", 1000);

        while (!cancellationToken.IsCancellationRequested)
        {
            var deletedCount = await dbContext.AuditLogs
                .Where(log => log.CreatedAt < cutoffDate)
                .Take(batchSize)
                .ExecuteDeleteAsync(cancellationToken);

            totalDeleted += deletedCount;

            if (deletedCount > 0)
            {
                _logger.LogDebug("Deleted batch of {Count} logs", deletedCount);
            }

            if (deletedCount < batchSize)
            {
                break;
            }

            // Brief pause between batches to reduce database load
            await Task.Delay(100, cancellationToken);
        }

        _logger.LogInformation(
            "Audit log cleanup completed. Total deleted: {TotalDeleted}",
            totalDeleted);
    }
}
