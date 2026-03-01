namespace ElevatorSimulator.Api.Models;

/// <summary>
/// In-memory singleton representing the full state of the building simulation.
/// </summary>
public class BuildingState
{
    public int NumberOfFloors { get; set; } = 10;
    public List<Elevator> Elevators { get; set; } = [];
}
