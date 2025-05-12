using Microsoft.EntityFrameworkCore;
using MySite.Application.Interfaces;
using MySite.Application.Interfaces.Auth;
using MySite.DataAccess.Persistence;
using MySite.DataAccess.Persistence.Repositories;
using MySite.DataAccess.Services;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;
// Add services to the container.

services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddDbContext<MySiteDbContext>(options =>
{
    options.UseNpgsql(configuration.GetConnectionString(nameof(MySiteDbContext)));
});

services.AddScoped<IUsersRepository, UsersRepository>();
services.AddScoped<IJwtProvider, JwtProvider>();
services.AddScoped<IPasswordHasher, PasswordHasher>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
