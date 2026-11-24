using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Cases;

namespace SmartMaint.Api.Modules.Cases;

/// <summary>Everything a user can do to a case. Sent to the web as snake_case ("hold", "edit"...).</summary>
public enum CaseAction
{
    Cancel,
    Approve,
    Reject,
    Accept,
    Decline,
    Start,
    Hold,
    Resume,
    Complete,
    Rate,

    /// <summary>
    /// Meeting 07/11: the staff's one "Edit" (the work order form). Replaces change urgency, "let technician continue",
    /// close and reopen.
    /// </summary>
    Edit,

    /// <summary>Meeting note 18/11: a button of its own, so the followers are easy to find.</summary>
    ManageFollowers,

    /// <summary>Meeting note 18/11: the technician is no longer a field of Edit.</summary>
    Reassign,

    // Part 4 (02/11): the deadline, the customer's "not fixed" report, cost lines
    ExtendDue,
    ReportNotFixed,
    AcceptNotFixed,
    DeclineNotFixed,

    /// <summary>Meeting note 18/11: a closed case is read only; reopening it is the one thing the staff may still do.</summary>
    Reopen,
    UpdateCosts,

    // Meeting 07/11, the technician: work hours (clock or typed) and the costs they paid, with a receipt
    LogHours,
    AddCost,

    /// <summary>Meeting note 18/11: the technician corrects a cost line they added themselves.</summary>
    EditOwnCost,

    // Tasks (F2 checklist items; meeting 07/11: every case has tasks)
    UpdateTask,
    SetTaskThirdParty,

    // Part 6 (F4): emergency. Meeting 07/11: no merge (a repeat is rejected as a duplicate), no separate downgrade (Edit).
    Acknowledge,
    AddLog,
    WriteIncidentReport,

    // Part 7 (F5). Meeting 07/11: the staff only records the hand-over to an outside party (no quote, no refusal);
    // the tasks the party did are marked on the task list.
    RequestDecision,
    Outsource,
}

/// <summary>One arrow of the state machine: who, from where, to where.</summary>
internal sealed record TransitionRule(CaseAction Action, UserRole Role, CaseStatus[] From, CaseStatus? To);

/// <summary>
/// The case state machine as a table, one row per arrow. CaseService is the only place that reads it.
/// To = null: the status stays, or (Edit) the service decides it from the request.
/// </summary>
internal static class CaseTransitions
{
    /// <summary>Approved and being handled: a staff member may still cancel or edit.</summary>
    internal static readonly CaseStatus[] Handling =
    [
        CaseStatus.Approved, CaseStatus.Accepted, CaseStatus.InProgress, CaseStatus.OnHold, CaseStatus.Reopened,
    ];

    /// <summary>The technician already took the job: a new technician means a new case (meeting 07/11).</summary>
    internal static readonly CaseStatus[] Taken = [CaseStatus.Accepted, CaseStatus.InProgress, CaseStatus.OnHold];

    public static readonly IReadOnlyList<TransitionRule> Rules =
    [
        // Intake and review. Approve sets the technician too (no "Assigned" status, meeting 07/11).
        new(CaseAction.Cancel, UserRole.Customer, [CaseStatus.Pending], CaseStatus.Cancelled),
        new(CaseAction.Cancel, UserRole.Staff, [CaseStatus.Pending, .. Handling], CaseStatus.Cancelled),
        new(CaseAction.Approve, UserRole.Staff, [CaseStatus.Pending], CaseStatus.Approved),
        new(CaseAction.Reject, UserRole.Staff, [CaseStatus.Pending], CaseStatus.Rejected),

        // The technician named on an approved (or reopened) case accepts or declines it, then does the work.
        new(CaseAction.Accept, UserRole.Technician, [CaseStatus.Approved, CaseStatus.Reopened], CaseStatus.Accepted),
        new(CaseAction.Decline, UserRole.Technician, [CaseStatus.Approved, CaseStatus.Reopened], CaseStatus.Approved),
        new(CaseAction.Start, UserRole.Technician, [CaseStatus.Accepted], CaseStatus.InProgress),
        new(CaseAction.Hold, UserRole.Technician, [CaseStatus.InProgress], CaseStatus.OnHold),
        new(CaseAction.Resume, UserRole.Technician, [CaseStatus.OnHold], CaseStatus.InProgress),
        new(CaseAction.Complete, UserRole.Technician, [CaseStatus.InProgress], CaseStatus.Completed),
        new(CaseAction.Rate, UserRole.Customer, [CaseStatus.Closed], null),

        // The staff's one Edit: the status it moves to (continue / close / reopen) comes from the request.
        // Meeting note 18/11: a closed case is only read (and reopened, below).
        new(CaseAction.Edit, UserRole.Staff, [.. Handling, CaseStatus.Completed], null),
        new(CaseAction.ManageFollowers, UserRole.Staff, [.. Handling, CaseStatus.Completed], null),
        new(CaseAction.Reassign, UserRole.Staff, Handling, null),
        new(CaseAction.ExtendDue, UserRole.Staff, [.. Taken, CaseStatus.Reopened], null),
        new(CaseAction.UpdateCosts, UserRole.Staff, [.. Handling, CaseStatus.Completed], null),
        new(CaseAction.Reopen, UserRole.Staff, [CaseStatus.Closed], CaseStatus.Reopened),

        // Customer: "not fixed" within 7 days of closing; the staff member reopens or declines it
        new(CaseAction.ReportNotFixed, UserRole.Customer, [CaseStatus.Closed], null),
        new(CaseAction.AcceptNotFixed, UserRole.Staff, [CaseStatus.Closed], CaseStatus.Reopened),
        new(CaseAction.DeclineNotFixed, UserRole.Staff, [CaseStatus.Closed], null),

        // Meeting 07/11: the technician keeps the work hours and records the costs they paid.
        new(CaseAction.LogHours, UserRole.Technician, [CaseStatus.Accepted, CaseStatus.InProgress, CaseStatus.OnHold], null),
        new(CaseAction.AddCost, UserRole.Technician, [CaseStatus.Accepted, CaseStatus.InProgress, CaseStatus.OnHold, CaseStatus.Completed], null),
        new(CaseAction.EditOwnCost, UserRole.Technician, Taken, null),

        // Tasks: the technician marks them; the staff marks the ones an outside party did.
        new(CaseAction.UpdateTask, UserRole.Technician, [CaseStatus.InProgress, CaseStatus.OnHold], null),
        new(CaseAction.SetTaskThirdParty, UserRole.Staff, [.. Handling, CaseStatus.Completed], null),

        // F4 (docs/flows/F4_Khan_Cap/F4_Trang5.png): an emergency starts at Approved and then uses the arrows above.
        // CaseService.IsAllowed keeps these to emergency cases.
        new(CaseAction.Acknowledge, UserRole.Staff, Handling, null),
        new(CaseAction.AddLog, UserRole.Staff, [.. Handling, CaseStatus.Completed], null),
        new(CaseAction.WriteIncidentReport, UserRole.Staff, [CaseStatus.Closed], null),

        // F5: the technician asks the staff (assessment), the staff hands the job to an outside party.
        new(CaseAction.RequestDecision, UserRole.Technician, [CaseStatus.InProgress], CaseStatus.OnHold),
        new(CaseAction.Outsource, UserRole.Staff, Taken, CaseStatus.OnHold),
    ];

    public static TransitionRule? Find(CaseAction action, UserRole role, CaseStatus status) =>
        Rules.FirstOrDefault(r => r.Action == action && r.Role == role && r.From.Contains(status));
}
