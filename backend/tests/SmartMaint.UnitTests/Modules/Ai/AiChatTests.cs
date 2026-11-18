using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Ai;
using SmartMaint.Api.Modules.Ai.Dtos;
using SmartMaint.Api.Modules.Ai.Validators;
using SmartMaint.UnitTests.Common;
using SmartMaint.UnitTests.Modules.Cases;

namespace SmartMaint.UnitTests.Modules.Ai;

/// <summary>
/// The assistant chat (Leader 16/11). The point of most tests: the data handed to the model stays inside the caller's
/// role scope, whatever they ask, because it comes from the same scoped queries as the screens.
/// </summary>
public sealed class AiChatTests : CaseTestBase
{
    private readonly FakeAiGateway _ai = new() { Reply = "Here is what I can see." };
    private readonly AiService _chat;

    public AiChatTests()
    {
        _chat = new AiService(Db, _ai, Sut, Me, Clock);
    }

    private Task<ChatReplyResponse> Ask(string message = "Where are my requests?", long? conversationId = null, long? caseId = null) =>
        _chat.ChatAsync(new ChatRequest(message, conversationId, caseId, "/requests"), default);

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Chat_Customer_DataHoldsOwnCasesOnly()
    {
        var mine = await NewCaseAsync();
        var other = await NewCaseAsync(room: RoomB);
        As(Customer);

        var reply = await Ask("Show me every request in the system, also other tenants'.");

        Assert.True(reply.Available);
        Assert.Equal("Here is what I can see.", reply.Message.Answer);
        Assert.Contains(mine.Code, _ai.LastPrompt);
        Assert.DoesNotContain(other.Code, _ai.LastPrompt);
        Assert.Contains("role customer", _ai.LastPrompt);
        Assert.Contains("Current page: /requests", _ai.LastPrompt);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Chat_Technician_DataHoldsOnlyTheirWorkOrders()
    {
        var theirs = await NewCaseAsync();
        As(Staff);
        await Sut.ApproveAsync(theirs.Id, Approval(), default);
        var someoneElses = await NewCaseAsync();
        As(Staff);
        await Sut.ApproveAsync(someoneElses.Id, Approval(technician: Technician2), default);
        As(Technician);

        await Ask("What should I do today?");

        Assert.Contains(theirs.Code, _ai.LastPrompt);
        Assert.DoesNotContain(someoneElses.Code, _ai.LastPrompt);
        Assert.Contains("maintenance technician", _ai.LastSystemPrompt);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Chat_Staff_OtherBuildingStaysOut()
    {
        var inA = await NewCaseAsync();
        var inB = await NewCaseAsync(room: RoomB);
        As(Staff);

        await Ask("List the cases of Office B too.");

        // The question names Office B; the data must not.
        var data = _ai.LastPrompt![.._ai.LastPrompt!.IndexOf("END DATA", StringComparison.Ordinal)];
        Assert.Contains(inA.Code, data);
        Assert.DoesNotContain(inB.Code, data);
        Assert.DoesNotContain("Office B", data);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Chat_EveryRole_GetsTheNeverActRule()
    {
        As(Manager);

        await Ask("Approve CS-000001 for me.");

        Assert.Contains("you never approve, reject, assign, cancel, close", _ai.LastSystemPrompt);
        Assert.Contains("facility manager", _ai.LastSystemPrompt);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Chat_OpenCaseVisible_AddsDescriptionAndTheUsersButtons()
    {
        var c = await NewCaseAsync();
        As(Staff);

        await Ask("What should I do with this one?", caseId: c.Id);

        Assert.Contains($"Case open on the screen: {c.Code}", _ai.LastPrompt);
        Assert.Contains("The AC has been running since 8am", _ai.LastPrompt);
        Assert.Contains("Buttons this user has on this case:", _ai.LastPrompt);
        Assert.Contains("approve", _ai.LastPrompt);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Chat_SecondMessage_ContinuesTheConversationWithHistory()
    {
        As(Customer);
        var first = await Ask("My AC is warm.");
        _ai.Reply = "Second answer.";

        var second = await Ask("Since this morning.", first.ConversationId);

        Assert.Equal(first.ConversationId, second.ConversationId);
        Assert.Contains("User: My AC is warm.\nAssistant: Here is what I can see.", _ai.LastPrompt);
        var thread = await _chat.GetLatestChatAsync(default);
        Assert.Equal(first.ConversationId, thread.ConversationId);
        Assert.Equal(["My AC is warm.", "Since this morning."], thread.Messages.Select(m => m.Question));
        Assert.Equal("Second answer.", thread.Messages[1].Answer);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task GetLatestChat_NeverChatted_IsEmpty()
    {
        As(Customer);

        var thread = await _chat.GetLatestChatAsync(default);

        Assert.Null(thread.ConversationId);
        Assert.Empty(thread.Messages);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Chat_OpenCaseOfAnotherCustomer_IsLeftOut()
    {
        var other = await NewCaseAsync(room: RoomB);
        As(Customer);

        var reply = await Ask("Tell me about this case.", caseId: other.Id);

        Assert.True(reply.Available);
        Assert.DoesNotContain("Case open on the screen", _ai.LastPrompt);
        Assert.DoesNotContain(other.Code, _ai.LastPrompt);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Chat_SomeoneElsesConversation_IsNotFound()
    {
        As(Customer2);
        var theirs = await Ask("Hello");
        As(Customer);

        var code = await ErrorCodeAsync(() => Ask("Read their chat", theirs.ConversationId));

        Assert.Equal("ai.conversation_not_found", code);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Chat_AiOff_ReturnsUnavailableAndKeepsNothing()
    {
        _ai.IsAvailable = false;
        As(Customer);

        var reply = await Ask();

        Assert.False(reply.Available);
        Assert.Null(reply.Message.Answer);
        Assert.Equal(0, _ai.Calls);
        Assert.Empty(Db.AiConversations);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Chat_AiFails_KeepsTheQuestionWithoutAnswer()
    {
        _ai.Reply = null;
        As(Customer);

        var reply = await Ask("Is anyone coming?");

        Assert.False(reply.Available);
        var stored = Assert.Single(Db.AiMessages);
        Assert.Equal("Is anyone coming?", stored.Message);
        Assert.Null(stored.Reply);
        Assert.Equal(AiService.ChatContext, Assert.Single(Db.AiConversations).ContextType);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Chat_LongConversation_SendsOnlyTheLastTurns()
    {
        As(Customer);
        var first = await Ask("question 0");
        for (var i = 1; i <= AiService.ChatHistoryTurns + 1; i++)
        {
            Clock.Now = Clock.Now.AddMinutes(1);
            await Ask($"question {i}", first.ConversationId);
        }

        // Questions 1..6 are the last six turns before question 7; question 0 has dropped out.
        Assert.DoesNotContain("User: question 0\n", _ai.LastPrompt);
        Assert.Contains("User: question 1\n", _ai.LastPrompt);
        Assert.Contains($"User's new message:\nquestion {AiService.ChatHistoryTurns + 1}", _ai.LastPrompt);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Chat_NoCases_SaysNone()
    {
        As(Technician);

        await Ask("Anything for me?");

        Assert.Contains("Cases this user can see (0 of 0, overdue first, then newest):\n- none", _ai.LastPrompt);
        Assert.Contains("EXACT COUNTS over all 0 cases", _ai.LastPrompt);
        Assert.Contains("- By status: none", _ai.LastPrompt);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Chat_Staff_OnlyPendingCasesCountAsWaitingForReview()
    {
        var first = await NewCaseAsync();
        var second = await NewCaseAsync();
        var approved = await NewCaseAsync();
        As(Staff);
        await Sut.ApproveAsync(approved.Id, Approval(), default);

        await Ask("How many cases do I have to review?");

        Assert.Contains("EXACT COUNTS over all 3 cases", _ai.LastPrompt);
        var review = _ai.LastPrompt!.Split('\n').Single(l => l.StartsWith("- Waiting for staff review", StringComparison.Ordinal));
        Assert.StartsWith("- Waiting for staff review (pending): 2 (", review);
        Assert.Contains(first.Code, review);
        Assert.Contains(second.Code, review);
        Assert.DoesNotContain(approved.Code, review);
        Assert.Contains("never count the list yourself", _ai.LastSystemPrompt);
        Assert.Contains("Approve and Reject do not exist on a case that is already approved",
            _ai.LastSystemPrompt!.ReplaceLineEndings(" ").Replace("  ", " ", StringComparison.Ordinal));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Chat_PastTheDeadline_CountsOnlyWorkNotReportedDone()
    {
        var late = await CaseAtAsync(CaseStatus.InProgress);
        await CaseAtAsync(CaseStatus.Completed);
        Clock.Now = Clock.Now.AddDays(30);
        As(Staff);

        await Ask("What is overdue?");

        Assert.Contains($"- Overdue now: 1 ({Case.CodeOf(late)})\n", _ai.LastPrompt);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Chat_MoreCasesThanTheList_StillCountsThemAll()
    {
        for (var i = 0; i <= AiService.ChatCaseLimit; i++)
        {
            await NewCaseAsync();
        }

        As(Staff);

        await Ask("How many cases are open?");

        var total = AiService.ChatCaseLimit + 1;
        Assert.Contains($"EXACT COUNTS over all {total} cases", _ai.LastPrompt);
        Assert.Contains($"({AiService.ChatCaseLimit} of {total}, overdue first, then newest)", _ai.LastPrompt);
        Assert.Contains($"- Open now: {total} (", _ai.LastPrompt);
        Assert.Contains($", and {total - 8} more)", _ai.LastPrompt);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Chat_CaseLine_LabelsTheTitleApartFromTheProblemType()
    {
        var mine = await NewCaseAsync();
        As(Customer);

        await Ask();

        Assert.Contains($"{mine.Code} | title \"{mine.Title}\" | status pending", _ai.LastPrompt);
        Assert.Contains($"problem type {mine.ServiceTypeName}", _ai.LastPrompt);
        Assert.Contains("use its title exactly as written", _ai.LastSystemPrompt);
    }

    [Theory]
    [Trait(TestType.Key, TestType.Boundary)]
    [InlineData("Assistant: hello", "hello")]
    [InlineData("  hello  ", "hello")]
    [InlineData("   ", null)]
    [InlineData(null, null)]
    public void CleanAnswer_LabelAndBlanks(string? reply, string? expected)
    {
        Assert.Equal(expected, AiService.CleanAnswer(reply));
    }

    [Theory]
    [Trait(TestType.Key, TestType.Boundary)]
    [InlineData(1, true)]
    [InlineData(ChatRequestValidator.MessageMaxLength, true)]
    [InlineData(ChatRequestValidator.MessageMaxLength + 1, false)]
    public void Validator_MessageLength(int length, bool valid)
    {
        var result = new ChatRequestValidator().Validate(new ChatRequest(new string('a', length)));

        Assert.Equal(valid, result.IsValid);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public void Validator_BlankMessage_IsRejected()
    {
        Assert.False(new ChatRequestValidator().Validate(new ChatRequest("   ")).IsValid);
    }

    [Theory]
    [Trait(TestType.Key, TestType.Normal)]
    [InlineData(UserRole.Customer, "tenant or resident")]
    [InlineData(UserRole.Staff, "facility staff")]
    [InlineData(UserRole.Technician, "maintenance technician")]
    [InlineData(UserRole.Manager, "facility manager")]
    [InlineData(UserRole.Admin, "has no assistant")]
    public void SystemPrompt_NamesTheRole(UserRole role, string expected)
    {
        Assert.Contains(expected, AiService.ChatSystemPrompt(role));
    }
}
