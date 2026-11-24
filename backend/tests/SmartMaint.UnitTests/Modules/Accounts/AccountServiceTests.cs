using SmartMaint.Api.Common.Errors;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Buildings;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Entities.Core;
using SmartMaint.Api.Modules.Accounts;
using SmartMaint.Api.Modules.Accounts.Dtos;
using SmartMaint.Api.Modules.Audit;
using SmartMaint.UnitTests.Common;

using Microsoft.Extensions.Options;
using SmartMaint.Api.Modules.Notifications;

namespace SmartMaint.UnitTests.Modules.Accounts;

/// <summary>ADM-02..04 and DLG-16, called as the Administrator <see cref="_admin"/>.</summary>
public sealed class AccountServiceTests : IDisposable
{
    private readonly FixedTimeProvider _clock = new(new DateTimeOffset(2026, 11, 2, 8, 0, 0, TimeSpan.Zero));
    private readonly FakeCurrentUser _me = new();
    private readonly PasswordHasher _hasher = new();
    private readonly AppDbContext _db;
    private readonly AccountService _sut;

    private readonly Admin _admin;
    private readonly Building _towerA;
    private readonly Specialty _electrical, _plumbing;

    public AccountServiceTests()
    {
        _db = TestDbContextFactory.Create(_clock);
        _towerA = new Building { Name = "Tower A" };
        var floor = new Floor { Building = _towerA, FloorNumber = "5" };
        _electrical = new Specialty { Name = "Electrical" };
        _plumbing = new Specialty { Name = "Plumbing" };
        _admin = new Admin("root", "Root Admin", "hash", false);
        _db.AddRange(_towerA, floor, _electrical, _plumbing, _admin);
        _db.SaveChanges();

        _me.Id = _admin.Id;
        _me.Role = UserRole.Admin;
        _sut = new AccountService(
            _db, _me, _hasher, new AuditService(_db, _me, _clock), _email, Options.Create(new EmailOptions { WebUrl = "https://app.test" }),
            _clock);
    }

    public void Dispose() => _db.Dispose();

    private readonly FakeEmailSender _email = new();

    private static CreateAccountRequest NewTechnician(string email = "duc@test.local") =>
        new("Pham Minh Duc", email, "0901 234 567", UserRole.Technician, 80000m, [], null);

    private async Task<long> CreateAsync(CreateAccountRequest request) => (await _sut.CreateAsync(request, default)).Account.Id;

    private static async Task<string> ErrorCodeAsync(Func<Task> action) =>
        (await Assert.ThrowsAsync<AppException>(action)).Code;

