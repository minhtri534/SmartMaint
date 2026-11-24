using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Buildings;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Entities.Core;
using SmartMaint.Api.Modules.Accounts.Dtos;
using SmartMaint.Api.Modules.Audit;
using SmartMaint.Api.Modules.Audit.Dtos;
using SmartMaint.Api.Modules.Notifications;
using Microsoft.Extensions.Options;

namespace SmartMaint.Api.Modules.Accounts;

internal sealed class AccountService(
    AppDbContext db,
    ICurrentUser currentUser,
    IPasswordHasher passwordHasher,
    IAuditService audit,
    IEmailSender email,
    IOptions<EmailOptions> emailOptions,
    TimeProvider clock) : IAccountService
{
    /// <summary>Statuses where a technician is still holding the case.</summary>
    private static readonly CaseStatus[] OpenStatuses =
    [
        CaseStatus.Accepted, CaseStatus.InProgress, CaseStatus.OnHold, CaseStatus.Reopened,
    ];

    // ----- reads ------------------------------------------------------------------------------------------------

    public async Task<AdminOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var counts = await db.Accounts.AsNoTracking()
            .GroupBy(a => new { a.Role, a.Status })
            .Select(g => new { g.Key.Role, g.Key.Status, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var byRole = Enum.GetValues<UserRole>()
            .Select(role => new RoleCountResponse(
                role,
                counts.Where(c => c.Role == role && c.Status == UserStatus.Active).Sum(c => c.Count),
                counts.Where(c => c.Role == role && c.Status == UserStatus.Deactivated).Sum(c => c.Count)))
            .ToList();

        var lockedNow = await db.Accounts.CountAsync(a => a.LockedUntil != null && a.LockedUntil > now, cancellationToken);
        var mustChange = await db.Accounts.CountAsync(
            a => a.MustChangePassword && a.Status == UserStatus.Active, cancellationToken);
        var recent = await audit.ListAsync(new AuditLogQuery { PageSize = 5 }, cancellationToken);

        return new AdminOverviewResponse(byRole, lockedNow, mustChange, recent.Items);
    }

    public async Task<PagedResult<AccountSummaryResponse>> ListAsync(AccountListQuery query, CancellationToken cancellationToken)
    {
        var rows = db.Accounts.AsNoTracking();
        if (query.Role is { } role)
        {
            rows = rows.Where(a => a.Role == role);
        }

        if (query.Status is { } status)
        {
            rows = rows.Where(a => a.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            rows = rows.Where(a =>
                a.Username.Contains(term) || a.FullName.ToLower().Contains(term) || (a.Email != null && a.Email.Contains(term))
                || (a.PhoneNumber != null && a.PhoneNumber.Contains(term)));
        }

        var now = clock.GetUtcNow();
        var total = await rows.CountAsync(cancellationToken);
        var sorted = query.LastSignIn?.ToLowerInvariant() switch
        {
            "desc" => rows.OrderBy(a => a.LastLoginAt == null).ThenByDescending(a => a.LastLoginAt),
            "asc" => rows.OrderBy(a => a.LastLoginAt == null).ThenBy(a => a.LastLoginAt),
            _ => rows.OrderBy(a => a.Role),
        };
        var items = await sorted.ThenBy(a => a.FullName).ThenBy(a => a.Id)
            .Skip(query.Skip).Take(query.PageSize)
            .Select(a => new AccountSummaryResponse(
                a.Id, a.Username, a.FullName, a.Email, a.PhoneNumber, a.Role, a.Status,
                a.LockedUntil != null && a.LockedUntil > now, a.LastLoginAt, a.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<AccountSummaryResponse>(items, query.Page, query.PageSize, total);
    }

    public async Task<AccountDetailResponse> GetAsync(long id, CancellationToken cancellationToken)
    {
        var account = await db.Accounts.AsNoTracking().SingleOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw AccountErrors.NotFound();
        return await ToDetailAsync(account, cancellationToken);
    }

    // ----- create / edit ----------------------------------------------------------------------------------------

    public async Task<AccountWithPasswordResponse> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email)!;
        await EnsureEmailFreeAsync(email, exceptId: null, cancellationToken);
        var phone = NormalizePhone(request.PhoneNumber);
        await EnsurePhoneFreeAsync(phone, exceptId: null, cancellationToken);

        var temporaryPassword = TemporaryPassword.Generate();
        var account = Account.New(
            request.Role, await NewUsernameAsync(email, cancellationToken), request.FullName,
            passwordHasher.Hash(temporaryPassword), mustChangePassword: true);
        account.Email = email;
        account.PhoneNumber = phone;
        db.Accounts.Add(account);
        await ApplyRoleDetailsAsync(account, request, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        audit.Record(AuditTarget.Account, AuditOperation.Create, null, await SnapshotAsync(account.Id, cancellationToken));
        await db.SaveChangesAsync(cancellationToken);

        // The e-mail is the check that the address is right: signing in with this password proves it (Leader 24/11).
        var sent = await SendPasswordAsync(account, temporaryPassword, isNew: true, cancellationToken);
        return new AccountWithPasswordResponse(await GetAsync(account.Id, cancellationToken), sent, sent ? null : temporaryPassword);
    }

    public async Task<AccountDetailResponse> UpdateAsync(long id, UpdateAccountRequest request, CancellationToken cancellationToken)
    {
        var account = await FindAsync(id, cancellationToken);
        await EnsureAllowedAsync(account, AccountAction.Edit, cancellationToken);

        var email = NormalizeEmail(request.Email);
        await EnsureEmailFreeAsync(email, exceptId: id, cancellationToken);
        var phone = NormalizePhone(request.PhoneNumber);
        await EnsurePhoneFreeAsync(phone, exceptId: id, cancellationToken);

        var before = await SnapshotAsync(id, cancellationToken);
        account.FullName = request.FullName.Trim();
        account.Email = email;
        account.PhoneNumber = phone;
        await ApplyRoleDetailsAsync(account, request, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        audit.Record(AuditTarget.Account, AuditOperation.Update, before, await SnapshotAsync(id, cancellationToken));
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(id, cancellationToken);
    }

    // ----- DLG-16 actions ---------------------------------------------------------------------------------------

    public Task<AccountDetailResponse> DeactivateAsync(long id, CancellationToken cancellationToken) =>
        ActAsync(id, AccountAction.Deactivate, AuditOperation.Deactivate, async account =>
        {
            var openCases = await db.Cases.CountAsync(
                c => c.AssignedTechnicianId == id && OpenStatuses.Contains(c.Status), cancellationToken);
            if (openCases > 0)
            {
                throw AccountErrors.OpenWork(openCases);
            }

            account.Status = UserStatus.Deactivated;
        }, cancellationToken);

    public Task<AccountDetailResponse> ReactivateAsync(long id, CancellationToken cancellationToken) =>
        ActAsync(id, AccountAction.Reactivate, AuditOperation.Reactivate, account =>
        {
            account.Status = UserStatus.Active;
            return Task.CompletedTask;
        }, cancellationToken);

    public async Task<TemporaryPasswordResponse> ResetPasswordAsync(long id, CancellationToken cancellationToken)
    {
        var temporaryPassword = TemporaryPassword.Generate();
        await ActAsync(id, AccountAction.ResetPassword, AuditOperation.ResetPassword, account =>
        {
            account.ResetPassword(passwordHasher.Hash(temporaryPassword));
            return Task.CompletedTask;
        }, cancellationToken);
        var sent = await SendPasswordAsync(await FindAsync(id, cancellationToken), temporaryPassword, isNew: false, cancellationToken);
        return new TemporaryPasswordResponse(sent, sent ? null : temporaryPassword);
    }

    public Task<AccountDetailResponse> UnlockAsync(long id, CancellationToken cancellationToken) =>
        ActAsync(id, AccountAction.Unlock, AuditOperation.Unlock, account =>
        {
            account.Unlock();
            return Task.CompletedTask;
        }, cancellationToken);

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        var account = await FindAsync(id, cancellationToken);
        await EnsureAllowedAsync(account, AccountAction.Delete, cancellationToken);
        var before = await SnapshotAsync(id, cancellationToken);

        // Its settings go with it: specialties, buildings, the notifications it received.
        db.TechnicianSpecialties.RemoveRange(db.TechnicianSpecialties.Where(t => t.TechnicianId == id));
        db.StaffBuildingAssignments.RemoveRange(db.StaffBuildingAssignments.Where(a => a.StaffId == id));
        db.ManagerBuildingAssignments.RemoveRange(db.ManagerBuildingAssignments.Where(a => a.ManagerId == id));
        db.Notifications.RemoveRange(db.Notifications.Where(n => n.UserId == id));
        db.Accounts.Remove(account);
        audit.Record(AuditTarget.Account, AuditOperation.Delete, before, null);
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// The temporary password by e-mail (meeting note 18/11). False when e-mail is not set up or the server refused:
    /// the Administrator then gets the password on screen, as before.
    /// </summary>
    private async Task<bool> SendPasswordAsync(Account account, string password, bool isNew, CancellationToken cancellationToken)
    {
        if (account.Email is null) return false;
        var signIn = emailOptions.Value.WebUrl is { Length: > 0 } url ? $"{url.TrimEnd('/')}/login" : "the SmartMaint sign-in page";
        var ways = account.PhoneNumber is null ? account.Email : $"{account.Email} or {account.PhoneNumber}";
        var intro = isNew
            ? "An account was made for you in SmartMaint."
            : "Your SmartMaint password was reset by an administrator.";
        var body = string.Join("\n",
            $"Hello {account.FullName},",
            "",
            intro,
            "",
            $"Sign in at {signIn} with {ways} and this temporary password:",
            "",
            $"    {password}",
            "",
            "You will choose your own password right after signing in.",
            "If you did not expect this e-mail, tell your building's facility office.",
            "",
            "SmartMaint");
        return await email.SendAsync(
            account.Email, isNew ? "Your SmartMaint account" : "Your new SmartMaint password", body, cancellationToken);
    }

    /// <summary>Check the action, change the account, write the audit row (old / new) in the same save.</summary>
    private async Task<AccountDetailResponse> ActAsync(
        long id, AccountAction action, AuditOperation operation, Func<Account, Task> apply, CancellationToken cancellationToken)
    {
        var account = await FindAsync(id, cancellationToken);
        await EnsureAllowedAsync(account, action, cancellationToken);

        var before = await SnapshotAsync(id, cancellationToken);
        await apply(account);
        audit.Record(AuditTarget.Account, operation, before, Snapshot(account, before));
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(id, cancellationToken);
    }

    // ----- rules ------------------------------------------------------------------------------------------------

    private async Task<IReadOnlyList<AccountAction>> AllowedActionsAsync(
        Account account, CancellationToken cancellationToken)
    {
        var self = account.Id == currentUser.Id;
        var active = account.Status == UserStatus.Active;
        var lastAdmin = account.Role == UserRole.Admin && active && !await db.Accounts.AnyAsync(
            a => a.Role == UserRole.Admin && a.Status == UserStatus.Active && a.Id != account.Id, cancellationToken);

        var actions = new List<AccountAction> { AccountAction.Edit };
        if (active && !self && !lastAdmin) actions.Add(AccountAction.Deactivate);
        if (!active) actions.Add(AccountAction.Reactivate);
        if (active && !self) actions.Add(AccountAction.ResetPassword);
        if (account.IsLockedOut(clock.GetUtcNow())) actions.Add(AccountAction.Unlock);
        if (!self && !lastAdmin && !await HasHistoryAsync(account.Id, cancellationToken)) actions.Add(AccountAction.Delete);
        return actions;
    }

    private async Task EnsureAllowedAsync(Account account, AccountAction action, CancellationToken cancellationToken)
    {
        if ((await AllowedActionsAsync(account, cancellationToken)).Contains(action))
        {
            return;
        }

        var lastAdmin = action == AccountAction.Deactivate && account.Role == UserRole.Admin && account.Id != currentUser.Id
                        && account.Status == UserStatus.Active;
        throw lastAdmin ? AccountErrors.LastAdmin() : AccountErrors.ActionNotAllowed();
    }

    /// <summary>
    /// Has this account left a trace in the system?
    /// </summary>
    private async Task<bool> HasHistoryAsync(long id, CancellationToken cancellationToken) =>
        await db.CaseStatusHistory.AnyAsync(h => h.ChangedById == id, cancellationToken)
        || await db.Cases.AnyAsync(
            c => c.RequestingCustomerId == id || c.StaffId == id || c.AssignedTechnicianId == id, cancellationToken)
        || await db.TechnicianRejections.AnyAsync(r => r.TechnicianId == id, cancellationToken)
        || await db.AuditLogs.AnyAsync(a => a.AdminId == id, cancellationToken)
        // meeting note 18/11: "data" also means a cost line, a followed case, a plan, a change list or an assistant chat
        || await db.Costs.AnyAsync(c => c.AddedById == id, cancellationToken)
        || await db.CaseFollowers.AnyAsync(f => f.CustomerId == id, cancellationToken)
        || await db.MaintenanceTriggers.AnyAsync(p => p.StaffId == id || p.ApprovingManagerId == id, cancellationToken)
        || await db.AssetChangeLists.AnyAsync(l => l.StaffId == id || l.ReviewerId == id, cancellationToken)
        || await db.AiConversations.AnyAsync(c => c.UserId == id, cancellationToken);

    /// <summary>Technician rate + specialties, staff / manager buildings: only those of the account's role.</summary>
    private async Task ApplyRoleDetailsAsync(Account account, IRoleDetails details, CancellationToken cancellationToken)
    {
        switch (account)
        {
            case Technician technician:
                technician.HourlyRate = details.HourlyRate;
                await SetSpecialtiesAsync(technician, details.SpecialtyIds ?? [], cancellationToken);
                break;

            case Staff staff:
                await SetStaffBuildingsAsync(staff, await EnsureBuildingsAsync(details.Buildings, cancellationToken), cancellationToken);
                break;

            case Manager manager:
                await SetManagerBuildingsAsync(manager, await EnsureBuildingsAsync(details.Buildings, cancellationToken), cancellationToken);
                break;
        }
    }

    // The three Set* methods below change only what differs (remove the missing rows, add the new ones):
    // removing and re-adding the same composite key in one save is not allowed by EF.

    private async Task SetSpecialtiesAsync(Technician technician, IReadOnlyList<long> wanted, CancellationToken cancellationToken)
    {
        if (await db.Specialties.CountAsync(s => wanted.Contains(s.Id), cancellationToken) != wanted.Count)
        {
            throw AccountErrors.UnknownSpecialty();
        }

        var current = await db.TechnicianSpecialties.Where(t => t.TechnicianId == technician.Id).ToListAsync(cancellationToken);
        db.TechnicianSpecialties.RemoveRange(current.Where(t => !wanted.Contains(t.SpecialtyId)));
        foreach (var specialtyId in wanted.Where(s => current.All(t => t.SpecialtyId != s)))
        {
            db.TechnicianSpecialties.Add(new TechnicianSpecialty { Technician = technician, SpecialtyId = specialtyId });
        }
    }

    private async Task SetStaffBuildingsAsync(
        Staff staff, IReadOnlyList<BuildingAssignmentRequest> wanted, CancellationToken cancellationToken)
    {
        var current = await db.StaffBuildingAssignments.Where(a => a.StaffId == staff.Id).ToListAsync(cancellationToken);
        db.StaffBuildingAssignments.RemoveRange(current.Where(a => wanted.All(w => w.BuildingId != a.BuildingId)));
        foreach (var w in wanted)
        {
            if (w.IsPrimary)
            {
                // One primary staff member per building (E8): the new primary replaces the old one.
                await db.StaffBuildingAssignments
                    .Where(a => a.BuildingId == w.BuildingId && a.IsPrimary && a.StaffId != staff.Id)
                    .ForEachAsync(a => a.IsPrimary = false, cancellationToken);
            }

            var existing = current.SingleOrDefault(a => a.BuildingId == w.BuildingId);
            if (existing is null)
            {
                db.StaffBuildingAssignments.Add(new StaffBuildingAssignment { Staff = staff, BuildingId = w.BuildingId, IsPrimary = w.IsPrimary });
            }
            else
            {
                existing.IsPrimary = w.IsPrimary;
            }
        }
    }

    private async Task SetManagerBuildingsAsync(
        Manager manager, IReadOnlyList<BuildingAssignmentRequest> wanted, CancellationToken cancellationToken)
    {
        var current = await db.ManagerBuildingAssignments.Where(a => a.ManagerId == manager.Id).ToListAsync(cancellationToken);
        db.ManagerBuildingAssignments.RemoveRange(current.Where(a => wanted.All(w => w.BuildingId != a.BuildingId)));
        foreach (var w in wanted.Where(w => current.All(a => a.BuildingId != w.BuildingId)))
        {
            db.ManagerBuildingAssignments.Add(new ManagerBuildingAssignment { Manager = manager, BuildingId = w.BuildingId });
        }
    }

    private async Task<IReadOnlyList<BuildingAssignmentRequest>> EnsureBuildingsAsync(
        IReadOnlyList<BuildingAssignmentRequest>? buildings, CancellationToken cancellationToken)
    {
        var list = buildings ?? [];
        var ids = list.Select(b => b.BuildingId).ToList();
        if (await db.Buildings.CountAsync(b => ids.Contains(b.Id), cancellationToken) != ids.Count)
        {
            throw AccountErrors.UnknownBuilding();
        }

        return list;
    }

    private async Task EnsureEmailFreeAsync(string? email, long? exceptId, CancellationToken cancellationToken)
    {
        if (email is not null && await db.Accounts.AnyAsync(a => a.Email == email && a.Id != exceptId, cancellationToken))
        {
            throw AccountErrors.EmailTaken();
        }
    }

    private async Task<Account> FindAsync(long id, CancellationToken cancellationToken) =>
        await db.Accounts.SingleOrDefaultAsync(a => a.Id == id, cancellationToken) ?? throw AccountErrors.NotFound();

    private static string? NormalizeEmail(string? email) => string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();

    /// <summary>Stored as digits (a leading + kept), so a sign-in with spaces or dashes still matches.</summary>
    private static string? NormalizePhone(string? phone) =>
        string.IsNullOrWhiteSpace(phone) ? null : PhoneNumbers.Normalize(phone);

    /// <summary>A phone number signs in (meeting note 18/11), so two accounts cannot share one.</summary>
    private async Task EnsurePhoneFreeAsync(string? phone, long? exceptId, CancellationToken cancellationToken)
    {
        if (phone is null) return;
        var others = await db.Accounts.Where(a => a.PhoneNumber != null && a.Id != exceptId)
            .Select(a => a.PhoneNumber!).ToListAsync(cancellationToken);
        if (others.Any(p => PhoneNumbers.Normalize(p) == phone)) throw AccountErrors.PhoneTaken();
    }

    /// <summary>
    /// The ERD's username column stays (unique, not null) but nobody types it any more: made from the e-mail, with a
    /// number added when taken ("lan", "lan2").
    /// </summary>
    private async Task<string> NewUsernameAsync(string email, CancellationToken cancellationToken)
    {
        var local = new string(email.Split('@')[0].ToLowerInvariant().Where(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '_' or '-').ToArray());
        var stem = local.Length >= 3 ? local[..Math.Min(local.Length, 40)] : $"user{local}";
        var candidate = stem;
        for (var n = 2; await db.Accounts.AnyAsync(a => a.Username == candidate, cancellationToken); n++)
        {
            candidate = $"{stem}{n}";
        }

        return candidate;
    }

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    // ----- mapping ----------------------------------------------------------------------------------------------

    private async Task<AccountDetailResponse> ToDetailAsync(Account a, CancellationToken cancellationToken)
    {
        var id = a.Id;
        var specialties = await db.TechnicianSpecialties.AsNoTracking()
            .Where(t => t.TechnicianId == id)
            .OrderBy(t => t.Specialty.Name)
            .Select(t => new SpecialtyResponse(t.SpecialtyId, t.Specialty.Name))
            .ToListAsync(cancellationToken);

        var buildings = a.Role == UserRole.Manager
            ? await db.ManagerBuildingAssignments.AsNoTracking()
                .Where(m => m.ManagerId == id)
                .Select(m => new AssignedBuildingResponse(m.BuildingId, m.Building.Name, false))
                .ToListAsync(cancellationToken)
            : await db.StaffBuildingAssignments.AsNoTracking()
                .Where(s => s.StaffId == id)
                .Select(s => new AssignedBuildingResponse(s.BuildingId, s.Building.Name, s.IsPrimary))
                .ToListAsync(cancellationToken);

        var hasHistory = await HasHistoryAsync(id, cancellationToken);
        var now = clock.GetUtcNow();
        return new AccountDetailResponse(
            a.Id, a.Username, a.FullName, a.Email, a.PhoneNumber, a.Role, a.Status, a.IsLockedOut(now), a.LastLoginAt,
            a.CreatedAt, a.MustChangePassword, a.IsLockedOut(now) ? a.LockedUntil : null,
            (a as Technician)?.HourlyRate, specialties, buildings.OrderBy(b => b.BuildingName).ToList(), hasHistory,
            await AllowedActionsAsync(a, cancellationToken));
    }

    /// <summary>What the audit log keeps of an account (never the password hash).</summary>
    private sealed record AccountSnapshot(
        long Id,
        string Username,
        string FullName,
        string? Email,
        string? PhoneNumber,
        UserRole Role,
        UserStatus Status,
        bool MustChangePassword,
        bool Locked,
        decimal? HourlyRate,
        IReadOnlyList<long> SpecialtyIds,
        IReadOnlyList<long> BuildingIds);

    private async Task<AccountSnapshot> SnapshotAsync(long id, CancellationToken cancellationToken)
    {
        var detail = await GetAsync(id, cancellationToken);
        return new AccountSnapshot(
            detail.Id, detail.Username, detail.FullName, detail.Email, detail.PhoneNumber, detail.Role, detail.Status,
            detail.MustChangePassword, detail.IsLocked, detail.HourlyRate,
            detail.Specialties.Select(s => s.Id).ToList(), detail.Buildings.Select(b => b.BuildingId).ToList());
    }

    /// <summary>The snapshot after a DLG-16 action: only status, password and lock fields change.</summary>
    private AccountSnapshot Snapshot(Account account, AccountSnapshot before) => before with
    {
        Status = account.Status,
        MustChangePassword = account.MustChangePassword,
        Locked = account.IsLockedOut(clock.GetUtcNow()),
    };
}
