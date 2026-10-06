namespace MySite.Domain.Content;

/// <summary>
/// One technology shown on a project. It is its own row so the list keeps an order and stays
/// queryable, rather than being packed into a single text column.
/// </summary>
public class ProjectStackItem
{
    // Entity Framework materialises through this constructor.
    private ProjectStackItem()
    {
    }

    private ProjectStackItem(Guid id, Guid projectId, string name, int position)
    {
        Id = id;
        ProjectId = projectId;
        Name = name;
        Position = position;
    }

    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public int Position { get; private set; }

    public static ProjectStackItem Create(Guid projectId, string name, int position) =>
        new(Guid.NewGuid(), projectId, name, position);
}
