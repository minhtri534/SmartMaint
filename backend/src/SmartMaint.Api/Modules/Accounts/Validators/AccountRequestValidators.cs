using FluentValidation;
using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Modules.Accounts.Dtos;

namespace SmartMaint.Api.Modules.Accounts.Validators;

// Same rules as the web form (frontend/src/features/accounts/schemas.ts).

internal static class AccountRules
{
    public const int UsernameMinLength = 3;
    public const int UsernameMaxLength = 50;
    public const int FullNameMinLength = 2;
    public const int FullNameMaxLength = 60;
    public const decimal MaxHourlyRate = 10_000_000m;

    public static IRuleBuilderOptions<T, string> ValidUsername<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .Length(UsernameMinLength, UsernameMaxLength)
            .WithMessage($"{UsernameMinLength} to {UsernameMaxLength} characters.")
            .Matches("^[A-Za-z0-9._-]+$").WithMessage("Letters, digits, dot, dash and underscore only.");

    public static IRuleBuilderOptions<T, string> ValidFullName<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .Must(n => n.Trim().Length >= FullNameMinLength).WithMessage($"At least {FullNameMinLength} characters.")
            .MaximumLength(FullNameMaxLength).WithMessage($"At most {FullNameMaxLength} characters.");

    /// <summary>Optional: empty is fine.</summary>
    public static IRuleBuilderOptions<T, string?> ValidEmail<T>(this IRuleBuilder<T, string?> rule) =>
        rule.MaximumLength(254)
            .Must(e => string.IsNullOrWhiteSpace(e) || System.Net.Mail.MailAddress.TryCreate(e.Trim(), out _))
            .WithMessage("Enter a valid e-mail address.");

    /// <summary>Optional: empty is fine.</summary>
    public static IRuleBuilderOptions<T, string?> ValidPhone<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(p => string.IsNullOrWhiteSpace(p) || System.Text.RegularExpressions.Regex.IsMatch(p.Trim(), @"^\+?[0-9 ]{8,20}$"))
            .WithMessage("8 to 20 digits, may start with +.");
}

/// <summary>The role-dependent fields, shared by create / edit / change role (IValidator is contravariant).</summary>
public sealed class RoleDetailsValidator : AbstractValidator<IRoleDetails>
{
    public RoleDetailsValidator()
    {
        RuleFor(x => x.RoomId).GreaterThan(0).When(x => x.RoomId is not null);
        RuleFor(x => x.HourlyRate).InclusiveBetween(0, AccountRules.MaxHourlyRate).When(x => x.HourlyRate is not null)
            .WithMessage($"Between 0 and {AccountRules.MaxHourlyRate:N0}.");
        RuleFor(x => x.SpecialtyIds)
            .Must(ids => ids is null || ids.Distinct().Count() == ids.Count).WithMessage("A specialty is listed twice.");
        RuleFor(x => x.Buildings)
            .Must(b => b is null || b.Select(x => x.BuildingId).Distinct().Count() == b.Count)
            .WithMessage("A building is listed twice.");
    }
}

public sealed class AccountListQueryValidator : AbstractValidator<AccountListQuery>
{
    public AccountListQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, PageRequest.MaxPageSize);
        RuleFor(x => x.Role).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Search).MaximumLength(100);
    }
}

public sealed class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequest>
{
    public CreateAccountRequestValidator()
    {
        RuleFor(x => x.Username).ValidUsername();
        RuleFor(x => x.FullName).ValidFullName();
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.PhoneNumber).ValidPhone();
        RuleFor(x => x.Role).IsInEnum();
        RuleFor(x => x.RoomId).NotNull().When(x => x.Role == UserRole.Customer).WithMessage("Choose the customer's room.");
        Include(new RoleDetailsValidator());
    }
}

public sealed class UpdateAccountRequestValidator : AbstractValidator<UpdateAccountRequest>
{
    public UpdateAccountRequestValidator()
    {
        RuleFor(x => x.FullName).ValidFullName();
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.PhoneNumber).ValidPhone();
        // The role is not in the request: the service checks that a customer keeps a room.
        Include(new RoleDetailsValidator());
    }
}

public sealed class ChangeRoleRequestValidator : AbstractValidator<ChangeRoleRequest>
{
    public ChangeRoleRequestValidator()
    {
        RuleFor(x => x.Role).IsInEnum();
        RuleFor(x => x.RoomId).NotNull().When(x => x.Role == UserRole.Customer).WithMessage("Choose the customer's room.");
        Include(new RoleDetailsValidator());
    }
}
