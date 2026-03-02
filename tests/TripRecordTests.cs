using ElevatorSimulator.Api.Models;

namespace ElevatorSimulator.Tests;

public class TripRecordTests
{
    [Fact]
    public void WaitTime_IsPickedUpMinusRequested()
    {
        var requested = new DateTime(2026, 1, 1, 12, 0, 0);
        var pickedUp = requested.AddSeconds(10);
        var delivered = pickedUp.AddSeconds(20);

        var record = new TripRecord(1, 1, 5, requested, pickedUp, delivered);

        Assert.Equal(TimeSpan.FromSeconds(10), record.WaitTime);
    }

    [Fact]
    public void TravelTime_IsDeliveredMinusPickedUp()
    {
        var requested = new DateTime(2026, 1, 1, 12, 0, 0);
        var pickedUp = requested.AddSeconds(10);
        var delivered = pickedUp.AddSeconds(20);

        var record = new TripRecord(1, 1, 5, requested, pickedUp, delivered);

        Assert.Equal(TimeSpan.FromSeconds(20), record.TravelTime);
    }

    [Fact]
    public void TotalTime_IsDeliveredMinusRequested()
    {
        var requested = new DateTime(2026, 1, 1, 12, 0, 0);
        var pickedUp = requested.AddSeconds(10);
        var delivered = pickedUp.AddSeconds(20);

        var record = new TripRecord(1, 1, 5, requested, pickedUp, delivered);

        Assert.Equal(TimeSpan.FromSeconds(30), record.TotalTime);
    }

    [Fact]
    public void StoresFloorInformation()
    {
        var now = DateTime.UtcNow;
        var record = new TripRecord(2, 3, 8, now, now, now);

        Assert.Equal(2, record.ElevatorId);
        Assert.Equal(3, record.PickupFloor);
        Assert.Equal(8, record.DestinationFloor);
    }
}
