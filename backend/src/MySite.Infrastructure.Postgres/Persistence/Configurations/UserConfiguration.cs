using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySite.Domain.Models;
using MySite.Infrastructure.Postgres.Entities;

namespace MySite.Infrastructure.Postgres.Persistence.Configurations;
public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Login)
            .HasMaxLength(User.MaxTitleLength)
            .IsRequired();

        builder.Property(u => u.Created)
            .IsRequired();

        builder.Property(u => u.Email)
            .IsRequired();

        builder.Property(u => u.FirstName)
            .HasMaxLength(User.MaxTitleLength)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasMaxLength(User.MaxTitleLength)
            .IsRequired();

        builder.Property(u => u.Modified)
            .IsRequired();

        builder.Property(u => u.ProfileImage)
            .HasMaxLength(User.MaxTitleLength);

        builder.Property(u => u.Right)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .IsRequired();
    }
}
