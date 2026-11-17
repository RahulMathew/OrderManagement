using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace OrderManagement.Presentation.ExceptionHandling
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var traceId = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;

            // Record the exception against the current trace.
            Activity.Current?.SetStatus(ActivityStatusCode.Error, exception.Message);

            // Structured error log including the exception.
            _logger.LogError(
                exception,
                "Unhandled exception. TraceId: {TraceId}, " +
                "Path: {RequestPath}, Method: {Method}",
                traceId,
                httpContext.Request.Path,
                httpContext.Request.Method);

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = _environment.IsDevelopment()
                    ? exception.Message
                    : "An internal server error occurred.",
                Instance = httpContext.Request.Path
            };

            problem.Extensions["traceId"] = traceId;

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

            return true;
        }
    }
}
