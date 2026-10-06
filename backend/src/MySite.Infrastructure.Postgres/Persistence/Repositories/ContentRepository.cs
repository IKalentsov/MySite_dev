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

    public async Task<IReadOnlyList<Project>> GetPublishedProjectsAsync(CancellationToken cancellationToken)
    {
        return await _context.Projects
            .AsNoTracking()
            .Include(project => project.Translations)
            .Include(project => project.Stack)
            .Where(project => project.IsPublished)
            .OrderBy(project => project.SortOrder)
            .ThenByDescending(project => project.Year)
            .ToListAsync(cancellationToken);
    }
}
