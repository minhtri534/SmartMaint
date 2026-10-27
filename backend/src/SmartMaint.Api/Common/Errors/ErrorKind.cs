namespace SmartMaint.Api.Common.Errors;

/// <summary>Maps 1-1 to an HTTP status in the API's GlobalExceptionHandler.</summary>
public enum ErrorKind
{
    BadRequest,
    Unauthorized,
    Forbidden,
    NotFound,
    Conflict,
}
