using ElevatorSimulator.Api.Models;

namespace ElevatorSimulator.Api.Services;

/// <summary>
/// Singleton service managing the building's elevator state.
/// Thread-safe via locking to support concurrent API calls and the background tick.
/// </summary>
public class BuildingService
{
    private readonly BuildingState _state;
    private readonly Lock _lock = new();

    public BuildingService()
    {
        _state = new BuildingState
        {
            NumberOfFloors = 10,
            Elevators =
            [
                new Elevator { Id = 1, CurrentFloor = 1 },
                new Elevator { Id = 2, CurrentFloor = 5 },
                new Elevator { Id = 3, CurrentFloor = 10 }
            ]
        };
    }

    /// <summary>
    /// Returns an immutable snapshot of the current building state.
    /// </summary>
    public BuildingStateDto GetState()
    {
        lock (_lock)
        {
            var elevatorDtos = _state.Elevators
                .Select(e => new ElevatorDto(e.Id, e.CurrentFloor, e.TargetFloor, e.State.ToString()))
                .ToList();

            return new BuildingStateDto(_state.NumberOfFloors, elevatorDtos);
        }
    }

    /// <summary>
    /// Dispatches the nearest idle elevator to the requested floor.
    /// If no elevators are idle, the closest elevator is reassigned.
    /// </summary>
    public ElevatorDto CallElevator(int floor)
    {
        lock (_lock)
        {
            if (floor < 1 || floor > _state.NumberOfFloors)
                throw new ArgumentOutOfRangeException(nameof(floor),
                    $"Floor must be between 1 and {_state.NumberOfFloors}.");

            // Prefer idle elevators, closest first
            var elevator = _state.Elevators
                .Where(e => e.State == ElevatorState.Idle)
                .OrderBy(e => Math.Abs(e.CurrentFloor - floor))
                .FirstOrDefault();

            // Fallback: pick the closest elevator regardless of state (excluding Maintenance)
            elevator ??= _state.Elevators
                .Where(e => e.State != ElevatorState.Maintenance)
                .OrderBy(e => Math.Abs(e.CurrentFloor - floor))
                .First();

            elevator.AssignTarget(floor);

            return new ElevatorDto(elevator.Id, elevator.CurrentFloor, elevator.TargetFloor, elevator.State.ToString());
        }
    }

    /// <summary>
    /// Advances all elevators by one simulation tick.
    /// Called by the background service on a timer.
    /// </summary>
    public void Tick()
    {
        lock (_lock)
        {
            foreach (var elevator in _state.Elevators)
            {
                elevator.Tick();
            }
        }
    }
}
