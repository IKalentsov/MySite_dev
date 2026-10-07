using System.Globalization;
using Microsoft.EntityFrameworkCore;
using MySite.Application.Content;
using MySite.Domain.Content;

namespace MySite.Infrastructure.Postgres.Persistence.Repositories;

public class ContentRepository : IContentRepository
{
    private readonly MySiteDbContext _context;

    public ContentRepository(MySiteDbContext context)
    {
        _context = context;
    }

    public async Task<OwnerProfile?> GetProfileAsync(CancellationToken cancellationToken)
    {
        return await _context.OwnerProfiles
            .AsNoTracking()
            .Include(ownerProfile => ownerProfile.Translations)
            .OrderBy(ownerProfile => ownerProfile.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetPublishedProjectsAsync(
        string? cursor, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Projects
            .AsNoTracking()
            .Include(project => project.Translations)
            .Include(project => project.Stack)
            .Where(project => project.IsPublished);

        // Keyset filter: (year DESC, id DESC). Cursor encodes the last seen row.
        if (cursor is not null && TryParseCursor(cursor, out var cursorYear, out var cursorId))
        {
            query = query.Where(
                project =>
                    (project.Year < cursorYear)
                    || (project.Year == cursorYear && project.Id < cursorId));
        }

        // Fetch one extra to determine hasMore.
        var projects = await query
            .OrderByDescending(project => project.Year)
            .ThenByDescending(project => project.Id)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        return [.. projects];
    }

    private static bool TryParseCursor(string cursor, out int year, out Guid id)
    {
        var parts = cursor.Split(':', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2
            || !int.TryParse(parts[0], CultureInfo.InvariantCulture, out year)
            || !Guid.TryParse(parts[1], null, out id))
        {
            year = default;
            id = Guid.Empty;
            return false;
        }

        return true;
    }
}
