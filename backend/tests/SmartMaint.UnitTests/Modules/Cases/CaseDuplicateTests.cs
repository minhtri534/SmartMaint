using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Cases;
using SmartMaint.Api.Modules.Cases.Dtos;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Cases;

/// <summary>
/// Meeting 07/11 (replaces merge): a request that repeats an open case hands its content and its customer to that case
/// and is rejected as a duplicate; several customers can follow one case.
/// </summary>
public sealed class CaseDuplicateTests : CaseTestBase
{
    private bool Told(Account user, string text) => Db.Notifications.Any(n => n.UserId == user.Id && n.Details.Contains(text));

    /// <summary>The same problem reported again by Customer2 in the corridor of building A, with a photo.</summary>
    private async Task<long> RepeatAsync()
    {
        As(Customer2);
        return (await Sut.CreateAsync(NewRequest(NotCooling, CorridorA, photos: Photo) with { Title = "AC in the corridor is warm" }, default)).Id;
    }

    private static RejectCaseRequest Duplicate(long caseId, bool task = true, bool photos = true, bool follow = true) =>
        new(RejectReason.Duplicate, "The same AC is already being repaired", new DuplicateOfRequest(caseId, task, photos, follow));

    // ----- reject as a duplicate -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Reject_DuplicateOfAnOpenCase_AddsATaskCopiesThePhotosAndTheCustomerFollows()
    {
        var main = await CaseAtAsync(CaseStatus.InProgress);
        var repeat = await RepeatAsync();
        As(Staff);

        var rejected = await Sut.RejectAsync(repeat, Duplicate(main), default);

        Assert.Equal(CaseStatus.Rejected, rejected.Status);
        Assert.EndsWith($"(see CS-{main:D6})", rejected.History[^1].Reason);
        var target = await Sut.GetAsync(main, default);
        Assert.Equal(2, target.Tasks.Count);
        Assert.Equal("AC in the corridor is warm", target.Tasks[^1].Name);
        Assert.Single(target.Photos, p => p.Kind == CaseImageKind.Report);
        Assert.Equal(Customer2.Id, Assert.Single(target.Followers).Id);
        Assert.True(Told(Technician, "has a new task"));
        Assert.True(Told(Customer2, "rejected"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Reject_DuplicateOfACaseInAnotherBuilding_Throws()
    {
        var other = (await NewCaseAsync(room: RoomB)).Id;
        var repeat = await RepeatAsync();
        As(Staff);

        Assert.Equal("case.invalid_duplicate_target", await ErrorCodeAsync(() => Sut.RejectAsync(repeat, Duplicate(other), default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Reject_DuplicateOfItselfOrOfAClosedCase_Throws()
    {
        var closed = await CaseAtAsync(CaseStatus.Closed);
        var repeat = await RepeatAsync();
        As(Staff);

        Assert.Equal("case.invalid_duplicate_target", await ErrorCodeAsync(() => Sut.RejectAsync(repeat, Duplicate(repeat), default)));
        Assert.Equal("case.invalid_duplicate_target", await ErrorCodeAsync(() => Sut.RejectAsync(repeat, Duplicate(closed), default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Reject_DuplicateWithNothingToHandOver_OnlyRejects()
    {
        var main = await CaseAtAsync(CaseStatus.Approved);
        var repeat = await RepeatAsync();
        As(Staff);

        await Sut.RejectAsync(repeat, Duplicate(main, task: false, photos: false, follow: false), default);

        var target = await Sut.GetAsync(main, default);
        Assert.Single(target.Tasks);
        Assert.Empty(target.Followers);
    }

    // ----- followers -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Follower_SeesTheCaseInTheirListAndHearsWhenItCloses()
    {
        var main = await CaseAtAsync(CaseStatus.Completed);
        As(Staff);
        await Sut.SetFollowersAsync(main, new SetFollowersRequest([Customer2.Id]), default);
        await Sut.EditAsync(main, await EditOfAsync(main) with { StatusChange = EditStatusChange.Close }, default);
        As(Customer2);

        var list = await Sut.ListAsync(new SmartMaint.Api.Common.Models.PageRequest(), default);
        var detail = await Sut.GetAsync(main, default);

        Assert.Contains(list.Items, c => c.Id == main);
        Assert.Empty(detail.AllowedActions); // a follower only reads (no rating, no "not fixed")
        Assert.True(Told(Customer2, "You now follow"));
        Assert.True(Told(Customer2, "was closed"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task SetFollowers_SomeoneWhoIsNotACustomer_Throws()
    {
        var main = await CaseAtAsync(CaseStatus.Approved);
        As(Staff);

        Assert.Equal("case.invalid_follower",
            await ErrorCodeAsync(() => Sut.SetFollowersAsync(main, new SetFollowersRequest([Technician.Id]), default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task FollowerOptions_SearchByUsername_FindsOnlyThatCustomerAndListsTheCurrentOnes()
    {
        var main = await CaseAtAsync(CaseStatus.Approved);
        As(Staff);
        await Sut.SetFollowersAsync(main, new SetFollowersRequest([Customer2.Id]), default);

        var options = await Sut.GetFollowerOptionsAsync(main, "CUSTOMERB", default);

        Assert.Equal(Customer2.Id, Assert.Single(options.Current).Id);
        Assert.Equal(CustomerB.Id, Assert.Single(options.Matches).Id);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task FollowerOptions_AreAllTheOtherCustomers()
    {
        var main = await CaseAtAsync(CaseStatus.Approved);
        As(Staff);

        var options = await Sut.GetFollowerOptionsAsync(main, null, default);

        Assert.Equal([Customer2.Id, CustomerB.Id], options.Matches.Select(o => o.Id).Order()); // everyone but the requester
    }
}
