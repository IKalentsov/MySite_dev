using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySite.DataAccess.Entities;
using MySite.Domain.Models;

namespace MySite.DataAccess.Persistence.Configurations;
public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Login)
            .HasMaxLength(User.MAX_TITLE_LENGTH)
            .IsRequired();

        builder.Property(u => u.Created)
            .IsRequired();

        builder.Property(u => u.Email)
            .IsRequired();

        builder.Property(u => u.FirstName)
            .HasMaxLength(User.MAX_TITLE_LENGTH)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasMaxLength(User.MAX_TITLE_LENGTH)
            .IsRequired();

        builder.Property(u => u.Modified)
            .IsRequired();

        builder.Property(u => u.ProfileImage)
            .HasMaxLength(User.MAX_TITLE_LENGTH);

        builder.Property(u => u.Right)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .IsRequired();
    }
}
