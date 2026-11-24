using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SmartMaint.Api.Common.Ai;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Ai;
using SmartMaint.Api.Modules.Ai.Dtos;
using SmartMaint.Api.Modules.Ai.Validators;
using SmartMaint.Api.Modules.Cases;
using SmartMaint.Api.Modules.Notifications;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Ai;

/// <summary>EMP-02 AI suggestion (F3 section 4, point 1): suggestion only, output checked, every question kept.</summary>
public sealed class AiServiceTests : IDisposable
{
    private readonly FixedTimeProvider _clock = new(new DateTimeOffset(2026, 11, 16, 8, 0, 0, TimeSpan.Zero));
    private readonly FakeCurrentUser _me = new();
    private readonly FakeAiGateway _ai = new();
    private readonly AppDbContext _db;
    private readonly AiService _sut;
    private readonly CaseCategory _notCooling;
    private readonly CaseCategory _burstPipe;

    private const string Description = "The air conditioner in room 501 blows warm air since this morning.";

    public AiServiceTests()
    {
        _db = TestDbContextFactory.Create(_clock);
        var customer = new Customer("customer", "Customer", "hash", false, roomId: 1);
        var normal = new PriorityLevel { Code = CasePriority.Normal, Name = "Normal", SlaHours = 24 };
        var emergency = new PriorityLevel { Code = CasePriority.Emergency, Name = "Emergency", SlaHours = 2 };
        _notCooling = new CaseCategory { GroupName = "Air conditioning", Name = "Not cooling", DefaultPriority = normal };
        _burstPipe = new CaseCategory
        {
            GroupName = "Water", Name = "Burst pipe or flooding", DefaultPriority = emergency, IsEmergency = true,
        };
        _db.AddRange(customer, normal, emergency, _notCooling, _burstPipe);
        _db.SaveChanges();
        _me.Id = customer.Id;
        _me.Role = UserRole.Customer;
        _sut = new AiService(
            _db, _ai, new CaseService(_db, _me, new FakeFileStorage(), new NotificationService(_db, _me, _clock), _clock), _me, _clock);
    }

    public void Dispose() => _db.Dispose();

