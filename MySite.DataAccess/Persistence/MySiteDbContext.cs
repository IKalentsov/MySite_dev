using Microsoft.EntityFrameworkCore;
using MySite.DataAccess.Entities;

namespace MySite.DataAccess.Persistence;

public class MySiteDbContext : DbContext
{
    public MySiteDbContext(DbContextOptions<MySiteDbContext> options)
    : base(options)
    {

    }

    public DbSet<UserEntity> Users { get; set; }
}
