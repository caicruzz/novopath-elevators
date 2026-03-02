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
            return GetStateUnsafe();
        }
    }

    /// <summary>
    /// Dispatches an elevator to the pickup floor, then routes it to the destination.
    /// </summary>
    public ElevatorDto CallElevator(ElevatorCallRequest request)
    {
        lock (_lock)
        {
            if (_state.IsEmergencyMode)
                throw new InvalidOperationException("Emergency mode active. All elevator calls are suspended.");

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

            elevator.Passengers.Add(new Passenger
            {
                PickupFloor = request.Floor,
                DestinationFloor = request.DestinationFloor,
                WeightKg = request.WeightKg,
                RequestedAt = DateTime.UtcNow
            });
            elevator.EnqueueStop(request.Floor);
            elevator.EnqueueStop(request.DestinationFloor);

            return new ElevatorDto(
                elevator.Id,
                elevator.CurrentFloor,
                elevator.TargetFloor,
                elevator.State.ToString(),
                elevator.CurrentWeightKg,
                elevator.WeightLimitKg,
                elevator.Passengers.Count);
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
    /// Activates emergency fire mode. All elevators descend to ground floor.
    /// </summary>
    public BuildingStateDto ActivateEmergencyMode()
    {
        lock (_lock)
        {
            if (_state.IsEmergencyMode) return GetStateUnsafe();

            _state.IsEmergencyMode = true;
            _state.ComplianceLog.Add(new ComplianceEvent(
                DateTime.UtcNow, 0, "EmergencyActivated",
                "Fire mode activated. All elevators returning to ground floor."));

            foreach (var elevator in _state.Elevators)
                elevator.ForceEmergencyDescent();

            return GetStateUnsafe();
        }
    }

    /// <summary>
    /// Deactivates emergency fire mode. All elevators reset to idle at ground floor.
    /// </summary>
    public BuildingStateDto DeactivateEmergencyMode()
    {
        lock (_lock)
        {
            if (!_state.IsEmergencyMode) return GetStateUnsafe();

            _state.IsEmergencyMode = false;
            _state.ComplianceLog.Add(new ComplianceEvent(
                DateTime.UtcNow, 0, "EmergencyDeactivated",
                "Fire mode deactivated. Normal operation resumed."));

            foreach (var elevator in _state.Elevators)
                elevator.ResetFromEmergency();

            return GetStateUnsafe();
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
                var prevState = elevator.State;
                elevator.Tick();

                // Collect completed trips from this elevator
                if (elevator.RecentlyCompletedTrips.Count > 0)
                {
                    _state.CompletedTrips.AddRange(elevator.RecentlyCompletedTrips);
                    elevator.RecentlyCompletedTrips.Clear();
                }

                // Log compliance event on state transition to CapacityExceeded
                if (elevator.State == ElevatorState.CapacityExceeded
                    && prevState != ElevatorState.CapacityExceeded)
                {
                    _state.ComplianceLog.Add(new ComplianceEvent(
                        DateTime.UtcNow, elevator.Id, "CapacityExceeded",
                        $"Car {elevator.Id} exceeded weight limit " +
                        $"({elevator.CurrentWeightKg:F1}kg / {elevator.WeightLimitKg:F1}kg) " +
                        $"at floor {elevator.CurrentFloor}."));
                }
            }
        }
    }

    /// <summary>
    /// Generates an aggregated simulation report from all completed trips.
    /// </summary>
    public SimulationReportDto GetReport()
    {
        lock (_lock)
        {
            var trips = _state.CompletedTrips;
            var totalTrips = trips.Count;

            double avgWait = 0, maxWait = 0, avgTravel = 0;
            if (totalTrips > 0)
            {
                avgWait = trips.Average(t => t.WaitTime.TotalSeconds);
                maxWait = trips.Max(t => t.WaitTime.TotalSeconds);
                avgTravel = trips.Average(t => t.TravelTime.TotalSeconds);
            }

            var totalFloors = _state.Elevators.Sum(e => e.TotalFloorsTraversed);

            var elevatorStats = _state.Elevators.Select(e =>
            {
                var elevatorTrips = trips.Where(t => t.ElevatorId == e.Id).ToList();
                return new ElevatorStatsDto(
                    e.Id,
                    elevatorTrips.Count,
                    e.TotalFloorsTraversed,
                    elevatorTrips.Count > 0 ? elevatorTrips.Average(t => t.WaitTime.TotalSeconds) : 0,
                    elevatorTrips.Count > 0 ? elevatorTrips.Max(t => t.WaitTime.TotalSeconds) : 0);
            }).ToList();

            return new SimulationReportDto(
                totalTrips,
                totalTrips, // Each trip = 1 passenger served
                Math.Round(avgWait, 1),
                Math.Round(maxWait, 1),
                Math.Round(avgTravel, 1),
                totalFloors,
                _state.ComplianceLog.Count,
                elevatorStats,
                _state.ComplianceLog.ToList(),
                DateTime.UtcNow);
        }
    }

    /// <summary>
    /// Builds a state snapshot without acquiring the lock. Caller must hold the lock.
    /// </summary>
    private BuildingStateDto GetStateUnsafe()
    {
        var elevatorDtos = _state.Elevators
            .Select(e => new ElevatorDto(
                e.Id,
                e.CurrentFloor,
                e.TargetFloor,
                e.State.ToString(),
                e.CurrentWeightKg,
                e.WeightLimitKg,
                e.Passengers.Count))
            .ToList();

        return new BuildingStateDto(
            _state.NumberOfFloors,
            _state.WeightLimitKg,
            elevatorDtos,
            _state.IsEmergencyMode,
            _state.ComplianceLog.ToList());
    }
}
