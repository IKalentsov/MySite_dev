namespace MySite.Domain.Interfaces;

public interface IAuditable
{
    public Guid Id { get; }
    public DateTime Created { get; }
    public DateTime Modified { get; }
}
