using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MyCompany.MinimalApi.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // The client disconnected: nothing to send back, and it is not a server error.
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            return true;
        }

        var isBadRequest = exception is ArgumentException or BadHttpRequestException;

        if (isBadRequest)
        {
            _logger.LogWarning(exception, "Bad request on {Path}", httpContext.Request.Path);
        }
        else
        {
            _logger.LogError(exception, "Unhandled exception on {Path}", httpContext.Request.Path);
        }

        var problemDetails = new ProblemDetails
        {
            Status = isBadRequest ? StatusCodes.Status400BadRequest : StatusCodes.Status500InternalServerError,
            Title = isBadRequest ? "Bad Request" : "Server Error",
            Detail = isBadRequest ? exception.Message : "An unexpected error occurred on the server.",
            Instance = httpContext.Request.Path
        };
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            options: null,
            contentType: "application/problem+json",
            cancellationToken);

        return true;
    }
}