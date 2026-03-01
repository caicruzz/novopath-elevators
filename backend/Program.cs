using ElevatorSimulator.Api.Models;
using ElevatorSimulator.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──
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
        var elevator = svc.CallElevator(request.Floor);
        return Results.Ok(elevator);
    }
    catch (ArgumentOutOfRangeException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
    .WithName("CallElevator");

app.Run();
