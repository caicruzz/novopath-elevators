using ElevatorSimulator.Api.Models;
using ElevatorSimulator.Api.Services;

namespace ElevatorSimulator.Tests;

public class NearestCarStrategyTests
{
    private readonly NearestCarStrategy _strategy = new();

    private static Elevator CreateAt(int id, int floor, ElevatorState state = ElevatorState.Idle) =>
        new() { Id = id, CurrentFloor = floor, State = state };

    [Fact]
    public void SelectsNearestElevator()
    {
        var elevators = new List<Elevator>
        {
            CreateAt(1, 1),
            CreateAt(2, 5),
            CreateAt(3, 10)
        };

        var selected = _strategy.SelectElevator(elevators, 6);

        Assert.NotNull(selected);
        Assert.Equal(2, selected!.Id);
    }

    [Fact]
    public void SelectsFirst_WhenEquidistant()
    {
        var elevators = new List<Elevator>
        {
            CreateAt(1, 3),
            CreateAt(2, 7)
        };

        var selected = _strategy.SelectElevator(elevators, 5);

        Assert.NotNull(selected);
        // Both are 2 floors away; first in list wins
        Assert.Equal(1, selected!.Id);
    }

    [Fact]
    public void ExcludesMaintenanceElevators()
    {
        var elevators = new List<Elevator>
        {
            CreateAt(1, 1, ElevatorState.Maintenance),
            CreateAt(2, 10)
        };

        var selected = _strategy.SelectElevator(elevators, 2);

        Assert.NotNull(selected);
        Assert.Equal(2, selected!.Id);
    }

    [Fact]
    public void ExcludesCapacityExceededElevators()
    {
        var elevators = new List<Elevator>
        {
            CreateAt(1, 1, ElevatorState.CapacityExceeded),
            CreateAt(2, 8)
        };

        var selected = _strategy.SelectElevator(elevators, 1);

        Assert.NotNull(selected);
        Assert.Equal(2, selected!.Id);
    }

    [Fact]
    public void ExcludesEmergencyStopElevators()
    {
        var elevators = new List<Elevator>
        {
            CreateAt(1, 1, ElevatorState.EmergencyStop),
            CreateAt(2, 5)
        };

        var selected = _strategy.SelectElevator(elevators, 1);

        Assert.NotNull(selected);
        Assert.Equal(2, selected!.Id);
    }

    [Fact]
    public void ReturnsNull_WhenAllUnavailable()
    {
        var elevators = new List<Elevator>
        {
            CreateAt(1, 1, ElevatorState.Maintenance),
            CreateAt(2, 1, ElevatorState.EmergencyStop),
            CreateAt(3, 1, ElevatorState.CapacityExceeded)
        };

        var selected = _strategy.SelectElevator(elevators, 1);

        Assert.Null(selected);
    }

    [Fact]
    public void ReturnsNull_WhenListEmpty()
    {
        var selected = _strategy.SelectElevator([], 5);

        Assert.Null(selected);
    }

    [Fact]
    public void SelectsMovingElevator_WhenOnlyAvailable()
    {
        var elevators = new List<Elevator>
        {
            CreateAt(1, 3, ElevatorState.MovingUp),
            CreateAt(2, 8, ElevatorState.Maintenance)
        };

        var selected = _strategy.SelectElevator(elevators, 4);

        Assert.NotNull(selected);
        Assert.Equal(1, selected!.Id);
    }
}
