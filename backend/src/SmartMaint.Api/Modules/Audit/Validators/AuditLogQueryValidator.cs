using FluentValidation;
using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Modules.Audit.Dtos;

namespace SmartMaint.Api.Modules.Audit.Validators;

public sealed class AuditLogQueryValidator : AbstractValidator<AuditLogQuery>
{
    public AuditLogQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, PageRequest.MaxPageSize);
        RuleFor(x => x.TargetType).IsInEnum();
        RuleFor(x => x.Operation).IsInEnum();
        RuleFor(x => x.To)
            .GreaterThanOrEqualTo(x => x.From).When(x => x.From is not null && x.To is not null)
            .WithMessage("The end day must be on or after the start day.");
    }
}
