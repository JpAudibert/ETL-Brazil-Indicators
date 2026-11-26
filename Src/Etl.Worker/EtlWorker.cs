using Etl.Core;

namespace Etl.Worker;

public sealed class EtlWorker(
    PipelineRunner<BcbRawPoint, EconomicIndicator> runner,
    IHostApplicationLifetime lifetime,
    ILogger<EtlWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        ExtractionWindow window = new(today.AddYears(-3), today);

        try
        {
            RunStats stats = await runner.RunAsync(window, ct: stoppingToken);
            logger.LogInformation("Done. Read={Read} Loaded={Loaded} Rejected={Rejected}",
                stats.Read, stats.Loaded, stats.Rejected);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "ETL run failed");
            Environment.ExitCode = 1;
        }
        finally
        {
            lifetime.StopApplication(); // one-shot for now; replace with Quartz.NET scheduling later
        }
    }
}
