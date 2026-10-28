using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Entities.Users;

namespace SmartMaint.Api.Modules.Cases;

/// <summary>Everything a user can do to a case in F3. Sent to the web as snake_case ("hold", "close"...).</summary>
public enum CaseAction
{
    Cancel,
    Approve,
    Reject,
    Assign,
    Accept,
    Decline,
    Start,
    Hold,
    Resume,
    Complete,
    Close,
    Reopen,
    Rate,
}

/// <summary>One arrow of the F3 state machine (docs/flows/F3_Su_Co_Thong_Thuong/F3_Trang5.png): who, from where, to where.</summary>
internal sealed record TransitionRule(CaseAction Action, UserRole Role, CaseStatus[] From, CaseStatus? To);

/// <summary>
/// The F3 state machine as a table, one row per arrow. CaseService is the only place that reads it.
/// Not here (next iterations): reassign, change urgency / extend the deadline, customer "not fixed" report,
/// the F5 hand-over decision beyond the hold reason.
/// </summary>
internal static class CaseTransitions
{
    /// <summary>The "approved, being handled" frame of page 5, where a staff member may still cancel.</summary>
    private static readonly CaseStatus[] Handling =
    [
        CaseStatus.Approved, CaseStatus.Assigned, CaseStatus.Accepted,
        CaseStatus.InProgress, CaseStatus.OnHold, CaseStatus.Reopened,
    ];

    public static readonly IReadOnlyList<TransitionRule> Rules =
    [
        // Page 1: intake and review
        new(CaseAction.Cancel, UserRole.Customer, [CaseStatus.Pending], CaseStatus.Cancelled),
        new(CaseAction.Cancel, UserRole.Staff, [CaseStatus.Pending, .. Handling], CaseStatus.Cancelled),
        new(CaseAction.Approve, UserRole.Staff, [CaseStatus.Pending], CaseStatus.Approved),
        new(CaseAction.Reject, UserRole.Staff, [CaseStatus.Pending], CaseStatus.Rejected),

        // Page 2: assignment and repair
        new(CaseAction.Assign, UserRole.Staff, [CaseStatus.Approved], CaseStatus.Assigned),
        new(CaseAction.Accept, UserRole.Technician, [CaseStatus.Assigned, CaseStatus.Reopened], CaseStatus.Accepted),
        new(CaseAction.Decline, UserRole.Technician, [CaseStatus.Assigned, CaseStatus.Reopened], CaseStatus.Approved),
        new(CaseAction.Start, UserRole.Technician, [CaseStatus.Accepted], CaseStatus.InProgress),
        new(CaseAction.Hold, UserRole.Technician, [CaseStatus.InProgress], CaseStatus.OnHold),
        new(CaseAction.Resume, UserRole.Technician, [CaseStatus.OnHold], CaseStatus.InProgress),
        // "Beyond the technician's ability" waits for the staff member, who may let the technician continue.
        new(CaseAction.Resume, UserRole.Staff, [CaseStatus.OnHold], CaseStatus.InProgress),
        new(CaseAction.Complete, UserRole.Technician, [CaseStatus.InProgress], CaseStatus.Completed),

        // Page 4: verification and after closing
        new(CaseAction.Close, UserRole.Staff, [CaseStatus.Completed], CaseStatus.Closed),
        new(CaseAction.Reopen, UserRole.Staff, [CaseStatus.Completed, CaseStatus.Closed], CaseStatus.Reopened),
        new(CaseAction.Rate, UserRole.Customer, [CaseStatus.Closed], null),
    ];

    public static TransitionRule? Find(CaseAction action, UserRole role, CaseStatus status) =>
        Rules.FirstOrDefault(r => r.Action == action && r.Role == role && r.From.Contains(status));
}
