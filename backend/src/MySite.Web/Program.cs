using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySite.Application.Content;
using MySite.Application.Interfaces;
using MySite.Application.Interfaces.Auth;
using MySite.Application.Services;
using MySite.Infrastructure.Postgres.Persistence;
using MySite.Infrastructure.Postgres.Persistence.Repositories;
using MySite.Infrastructure.Postgres.Services;
using MySite.Web.HealthChecks;
using MySite.Web.Middleware;
using Scalar.AspNetCore;

const string LivenessCheck = "self";
const string ReadinessCheck = "postgres";

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddControllers();
services.AddOpenApi();

// One error shape for the whole API, in the RFC 7807 form.
services.AddProblemDetails();

// Liveness answers whether the process is up; readiness answers whether it can serve traffic.
services.AddHealthChecks()
    .AddCheck(LivenessCheck, () => HealthCheckResult.Healthy())
    .AddCheck<PostgresHealthCheck>(ReadinessCheck);

services.AddDbContext<MySiteDbContext>(options =>
{
    options
        .UseNpgsql(configuration.GetConnectionString(nameof(MySiteDbContext)))
        .UseSnakeCaseNamingConvention();
});

services.AddScoped<IContentRepository, ContentRepository>();
services.AddScoped<IUsersRepository, UsersRepository>();
services.AddScoped<IJwtProvider, JwtProvider>();
services.AddScoped<IPasswordHasher, PasswordHasher>();
services.AddScoped<UsersService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseMiddleware<CorrelationIdMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = registration => string.Equals(registration.Name, LivenessCheck, StringComparison.Ordinal)
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => string.Equals(registration.Name, ReadinessCheck, StringComparison.Ordinal)
});

await app.RunAsync();
