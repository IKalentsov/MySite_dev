using Microsoft.EntityFrameworkCore;
using MySite.Domain.Content;
using MySite.Infrastructure.Postgres.Entities;

namespace MySite.Infrastructure.Postgres.Persistence;

public class MySiteDbContext : DbContext
{
    public MySiteDbContext(DbContextOptions<MySiteDbContext> options)
    : base(options)
    {

    }

    public DbSet<UserEntity> Users { get; set; }

    public DbSet<OwnerProfile> OwnerProfiles { get; set; }

    public DbSet<Project> Projects { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Every IEntityTypeConfiguration in this assembly is picked up. Without this the
        // configurations exist but nothing applies them.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MySiteDbContext).Assembly);
    }
}
