using ElevatorSimulator.Api.Models;
using ElevatorSimulator.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──
builder.Services.AddSingleton<IElevatorDispatchStrategy, NearestCarStrategy>();
builder.Services.AddSingleton<BuildingService>();
builder.Services.AddHostedService<SimulationBackgroundService>();

// ── CORS (allow React dev server) ──
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();

// ── Endpoints ──

app.MapGet("/api/building/state", (BuildingService svc) => Results.Ok(svc.GetState()))
    .WithName("GetBuildingState");

app.MapPost("/api/elevator/call", (ElevatorCallRequest request, BuildingService svc) =>
{
    try
    {
        var elevator = svc.CallElevator(request);
        return Results.Ok(elevator);
    }
    catch (ArgumentOutOfRangeException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.UnprocessableEntity(new { error = ex.Message });
    }
})
    .WithName("CallElevator");

app.MapPost("/api/building/configure", (ConfigureRequest request, BuildingService svc) =>
{
    try
    {
        svc.ConfigureBuilding(request);
        return Results.Ok(svc.GetState());
    }
    catch (ArgumentOutOfRangeException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
    .WithName("ConfigureBuilding");

app.MapPost("/api/building/emergency/activate", (BuildingService svc) =>
{
    var state = svc.ActivateEmergencyMode();
    return Results.Ok(state);
})
    .WithName("ActivateEmergency");

app.MapPost("/api/building/emergency/deactivate", (BuildingService svc) =>
{
    var state = svc.DeactivateEmergencyMode();
    return Results.Ok(state);
})
    .WithName("DeactivateEmergency");

app.Run();
