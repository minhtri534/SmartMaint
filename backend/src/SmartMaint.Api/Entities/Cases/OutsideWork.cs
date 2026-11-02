using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Buildings;

namespace SmartMaint.Api.Entities.Cases;

// F5 repair or replace / outside party (docs/flows/F5_Sua_Hay_Thay, Leader 02/11). None of this is in ERD v5
// (it only has Cases.TimeOutsourced): listed in docs/database/ERD_v5_Thay_Doi_Khi_Code.md.

/// <summary>F5 step ①: what the technician recommends after the assessment.</summary>
public enum TechnicianRecommendation
{
    /// <summary>It can be repaired, but not by the technician alone (parts, time).</summary>
    Repairable,

    /// <summary>Better to replace the asset.</summary>
    Replace,

    /// <summary>Needs an outside specialist (elevator, fire system, high voltage).</summary>
    Specialist,
}

/// <summary>Who the outside party is and how it is paid.</summary>
public enum OutsideWorkKind
{
    /// <summary>The manufacturer under warranty (step ④).</summary>
    Warranty,

    /// <summary>A contractor with a package maintenance contract: no quote (elevator, fire system...).</summary>
    Contract,

    /// <summary>A contractor quoting per job: the quote goes to whoever pays (step ⑤–⑥).</summary>
    Quote,
}

public enum OutsideWorkJob
{
    Repair,
    Replace,
}

/// <summary>
/// Quote → AwaitingDecision → Working (warranty / contract start at Working) → Done once the technician checked the
/// result; FailedCheck → Working again after a redo; Declined (the party refused, or the payer said no); Cancelled
/// (the staff chose another way, or the case was cancelled).
/// </summary>
public enum OutsideWorkStatus
{
    Quoting,
    AwaitingDecision,
    Working,
    FailedCheck,
    Declined,
    Done,
    Cancelled,
}

/// <summary>
/// One call to an outside party on a case (N3). A case may have several: the manufacturer refuses, then a
/// contractor; a redo stays on the same row (RedoCount). No money is stored: the building decides and pays
/// outside the system (N22). CreatedAt → FinishedAt is the outside phase F6 reports separately (N16).
/// </summary>
public class CaseThirdParty : AuditableEntity
{
    public long CaseId { get; set; }
    public Case Case { get; set; } = null!;

    public OutsideWorkKind Kind { get; set; }
    public OutsideWorkJob Job { get; set; }
    public OutsideWorkStatus Status { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    /// <summary>Name and phone of the person to call.</summary>
    public string Contact { get; set; } = string.Empty;

    public DateTimeOffset? AppointmentAt { get; set; }

    /// <summary>Why this party, what they were asked to do; redo and refusal reasons are added to the case history.</summary>
    public string? Note { get; set; }

    public int RedoCount { get; set; }

    // Quote (Kind = Quote): who pays, and for the customer, to whom the quote was forwarded (R04).
    public CostBearer? QuoteBearer { get; set; }
    public string? QuoteSentTo { get; set; }
    public DateTimeOffset? QuoteSentAt { get; set; }

    /// <summary>The staff were reminded once that the customer has not answered the quote for 3 days.</summary>
    public DateTimeOffset? QuoteRemindedAt { get; set; }

    // The payer's decision (step ⑥) with its evidence: who decided, and a photo of the email / signed paper.
    public bool? DecisionGo { get; set; }
    public string? DecidedBy { get; set; }
    public DateTimeOffset? DecidedAt { get; set; }

    /// <summary>Storage path of the evidence photo, like CaseImages.Image.</summary>
    public string? EvidenceImage { get; set; }

    /// <summary>Done (checked OK), Declined or Cancelled.</summary>
    public DateTimeOffset? FinishedAt { get; set; }

    public bool IsActive => Status is OutsideWorkStatus.Quoting or OutsideWorkStatus.AwaitingDecision
        or OutsideWorkStatus.Working or OutsideWorkStatus.FailedCheck;
}

public enum ReplacementStatus
{
    Open,
    Done,
    Dismissed,
}

/// <summary>
/// A06 / A07: a closed case replaced an asset; the Administrator registers the new asset (it "replaces" the old one)
/// and retires the old one. The Administrator sees this request, never the case content (BR-11).
/// </summary>
public class AssetReplacementRequest : AuditableEntity
{
    public long CaseId { get; set; }
    public Case Case { get; set; } = null!;

    public long OldAssetId { get; set; }
    public Asset OldAsset { get; set; } = null!;

    /// <summary>What the technician wrote in the repair record.</summary>
    public string NewModel { get; set; } = string.Empty;
    public string? NewSerial { get; set; }

    /// <summary>The staff member who closed the case.</summary>
    public long RequestedById { get; set; }
    public Staff RequestedBy { get; set; } = null!;

    public ReplacementStatus Status { get; set; }

    public long? NewAssetId { get; set; }
    public Asset? NewAsset { get; set; }

    public DateTimeOffset? FinishedAt { get; set; }
    public string? DismissReason { get; set; }
}
