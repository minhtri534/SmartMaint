using Microsoft.AspNetCore.Mvc;
using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Modules.Accounts;
using SmartMaint.Api.Modules.Accounts.Dtos;

namespace SmartMaint.Api.Controllers;

/// <summary>Administrator account management (ADM-01..04, DLG-16). Admin only.</summary>
[ApiController]
[Route("api/accounts")]
[Produces("application/json")]
[AuthorizeRoles(UserRole.Admin)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
public sealed class AccountsController(IAccountService accounts) : ControllerBase
{
    /// <summary>ADM-01.</summary>
    [HttpGet("overview")]
    public Task<AdminOverviewResponse> Overview(CancellationToken cancellationToken) =>
        accounts.GetOverviewAsync(cancellationToken);

    /// <summary>ADM-02.</summary>
    [HttpGet]
    public Task<PagedResult<AccountSummaryResponse>> List([FromQuery] AccountListQuery query, CancellationToken cancellationToken) =>
        accounts.ListAsync(query, cancellationToken);

    /// <summary>ADM-04.</summary>
    [HttpGet("{id:long}")]
    public Task<AccountDetailResponse> Get(long id, CancellationToken cancellationToken) =>
        accounts.GetAsync(id, cancellationToken);

    /// <summary>ADM-03 create. The temporary password is in the response only.</summary>
    [HttpPost]
    public async Task<CreatedAtActionResult> Create(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        var created = await accounts.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Account.Id }, created);
    }

    /// <summary>ADM-03 edit.</summary>
    [HttpPut("{id:long}")]
    public Task<AccountDetailResponse> Update(long id, UpdateAccountRequest request, CancellationToken cancellationToken) =>
        accounts.UpdateAsync(id, request, cancellationToken);

    /// <summary>DLG-16. The account gets a new id (see IAccountService).</summary>
    [HttpPost("{id:long}/change-role")]
    public Task<AccountDetailResponse> ChangeRole(long id, ChangeRoleRequest request, CancellationToken cancellationToken) =>
        accounts.ChangeRoleAsync(id, request, cancellationToken);

    [HttpPost("{id:long}/deactivate")]
    public Task<AccountDetailResponse> Deactivate(long id, CancellationToken cancellationToken) =>
        accounts.DeactivateAsync(id, cancellationToken);

    [HttpPost("{id:long}/reactivate")]
    public Task<AccountDetailResponse> Reactivate(long id, CancellationToken cancellationToken) =>
        accounts.ReactivateAsync(id, cancellationToken);

    [HttpPost("{id:long}/reset-password")]
    public Task<TemporaryPasswordResponse> ResetPassword(long id, CancellationToken cancellationToken) =>
        accounts.ResetPasswordAsync(id, cancellationToken);

    [HttpPost("{id:long}/unlock")]
    public Task<AccountDetailResponse> Unlock(long id, CancellationToken cancellationToken) =>
        accounts.UnlockAsync(id, cancellationToken);
}
