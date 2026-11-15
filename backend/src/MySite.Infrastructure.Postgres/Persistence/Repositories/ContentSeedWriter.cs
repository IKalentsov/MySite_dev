using Microsoft.EntityFrameworkCore;
using MySite.Application.Content.Seeding;
using MySite.Domain.Content;

namespace MySite.Infrastructure.Postgres.Persistence.Repositories;

/// <summary>Writes a validated seed plan to the database inside one transaction.</summary>
internal sealed class ContentSeedWriter : IContentSeedWriter
{
    private readonly MySiteDbContext _context;

    /// <summary>Creates the writer with the specified database context.</summary>
    /// <param name="context">The database context used to read and persist entities.</param>
    public ContentSeedWriter(MySiteDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<ContentSeedOutcome> SeedAsync(
        ContentSeedPlan plan, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        await using var transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);

        // --- Profile (singleton) ---
        var profile = await _context.OwnerProfiles
            .Include(p => p.Translations)
            .OrderBy(p => p.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        bool profileCreated = false;
        bool profileUpdated = false;

        if (profile is null)
        {
            profile = OwnerProfile.Create(Guid.NewGuid(), now, now);
            profile.ReplaceTexts(plan.Profile.Texts);
            _context.OwnerProfiles.Add(profile);
            profileCreated = true;
        }
        else if (!ProfileMatches(profile, plan.Profile.Texts))
        {
            ReplaceProfile(profile, plan.Profile.Texts, now);
            profileUpdated = true;
        }

        // --- Projects (identity by link) ---
        var projects = await _context.Projects
            .Include(p => p.Translations)
            .Include(p => p.Stack)
            .ToListAsync(cancellationToken);

        int created = 0;
        int updated = 0;
        int unchanged = 0;

        foreach (var seed in plan.Projects)
        {
            var stored = projects.Find(p => string.Equals(p.Link, seed.Link, StringComparison.Ordinal));

            if (stored is null)
            {
                var project = Project.Create(
                    Guid.NewGuid(),
                    seed.Link,
                    seed.Year,
                    seed.SortOrder,
                    seed.IsPublished,
                    now,
                    now);
                project.ReplaceTexts(seed.Texts);
                project.ReplaceStack(seed.Stack);
                _context.Projects.Add(project);
                created++;
            }
            else if (!ProjectMatches(stored, seed))
            {
                ReplaceProject(stored, seed, now);
                updated++;
            }
            else
            {
                unchanged++;
            }
        }

        // --- Absent links (never delete) ---
        var planLinks = new HashSet<string>(StringComparer.Ordinal);
        foreach (var seed in plan.Projects)
        {
            planLinks.Add(seed.Link);
        }

        var absentLinks = projects
            .Where(p => !planLinks.Contains(p.Link))
            .Select(p => p.Link)
            .OrderBy(l => l, StringComparer.Ordinal)
            .ToList();

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new ContentSeedOutcome(
            profileCreated,
            profileUpdated,
            created,
            updated,
            unchanged,
            absentLinks);
    }

    /// <summary>Checks whether the profile's translations match the expected texts.</summary>
    private static bool ProfileMatches(
        OwnerProfile profile, IReadOnlyList<ProfileText> texts)
    {
        if (profile.Translations.Count != texts.Count)
        {
            return false;
        }

        foreach (var text in texts)
        {
            var translation = profile.Translations.FirstOrDefault(t =>
                t.Locale.Equals(text.Locale));

            if (translation is null)
            {
                return false;
            }

            if (!string.Equals(translation.Headline, text.Headline, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(translation.About, text.About, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Checks whether the project's data matches the expected seed values.</summary>
    private static bool ProjectMatches(Project project, ProjectSeed seed)
    {
        if (project.Year != seed.Year)
        {
            return false;
        }

        if (project.SortOrder != seed.SortOrder)
        {
            return false;
        }

        if (project.IsPublished != seed.IsPublished)
        {
            return false;
        }

        // Compare translations.
        var projectTexts = project.Translations.ToList();
        if (projectTexts.Count != seed.Texts.Count)
        {
            return false;
        }

        foreach (var seedText in seed.Texts)
        {
            var translation = projectTexts.FirstOrDefault(t => t.Locale.Equals(seedText.Locale));

            if (translation is null)
            {
                return false;
            }

            if (!string.Equals(translation.Title, seedText.Title, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(translation.Summary, seedText.Summary, StringComparison.Ordinal))
            {
                return false;
            }
        }

        // Compare stack.
        var stackItems = project.Stack.OrderBy(s => s.Position).ToList();
        if (stackItems.Count != seed.Stack.Count)
        {
            return false;
        }

        for (var i = 0; i < seed.Stack.Count; i++)
        {
            if (!string.Equals(stackItems[i].Name, seed.Stack[i], StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Replaces the profile's texts and attaches any newly created translations.</summary>
    private void ReplaceProfile(
        OwnerProfile profile, IReadOnlyList<ProfileText> texts, DateTime updatedAtUtc)
    {
        var knownTranslationIds = profile.Translations.Select(t => t.Id).ToHashSet();

        profile.ReplaceTexts(texts);
        profile.MarkUpdated(updatedAtUtc);

        // A row the domain creates is not known to the change tracker. Without an explicit Add,
        // EF treats it as an existing row and issues an UPDATE that matches nothing.
        profile.Translations
            .Where(t => !knownTranslationIds.Contains(t.Id))
            .ToList()
            .ForEach(t => _context.Add(t));
    }

    /// <summary>Replaces the project's data and attaches any newly created translations or stack items.</summary>
    private void ReplaceProject(Project project, ProjectSeed seed, DateTime updatedAtUtc)
    {
        var knownTranslationIds = project.Translations.Select(t => t.Id).ToHashSet();
        var knownStackItemIds = project.Stack.Select(i => i.Id).ToHashSet();

        project.Update(seed.Year, seed.SortOrder, seed.IsPublished, updatedAtUtc);
        project.ReplaceTexts(seed.Texts);
        project.ReplaceStack(seed.Stack);

        // Rows the domain creates are not known to the change tracker; attach them explicitly as
        // added, or EF will UPDATE a key that does not exist yet.
        project.Translations
            .Where(t => !knownTranslationIds.Contains(t.Id))
            .ToList()
            .ForEach(t => _context.Add(t));

        project.Stack
            .Where(i => !knownStackItemIds.Contains(i.Id))
            .ToList()
            .ForEach(i => _context.Add(i));
    }
}
