using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MySite.Web;
using MySite.Web.HealthChecks;
using MySite.Web.Middleware;
using MySite.Web.Seeding;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProgramDependencies(builder.Configuration);

var app = builder.Build();

if (SeedCommand.IsRequested(args))
{
    await using var scope = app.Services.CreateAsyncScope();
    return await SeedCommand.RunAsync(scope.ServiceProvider, args, CancellationToken.None);
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains(HealthCheckTags.Live)
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains(HealthCheckTags.Ready)
});

await app.RunAsync();
return 0;
