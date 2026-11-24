using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Modules.Accounts.Dtos;

namespace SmartMaint.Api.Modules.Accounts;

/// <summary>
/// Administrator account management (ADM-01..04, DLG-16). There is no self-registration (BR-14): every account is made
/// here. Every change is written to the audit log with the old and new values (BR-16). Admin only (controller).
/// </summary>
public interface IAccountService
{
    /// <summary>ADM-01: accounts per role, locks, recent audit rows.</summary>
    Task<AdminOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken);

    /// <summary>ADM-02.</summary>
    Task<PagedResult<AccountSummaryResponse>> ListAsync(AccountListQuery query, CancellationToken cancellationToken);

    /// <summary>ADM-04.</summary>
    Task<AccountDetailResponse> GetAsync(long id, CancellationToken cancellationToken);

    /// <summary>ADM-03 create: returns the temporary password once (AUTH-04 forces a change at first sign-in).</summary>
    Task<AccountWithPasswordResponse> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken);

    /// <summary>ADM-03 edit.</summary>
    Task<AccountDetailResponse> UpdateAsync(long id, UpdateAccountRequest request, CancellationToken cancellationToken);

    /// <summary>DLG-16. Blocked for yourself, for the last active admin and for a technician holding open cases.</summary>
    Task<AccountDetailResponse> DeactivateAsync(long id, CancellationToken cancellationToken);

    Task<AccountDetailResponse> ReactivateAsync(long id, CancellationToken cancellationToken);

    /// <summary>DLG-16: a new temporary password, shown once; the user must change it at the next sign-in.</summary>
    Task<TemporaryPasswordResponse> ResetPasswordAsync(long id, CancellationToken cancellationToken);

    /// <summary>DLG-16: ends a BR-18 sign-in lock early.</summary>
    Task<AccountDetailResponse> UnlockAsync(long id, CancellationToken cancellationToken);

    /// <summary>Only an account with no data at all (meeting note 18/11); otherwise deactivate it.</summary>
    Task DeleteAsync(long id, CancellationToken cancellationToken);
}
