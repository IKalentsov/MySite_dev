using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySite.Domain.Content;

namespace MySite.Infrastructure.Postgres.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");
        builder.HasKey(project => project.Id);

        builder.Property(project => project.Link).HasMaxLength(500).IsRequired();
        builder.Property(project => project.Year).IsRequired();
        builder.Property(project => project.SortOrder).IsRequired();
        builder.Property(project => project.IsPublished).IsRequired();
        builder.Property(project => project.CreatedAtUtc).IsRequired();
        builder.Property(project => project.UpdatedAtUtc).IsRequired();

        builder
            .HasMany(project => project.Translations)
            .WithOne()
            .HasForeignKey(translation => translation.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(project => project.Stack)
            .WithOne()
            .HasForeignKey(item => item.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Every read filters on the first and orders by the second.
        builder.HasIndex(project => project.IsPublished);
        builder.HasIndex(project => project.SortOrder);
    }
}
