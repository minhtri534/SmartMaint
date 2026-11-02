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
    private readonly Room _room501, _lobby;
    private readonly Specialty _electrical, _plumbing;

    public AccountServiceTests()
    {
        _db = TestDbContextFactory.Create(_clock);
        _towerA = new Building { Name = "Tower A" };
        var floor = new Floor { Building = _towerA, FloorNumber = "5" };
        _room501 = new Room { Floor = floor, RoomNumber = "501" };
        _lobby = new Room { Floor = floor, RoomNumber = "Lobby", IsCommonArea = true };
        _electrical = new Specialty { Name = "Electrical" };
        _plumbing = new Specialty { Name = "Plumbing" };
        _admin = new Admin("root", "Root Admin", "hash", false);
        _db.AddRange(_towerA, floor, _room501, _lobby, _electrical, _plumbing, _admin);
        _db.SaveChanges();

        _me.Id = _admin.Id;
        _me.Role = UserRole.Admin;
        _sut = new AccountService(_db, _me, _hasher, new AuditService(_db, _me, _clock), _clock);
    }

    public void Dispose() => _db.Dispose();

    private static CreateAccountRequest NewTechnician(string username = "duc") =>
        new(username, "Pham Minh Duc", "duc@test.local", "0901 234 567", UserRole.Technician, null, 80000m, [], null);

    private async Task<long> CreateAsync(CreateAccountRequest request) => (await _sut.CreateAsync(request, default)).Account.Id;

    private static async Task<string> ErrorCodeAsync(Func<Task> action) =>
        (await Assert.ThrowsAsync<AppException>(action)).Code;

    // ----- Create -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Create_Technician_SavesDetailsAndReturnsATemporaryPasswordOnce()
    {
        var result = await _sut.CreateAsync(NewTechnician() with { SpecialtyIds = [_electrical.Id] }, default);

        var account = result.Account;
        Assert.Equal("duc", account.Username);
        Assert.Equal(UserRole.Technician, account.Role);
        Assert.True(account.MustChangePassword);
        Assert.Equal(80000m, account.HourlyRate);
        Assert.Equal("Electrical", Assert.Single(account.Specialties).Name);
        Assert.Matches("^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9]).{12}$", result.TemporaryPassword);
        var saved = _db.Technicians.Single(t => t.Id == account.Id);
        Assert.True(_hasher.Verify(saved.PasswordHash, result.TemporaryPassword));
        var audit = Assert.Single(_db.AuditLogs);
        Assert.Equal(AuditOperation.Create, audit.Operation);
        Assert.DoesNotContain("hash", audit.NewValue, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Create_UsernameAlreadyUsedInOtherCase_Throws()
    {
        await CreateAsync(NewTechnician("duc"));

        Assert.Equal("account.username_taken", await ErrorCodeAsync(() => _sut.CreateAsync(NewTechnician(" DUC "), default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task Create_CustomerInACommonArea_Throws()
    {
        var request = new CreateAccountRequest("lan", "Do Ngoc Lan", null, null, UserRole.Customer, _lobby.Id, null, null, null);

        Assert.Equal("account.common_area_room", await ErrorCodeAsync(() => _sut.CreateAsync(request, default)));
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Create_EmailDifferingOnlyInCase_IsTaken()
    {
        await CreateAsync(NewTechnician("duc"));

        var code = await ErrorCodeAsync(() => _sut.CreateAsync(NewTechnician("duc2") with { Email = " DUC@Test.Local " }, default));

        Assert.Equal("account.email_taken", code);
    }

    // ----- Update -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task Update_StaffMadePrimary_DemotesThePreviousPrimaryOfTheBuilding()
    {
        var first = await CreateAsync(new("staff1", "Staff One", null, null, UserRole.Staff, null, null, null,
            [new BuildingAssignmentRequest(_towerA.Id, true)]));
        var second = await CreateAsync(new("staff2", "Staff Two", null, null, UserRole.Staff, null, null, null,
            [new BuildingAssignmentRequest(_towerA.Id, false)]));

        var result = await _sut.UpdateAsync(second, new UpdateAccountRequest("Staff Two", null, null, null, null, null,
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
            new UpdateAccountRequest("Pham Minh Duc", null, null, null, null, [9999], null), default));

        Assert.Equal("account.unknown_specialty", code);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task Update_SameSpecialtiesAgainPlusOne_KeepsOneRowEach()
    {
        var id = await CreateAsync(NewTechnician() with { SpecialtyIds = [_electrical.Id] });

        var result = await _sut.UpdateAsync(id,
            new UpdateAccountRequest("Pham Minh Duc", null, null, null, 90000m, [_electrical.Id, _plumbing.Id], null), default);

        Assert.Equal(["Electrical", "Plumbing"], result.Specialties.Select(s => s.Name));
        Assert.Equal(2, _db.TechnicianSpecialties.Count(t => t.TechnicianId == id));
        Assert.Equal(90000m, result.HourlyRate);
    }

    // ----- Change role -----

    [Fact]
    [Trait(TestType.Key, TestType.Normal)]
    public async Task ChangeRole_AccountWithoutHistory_RecreatesItInTheNewRoleWithTheSameLogin()
    {
        var created = await _sut.CreateAsync(NewTechnician(), default);

        var result = await _sut.ChangeRoleAsync(created.Account.Id,
            new ChangeRoleRequest(UserRole.Staff, null, null, null, [new BuildingAssignmentRequest(_towerA.Id, true)]), default);

        Assert.NotEqual(created.Account.Id, result.Id);
        Assert.Equal(UserRole.Staff, result.Role);
        Assert.Equal("duc", result.Username);
        Assert.Equal("duc@test.local", result.Email);
        Assert.Single(result.Buildings);
        Assert.Empty(_db.Technicians);
        Assert.True(_hasher.Verify(_db.Staff.Single().PasswordHash, created.TemporaryPassword));
        Assert.Contains(_db.AuditLogs, a => a.Operation == AuditOperation.ChangeRole);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Abnormal)]
    public async Task ChangeRole_AccountWithHistory_Throws()
    {
        var id = await CreateAsync(NewTechnician());
        _db.CaseStatusHistory.Add(new CaseStatusHistory { CaseId = 1, ChangedById = id, NewStatus = CaseStatus.Accepted });
        await _db.SaveChangesAsync();

        var code = await ErrorCodeAsync(() => _sut.ChangeRoleAsync(id,
            new ChangeRoleRequest(UserRole.Staff, null, null, null, null), default));

        Assert.Equal("account.has_history", code);
        Assert.DoesNotContain(AccountAction.ChangeRole, (await _sut.GetAsync(id, default)).AllowedActions);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task ChangeRole_ToTheSameRole_IsNotAllowed()
    {
        var id = await CreateAsync(NewTechnician());

        var code = await ErrorCodeAsync(() => _sut.ChangeRoleAsync(id,
            new ChangeRoleRequest(UserRole.Technician, null, null, null, null), default));

        Assert.Equal("account.action_not_allowed", code);
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

        var result = await _sut.ResetPasswordAsync(id, default);

        Assert.True(_hasher.Verify(account.PasswordHash, result.TemporaryPassword));
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
        await CreateAsync(NewTechnician("duc"));
        await CreateAsync(NewTechnician("long") with { FullName = "Vo Thanh Long", Email = null });

        var page = await _sut.ListAsync(new AccountListQuery { Role = UserRole.Technician, Search = "thanh" }, default);

        Assert.Equal("long", Assert.Single(page.Items).Username);
    }

    [Fact]
    [Trait(TestType.Key, TestType.Boundary)]
    public async Task List_SecondPageOfOne_HasOneItem()
    {
        await CreateAsync(NewTechnician("duc"));
        await CreateAsync(NewTechnician("long") with { Email = null });

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
}
