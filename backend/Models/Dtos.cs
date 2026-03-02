namespace ElevatorSimulator.Api.Models;

// ── Domain Models ──

/// <summary>
/// Represents a single passenger with a destination, weight, and trip timing data.
/// </summary>
public class Passenger
{
    public int PickupFloor { get; init; }
    public int DestinationFloor { get; init; }
    public double WeightKg { get; init; }
    public DateTime RequestedAt { get; init; }
    public DateTime? PickedUpAt { get; set; }
}

/// <summary>
/// A safety or compliance event logged during the simulation.
/// </summary>
public record ComplianceEvent(DateTime Timestamp, int ElevatorId, string EventType, string Description);

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
public record ElevatorDto(int Id, int CurrentFloor, int TargetFloor, string State,
    double CurrentWeightKg, double WeightLimitKg, int PassengerCount);

/// <summary>
/// Snapshot of the entire building state for the API response.
/// </summary>
public record BuildingStateDto(int NumberOfFloors, double WeightLimitKg,
    List<ElevatorDto> Elevators, bool IsEmergencyMode, List<ComplianceEvent> ComplianceLog);

/// <summary>
/// Per-elevator performance statistics.
/// </summary>
public record ElevatorStatsDto(
    int ElevatorId,
    int TripsCompleted,
    int FloorsTraversed,
    double AvgWaitTimeSeconds,
    double MaxWaitTimeSeconds);

/// <summary>
/// Full simulation metrics report.
/// </summary>
public record SimulationReportDto(
    int TotalTrips,
    int TotalPassengersServed,
    double AvgWaitTimeSeconds,
    double MaxWaitTimeSeconds,
    double AvgTravelTimeSeconds,
    int TotalFloorsTraversed,
    int ComplianceEventCount,
    List<ElevatorStatsDto> ElevatorStats,
    List<ComplianceEvent> ComplianceLog,
    DateTime GeneratedAt);
