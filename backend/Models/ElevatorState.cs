namespace ElevatorSimulator.Api.Models;

/// <summary>
/// Represents the possible states of an elevator car governed by a strict state machine.
/// </summary>
public enum ElevatorState
{
    Idle,
    MovingUp,
    MovingDown,
    DoorsOpen,
    Maintenance
}
