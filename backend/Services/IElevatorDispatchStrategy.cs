using ElevatorSimulator.Api.Models;

namespace ElevatorSimulator.Api.Services;

/// <summary>
/// Defines the contract for an elevator dispatch algorithm.
/// Implementations select which elevator should answer a call.
/// </summary>
public interface IElevatorDispatchStrategy
{
    /// <summary>
    /// Selects the best elevator from the available pool to service the given pickup floor.
    /// Returns null if no elevator can be dispatched (e.g., all are in Maintenance).
    /// </summary>
    Elevator? SelectElevator(IReadOnlyList<Elevator> elevators, int pickupFloor);
}
