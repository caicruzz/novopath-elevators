namespace ElevatorSimulator.Api.Models;

// ── Domain Models ──

/// <summary>
/// Represents a single passenger with a destination and weight.
/// </summary>
public record Passenger(int DestinationFloor, double WeightKg);

// ── Request DTOs ──

/// <summary>
/// Request to call an elevator to a specific floor.
/// </summary>
public record ElevatorCallRequest(int Floor, int DestinationFloor, double WeightKg);

/// <summary>
/// Request to reconfigure the building simulation parameters.
/// Resets all elevator positions and clears passengers.
/// </summary>
public record ConfigureRequest(int NumberOfFloors, int NumberOfElevators, double WeightLimitKg);

// ── Response DTOs ──

/// <summary>
/// Snapshot of a single elevator's state for the API response.
/// </summary>
public record ElevatorDto(int Id, int CurrentFloor, int TargetFloor, string State, double CurrentWeightKg, double WeightLimitKg);

/// <summary>
/// Snapshot of the entire building state for the API response.
/// </summary>
public record BuildingStateDto(int NumberOfFloors, double WeightLimitKg, List<ElevatorDto> Elevators);
