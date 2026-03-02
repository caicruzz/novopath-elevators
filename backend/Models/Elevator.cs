namespace ElevatorSimulator.Api.Models;

/// <summary>
/// Domain model for a single elevator car.
/// Contains floor-by-floor movement logic governed by the ElevatorState machine.
/// </summary>
public class Elevator
{
    private const int DoorsOpenDurationTicks = 3;

    public int Id { get; init; }
    public int CurrentFloor { get; set; } = 1;
    public int TargetFloor { get; set; } = 1;
    public ElevatorState State { get; set; } = ElevatorState.Idle;
    public double WeightLimitKg { get; set; } = 1000.0;
    public List<Passenger> Passengers { get; } = [];
    public double CurrentWeightKg => Passengers.Sum(p => p.WeightKg);
    public bool IsOverweight => CurrentWeightKg > WeightLimitKg;
    public bool IsInEmergencyDescent { get; set; }

    /// <summary>
    /// Total floors this elevator has traveled since creation/reset. Used for metrics.
    /// </summary>
    public int TotalFloorsTraversed { get; private set; }

    /// <summary>
    /// Completed trips collected after disembark. Drained by BuildingService on each tick.
    /// </summary>
    public List<TripRecord> RecentlyCompletedTrips { get; } = [];

    /// <summary>
    /// Tracks remaining ticks the doors stay open before transitioning to Idle.
    /// </summary>
    private int _doorsOpenTicksRemaining;

    /// <summary>
    /// Ordered queue of floors to visit. Dequeued one at a time after each door-close cycle.
    /// </summary>
    private readonly Queue<int> _stopQueue = new();

    /// <summary>
    /// Advances the elevator by one simulation tick.
    /// Movement is floor-by-floor to simulate realistic travel.
    /// </summary>
    public void Tick()
    {
        switch (State)
        {
            case ElevatorState.Maintenance:
            case ElevatorState.EmergencyStop:
                return; // Elevator is out of service

            case ElevatorState.CapacityExceeded:
                // Attempt to offload passengers at current floor
                DisembarkPassengers();
                if (!IsOverweight)
                {
                    // Weight is now within limits — resume normal operation
                    State = ElevatorState.DoorsOpen;
                    _doorsOpenTicksRemaining = DoorsOpenDurationTicks;
                }
                return;

            case ElevatorState.DoorsOpen:
                _doorsOpenTicksRemaining--;
                if (_doorsOpenTicksRemaining <= 0)
                {
                    // Mark passengers as picked up at this floor
                    MarkPassengersPickedUp();
                    // Passengers exit at this floor
                    DisembarkPassengers();

                    // Check weight before allowing movement
                    if (IsOverweight)
                    {
                        State = ElevatorState.CapacityExceeded;
                        return;
                    }

                    if (_stopQueue.Count > 0)
                        AdvanceToNextStop();
                    else
                        State = ElevatorState.Idle;
                }
                return;

            case ElevatorState.Idle:
                if (CurrentFloor != TargetFloor)
                {
                    State = CurrentFloor < TargetFloor
                        ? ElevatorState.MovingUp
                        : ElevatorState.MovingDown;
                }
                else
                {
                    return; // Nothing to do
                }
                break;

            case ElevatorState.MovingUp:
            case ElevatorState.MovingDown:
                break; // Continue processing below
        }

        // Move one floor toward target
        if (CurrentFloor < TargetFloor)
        {
            CurrentFloor++;
            TotalFloorsTraversed++;
        }
        else if (CurrentFloor > TargetFloor)
        {
            CurrentFloor--;
            TotalFloorsTraversed++;
        }

        // Check if we've arrived
        if (CurrentFloor == TargetFloor)
        {
            if (IsInEmergencyDescent)
            {
                // Emergency — freeze at ground floor
                State = ElevatorState.EmergencyStop;
                IsInEmergencyDescent = false;
            }
            else
            {
                State = ElevatorState.DoorsOpen;
                _doorsOpenTicksRemaining = DoorsOpenDurationTicks;
            }
        }
    }

    /// <summary>
    /// Assigns a new destination floor, waking the elevator from Idle if necessary.
    /// </summary>
    public void AssignTarget(int floor)
    {
        TargetFloor = floor;

        if (State == ElevatorState.Idle && CurrentFloor != TargetFloor)
        {
            State = CurrentFloor < TargetFloor
                ? ElevatorState.MovingUp
                : ElevatorState.MovingDown;
        }
    }

    /// <summary>
    /// Appends a floor to the stop queue. If the elevator is idle, starts moving immediately.
    /// </summary>
    public void EnqueueStop(int floor)
    {
        _stopQueue.Enqueue(floor);
        if (State == ElevatorState.Idle)
            AdvanceToNextStop();
    }

    /// <summary>
    /// Forces the elevator into emergency descent to floor 1.
    /// Clears all passengers and pending stops.
    /// </summary>
    public void ForceEmergencyDescent()
    {
        _stopQueue.Clear();
        Passengers.Clear();

        if (CurrentFloor == 1)
        {
            State = ElevatorState.EmergencyStop;
            TargetFloor = 1;
            IsInEmergencyDescent = false;
        }
        else
        {
            IsInEmergencyDescent = true;
            TargetFloor = 1;
            State = CurrentFloor > 1 ? ElevatorState.MovingDown : ElevatorState.MovingUp;
        }
    }

    /// <summary>
    /// Resets the elevator from emergency mode back to normal idle state.
    /// </summary>
    public void ResetFromEmergency()
    {
        IsInEmergencyDescent = false;
        State = ElevatorState.Idle;
        CurrentFloor = 1;
        TargetFloor = 1;
        Passengers.Clear();
        _stopQueue.Clear();
    }

    private void AdvanceToNextStop()
    {
        if (_stopQueue.TryDequeue(out var next))
        {
            TargetFloor = next;
            if (CurrentFloor == TargetFloor)
            {
                // Already on this floor — open doors immediately
                State = ElevatorState.DoorsOpen;
                _doorsOpenTicksRemaining = DoorsOpenDurationTicks;
            }
            else
            {
                AssignTarget(next);
            }
        }
    }

    /// <summary>
    /// Marks passengers whose pickup floor matches the current floor as "picked up".
    /// </summary>
    private void MarkPassengersPickedUp()
    {
        var now = DateTime.UtcNow;
        foreach (var p in Passengers)
        {
            if (p.PickupFloor == CurrentFloor && p.PickedUpAt is null)
            {
                p.PickedUpAt = now;
            }
        }
    }

    /// <summary>
    /// Removes passengers whose destination is the current floor.
    /// Creates TripRecords for successfully delivered passengers.
    /// </summary>
    private void DisembarkPassengers()
    {
        var now = DateTime.UtcNow;
        var delivered = Passengers
            .Where(p => p.DestinationFloor == CurrentFloor)
            .ToList();

        foreach (var p in delivered)
        {
            RecentlyCompletedTrips.Add(new TripRecord(
                Id,
                p.PickupFloor,
                p.DestinationFloor,
                p.RequestedAt,
                p.PickedUpAt ?? now,  // fallback if never explicitly picked up
                now));
        }

        Passengers.RemoveAll(p => p.DestinationFloor == CurrentFloor);
    }
}