    // ----- Create -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Create_Technician_SavesDetailsAndShowsThePasswordWhenNoEmailWentOut()
    {
        _email.Works = false;

        var result = await _sut.CreateAsync(NewTechnician() with { SpecialtyIds = [_electrical.Id] }, default);

        var account = result.Account;
        Assert.False(result.EmailSent);
        Assert.Equal("duc", account.Username); // made from the e-mail
        Assert.Equal("0901234567", account.PhoneNumber); // stored as digits
        Assert.Equal(UserRole.Technician, account.Role);
        Assert.True(account.MustChangePassword);
        Assert.Equal(80000m, account.HourlyRate);
        Assert.Equal("Electrical", Assert.Single(account.Specialties).Name);
        Assert.Matches("^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9]).{12}$", result.TemporaryPassword);
        var saved = _db.Technicians.Single(t => t.Id == account.Id);
        Assert.True(_hasher.Verify(saved.PasswordHash, result.TemporaryPassword!));
        var audit = Assert.Single(_db.AuditLogs);
        Assert.Equal(AuditOperation.Create, audit.Operation);
        Assert.DoesNotContain("hash", audit.NewValue, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Create_PhoneNumberAlreadyUsedWrittenDifferently_Throws()
    {
        await CreateAsync(NewTechnician("duc@test.local"));

        var error = await ErrorCodeAsync(() => _sut.CreateAsync(NewTechnician("duc2@test.local") with { PhoneNumber = "090-123-4567" }, default));

        Assert.Equal("account.phone_taken", error);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Create_EmailWorks_SendsThePasswordAndDoesNotShowIt()
    {
        var result = await _sut.CreateAsync(NewTechnician(), default);

        Assert.True(result.EmailSent);
        Assert.Null(result.TemporaryPassword);
        var mail = Assert.Single(_email.Sent);
        Assert.Equal("duc@test.local", mail.To);
        Assert.Contains("https://app.test/login", mail.Body);
        var saved = _db.Technicians.Single(t => t.Id == result.Account.Id);
        var password = mail.Body.Split('\n').Single(l => l.StartsWith("    ")).Trim();
        Assert.True(_hasher.Verify(saved.PasswordHash, password));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Create_UsernameFromAnEmailAlreadyUsed_GetsANumber()
    {
        await CreateAsync(NewTechnician("duc@test.local"));

        var second = await _sut.CreateAsync(NewTechnician("duc@other.local") with { PhoneNumber = null }, default);

        Assert.Equal("duc2", second.Account.Username);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Create_CustomerWithAnUnknownBuildingList_IgnoresIt()
    {
        var request = new CreateAccountRequest("Do Ngoc Lan", "lan@test.local", null, UserRole.Customer, null, null, [new BuildingAssignmentRequest(9999, false)]);

        var id = await CreateAsync(request);

        Assert.Null(_db.Customers.Single(c => c.Id == id).RoomId);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Create_EmailDifferingOnlyInCase_IsTaken()
    {
        await CreateAsync(NewTechnician("duc@test.local"));

        var code = await ErrorCodeAsync(() => _sut.CreateAsync(NewTechnician("duc2") with { Email = " DUC@Test.Local " }, default));

        Assert.Equal("account.email_taken", code);
    }

    // ----- Update -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Update_StaffMadePrimary_DemotesThePreviousPrimaryOfTheBuilding()
    {
        var first = await CreateAsync(new("Staff One", "staff1@test.local", null, UserRole.Staff, null, null,
            [new BuildingAssignmentRequest(_towerA.Id, true)]));
        var second = await CreateAsync(new("Staff Two", "staff2@test.local", null, UserRole.Staff, null, null,
            [new BuildingAssignmentRequest(_towerA.Id, false)]));

        var result = await _sut.UpdateAsync(second, new UpdateAccountRequest("Staff Two", null, null, null, null,
            [new BuildingAssignmentRequest(_towerA.Id, true)]), default);

        Assert.True(Assert.Single(result.Buildings).IsPrimary);
        Assert.False(Assert.Single((await _sut.GetAsync(first, default)).Buildings).IsPrimary);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Update_UnknownSpecialty_Throws()
    {
        var id = await CreateAsync(NewTechnician());

        var code = await ErrorCodeAsync(() => _sut.UpdateAsync(id,
            new UpdateAccountRequest("Pham Minh Duc", null, null, null, [9999], null), default));

        Assert.Equal("account.unknown_specialty", code);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Update_SameSpecialtiesAgainPlusOne_KeepsOneRowEach()
    {
        var id = await CreateAsync(NewTechnician() with { SpecialtyIds = [_electrical.Id] });

        var result = await _sut.UpdateAsync(id,
            new UpdateAccountRequest("Pham Minh Duc", null, null, 90000m, [_electrical.Id, _plumbing.Id], null), default);

        Assert.Equal(["Electrical", "Plumbing"], result.Specialties.Select(s => s.Name));
        Assert.Equal(2, _db.TechnicianSpecialties.Count(t => t.TechnicianId == id));
        Assert.Equal(90000m, result.HourlyRate);
    }

    // ----- Sort by last sign-in -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task List_SortedByLastSignIn_NewestFirstAndNeverLast()
    {
        var never = await CreateAsync(NewTechnician());
        var older = await CreateAsync(NewTechnician("older@test.local") with { PhoneNumber = "0911111111" });
        var newer = await CreateAsync(NewTechnician("newer@test.local") with { PhoneNumber = "0922222222" });
        var now = DateTimeOffset.UtcNow;
        _db.Accounts.Single(a => a.Id == older).RecordSuccessfulLogin(now.AddDays(-3));
        _db.Accounts.Single(a => a.Id == newer).RecordSuccessfulLogin(now.AddDays(-1));
        await _db.SaveChangesAsync();

        var page = await _sut.ListAsync(new AccountListQuery { Role = UserRole.Technician, LastSignIn = "desc" }, default);

        Assert.Equal([newer, older, never], page.Items.Select(a => a.Id).Where(id => id == newer || id == older || id == never));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task List_SortedOldestFirst_StillPutsNeverSignedInLast()
    {
        var never = await CreateAsync(NewTechnician());
        var older = await CreateAsync(NewTechnician("older@test.local") with { PhoneNumber = "0911111111" });
        _db.Accounts.Single(a => a.Id == older).RecordSuccessfulLogin(DateTimeOffset.UtcNow.AddDays(-3));
        await _db.SaveChangesAsync();

        var page = await _sut.ListAsync(new AccountListQuery { Role = UserRole.Technician, LastSignIn = "asc" }, default);

        Assert.Equal([older, never], page.Items.Select(a => a.Id).Where(id => id == older || id == never));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task AllowedActions_NeverOfferAChangeOfRole()
    {
        var id = await CreateAsync(NewTechnician());

        var account = await _sut.GetAsync(id, default);

        Assert.DoesNotContain("ChangeRole", account.AllowedActions.Select(a => a.ToString()));
    }

    // ----- Deactivate / reactivate -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Deactivate_ThenReactivate_ChangesStatusAndAuditsBoth()
    {
        var id = await CreateAsync(NewTechnician());

        Assert.Equal(UserStatus.Deactivated, (await _sut.DeactivateAsync(id, default)).Status);
        Assert.Equal(UserStatus.Active, (await _sut.ReactivateAsync(id, default)).Status);

        Assert.Contains(_db.AuditLogs, a => a.Operation == AuditOperation.Deactivate);
        Assert.Contains(_db.AuditLogs, a => a.Operation == AuditOperation.Reactivate);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Deactivate_TechnicianHoldingAnOpenCase_Throws()
    {
        var id = await CreateAsync(NewTechnician());
        _db.Cases.Add(new Case { Title = "AC", AssignedTechnicianId = id, Status = CaseStatus.InProgress });
        await _db.SaveChangesAsync();

        Assert.Equal("account.open_work", await ErrorCodeAsync(() => _sut.DeactivateAsync(id, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Deactivate_TheLastActiveAdministrator_Throws()
    {
        var other = new Admin("root2", "Second Admin", "hash", false);
        _db.Accounts.Add(other);
        _admin.Status = UserStatus.Deactivated; // the caller's own account no longer counts
        await _db.SaveChangesAsync();

        Assert.Equal("account.last_admin", await ErrorCodeAsync(() => _sut.DeactivateAsync(other.Id, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Deactivate_Yourself_IsNotAllowed() =>
        Assert.Equal("account.action_not_allowed", await ErrorCodeAsync(() => _sut.DeactivateAsync(_admin.Id, default)));

    // ----- Reset password / unlock -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task ResetPassword_LockedAccount_GivesANewTemporaryPasswordAndUnlocks()
    {
        var id = await CreateAsync(NewTechnician());
        var account = _db.Accounts.Single(a => a.Id == id);
        account.RecordFailedLogin(_clock.Now, 1, TimeSpan.FromMinutes(15));
        await _db.SaveChangesAsync();

        _email.Works = false;

        var result = await _sut.ResetPasswordAsync(id, default);

        Assert.False(result.EmailSent);
        Assert.True(_hasher.Verify(account.PasswordHash, result.TemporaryPassword!));
        Assert.True(account.MustChangePassword);
        Assert.False(account.IsLockedOut(_clock.Now));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Unlock_LockedAccount_ClearsTheLock()
    {
        var id = await CreateAsync(NewTechnician());
        _db.Accounts.Single(a => a.Id == id).RecordFailedLogin(_clock.Now, 1, TimeSpan.FromMinutes(15));
        await _db.SaveChangesAsync();
        Assert.Contains(AccountAction.Unlock, (await _sut.GetAsync(id, default)).AllowedActions);

        var result = await _sut.UnlockAsync(id, default);

        Assert.False(result.IsLocked);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Unlock_ExactlyWhenTheLockEnds_IsNoLongerOffered()
    {
        var id = await CreateAsync(NewTechnician());
        _db.Accounts.Single(a => a.Id == id).RecordFailedLogin(_clock.Now, 1, TimeSpan.FromMinutes(15));
        await _db.SaveChangesAsync();
        _clock.Now = _clock.Now.AddMinutes(15);

        Assert.Equal("account.action_not_allowed", await ErrorCodeAsync(() => _sut.UnlockAsync(id, default)));
    }

    // ----- List / overview -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task List_FilteredByRoleAndSearch_ReturnsTheMatches()
    {
        await CreateAsync(NewTechnician("duc@test.local"));
        await CreateAsync(NewTechnician("long@test.local") with { FullName = "Vo Thanh Long", PhoneNumber = null });

        var page = await _sut.ListAsync(new AccountListQuery { Role = UserRole.Technician, Search = "thanh" }, default);

        Assert.Equal("long", Assert.Single(page.Items).Username);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task List_SecondPageOfOne_HasOneItem()
    {
        await CreateAsync(NewTechnician("duc@test.local"));
        await CreateAsync(NewTechnician("long@test.local") with { PhoneNumber = null });

        var page = await _sut.ListAsync(new AccountListQuery { Role = UserRole.Technician, Page = 2, PageSize = 1 }, default);

        Assert.Equal(2, page.TotalCount);
        Assert.Single(page.Items);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task GetOverview_CountsAccountsPerRoleAndShowsRecentActivity()
    {
        await CreateAsync(NewTechnician());

        var overview = await _sut.GetOverviewAsync(default);

        Assert.Equal(1, overview.Accounts.Single(r => r.Role == UserRole.Technician).Active);
        Assert.Equal(1, overview.Accounts.Single(r => r.Role == UserRole.Admin).Active);
        Assert.Equal(1, overview.MustChangePassword);
        Assert.Equal(AuditOperation.Create, Assert.Single(overview.RecentActivity).Operation);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Get_UnknownAccount_Throws() =>
        Assert.Equal("account.not_found", await ErrorCodeAsync(() => _sut.GetAsync(9999, default)));

    // ----- Delete (meeting note 18/11) -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Delete_AccountWithNoData_IsGone()
    {
        var id = await CreateAsync(NewTechnician() with { SpecialtyIds = [_electrical.Id] });
        Assert.Contains(AccountAction.Delete, (await _sut.GetAsync(id, default)).AllowedActions);

        await _sut.DeleteAsync(id, default);

        Assert.DoesNotContain(_db.Accounts, a => a.Id == id);
        Assert.DoesNotContain(_db.TechnicianSpecialties, t => t.TechnicianId == id);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Delete_AccountThatAddedACost_IsRefused()
    {
        var id = await CreateAsync(NewTechnician());
        _db.Costs.Add(new Cost { CaseId = 1, Name = "Filter", Amount = 1000, AddedById = id });
        await _db.SaveChangesAsync();

        Assert.Equal("account.action_not_allowed", await ErrorCodeAsync(() => _sut.DeleteAsync(id, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Delete_OwnAccount_IsNotOffered()
    {
        Assert.DoesNotContain(AccountAction.Delete, (await _sut.GetAsync(_me.Id, default)).AllowedActions);
    }
}
