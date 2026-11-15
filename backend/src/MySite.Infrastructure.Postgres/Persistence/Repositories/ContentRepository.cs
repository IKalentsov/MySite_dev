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
        ProjectCursor? cursor, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Projects
            .AsNoTracking()
            .Include(project => project.Translations)
            .Include(project => project.Stack)
            .Where(project => project.IsPublished);

        // Keyset filter: (year DESC, id DESC). The cursor is the last row the caller has seen.
        if (cursor is not null)
        {
            var cursorYear = cursor.Year;
            var cursorId = cursor.Id;

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
}
