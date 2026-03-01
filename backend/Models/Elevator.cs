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
                return; // Elevator is out of service

            case ElevatorState.DoorsOpen:
                _doorsOpenTicksRemaining--;
                if (_doorsOpenTicksRemaining <= 0)
                {
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
        }
        else if (CurrentFloor > TargetFloor)
        {
            CurrentFloor--;
        }

        // Check if we've arrived
        if (CurrentFloor == TargetFloor)
        {
            State = ElevatorState.DoorsOpen;
            _doorsOpenTicksRemaining = DoorsOpenDurationTicks;
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
}