    private Task<RequestSuggestionResponse> Suggest(string description = Description) =>
        _sut.SuggestRequestAsync(new RequestSuggestionRequest(description), default);

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task SuggestRequest_ValidReply_ReturnsTypeUrgencyTipsAndKeepsTheQuestion()
    {
        _ai.Reply = $$"""{"serviceTypeId": {{_notCooling.Id}}, "urgency": "normal", "tips": ["Say which unit."]}""";

        var result = await Suggest();

        Assert.True(result.Available);
        Assert.Equal(_notCooling.Id, result.ServiceTypeId);
        Assert.Equal(CasePriority.Normal, result.Urgency);
        Assert.Equal(["Say which unit."], result.Tips);
        Assert.Contains($"{_notCooling.Id} | Air conditioning · Not cooling", _ai.LastPrompt);
        var conversation = Assert.Single(_db.AiConversations);
        Assert.Equal(AiService.RequestContext, conversation.ContextType);
        Assert.Equal(_me.Id, conversation.UserId);
        var message = Assert.Single(_db.AiMessages);
        Assert.Equal(Description, message.Message);
        Assert.Equal(_ai.Reply, message.Reply);
        Assert.Equal(_clock.Now, message.SentAt);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task SuggestRequest_JsonInsideProse_StillReadsTheAnswer()
    {
        _ai.Reply = $$"""Here you go: ```json {"serviceTypeId": {{_notCooling.Id}}, "urgency": "low", "tips": []} ```""";

        var result = await Suggest();

        Assert.Equal(_notCooling.Id, result.ServiceTypeId);
        Assert.Equal(CasePriority.Low, result.Urgency);
        Assert.Empty(result.Tips);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task SuggestRequest_TwoQuestions_KeepsTwoConversationsForTheSameCustomer()
    {
        _ai.Reply = """{"serviceTypeId": null, "urgency": null, "tips": []}""";

        await Suggest();
        await Suggest();

        Assert.Equal(2, _db.AiConversations.Count(c => c.UserId == _me.Id));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task SuggestRequest_NoToken_ReturnsUnavailableWithoutAskingOrLogging()
    {
        _ai.IsAvailable = false;

        var result = await Suggest();

        Assert.False(result.Available);
        Assert.Equal(0, _ai.Calls);
        Assert.Empty(_db.AiConversations);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task SuggestRequest_AiFails_ReturnsUnavailableAndKeepsTheQuestionWithoutReply()
    {
        _ai.Reply = null;

        var result = await Suggest();

        Assert.False(result.Available);
        Assert.Null(Assert.Single(_db.AiMessages).Reply);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task SuggestRequest_NotJson_ReturnsNoSuggestion()
    {
        _ai.Reply = "I think it is the air conditioner.";

        var result = await Suggest();

        Assert.True(result.Available);
        Assert.Null(result.ServiceTypeId);
        Assert.Null(result.Urgency);
        Assert.Empty(result.Tips);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task SuggestRequest_IdOutsideTheCatalogueAndUnknownUrgency_AreDropped()
    {
        _ai.Reply = """{"serviceTypeId": 9999, "urgency": "urgent!!", "tips": [42, "Add a photo."]}""";

        var result = await Suggest();

        Assert.Null(result.ServiceTypeId);
        Assert.Null(result.Urgency);
        Assert.Equal(["Add a photo."], result.Tips);
    }

    private static readonly SmartMaint.Api.Modules.Catalog.Dtos.TechnicianOptionResponse[] Offered =
        [new(5, "Duc", 0, ["HVAC"], [], true), new(6, "Long", 2, ["Plumbing"], [], false)];

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public void ParseTechnicianReply_KnownTechnician_IsTheSuggestion()
    {
        var result = AiService.ParseTechnicianReply("""{"technicianId": 6, "reason": "Knows pipes."}""", Offered);

        Assert.Equal(6, result!.TechnicianId);
        Assert.Equal("Knows pipes.", result.Reason);
        Assert.Equal(SuggestionSource.Ai, result.Source);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public void ParseTechnicianReply_TechnicianNotOffered_IsIgnored() =>
        Assert.Null(AiService.ParseTechnicianReply("""{"technicianId": 99, "reason": "Made up."}""", Offered));

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public void ParseTechnicianReply_NotJson_IsIgnored() =>
        Assert.Null(AiService.ParseTechnicianReply("I would choose Duc.", Offered));

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task SuggestRequest_EmergencyForAnyFault_StaysASuggestion()
    {
        _ai.Reply = $$"""{"serviceTypeId": {{_notCooling.Id}}, "urgency": "emergency", "tips": []}""";

        var result = await Suggest();

        Assert.Equal(CasePriority.Emergency, result.Urgency);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task SuggestRequest_EmergencyForAnEmergencyType_IsKept()
    {
        _ai.Reply = $$"""{"serviceTypeId": {{_burstPipe.Id}}, "urgency": "emergency", "tips": []}""";

        var result = await Suggest("Water is pouring from the ceiling in the corridor of floor 5.");

        Assert.Equal(_burstPipe.Id, result.ServiceTypeId);
        Assert.Equal(CasePriority.Emergency, result.Urgency);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task SuggestRequest_TooManyAndTooLongTips_KeepsThreeCutAt200()
    {
        var longTip = new string('x', AiService.MaxTipLength + 1);
        _ai.Reply = JsonSerializer.Serialize(new
        {
            serviceTypeId = (long?)null,
            urgency = (string?)null,
            tips = new[] { longTip, new string('y', AiService.MaxTipLength), "c", "d" },
        });

        var result = await Suggest();

        Assert.Equal(AiService.MaxTips, result.Tips.Count);
        Assert.Equal(new string('x', AiService.MaxTipLength) + "…", result.Tips[0]);
        Assert.Equal(new string('y', AiService.MaxTipLength), result.Tips[1]);
    }

    [Theory]
    [Trait(TestType.Key, TestType.Boundary)]
    [InlineData(14, false)]
    [InlineData(15, true)]
    [InlineData(2000, true)]
    [InlineData(2001, false)]
    public void Validator_DescriptionLength_FollowsTheRequestForm(int length, bool valid)
    {
        var result = new RequestSuggestionRequestValidator().Validate(new RequestSuggestionRequest(new string('a', length)));

        Assert.Equal(valid, result.IsValid);
    }
}

/// <summary>The Replicate gateway against a fake HTTP server: create, poll, give up quietly.</summary>
public sealed class ReplicateAiGatewayTests
{
    private static ReplicateAiGateway Gateway(StubHandler handler, string token = "test-token") =>
        new(new HttpClient(handler),
            Options.Create(new ReplicateOptions { ApiToken = token, TimeoutSeconds = 5 }),
            NullLogger<ReplicateAiGateway>.Instance);

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Ask_ProcessingThenSucceeded_PollsAndJoinsTheStreamedPieces()
    {
        var handler = new StubHandler(
            """{"id": "p1", "status": "processing", "output": null, "error": null}""",
            """{"id": "p1", "status": "succeeded", "output": ["{\"a\":", " 1}"], "error": null}""");

        var answer = await Gateway(handler).AskAsync("system", "prompt", 400, default);

        Assert.Equal("{\"a\": 1}", answer);
        Assert.Equal(HttpMethod.Post, handler.Requests[0].Method);
        Assert.EndsWith("/models/meta/meta-llama-3-70b-instruct/predictions", handler.Requests[0].RequestUri!.AbsolutePath);
        Assert.Equal("Bearer test-token", handler.Requests[0].Headers.Authorization!.ToString());
        Assert.Contains("\"system_prompt\":\"system\"", handler.Bodies[0]);
        Assert.EndsWith("/predictions/p1", handler.Requests[1].RequestUri!.AbsolutePath);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Ask_PredictionFailed_ReturnsNull()
    {
        var handler = new StubHandler("""{"id": "p1", "status": "failed", "output": null, "error": "boom"}""");

        Assert.Null(await Gateway(handler).AskAsync("system", "prompt", 400, default));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Ask_Unauthorized_ReturnsNullInsteadOfThrowing()
    {
        var handler = new StubHandler(HttpStatusCode.Unauthorized, """{"detail": "Invalid token"}""");

        Assert.Null(await Gateway(handler).AskAsync("system", "prompt", 400, default));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Ask_ThrottledOnce_WaitsAndRetries()
    {
        var handler = new StubHandler("""{"id": "p1", "status": "succeeded", "output": "ok", "error": null}""")
        {
            ThrottleFirst = """{"detail": "Request was throttled. Your rate limit resets in ~0s."}""",
        };

        var answer = await Gateway(handler).AskAsync("system", "prompt", 400, default);

        Assert.Equal("ok", answer);
        Assert.Equal(2, handler.Requests.Count);
        Assert.Contains("\"max_tokens\":400", handler.Bodies[1]);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Ask_NoToken_DoesNotCallReplicate()
    {
        var handler = new StubHandler();
        var gateway = Gateway(handler, token: " ");

        Assert.False(gateway.IsAvailable);
        Assert.Null(await gateway.AskAsync("system", "prompt", 400, default));
        Assert.Empty(handler.Requests);
    }

    [Theory]
    [Trait(TestType.Key, TestType.Boundary)]
    [InlineData("\"plain\"", "plain")]
    [InlineData("[]", "")]
    [InlineData("null", null)]
    public void OutputText_StringListOrNothing(string json, string? expected)
    {
        using var doc = JsonDocument.Parse(json);

        Assert.Equal(expected, ReplicateAiGateway.OutputText(doc.RootElement));
    }

    /// <summary>Answers each request with the next body in order (the last one repeats).</summary>
    private sealed class StubHandler(HttpStatusCode status, params string[] bodies) : HttpMessageHandler
    {
        public StubHandler(params string[] bodies) : this(HttpStatusCode.OK, bodies)
        {
        }

        public List<HttpRequestMessage> Requests { get; } = [];
        public List<string> Bodies { get; } = [];

        /// <summary>The first request answers 429 with this body (Replicate's throttle), the next ones as usual.</summary>
        public string? ThrottleFirst { get; init; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            Bodies.Add(request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken));
            if (ThrottleFirst is not null && Requests.Count == 1)
            {
                return new HttpResponseMessage(HttpStatusCode.TooManyRequests) { Content = new StringContent(ThrottleFirst) };
            }

            var offset = ThrottleFirst is null ? 0 : 1;
            var body = bodies.Length == 0 ? "{}" : bodies[Math.Min(Requests.Count - offset, bodies.Length) - 1];
            return new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
        }
    }
}
