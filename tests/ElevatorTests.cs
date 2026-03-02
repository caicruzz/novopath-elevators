using ElevatorSimulator.Api.Models;

namespace ElevatorSimulator.Tests;

public class ElevatorTests
{
    private static Elevator CreateElevator(int id = 1, double weightLimit = 1000.0) =>
        new() { Id = id, CurrentFloor = 1, WeightLimitKg = weightLimit };

    private static Passenger CreatePassenger(
        int pickup, int destination, double weight = 70.0) =>
        new()
        {
            PickupFloor = pickup,
            DestinationFloor = destination,
            WeightKg = weight,
            RequestedAt = DateTime.UtcNow
        };

    // ── Initial State ──

    [Fact]
    public void NewElevator_StartsIdle_AtFloor1()
    {
        var e = CreateElevator();

        Assert.Equal(ElevatorState.Idle, e.State);
        Assert.Equal(1, e.CurrentFloor);
        Assert.Equal(1, e.TargetFloor);
        Assert.Empty(e.Passengers);
        Assert.Equal(0, e.TotalFloorsTraversed);
    }

    // ── Movement ──

    [Fact]
    public void EnqueueStop_MovesElevatorUpward()
    {
        var e = CreateElevator();
        e.EnqueueStop(5);

        Assert.Equal(ElevatorState.MovingUp, e.State);
        Assert.Equal(5, e.TargetFloor);
    }

    [Fact]
    public void Tick_MovesOneFloorPerTick_Upward()
    {
        var e = CreateElevator();
        e.EnqueueStop(4);

        e.Tick(); // 1 → 2
        Assert.Equal(2, e.CurrentFloor);
        Assert.Equal(ElevatorState.MovingUp, e.State);

        e.Tick(); // 2 → 3
        Assert.Equal(3, e.CurrentFloor);

        e.Tick(); // 3 → 4, arrives → DoorsOpen
        Assert.Equal(4, e.CurrentFloor);
        Assert.Equal(ElevatorState.DoorsOpen, e.State);
    }

    [Fact]
    public void Tick_MovesOneFloorPerTick_Downward()
    {
        var e = CreateElevator();
        e.CurrentFloor = 5;
        e.EnqueueStop(3);

        e.Tick(); // 5 → 4
        Assert.Equal(4, e.CurrentFloor);
        Assert.Equal(ElevatorState.MovingDown, e.State);

        e.Tick(); // 4 → 3, arrives
        Assert.Equal(3, e.CurrentFloor);
        Assert.Equal(ElevatorState.DoorsOpen, e.State);
    }

    [Fact]
    public void Tick_TracksFloorsTraversed()
    {
        var e = CreateElevator();
        e.EnqueueStop(4);

        e.Tick(); e.Tick(); e.Tick(); // 3 floors moved

        Assert.Equal(3, e.TotalFloorsTraversed);
    }

    // ── Doors Open Cycle ──

    [Fact]
    public void DoorsOpen_LastsThreeTicks_ThenTransitionsToIdle()
    {
        var e = CreateElevator();
        e.EnqueueStop(2);

        e.Tick(); // move to floor 2, DoorsOpen
        Assert.Equal(ElevatorState.DoorsOpen, e.State);

        e.Tick(); // doors open tick 1
        e.Tick(); // doors open tick 2
        e.Tick(); // doors open tick 3 → Idle

        Assert.Equal(ElevatorState.Idle, e.State);
    }

    [Fact]
    public void DoorsOpen_AdvancesToNextStop_IfQueued()
    {
        var e = CreateElevator();
        e.EnqueueStop(3);
        e.EnqueueStop(5);

        // Move to floor 3
        e.Tick(); e.Tick(); // 1→2→3
        Assert.Equal(ElevatorState.DoorsOpen, e.State);

        // Doors open ticks
        e.Tick(); e.Tick(); e.Tick();

        // Should now be heading to 5
        Assert.Equal(5, e.TargetFloor);
        Assert.True(e.State == ElevatorState.MovingUp || e.State == ElevatorState.DoorsOpen);
    }

