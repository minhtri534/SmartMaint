using FluentValidation;
using SmartMaint.Api.Modules.Facilities.Dtos;

namespace SmartMaint.Api.Modules.Facilities.Validators;

// Same rules as the web dialogs (frontend/src/features/facilities/schemas.ts).

public sealed class BuildingRequestValidator : AbstractValidator<BuildingRequest>
{
    public BuildingRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Must(n => n.Trim().Length >= 2).WithMessage("At least 2 characters.")
            .MaximumLength(100);
        RuleFor(x => x.Address).MaximumLength(200);
    }
}

public sealed class FloorRequestValidator : AbstractValidator<FloorRequest>
{
    public FloorRequestValidator()
    {
        RuleFor(x => x.BuildingId).GreaterThan(0);
        RuleFor(x => x.FloorNumber).NotEmpty().MaximumLength(10)
            .Matches("^[A-Za-z0-9]+$").WithMessage("Letters and digits only, e.g. G, B1, 12.");
    }
}

public sealed class RoomRequestValidator : AbstractValidator<RoomRequest>
{
    public RoomRequestValidator()
    {
        RuleFor(x => x.FloorId).GreaterThan(0);
        RuleFor(x => x.RoomNumber).NotEmpty().MaximumLength(50);
    }
}
