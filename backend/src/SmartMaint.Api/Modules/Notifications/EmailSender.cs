using System.Net;
using System.Net.Http.Json;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace SmartMaint.Api.Modules.Notifications;

/// <summary>
/// SMTP settings (section "Email", environment Email__Host...). Meeting note 18/11 (decision 1): the temporary password
/// of a new or reset account goes out by e-mail. Empty Host = no e-mail is sent (local development): the Administrator
/// then sees the temporary password on screen, as before.
/// </summary>
public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string? Host { get; init; }
    public int Port { get; init; } = 587;
    public string? Username { get; init; }
    public string? Password { get; init; }

    /// <summary>The sender address, e.g. no-reply@smartmaint.app (often the SMTP account itself).</summary>
    public string? From { get; init; }

    public string FromName { get; init; } = "SmartMaint";

    /// <summary>STARTTLS (Gmail, Outlook, SendGrid on 587 all need it).</summary>
    public bool EnableSsl { get; init; } = true;

    /// <summary>The web app's address, written in the mail as the place to sign in.</summary>
    public string? WebUrl { get; init; }

    /// <summary>
    /// Brevo API key (Email__BrevoApiKey). Railway's Hobby plan blocks outgoing SMTP, so the deployed API sends through
    /// Brevo's HTTPS API instead; set, it replaces SMTP (Host, Port, Username, Password are then unused). From must be a
    /// sender verified in Brevo.
    /// </summary>
    public string? BrevoApiKey { get; init; }

    public bool UseBrevo => !string.IsNullOrWhiteSpace(BrevoApiKey);

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(From);
}

/// <summary>Sends one plain-text e-mail. Never throws: false = not sent (not set up, or the server refused).</summary>
public interface IEmailSender
{
    Task<bool> SendAsync(string to, string subject, string body, CancellationToken cancellationToken);
}

/// <summary>With .NET's own SmtpClient: no extra library (CLAUDE.md: ask before adding one).</summary>
internal sealed class SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task<bool> SendAsync(string to, string subject, string body, CancellationToken cancellationToken)
    {
        var o = options.Value;
        if (!o.IsConfigured)
        {
            logger.LogWarning("E-mail is not set up (Email__Host / Email__From): \"{Subject}\" to {To} was not sent", subject, to);
            return false;
        }

        try
        {
            using var message = new MailMessage(new MailAddress(o.From!, o.FromName), new MailAddress(to))
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = false,
            };
            using var client = new SmtpClient(o.Host, o.Port)
            {
                EnableSsl = o.EnableSsl,
                Timeout = 15_000,
                Credentials = string.IsNullOrEmpty(o.Username) ? null : new NetworkCredential(o.Username, o.Password),
            };
            await client.SendMailAsync(message, cancellationToken);
            return true;
        }
        catch (Exception e) when (e is SmtpException or InvalidOperationException or FormatException)
        {
            logger.LogError(e, "Sending \"{Subject}\" to {To} failed", subject, to);
            return false;
        }
    }
}

/// <summary>
/// Sends through Brevo's transactional e-mail API over HTTPS (POST /v3/smtp/email) for hosts that block SMTP, such as
/// Railway's Hobby plan. Plain HttpClient and System.Text.Json: no extra library.
/// </summary>
internal sealed class BrevoEmailSender(HttpClient http, IOptions<EmailOptions> options, ILogger<BrevoEmailSender> logger)
    : IEmailSender
{
    public const string Endpoint = "https://api.brevo.com/v3/smtp/email";

    public async Task<bool> SendAsync(string to, string subject, string body, CancellationToken cancellationToken)
    {
        var o = options.Value;
        if (!o.UseBrevo || string.IsNullOrWhiteSpace(o.From))
        {
            logger.LogWarning("Brevo is not set up (Email__BrevoApiKey / Email__From): \"{Subject}\" to {To} was not sent", subject, to);
            return false;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
        {
            Content = JsonContent.Create(new
            {
                sender = new { name = o.FromName, email = o.From },
                to = new[] { new { email = to } },
                subject,
                textContent = body,
            }),
        };
        request.Headers.Add("api-key", o.BrevoApiKey);
        request.Headers.Accept.ParseAdd("application/json");

        try
        {
            using var response = await http.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            var reason = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Brevo refused \"{Subject}\" to {To}: {Status} {Reason}",
                subject, to, (int)response.StatusCode, reason.Length > 300 ? reason[..300] : reason);
            return false;
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            logger.LogError(e, "Sending \"{Subject}\" to {To} through Brevo failed", subject, to);
            return false;
        }
    }
}
