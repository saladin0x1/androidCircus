using System.Diagnostics;

namespace API.Middleware;

/// <summary>
/// Middleware to log incoming HTTP requests with timing and status codes
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;

        // Log request start
        _logger.LogInformation("Incoming request: {Method} {Path} from {RemoteIpAddress}",
            request.Method,
            request.Path,
            context.Connection.RemoteIpAddress?.ToString() ?? "Unknown");

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // Log request completion
            var statusCode = context.Response.StatusCode;
            var duration = stopwatch.ElapsedMilliseconds;

            var logLevel = statusCode >= 500 ? LogLevel.Error :
                           statusCode >= 400 ? LogLevel.Warning :
                           LogLevel.Information;

            _logger.Log(logLevel, "Request completed: {Method} {Path} - {StatusCode} in {Duration}ms",
                request.Method,
                request.Path,
                statusCode,
                duration);
        }
    }
}

/// <summary>
/// Extension method for registering the middleware
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
