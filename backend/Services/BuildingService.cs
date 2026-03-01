using ElevatorSimulator.Api.Models;

namespace ElevatorSimulator.Api.Services;

/// <summary>
/// Singleton service managing the building's elevator state.
/// Thread-safe via locking to support concurrent API calls and the background tick.
/// </summary>
public class BuildingService
{
    private BuildingState _state;
    private readonly Lock _lock = new();
    private readonly IElevatorDispatchStrategy _dispatchStrategy;

    public BuildingService(IElevatorDispatchStrategy dispatchStrategy)
    {
        _dispatchStrategy = dispatchStrategy;
        _state = CreateInitialState(10, 3, 1000.0);
    }

    private static BuildingState CreateInitialState(int floors, int elevatorCount, double weightLimitKg)
    {
        return new BuildingState
        {
            NumberOfFloors = floors,
            WeightLimitKg = weightLimitKg,
            Elevators = Enumerable.Range(1, elevatorCount)
                .Select(id => new Elevator { Id = id, CurrentFloor = 1, WeightLimitKg = weightLimitKg })
                .ToList()
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
                .Select(e => new ElevatorDto(
                    e.Id,
                    e.CurrentFloor,
                    e.TargetFloor,
                    e.State.ToString(),
                    e.CurrentWeightKg,
                    e.WeightLimitKg))
                .ToList();

            return new BuildingStateDto(_state.NumberOfFloors, _state.WeightLimitKg, elevatorDtos);
        }
    }

    /// <summary>
    /// Dispatches an elevator to the pickup floor, then routes it to the destination.
    /// </summary>
    public ElevatorDto CallElevator(ElevatorCallRequest request)
    {
        lock (_lock)
        {
            if (request.Floor < 1 || request.Floor > _state.NumberOfFloors)
                throw new ArgumentOutOfRangeException(nameof(request.Floor),
                    $"Pickup floor must be between 1 and {_state.NumberOfFloors}.");

            if (request.DestinationFloor < 1 || request.DestinationFloor > _state.NumberOfFloors)
                throw new ArgumentOutOfRangeException(nameof(request.DestinationFloor),
                    $"Destination floor must be between 1 and {_state.NumberOfFloors}.");

            if (request.Floor == request.DestinationFloor)
                throw new ArgumentException("Pickup and destination floors must differ.");

            if (request.WeightKg <= 0)
                throw new ArgumentOutOfRangeException(nameof(request.WeightKg),
                    "Passenger weight must be positive.");

            var elevator = _dispatchStrategy.SelectElevator(_state.Elevators, request.Floor)
                ?? throw new InvalidOperationException("No elevators are available.");

            elevator.Passengers.Add(new Passenger(request.DestinationFloor, request.WeightKg));
            elevator.EnqueueStop(request.Floor);
            elevator.EnqueueStop(request.DestinationFloor);

            return new ElevatorDto(
                elevator.Id,
                elevator.CurrentFloor,
                elevator.TargetFloor,
                elevator.State.ToString(),
                elevator.CurrentWeightKg,
                elevator.WeightLimitKg);
        }
    }

    /// <summary>
    /// Reconfigures the building, resetting all elevators.
    /// </summary>
    public void ConfigureBuilding(ConfigureRequest req)
    {
        if (req.NumberOfFloors < 2)
            throw new ArgumentOutOfRangeException(nameof(req.NumberOfFloors),
                "Building must have at least 2 floors.");
        if (req.NumberOfElevators < 1)
            throw new ArgumentOutOfRangeException(nameof(req.NumberOfElevators),
                "Building must have at least 1 elevator.");
        if (req.NumberOfElevators > 20)
            throw new ArgumentOutOfRangeException(nameof(req.NumberOfElevators),
                "Maximum 20 elevators supported.");
        if (req.WeightLimitKg <= 0)
            throw new ArgumentOutOfRangeException(nameof(req.WeightLimitKg),
                "Weight limit must be positive.");

        lock (_lock)
        {
            _state = CreateInitialState(req.NumberOfFloors, req.NumberOfElevators, req.WeightLimitKg);
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
