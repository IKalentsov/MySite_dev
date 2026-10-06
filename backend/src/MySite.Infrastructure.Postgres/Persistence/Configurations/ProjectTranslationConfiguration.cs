using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySite.Domain.Content;

namespace MySite.Infrastructure.Postgres.Persistence.Configurations;

/// <summary>
/// One text per locale: the unique index is what makes that true.
/// </summary>
public class ProjectTranslationConfiguration : IEntityTypeConfiguration<ProjectTranslation>
{
    public void Configure(EntityTypeBuilder<ProjectTranslation> builder)
    {
        builder.ToTable("project_translations");
        builder.HasKey(translation => translation.Id);

        builder
            .Property(translation => translation.Locale)
            .HasConversion(LocaleConversion.Converter)
            .HasMaxLength(5)
            .IsRequired();

        builder.Property(translation => translation.Title).HasMaxLength(200).IsRequired();
        builder.Property(translation => translation.Summary).HasMaxLength(500).IsRequired();

        builder
            .HasIndex(translation => new { translation.ProjectId, translation.Locale })
            .IsUnique();
    }
}
