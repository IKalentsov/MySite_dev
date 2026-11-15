using System.Text.Json;
using System.Text.Json.Serialization;

namespace MySite.Application.Content.Seeding;

/// <summary>Parses the raw JSON content file into a strongly-typed structure.</summary>
internal static class ContentSeedParser
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        AllowDuplicateProperties = false
    };

    /// <summary>Parses the JSON string into a <see cref="SiteContentFile"/>.</summary>
    /// <param name="json">The raw JSON content.</param>
    /// <returns>The parsed content file structure.</returns>
    /// <exception cref="JsonException">The JSON is invalid or the result is null.</exception>
    internal static SiteContentFile Parse(string json)
    {
        var result = JsonSerializer.Deserialize<SiteContentFile>(json, Options);

        if (result is null)
        {
            throw new JsonException("The content file is empty.");
        }

        return result;
    }
}
