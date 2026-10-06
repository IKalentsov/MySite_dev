using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySite.Domain.Content;

namespace MySite.Infrastructure.Postgres.Persistence.Configurations;

public class OwnerProfileConfiguration : IEntityTypeConfiguration<OwnerProfile>
{
    public void Configure(EntityTypeBuilder<OwnerProfile> builder)
    {
        builder.ToTable("owner_profiles");
        builder.HasKey(ownerProfile => ownerProfile.Id);

        builder.Property(ownerProfile => ownerProfile.CreatedAtUtc).IsRequired();
        builder.Property(ownerProfile => ownerProfile.UpdatedAtUtc).IsRequired();

        builder
            .HasMany(ownerProfile => ownerProfile.Translations)
            .WithOne()
            .HasForeignKey(translation => translation.OwnerProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
