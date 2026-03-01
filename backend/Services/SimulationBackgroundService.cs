namespace ElevatorSimulator.Api.Services;

/// <summary>
/// Background hosted service that advances the simulation on a fixed interval.
/// Each tick moves elevators one floor toward their targets.
/// </summary>
public class SimulationBackgroundService(BuildingService buildingService, ILogger<SimulationBackgroundService> logger)
    : BackgroundService
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromMilliseconds(800);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Simulation engine started. Tick interval: {Interval}ms", TickInterval.TotalMilliseconds);

        using var timer = new PeriodicTimer(TickInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            buildingService.Tick();
        }

        logger.LogInformation("Simulation engine stopped.");
    }
}
