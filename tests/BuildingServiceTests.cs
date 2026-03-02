using ElevatorSimulator.Api.Models;
using ElevatorSimulator.Api.Services;

namespace ElevatorSimulator.Tests;

public class BuildingServiceTests
{
    private static BuildingService CreateService() => new(new NearestCarStrategy());

    // ── GetState ──

    [Fact]
    public void GetState_ReturnsDefaultConfiguration()
    {
        var svc = CreateService();
        var state = svc.GetState();

        Assert.Equal(10, state.NumberOfFloors);
        Assert.Equal(3, state.Elevators.Count);
        Assert.False(state.IsEmergencyMode);
        Assert.Empty(state.ComplianceLog);
    }

    // ── ConfigureBuilding ──

    [Fact]
    public void Configure_UpdatesFloorAndElevatorCount()
    {
        var svc = CreateService();
        svc.ConfigureBuilding(new ConfigureRequest(15, 5, 2000));
        var state = svc.GetState();

        Assert.Equal(15, state.NumberOfFloors);
        Assert.Equal(5, state.Elevators.Count);
        Assert.Equal(2000, state.WeightLimitKg);
    }

    [Fact]
    public void Configure_ThrowsOnInvalidFloors()
    {
        var svc = CreateService();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            svc.ConfigureBuilding(new ConfigureRequest(1, 3, 1000)));
    }

    [Fact]
    public void Configure_ThrowsOnZeroElevators()
    {
        var svc = CreateService();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            svc.ConfigureBuilding(new ConfigureRequest(10, 0, 1000)));
    }

    [Fact]
    public void Configure_ThrowsOnTooManyElevators()
    {
        var svc = CreateService();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            svc.ConfigureBuilding(new ConfigureRequest(10, 21, 1000)));
    }

    [Fact]
    public void Configure_ThrowsOnZeroWeight()
    {
        var svc = CreateService();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            svc.ConfigureBuilding(new ConfigureRequest(10, 3, 0)));
    }

    // ── CallElevator ──

    [Fact]
    public void CallElevator_ReturnsElevatorDto()
    {
        var svc = CreateService();
        var dto = svc.CallElevator(new ElevatorCallRequest(1, 5, 70));

        Assert.True(dto.Id > 0);
        Assert.Equal(1, dto.PassengerCount);
    }

    [Fact]
    public void CallElevator_ThrowsOnSameFloor()
    {
        var svc = CreateService();

        Assert.Throws<ArgumentException>(() =>
            svc.CallElevator(new ElevatorCallRequest(3, 3, 70)));
    }

    [Fact]
    public void CallElevator_ThrowsOnInvalidPickupFloor()
    {
        var svc = CreateService();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            svc.CallElevator(new ElevatorCallRequest(0, 5, 70)));
    }

    [Fact]
    public void CallElevator_ThrowsOnInvalidDestinationFloor()
    {
        var svc = CreateService();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            svc.CallElevator(new ElevatorCallRequest(1, 11, 70)));
    }

    [Fact]
    public void CallElevator_ThrowsOnZeroWeight()
    {
        var svc = CreateService();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            svc.CallElevator(new ElevatorCallRequest(1, 5, 0)));
    }

    [Fact]
    public void CallElevator_ThrowsOnNegativeWeight()
    {
        var svc = CreateService();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            svc.CallElevator(new ElevatorCallRequest(1, 5, -10)));
    }

    // ── Emergency Mode ──

    [Fact]
    public void ActivateEmergency_SetsEmergencyMode()
    {
        var svc = CreateService();
        var state = svc.ActivateEmergencyMode();

        Assert.True(state.IsEmergencyMode);
    }

    [Fact]
    public void ActivateEmergency_LogsComplianceEvent()
    {
        var svc = CreateService();
        var state = svc.ActivateEmergencyMode();

        Assert.Single(state.ComplianceLog);
        Assert.Equal("EmergencyActivated", state.ComplianceLog[0].EventType);
    }

    [Fact]
    public void ActivateEmergency_IsIdempotent()
    {
        var svc = CreateService();
        svc.ActivateEmergencyMode();
        var state = svc.ActivateEmergencyMode();

        // Should still only have one log entry
        Assert.Single(state.ComplianceLog);
    }

    [Fact]
    public void DeactivateEmergency_ClearsEmergencyMode()
    {
        var svc = CreateService();
        svc.ActivateEmergencyMode();
        var state = svc.DeactivateEmergencyMode();

        Assert.False(state.IsEmergencyMode);
    }

    [Fact]
    public void DeactivateEmergency_LogsComplianceEvent()
    {
        var svc = CreateService();
        svc.ActivateEmergencyMode();
        var state = svc.DeactivateEmergencyMode();

        Assert.Equal(2, state.ComplianceLog.Count);
        Assert.Equal("EmergencyDeactivated", state.ComplianceLog[1].EventType);
    }

    [Fact]
    public void CallElevator_ThrowsDuringEmergency()
    {
        var svc = CreateService();
        svc.ActivateEmergencyMode();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            svc.CallElevator(new ElevatorCallRequest(1, 5, 70)));

        Assert.Contains("Emergency", ex.Message);
    }

    // ── Tick ──

    [Fact]
    public void Tick_AdvancesAllElevators()
    {
        var svc = CreateService();
        svc.CallElevator(new ElevatorCallRequest(1, 5, 70));

        svc.Tick();
        var state = svc.GetState();

        // At least one elevator should have moved or changed state
        var movedOrChanged = state.Elevators.Any(e =>
            e.State != "Idle" || e.CurrentFloor != 1);
        Assert.True(movedOrChanged);
    }

    // ── GetReport ──

    [Fact]
    public void GetReport_ReturnsEmptyReport_Initially()
    {
        var svc = CreateService();
        var report = svc.GetReport();

        Assert.Equal(0, report.TotalTrips);
        Assert.Equal(0, report.TotalPassengersServed);
        Assert.Equal(0.0, report.AvgWaitTimeSeconds);
        Assert.Equal(0.0, report.MaxWaitTimeSeconds);
        Assert.Equal(3, report.ElevatorStats.Count);
        Assert.Equal(0, report.ComplianceEventCount);
    }

    [Fact]
    public void GetReport_CountsTrips_AfterCompletion()
    {
        var svc = CreateService();
        svc.CallElevator(new ElevatorCallRequest(1, 3, 70));

        // Tick enough times to complete the trip (move 2 floors + 3 DoorsOpen + 3 DoorsOpen)
        for (int i = 0; i < 20; i++) svc.Tick();

        var report = svc.GetReport();

        Assert.True(report.TotalTrips >= 1, $"Expected at least 1 trip, got {report.TotalTrips}");
        Assert.True(report.TotalFloorsTraversed >= 2);
    }

    [Fact]
    public void GetReport_IncludesPerElevatorStats()
    {
        var svc = CreateService();
        var report = svc.GetReport();

        Assert.Equal(3, report.ElevatorStats.Count);
        Assert.All(report.ElevatorStats, s =>
        {
            Assert.True(s.ElevatorId > 0);
            Assert.True(s.TripsCompleted >= 0);
        });
    }

    [Fact]
    public void GetReport_IncludesComplianceLog_AfterEmergency()
    {
        var svc = CreateService();
        svc.ActivateEmergencyMode();
        svc.DeactivateEmergencyMode();

        var report = svc.GetReport();

        Assert.Equal(2, report.ComplianceEventCount);
        Assert.Equal(2, report.ComplianceLog.Count);
    }
}
