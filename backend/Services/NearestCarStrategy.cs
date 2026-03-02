using ElevatorSimulator.Api.Models;

namespace ElevatorSimulator.Api.Services;

/// <summary>
/// Dispatches the nearest idle elevator. Falls back to the nearest available
/// elevator if no idle cars are available. Excludes Maintenance, CapacityExceeded,
/// and EmergencyStop states from dispatch.
/// </summary>
public class NearestCarStrategy : IElevatorDispatchStrategy
{
    private static readonly ElevatorState[] UnavailableStates =
    [
        ElevatorState.Maintenance,
        ElevatorState.CapacityExceeded,
        ElevatorState.EmergencyStop
    ];

    public Elevator? SelectElevator(IReadOnlyList<Elevator> elevators, int pickupFloor)
    {
        // Prefer idle elevators, closest first
        var elevator = elevators
            .Where(e => e.State == ElevatorState.Idle)
            .OrderBy(e => Math.Abs(e.CurrentFloor - pickupFloor))
            .FirstOrDefault();

        // Fallback: closest available elevator (exclude unsafe states)
        elevator ??= elevators
            .Where(e => !UnavailableStates.Contains(e.State))
            .OrderBy(e => Math.Abs(e.CurrentFloor - pickupFloor))
            .FirstOrDefault();

        return elevator;
    }
}
