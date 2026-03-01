namespace ElevatorSimulator.Api.Models;

// ── Request DTOs ──

/// <summary>
/// Request to call an elevator to a specific floor.
/// </summary>
public record ElevatorCallRequest(int Floor);

// ── Response DTOs ──

/// <summary>
/// Snapshot of a single elevator's state for the API response.
/// </summary>
public record ElevatorDto(int Id, int CurrentFloor, int TargetFloor, string State);

/// <summary>
/// Snapshot of the entire building state for the API response.
/// </summary>
public record BuildingStateDto(int NumberOfFloors, List<ElevatorDto> Elevators);