    // ── Passenger Disembark ──

    [Fact]
    public void Passengers_DisembarkAtDestination()
    {
        var e = CreateElevator();
        e.Passengers.Add(CreatePassenger(1, 3));
        e.EnqueueStop(3);

        // Move to floor 3
        e.Tick(); e.Tick(); // DoorsOpen
        Assert.Equal(ElevatorState.DoorsOpen, e.State);

        // Doors open cycle (passenger removed after doors close)
        e.Tick(); e.Tick(); e.Tick();

        Assert.Empty(e.Passengers);
    }

    [Fact]
    public void DisembarkPassengers_CreatesTripRecords()
    {
        var e = CreateElevator();
        e.Passengers.Add(CreatePassenger(1, 3));
        e.EnqueueStop(3);

        // Move to floor 3
        e.Tick(); e.Tick();

        // Doors open cycle
        e.Tick(); e.Tick(); e.Tick();

        Assert.Single(e.RecentlyCompletedTrips);
        var trip = e.RecentlyCompletedTrips[0];
        Assert.Equal(1, trip.ElevatorId);
        Assert.Equal(1, trip.PickupFloor);
        Assert.Equal(3, trip.DestinationFloor);
    }

    [Fact]
    public void OnlyMatchingPassengers_Disembark()
    {
        var e = CreateElevator();
        e.Passengers.Add(CreatePassenger(1, 3));
        e.Passengers.Add(CreatePassenger(1, 5));
        e.EnqueueStop(3);
        e.EnqueueStop(5);

        // Move to floor 3
        e.Tick(); e.Tick();
        // Doors open → one passenger exits
        e.Tick(); e.Tick(); e.Tick();

        Assert.Single(e.Passengers);
        Assert.Equal(5, e.Passengers[0].DestinationFloor);
    }

    // ── Weight Limit / CapacityExceeded ──

    [Fact]
    public void Overweight_TransitionsToCapacityExceeded()
    {
        var e = CreateElevator(weightLimit: 100);
        // Two heavy passengers — over limit
        e.Passengers.Add(CreatePassenger(1, 3, 60));
        e.Passengers.Add(CreatePassenger(1, 5, 60));
        e.EnqueueStop(2);

        Assert.True(e.IsOverweight);

        // Move to floor 2 → DoorsOpen
        e.Tick();
        Assert.Equal(ElevatorState.DoorsOpen, e.State);

        // DoorsOpen ticks → checks weight → CapacityExceeded
        e.Tick(); e.Tick(); e.Tick();

        Assert.Equal(ElevatorState.CapacityExceeded, e.State);
    }

    [Fact]
    public void CapacityExceeded_ResumesWhenPassengersDisembark()
    {
        var e = CreateElevator(weightLimit: 100);
        // One passenger exits at floor 2, bringing weight under limit
        e.Passengers.Add(CreatePassenger(1, 2, 60));
        e.Passengers.Add(CreatePassenger(1, 5, 60));
        e.EnqueueStop(2);
        e.EnqueueStop(5);

        // Move to floor 2 → DoorsOpen → CapacityExceeded
        e.Tick();
        e.Tick(); e.Tick(); e.Tick();

        // If at destination floor 2, passenger exits during CapacityExceeded
        // The CapacityExceeded handler calls DisembarkPassengers
        // After disembark, weight is 60 < 100, so should resume to DoorsOpen
        if (e.State == ElevatorState.CapacityExceeded)
        {
            e.Tick(); // Try disembark again
        }

        Assert.True(e.State == ElevatorState.DoorsOpen || e.State == ElevatorState.Idle
                     || e.State == ElevatorState.MovingUp);
        Assert.True(e.CurrentWeightKg <= e.WeightLimitKg);
    }

    // ── Emergency Descent ──

