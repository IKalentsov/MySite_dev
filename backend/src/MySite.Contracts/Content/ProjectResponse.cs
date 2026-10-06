namespace MySite.Contracts.Content;

/// <summary>One project shown on the site, already resolved to one locale.</summary>
/// <param name="Title">The project's name.</param>
/// <param name="Summary">A short description of the project.</param>
/// <param name="Stack">The technologies the project was built with, in display order.</param>
/// <param name="Link">A link to the repository or to a live demo.</param>
/// <param name="Year">The year the project was made.</param>
public sealed record ProjectResponse(
    string Title,
    string Summary,
    IReadOnlyList<string> Stack,
    string Link,
    int Year);

/// <summary>The published projects, most relevant first.</summary>
/// <param name="Projects">The projects.</param>
public sealed record ProjectListResponse(IReadOnlyList<ProjectResponse> Projects);
