using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Modules.Cases.Dtos;

namespace SmartMaint.Api.Modules.Cases;

/// <summary>Entity -> DTO, by hand (B6). The case must be loaded with the includes of CaseService.WithRefs.</summary>
internal static class CaseMapper
{
    public static CaseSummaryResponse ToSummary(Case c) => new(
        c.Id,
        Case.CodeOf(c.Id),
        c.Title,
        c.Status,
        c.Priority.Code,
        ServiceTypeName(c.Category),
        ToLocation(c),
        ToPersonOrNull(c.Customer),
        ToPersonOrNull(c.Staff),
        ToPersonOrNull(c.Technician),
        c.SubmittedAt,
        c.DueAt,
        SlaStartedAt(c),
        c.CompletedAt,
        IsEmergencyFlow(c),
        c.IsPreventive,
        ToEmergency(c),
        c.Status == CaseStatus.OnHold ? c.HoldReason : null,
        c.IsSurvey,
        ToBrief(c),
        c.PlannedStartAt,
        c.PlannedEndAt);

    /// <summary>
    /// Needs Images, History(ChangedBy), Rejections(Technician) and Asset loaded too.
    /// <paramref name="photoUrls"/> maps CaseImages.Image (storage path) to a URL the browser can open.
    /// <paramref name="showWork"/> false (the customer, meeting 07/11): no costs, tasks, repair record, assessment or outside parties.
    /// </summary>
    public static CaseDetailResponse ToDetail(
        Case c,
        IReadOnlyList<CaseAction> allowedActions,
        IReadOnlyDictionary<string, string> photoUrls,
        bool showWork,
        DetailExtras extras)
    {
        var history = c.History.OrderBy(h => h.ChangedAt).ThenBy(h => h.Id).ToList();
        return new CaseDetailResponse(
            c.Id,
            Case.CodeOf(c.Id),
            c.Title,
            c.Status,
            c.Priority.Code,
            ServiceTypeName(c.Category),
            ToLocation(c),
            ToPersonOrNull(c.Customer),
            ToPersonOrNull(c.Staff),
            ToPersonOrNull(c.Technician),
            c.SubmittedAt,
            c.DueAt,
            SlaStartedAt(c),
            c.CompletedAt,
            IsEmergencyFlow(c),
            c.IsPreventive,
            ToEmergency(c),
            c.Status == CaseStatus.OnHold ? c.HoldReason : null,
            c.IsSurvey,
            ToBrief(c),
            c.PlannedStartAt,
            c.PlannedEndAt,
            c.Description,
            c.CategoryId,
            c.Asset is null ? null : new AssetResponse(c.Asset.Id, c.Asset.Name, c.Asset.RoomId),
            c.PreviousCaseId is { } previous ? ToRef(previous) : null,
            c.Followers.OrderBy(f => f.AddedAt).Select(f => ToPerson(f.Customer)).ToList(),
            c.Images
                .OrderBy(i => i.Id)
                .Where(i => photoUrls.ContainsKey(i.Image))
                .Select(i => new PhotoResponse(i.Id, photoUrls[i.Image], i.Kind))
                .ToList(),
            // The ERD keeps only the new status; the previous one is the row before.
            history
                .Select((h, i) => new StatusChangeResponse(
                    h.ChangedAt, i == 0 ? null : history[i - 1].NewStatus, h.NewStatus, ToPerson(h.ChangedBy), h.Reason))
                .ToList(),
            c.Rejections
                .OrderBy(r => r.RejectedAt)
                .Select(r => new RejectionResponse(ToPerson(r.Technician), r.RejectedAt, r.Reason))
                .ToList(),
            showWork && c.RootCause is not null && c.ActualCompletionHours is { } hours
                ? new RepairReportResponse(c.RootCause, c.PartsUsed, hours)
                : null,
            c.RatingStars is { } stars && c.RatedAt is { } ratedAt ? new RatingResponse(stars, c.RatingComment, ratedAt) : null,
            c.Status == CaseStatus.OnHold && c.HoldReason is { } reason
                ? new HoldResponse(reason, c.HoldUntil)
                : null,
            showWork
                ? c.Costs.OrderBy(x => x.Id).Select(x => new CostResponse(
                    x.Id, x.Name, x.Description, x.Amount, x.ReceiptCode,
                    x.ReceiptImage is { } receipt && photoUrls.TryGetValue(receipt, out var receiptUrl) ? receiptUrl : null)).ToList()
                : null,
            showWork && c.Costs.Count > 0 ? c.Costs.Sum(x => x.Amount) : null,
            showWork ? c.ActualCompletionHours : null,
            showWork ? c.ClockStartedAt : null,
            c.NotFixedReason is { } notFixed && c.NotFixedAt is { } reportedAt ? new NotFixedResponse(notFixed, reportedAt) : null,
            c.TriggerWorkOrder?.TriggerId,
            c.Tasks.Where(_ => showWork).OrderBy(t => t.Id).Select(t => new CaseTaskResponse(
                t.Id, t.Name, t.Description, t.Status,
                t.Notes.OrderBy(n => n.Id).Select(n => new TaskNoteResponse(n.Note, n.CreatedAt)).ToList(),
                t.Images.OrderBy(i => i.Id).Where(i => photoUrls.ContainsKey(i.Image))
                    .Select(i => new TaskPhotoResponse(photoUrls[i.Image], i.Notes)).ToList(),
                t.RepairCaseId, t.RepairCaseId is { } r ? Case.CodeOf(r) : null, t.DoneBy, t.DoneAt)).ToList(),
            extras.MergedCaseIds.Select(ToRef).ToList(),
            c.IncidentReportedAt is { } reportedOn && c.IncidentFollowUp is { } followUp
                ? new IncidentReportResponse(c.IncidentDamage ?? string.Empty, c.IncidentLessons ?? string.Empty, followUp, reportedOn)
                : null,
            // Meeting 07/11: the request for an outside party (description, reason); old assessments also have a recommendation.
            showWork && c.AssessedAt is { } assessedAt
                ? new AssessmentResponse(c.AssessmentCondition ?? string.Empty, c.AssessmentCause ?? string.Empty, c.AssessmentRecommendation, assessedAt)
                : null,
            c.ThirdParties.Where(_ => showWork).OrderBy(t => t.Id).Select(t => new ThirdPartyResponse(
                t.Id, t.Kind, t.Job, t.Status, t.CompanyName, t.Contact, t.AppointmentAt, t.Note, t.RedoCount,
                t.QuoteBearer, t.QuoteSentTo, t.QuoteSentAt, t.DecisionGo, t.DecidedBy, t.DecidedAt,
                t.EvidenceImage is { } e && photoUrls.TryGetValue(e, out var url) ? url : null,
                t.CreatedAt, t.FinishedAt)).ToList(),
            extras.DecisionInfo,
            c.ReplacementModel is { } model
                ? new ReplacementResponse(model, c.ReplacementSerial, extras.Replacement?.Status, extras.Replacement?.NewAssetId)
                : null,
            extras.NextCaseId is { } next ? ToRef(next) : null,
            extras.StatusChanges,
            allowedActions);
    }

