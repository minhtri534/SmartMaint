using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Modules.Cases.Dtos;

namespace SmartMaint.Api.Modules.Cases;

/// <summary>
/// F3 (normal incident). Every method reads the caller from ICurrentUser and applies the data scope itself (B3):
/// a customer sees own cases, staff and managers the buildings assigned to them, a technician the cases assigned to them.
/// A case outside the caller's scope is "not found". Status changes follow CaseTransitions, else "case.invalid_transition".
/// </summary>
public interface ICaseService
{
    Task<PagedResult<CaseSummaryResponse>> ListAsync(PageRequest page, CancellationToken cancellationToken);

    Task<CaseDetailResponse> GetAsync(long id, CancellationToken cancellationToken);

    /// <summary>F3 step 1 (customer).</summary>
    Task<CaseDetailResponse> CreateAsync(CreateCaseRequest request, CancellationToken cancellationToken);

    Task<CaseDetailResponse> CancelAsync(long id, ReasonRequest request, CancellationToken cancellationToken);

    /// <summary>F3 step 2 (staff): confirm urgency and who pays. Urgency "emergency" hands the case over to F4.</summary>
    Task<CaseDetailResponse> ApproveAsync(long id, ApproveCaseRequest request, CancellationToken cancellationToken);

    Task<CaseDetailResponse> RejectAsync(long id, RejectCaseRequest request, CancellationToken cancellationToken);

    /// <summary>F3 step 3 (staff).</summary>
    Task<CaseDetailResponse> AssignAsync(long id, AssignCaseRequest request, CancellationToken cancellationToken);

    Task<CaseDetailResponse> AcceptAsync(long id, CancellationToken cancellationToken);

    Task<CaseDetailResponse> DeclineAsync(long id, ReasonRequest request, CancellationToken cancellationToken);

    /// <summary>The SLA clock starts here: due = now + the urgency's SLA hours.</summary>
    Task<CaseDetailResponse> StartAsync(long id, CancellationToken cancellationToken);

    Task<CaseDetailResponse> HoldAsync(long id, HoldCaseRequest request, CancellationToken cancellationToken);

    Task<CaseDetailResponse> ResumeAsync(long id, CancellationToken cancellationToken);

    /// <summary>F3 step 5: the repair record, then Completed (not Closed: the staff member still verifies).</summary>
    Task<CaseDetailResponse> CompleteAsync(long id, CompleteWorkRequest request, CancellationToken cancellationToken);

    Task<CaseDetailResponse> CloseAsync(long id, CancellationToken cancellationToken);

    Task<CaseDetailResponse> ReopenAsync(long id, ReasonRequest request, CancellationToken cancellationToken);

    Task<CaseDetailResponse> RateAsync(long id, RateCaseRequest request, CancellationToken cancellationToken);
}
