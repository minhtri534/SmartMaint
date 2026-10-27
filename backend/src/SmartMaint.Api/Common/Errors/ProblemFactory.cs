using Microsoft.AspNetCore.Mvc;

namespace SmartMaint.Api.Common.Errors;

/// <summary>
/// Every error response is RFC 7807 ProblemDetails with an extra "code" field ("module.reason").
/// Clients switch on "code", never on "title" / "detail" text.
/// </summary>
internal static class ProblemFactory
{
    public const string CodeKey = "code";
    public const string ErrorsKey = "errors";

    public static ProblemDetails Create(HttpContext context, int status, string code, string detail)
    {
        var problem = new ProblemDetails
        {
            Status = status,
            Title = ReasonPhrase(status),
            Detail = detail,
            Instance = context.Request.Path,
        };
        problem.Extensions[CodeKey] = code;
        problem.Extensions["traceId"] = context.TraceIdentifier;
        return problem;
    }

    public static Task WriteAsync(HttpContext context, int status, string code, string detail)
    {
        context.Response.StatusCode = status;
        return context.Response.WriteAsJsonAsync(
            Create(context, status, code, detail), options: null, contentType: "application/problem+json");
    }

    private static string ReasonPhrase(int status) =>
        Microsoft.AspNetCore.WebUtilities.ReasonPhrases.GetReasonPhrase(status);
}
