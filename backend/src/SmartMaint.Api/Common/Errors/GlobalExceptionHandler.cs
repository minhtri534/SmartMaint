using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace SmartMaint.Api.Common.Errors;

/// <summary>
/// Turns exceptions into ProblemDetails. Controllers and services never build error responses themselves:
/// services throw <see cref="AppException"/>, validators throw <see cref="ValidationException"/>.
/// </summary>
internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case AppException app:
                await ProblemFactory.WriteAsync(context, ToStatus(app.Kind), app.Code, app.Message);
                return true;

            case ValidationException validation:
            {
                var problem = ProblemFactory.Create(
                    context, StatusCodes.Status400BadRequest, "validation.failed", "One or more fields are invalid.");
                problem.Extensions[ProblemFactory.ErrorsKey] = validation.Errors
                    .GroupBy(e => ToCamelCase(e.PropertyName))
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(problem, options: null, "application/problem+json", cancellationToken);
                return true;
            }

            default:
                logger.LogError(exception, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
                await ProblemFactory.WriteAsync(
                    context, StatusCodes.Status500InternalServerError, "server.error", "An unexpected error occurred.");
                return true;
        }
    }

    private static int ToStatus(ErrorKind kind) => kind switch
    {
        ErrorKind.BadRequest => StatusCodes.Status400BadRequest,
        ErrorKind.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorKind.Forbidden => StatusCodes.Status403Forbidden,
        ErrorKind.NotFound => StatusCodes.Status404NotFound,
        ErrorKind.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError,
    };

    private static string ToCamelCase(string name) =>
        string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name[1..];
}