    [Fact]
    public void ForceEmergencyDescent_ClearsPassengersAndDescends()
    {
        var e = CreateElevator();
        e.CurrentFloor = 5;
        e.Passengers.Add(CreatePassenger(1, 8));
        e.EnqueueStop(8);

        e.ForceEmergencyDescent();

        Assert.Empty(e.Passengers);
        Assert.Equal(1, e.TargetFloor);
        Assert.Equal(ElevatorState.MovingDown, e.State);
        Assert.True(e.IsInEmergencyDescent);
    }

    [Fact]
    public void ForceEmergencyDescent_AtGroundFloor_GoesDirectlyToEmergencyStop()
    {
        var e = CreateElevator();
        e.CurrentFloor = 1;

        e.ForceEmergencyDescent();

        Assert.Equal(ElevatorState.EmergencyStop, e.State);
        Assert.False(e.IsInEmergencyDescent);
    }

    [Fact]
    public void EmergencyDescent_ArrivesAtFloor1_FreezesInEmergencyStop()
    {
        var e = CreateElevator();
        e.CurrentFloor = 3;
        e.ForceEmergencyDescent();

        e.Tick(); // 3 → 2
        e.Tick(); // 2 → 1

        Assert.Equal(1, e.CurrentFloor);
        Assert.Equal(ElevatorState.EmergencyStop, e.State);
        Assert.False(e.IsInEmergencyDescent);
    }

    [Fact]
    public void EmergencyStop_DoesNotProcessTicks()
    {
        var e = CreateElevator();
        e.State = ElevatorState.EmergencyStop;
        e.CurrentFloor = 1;

        e.Tick(); e.Tick(); e.Tick();

        Assert.Equal(ElevatorState.EmergencyStop, e.State);
        Assert.Equal(1, e.CurrentFloor);
    }

    [Fact]
    public void Maintenance_DoesNotProcessTicks()
    {
        var e = CreateElevator();
        e.State = ElevatorState.Maintenance;

        e.Tick(); e.Tick();

        Assert.Equal(ElevatorState.Maintenance, e.State);
    }

    // ── ResetFromEmergency ──

    [Fact]
    public void ResetFromEmergency_RestoresToIdle()
    {
        var e = CreateElevator();
        e.CurrentFloor = 3;
        e.State = ElevatorState.EmergencyStop;
        e.IsInEmergencyDescent = true;

        e.ResetFromEmergency();

        Assert.Equal(ElevatorState.Idle, e.State);
        Assert.Equal(1, e.CurrentFloor);
        Assert.Equal(1, e.TargetFloor);
        Assert.False(e.IsInEmergencyDescent);
        Assert.Empty(e.Passengers);
    }

    // ── Weight Calculation ──

    [Fact]
    public void CurrentWeightKg_SumsAllPassengerWeights()
    {
        var e = CreateElevator();
        e.Passengers.Add(CreatePassenger(1, 3, 70));
        e.Passengers.Add(CreatePassenger(1, 5, 80));
        e.Passengers.Add(CreatePassenger(1, 7, 50));

        Assert.Equal(200, e.CurrentWeightKg);
    }

    [Fact]
    public void IsOverweight_ReturnsFalse_WhenUnderLimit()
    {
        var e = CreateElevator(weightLimit: 300);
        e.Passengers.Add(CreatePassenger(1, 3, 100));

        Assert.False(e.IsOverweight);
    }

    [Fact]
    public void IsOverweight_ReturnsTrue_WhenOverLimit()
    {
        var e = CreateElevator(weightLimit: 100);
        e.Passengers.Add(CreatePassenger(1, 3, 101));

        Assert.True(e.IsOverweight);
    }

    // ── EnqueueStop on same floor ──

    [Fact]
    public void EnqueueStop_AtCurrentFloor_OpensDoors()
    {
        var e = CreateElevator();
        e.CurrentFloor = 3;
        e.EnqueueStop(3);

        Assert.Equal(ElevatorState.DoorsOpen, e.State);
    }
}
