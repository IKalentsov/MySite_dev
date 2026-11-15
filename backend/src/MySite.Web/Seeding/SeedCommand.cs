using System.Data.Common;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using MySite.Application.Content.Seeding;

namespace MySite.Web.Seeding;

/// <summary>Implements the "seed" command that reads a content file and writes it to the database.</summary>
public static class SeedCommand
{
    /// <summary>The verb that triggers this command.</summary>
    public const string Verb = "seed";

    /// <summary>Returns true when the first argument is the seed verb.</summary>
    /// <param name="args">The command-line arguments passed to the application.</param>
    /// <returns>True if the seed command is requested.</returns>
    public static bool IsRequested(string[] args)
    {
        return args.Length > 0 && string.Equals(args[0], Verb, StringComparison.Ordinal);
    }

    /// <summary>Runs the seed command: reads a file and writes it to the database.</summary>
    /// <param name="services">The service provider for resolving dependencies.</param>
    /// <param name="args">The full command-line arguments (verb + options).</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>Zero on success, a non-zero exit code on failure.</returns>
    public static async Task<int> RunAsync(
        IServiceProvider services, string[] args, CancellationToken cancellationToken)
    {
        var filePath = ParseFilePath(args);

        if (filePath is null)
        {
            await Console.Error.WriteLineAsync(
                "The 'seed' command needs a file: --file <path>.");
            return 1;
        }

        string json;

        try
        {
            json = await File.ReadAllTextAsync(filePath, cancellationToken);
        }
        catch (IOException exception)
        {
            await Console.Error.WriteLineAsync(
                $"Cannot read file '{filePath}': {exception.Message}.");
            return 1;
        }
        catch (UnauthorizedAccessException exception)
        {
            await Console.Error.WriteLineAsync(
                $"Access denied to file '{filePath}': {exception.Message}.");
            return 1;
        }

        var fullFilePath = Path.GetFullPath(filePath);

        try
        {
            var service = services.GetRequiredService<ISeedContentService>();
            var result = await service.SeedAsync(json, cancellationToken);

            if (result.Status == SeedContentStatus.Invalid)
            {
                foreach (var error in result.Errors)
                {
                    await Console.Error.WriteLineAsync(error);
                }

                return 1;
            }

            var outcome = result.Outcome!;

            await Console.Out.WriteLineAsync($"Seeding from '{fullFilePath}'.");
            string profileStatus;

            if (outcome.ProfileCreated)
            {
                profileStatus = "Profile: created.";
            }
            else if (outcome.ProfileUpdated)
            {
                profileStatus = "Profile: updated.";
            }
            else
            {
                profileStatus = "Profile: unchanged.";
            }

            await Console.Out.WriteLineAsync(profileStatus);

            await Console.Out.WriteLineAsync(
                $"Projects: created {Count(outcome.ProjectsCreated)}, updated {Count(outcome.ProjectsUpdated)}, unchanged {Count(outcome.ProjectsUnchanged)}.");

            var gaps = result.TranslationGaps;
            if (gaps.Count == 0)
            {
                await Console.Out.WriteLineAsync("Translation gaps (deliberate): none");
            }
            else
            {
                await Console.Out.WriteLineAsync(
                    "Translation gaps (deliberate): " + string.Join("; ", gaps));
            }

            var absent = outcome.AbsentProjectLinks;
            if (absent.Count == 0)
            {
                await Console.Out.WriteLineAsync("Projects in the database but not in the file: none");
            }
            else
            {
                await Console.Out.WriteLineAsync(
                    "Projects in the database but not in the file: " + string.Join("; ", absent));
            }

            return 0;
        }
        catch (DbException exception)
        {
            await Console.Error.WriteLineAsync(
                $"The seed failed while writing to the database: {exception.Message}.");
            return 2;
        }
    }

    private static string? ParseFilePath(string[] args)
    {
        for (var i = 1; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], "--file", StringComparison.Ordinal))
            {
                return args[i + 1];
            }
        }

        return null;
    }

    private static string Count(int value) => value.ToString(CultureInfo.InvariantCulture);
}
