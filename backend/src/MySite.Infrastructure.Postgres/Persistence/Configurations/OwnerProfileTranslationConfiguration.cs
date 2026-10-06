using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySite.Domain.Content;

namespace MySite.Infrastructure.Postgres.Persistence.Configurations;

/// <summary>
/// One text per locale: the unique index is what makes that true, so a second Russian version of
/// the same profile cannot be written by accident.
/// </summary>
public class OwnerProfileTranslationConfiguration : IEntityTypeConfiguration<OwnerProfileTranslation>
{
    public void Configure(EntityTypeBuilder<OwnerProfileTranslation> builder)
    {
        builder.ToTable("owner_profile_translations");
        builder.HasKey(translation => translation.Id);

        builder
            .Property(translation => translation.Locale)
            .HasConversion(LocaleConversion.Converter)
            .HasMaxLength(5)
            .IsRequired();

        builder.Property(translation => translation.Headline).HasMaxLength(200).IsRequired();
        builder.Property(translation => translation.About).IsRequired();

        builder
            .HasIndex(translation => new { translation.OwnerProfileId, translation.Locale })
            .IsUnique();
    }
}
