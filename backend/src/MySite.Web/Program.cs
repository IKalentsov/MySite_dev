using Microsoft.EntityFrameworkCore;
using MySite.Application.Interfaces;
using MySite.Application.Interfaces.Auth;
using MySite.Application.Services;
using MySite.Infrastructure.Postgres.Persistence;
using MySite.Infrastructure.Postgres.Persistence.Repositories;
using MySite.Infrastructure.Postgres.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddControllers();
services.AddOpenApi();

services.AddDbContext<MySiteDbContext>(options =>
{
    options.UseNpgsql(configuration.GetConnectionString(nameof(MySiteDbContext)));
});

services.AddScoped<IUsersRepository, UsersRepository>();
services.AddScoped<IJwtProvider, JwtProvider>();
services.AddScoped<IPasswordHasher, PasswordHasher>();
services.AddScoped<UsersService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
