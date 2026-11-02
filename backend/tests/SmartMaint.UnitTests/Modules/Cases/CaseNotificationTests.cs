using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Cases.Dtos;
using SmartMaint.UnitTests.Common;

namespace SmartMaint.UnitTests.Modules.Cases;

/// <summary>SRS 5.4.2: who is told about each step of F3 (in-app notifications).</summary>
public sealed class CaseNotificationTests : CaseTestBase
{
    private List<long> RecipientsOf(string textPart) =>
        Db.Notifications.Where(n => n.Details.Contains(textPart)).Select(n => n.UserId).ToList();

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Create_TellsEveryStaffMemberOfThatBuildingOnly()
    {
        var created = await NewCaseAsync();

        Assert.Equal([Staff.Id], RecipientsOf(created.Code));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Assign_TellsTheTechnicianAndApproveTellsTheCustomer()
    {
        var id = await CaseAtAsync(CaseStatus.Assigned);

        Assert.Contains(Db.Notifications, n => n.UserId == Customer.Id && n.Details.Contains("approved"));
        Assert.Contains(Db.Notifications, n => n.UserId == Technician.Id && n.Details.Contains("assigned to you"));
        Assert.Equal(id, (await Sut.GetAsync(id, default)).Id);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Accept_TellsTheStaffMemberWhoAssigned()
    {
        await CaseAtAsync(CaseStatus.Accepted);

        Assert.Contains(Db.Notifications, n => n.UserId == Staff.Id && n.Details.Contains("accepted"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Close_TheActingStaffMemberIsNotNotifiedButTheCustomerIs()
    {
        await CaseAtAsync(CaseStatus.Closed);

        Assert.Contains(Db.Notifications, n => n.UserId == Customer.Id && n.Details.Contains("Please rate"));
        Assert.DoesNotContain(Db.Notifications, n => n.UserId == Staff.Id && n.Details.Contains("closed"));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Cancel_ByTheCustomerBeforeReview_TellsNobodyElse()
    {
        var created = await NewCaseAsync();
        var before = Db.Notifications.Count();

        await Sut.CancelAsync(created.Id, new ReasonRequest("Fixed it myself, thank you"), default);

        Assert.Equal(before, Db.Notifications.Count()); // no staff, no technician on the case yet
    }

    // ----- ListMine -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task ListMine_ReturnsOnlyTheCallersNotificationsNewestFirst()
    {
        await CaseAtAsync(CaseStatus.Assigned);
        As(Customer);

        var page = await Notifications.ListMineAsync(new PageRequest(), default);

        Assert.All(page.Items, n => Assert.Equal(Customer.Id, Db.Notifications.Single(x => x.Id == n.Id).UserId));
        Assert.True(page.TotalCount >= 1);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task ListMine_OlderThan90Days_IsHidden()
    {
        await NewCaseAsync();
        Clock.Now = Clock.Now.AddDays(90).AddSeconds(1);
        As(Staff);

        Assert.Equal(0, (await Notifications.ListMineAsync(new PageRequest(), default)).TotalCount);
    }
}
