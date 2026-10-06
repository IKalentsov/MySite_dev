using Microsoft.EntityFrameworkCore;
using MySite.Infrastructure.Postgres.Entities;

namespace MySite.Infrastructure.Postgres.Persistence;

public class MySiteDbContext : DbContext
{
    public MySiteDbContext(DbContextOptions<MySiteDbContext> options)
    : base(options)
    {

    }

    public DbSet<UserEntity> Users { get; set; }
}
