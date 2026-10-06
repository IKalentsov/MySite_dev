namespace MySite.Web.HealthChecks;

/// <summary>
/// The tags the health routes filter on. They are the contract between a probe and the endpoint that
/// reports it; the registration name is not.
/// </summary>
public static class HealthCheckTags
{
    /// <summary>The process is up.</summary>
    public const string Live = "live";

    /// <summary>The dependencies the site needs are reachable.</summary>
    public const string Ready = "ready";
}
