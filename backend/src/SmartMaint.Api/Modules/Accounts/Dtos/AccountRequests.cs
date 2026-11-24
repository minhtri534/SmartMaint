using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Entities.Accounts;

namespace SmartMaint.Api.Modules.Accounts.Dtos;

/// <summary>ADM-02 filters: ?role=technician&amp;status=active&amp;search=duc&amp;page=1.</summary>
public sealed record AccountListQuery : PageRequest
{
    public UserRole? Role { get; init; }
    public UserStatus? Status { get; init; }

    /// <summary>Matches the username, the full name or the e-mail.</summary>
    public string? Search { get; init; }

    /// <summary>"desc" or "asc": sort by the last sign-in; accounts that never signed in come last.</summary>
    public string? LastSignIn { get; init; }
}

/// <summary>A building a staff member or a manager looks after. IsPrimary only means something for staff (E8).</summary>
public sealed record BuildingAssignmentRequest(long BuildingId, bool IsPrimary);

/// <summary>
/// The fields that depend on the role. Only the ones of the account's role are used:
/// technician -> HourlyRate, SpecialtyIds; staff / manager -> Buildings.
/// </summary>
public interface IRoleDetails
{
    decimal? HourlyRate { get; }
    IReadOnlyList<long>? SpecialtyIds { get; }
    IReadOnlyList<BuildingAssignmentRequest>? Buildings { get; }
}

/// <summary>
/// ADM-03 (create). Meeting note 18/11: no username to type (the e-mail or the phone number signs in; a username is
/// still made from the e-mail for the database) and the e-mail is required: the temporary password goes there.
/// </summary>
public sealed record CreateAccountRequest(
    string FullName,
    string? Email,
    string? PhoneNumber,
    UserRole Role,
    decimal? HourlyRate,
    IReadOnlyList<long>? SpecialtyIds,
    IReadOnlyList<BuildingAssignmentRequest>? Buildings) : IRoleDetails;

/// <summary>ADM-03 (edit). The username and the role do not change here.</summary>
public sealed record UpdateAccountRequest(
    string FullName,
    string? Email,
    string? PhoneNumber,
    decimal? HourlyRate,
    IReadOnlyList<long>? SpecialtyIds,
    IReadOnlyList<BuildingAssignmentRequest>? Buildings) : IRoleDetails;

