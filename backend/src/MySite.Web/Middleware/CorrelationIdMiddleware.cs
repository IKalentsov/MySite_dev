using Microsoft.Extensions.Primitives;

namespace MySite.Web.Middleware;

/// <summary>
/// Gives every request a correlation id: the inbound <c>X-Correlation-Id</c> header when the caller
/// sent one, a generated identifier otherwise. It is echoed on the response and put into the log
/// scope, so a log line and a response can be tied together.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    /// <summary>The header read from the request and written back on the response.</summary>
    public const string HeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    /// <summary>Creates the middleware.</summary>
    /// <param name="next">The next stage of the request pipeline.</param>
    /// <param name="logger">The logger whose scope carries the correlation id.</param>
    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>Resolves the correlation id and runs the rest of the pipeline inside its scope.</summary>
    /// <param name="context">The current request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var correlationId = ResolveCorrelationId(context);

        context.Response.Headers[HeaderName] = correlationId;

        var scope = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [nameof(correlationId)] = correlationId
        };

        using (_logger.BeginScope(scope))
        {
            await _next(context);
        }
    }

    private static string ResolveCorrelationId(HttpContext context) =>
        context.Request.Headers.TryGetValue(HeaderName, out StringValues values)
        && !StringValues.IsNullOrEmpty(values)
            ? values.ToString()
            : context.TraceIdentifier;
}
