namespace SmartMaint.Api.Common.Errors;

/// <summary>
/// The only exception a service throws on purpose. <see cref="Code"/> is a stable machine-readable
/// key ("module.reason", e.g. "auth.invalid_credentials") that the web/mobile clients switch on.
/// Define codes in the module's *Errors class, never inline strings in services.
/// </summary>
public sealed class AppException(ErrorKind kind, string code, string message) : Exception(message)
{
    public ErrorKind Kind { get; } = kind;
    public string Code { get; } = code;
}
