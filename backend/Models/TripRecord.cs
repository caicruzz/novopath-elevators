namespace ElevatorSimulator.Api.Models;

/// <summary>
/// Records a completed passenger trip with timing data for metrics aggregation.
/// </summary>
public record TripRecord(
    int ElevatorId,
    int PickupFloor,
    int DestinationFloor,
    DateTime RequestedAt,
    DateTime PickedUpAt,
    DateTime DeliveredAt)
{
    /// <summary>Time between the call request and the elevator arriving at the pickup floor.</summary>
    public TimeSpan WaitTime => PickedUpAt - RequestedAt;

    /// <summary>Time between pickup and delivery at the destination.</summary>
    public TimeSpan TravelTime => DeliveredAt - PickedUpAt;

    /// <summary>Total door-to-door time for the passenger.</summary>
    public TimeSpan TotalTime => DeliveredAt - RequestedAt;
}
