using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySite.Domain.Content;

namespace MySite.Infrastructure.Postgres.Persistence.Configurations;

/// <summary>
/// The technology list of a project, one row per entry, so it keeps its order and stays queryable.
/// </summary>
public class ProjectStackItemConfiguration : IEntityTypeConfiguration<ProjectStackItem>
{
    public void Configure(EntityTypeBuilder<ProjectStackItem> builder)
    {
        builder.ToTable("project_stack_items");
        builder.HasKey(item => item.Id);

        builder.Property(item => item.Name).HasMaxLength(100).IsRequired();
        builder.Property(item => item.Position).IsRequired();

        builder.HasIndex(item => new { item.ProjectId, item.Position }).IsUnique();
    }
}
