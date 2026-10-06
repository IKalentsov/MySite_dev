namespace MySite.Web.HealthChecks;

/// <summary>The names the health probes are registered under.</summary>
public static class HealthCheckNames
{
    /// <summary>Liveness: the process itself.</summary>
    public const string Self = "self";

    /// <summary>Readiness: the database the site reads from.</summary>
    public const string Postgres = "postgres";
}