    /// <summary>What GetAsync loads besides the case itself.</summary>
    public sealed record DetailExtras(
        IReadOnlyList<long> MergedCaseIds,
        DecisionInfoResponse? DecisionInfo,
        AssetReplacementRequest? Replacement,
        long? NextCaseId,
        IReadOnlyList<EditStatusChange> StatusChanges);

    /// <summary>The latest outside party (lists and the F5 board).</summary>
    private static OutsideWorkBriefResponse? ToBrief(Case c) =>
        c.ThirdParties.OrderByDescending(t => t.Id).FirstOrDefault() is { } t
            ? new OutsideWorkBriefResponse(t.Kind, t.Job, t.Status, t.CompanyName, t.AppointmentAt)
            : null;

    /// <summary>
    /// In F4 = emergency urgency AND the 24/7 clock started (EmergencyAt). A customer may choose "Emergency" on a normal
    /// service type: that case stays Pending in F3 until the staff confirms it on approval (Leader 09/11).
    /// </summary>
    public static bool IsEmergencyFlow(Case c) => c.Priority.Code == CasePriority.Emergency && c.EmergencyAt is not null;

    /// <summary>F4 runs its clock from when the case became an emergency; F3 from when the technician started.</summary>
    private static DateTimeOffset? SlaStartedAt(Case c) => c.EmergencyAt ?? c.StartedAt;

    private static EmergencyResponse? ToEmergency(Case c) => c.EmergencyAt is { } since
        ? new EmergencyResponse(since, c.AcknowledgedAt, c.MadeSafeAt, c.MergedIntoCaseId is { } m ? ToRef(m) : null)
        : null;

    private static CaseRefResponse ToRef(long id) => new(id, Case.CodeOf(id));

    public static string ServiceTypeName(CaseCategory category) => $"{category.GroupName} · {category.Name}";

    private static PersonResponse ToPerson(Account account) => new(account.Id, account.FullName);

    private static PersonResponse? ToPersonOrNull(Account? account) => account is null ? null : ToPerson(account);

    private static LocationResponse ToLocation(Case c) => new(
        c.Room.Floor.BuildingId, c.Room.Floor.Building.Name, c.Room.Floor.DisplayName, c.RoomId, c.Room.RoomNumber);
}
