using System.Globalization;

namespace MySite.Application.Content;

/// <summary>
/// A position in the projects' keyset order: the year and the identifier of the last row read.
/// It crosses the wire in the form <c>{year}:{id}</c>, and this type is the only place that knows
/// that form — the repository filters by the values, the controller only checks that it parses.
/// </summary>
public sealed record ProjectCursor
{
    /// <summary>Creates a cursor.</summary>
    /// <param name="year">The year of the row the cursor points at.</param>
    /// <param name="id">The identifier of the row the cursor points at.</param>
    public ProjectCursor(int year, Guid id)
    {
        Year = year;
        Id = id;
    }

    /// <summary>The year of the row the cursor points at.</summary>
    public int Year { get; }

    /// <summary>The identifier of the row the cursor points at.</summary>
    public Guid Id { get; }

    /// <summary>Reads a cursor from its wire form.</summary>
    /// <param name="value">The cursor as it arrived, for example <c>2024:6f9619ff-8b86-d011-b42d-00c04fc964ff</c>.</param>
    /// <param name="cursor">The parsed cursor, or null when the value is not a cursor.</param>
    /// <returns>Whether the value is a cursor.</returns>
    public static bool TryParse(string? value, out ProjectCursor? cursor)
    {
        cursor = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var parts = value.Split(':', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2
            || !int.TryParse(parts[0], CultureInfo.InvariantCulture, out var year)
            || !Guid.TryParse(parts[1], null, out var id))
        {
            return false;
        }

        cursor = new ProjectCursor(year, id);
        return true;
    }

    /// <inheritdoc />
    public override string ToString() => $"{Year}:{Id}";
}
