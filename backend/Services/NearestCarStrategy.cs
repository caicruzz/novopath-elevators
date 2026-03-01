using ElevatorSimulator.Api.Models;

namespace ElevatorSimulator.Api.Services;

/// <summary>
/// Dispatches the nearest idle elevator. Falls back to the nearest non-Maintenance
/// elevator if no idle cars are available.
/// </summary>
public class NearestCarStrategy : IElevatorDispatchStrategy
{
    public Elevator? SelectElevator(IReadOnlyList<Elevator> elevators, int pickupFloor)
    {
        // Prefer idle elevators, closest first
        var elevator = elevators
            .Where(e => e.State == ElevatorState.Idle)
            .OrderBy(e => Math.Abs(e.CurrentFloor - pickupFloor))
            .FirstOrDefault();

        // Fallback: closest non-Maintenance elevator
        elevator ??= elevators
            .Where(e => e.State != ElevatorState.Maintenance)
            .OrderBy(e => Math.Abs(e.CurrentFloor - pickupFloor))
            .FirstOrDefault();

        return elevator;
    }
}
