using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SmartMaint.Api.Modules.Notifications;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Notifications;

/// <summary>Sending the temporary password through Brevo's HTTPS API (Railway's Hobby plan blocks SMTP).</summary>
public sealed class BrevoEmailSenderTests
{
    private static readonly EmailOptions Configured = new()
    {
        BrevoApiKey = "test-key",
        From = "sender@example.com",
        FromName = "SmartMaint",
    };

    private static (BrevoEmailSender Sender, FakeHandler Handler) Build(EmailOptions options, HttpStatusCode status = HttpStatusCode.Created)
    {
        var handler = new FakeHandler(status);
        var sender = new BrevoEmailSender(new HttpClient(handler), Options.Create(options), NullLogger<BrevoEmailSender>.Instance);
        return (sender, handler);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Send_PostsTheMailToBrevoWithTheKey()
    {
        var (sender, handler) = Build(Configured);

        var sent = await sender.SendAsync("user@example.com", "Your SmartMaint account", "Password: abc", CancellationToken.None);

        Assert.True(sent);
        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal(BrevoEmailSender.Endpoint, request.Uri);
        Assert.Equal("test-key", request.ApiKey);
        using var json = JsonDocument.Parse(request.Body);
        var root = json.RootElement;
        Assert.Equal("sender@example.com", root.GetProperty("sender").GetProperty("email").GetString());
        Assert.Equal("SmartMaint", root.GetProperty("sender").GetProperty("name").GetString());
        Assert.Equal("user@example.com", root.GetProperty("to")[0].GetProperty("email").GetString());
        Assert.Equal("Your SmartMaint account", root.GetProperty("subject").GetString());
        Assert.Equal("Password: abc", root.GetProperty("textContent").GetString());
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Send_BrevoRefuses_ReturnsFalseInsteadOfThrowing()
    {
        var (sender, _) = Build(Configured, HttpStatusCode.Unauthorized);

        var sent = await sender.SendAsync("user@example.com", "Subject", "Body", CancellationToken.None);

        Assert.False(sent);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Send_NetworkFails_ReturnsFalse()
    {
        var handler = new FakeHandler(HttpStatusCode.Created) { Throw = true };
        var sender = new BrevoEmailSender(new HttpClient(handler), Options.Create(Configured), NullLogger<BrevoEmailSender>.Instance);

        var sent = await sender.SendAsync("user@example.com", "Subject", "Body", CancellationToken.None);

        Assert.False(sent);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Send_KeyButNoSender_SendsNothing()
    {
        var (sender, handler) = Build(new EmailOptions { BrevoApiKey = "test-key", From = " " });

        var sent = await sender.SendAsync("user@example.com", "Subject", "Body", CancellationToken.None);

        Assert.False(sent);
        Assert.Empty(handler.Requests);
    }

    [Theory]
    [Trait(TestType.Key, TestType.Boundary)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("k", true)]
    public void UseBrevo_OnlyWithAKey(string? key, bool expected)
    {
        Assert.Equal(expected, new EmailOptions { BrevoApiKey = key }.UseBrevo);
    }

    private sealed record Captured(HttpMethod Method, string? Uri, string? ApiKey, string Body);

    private sealed class FakeHandler(HttpStatusCode status) : HttpMessageHandler
    {
        public bool Throw { get; init; }
        public List<Captured> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (Throw)
            {
                throw new HttpRequestException("network down");
            }

            var body = request.Content is null ? "" : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add(new Captured(
                request.Method,
                request.RequestUri?.ToString(),
                request.Headers.TryGetValues("api-key", out var keys) ? keys.Single() : null,
                body));
            return new HttpResponseMessage(status) { Content = new StringContent("{\"message\":\"x\"}") };
        }
    }
}
