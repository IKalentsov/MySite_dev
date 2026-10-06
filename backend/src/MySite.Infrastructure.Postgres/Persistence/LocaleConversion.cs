using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MySite.Domain.Content;

namespace MySite.Infrastructure.Postgres.Persistence;

/// <summary>Stores a <see cref="Locale"/> as its short code, in both directions.</summary>
internal static class LocaleConversion
{
    internal static ValueConverter<Locale, string> Converter { get; } = new(
        locale => locale.Value,
        value => Locale.From(value));
}
