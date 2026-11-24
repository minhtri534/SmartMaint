using FluentValidation;
using SmartMaint.Api.Entities.Buildings;
using SmartMaint.Api.Modules.AssetChanges.Dtos;

namespace SmartMaint.Api.Modules.AssetChanges.Validators;

// The shape of a list; what depends on the data (names taken, open cases...) is checked by the service, line by line.
// Same rules as frontend/src/features/asset-changes/schemas.ts.

public sealed class ChangeListRequestValidator : AbstractValidator<ChangeListRequest>
{
    public const int MaxItems = 300;

    private static readonly AssetChangeKind[] OnBuilding = [AssetChangeKind.EditBuilding, AssetChangeKind.DeleteBuilding];
    private static readonly AssetChangeKind[] OnFloor = [AssetChangeKind.RenameFloor, AssetChangeKind.DeleteFloor];
    private static readonly AssetChangeKind[] OnRoom = [AssetChangeKind.RenameRoom, AssetChangeKind.DeleteRoom];
    private static readonly AssetChangeKind[] OnAsset =
        [AssetChangeKind.EditAsset, AssetChangeKind.RetireAsset, AssetChangeKind.DeleteAsset];
    private static readonly AssetChangeKind[] AssetValues = [AssetChangeKind.AddAsset, AssetChangeKind.EditAsset];
    private static readonly AssetChangeKind[] BuildingValues = [AssetChangeKind.AddBuilding, AssetChangeKind.EditBuilding];

    public ChangeListRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().Must(t => t.Trim().Length >= 3).WithMessage("At least 3 characters.").MaximumLength(120);
        RuleFor(x => x.Note).MaximumLength(500);
        // An empty list is a draft the staff member is still working on; it cannot be sent (service).
        RuleFor(x => x.Items).Must(i => i.Count <= MaxItems).WithMessage($"At most {MaxItems} changes in one list.")
            .Must(i => i.Select(x => x.Key).Distinct().Count() == i.Count).WithMessage("Two lines have the same key.");
        RuleForEach(x => x.Items).ChildRules(i =>
        {
            i.RuleFor(x => x.Kind).IsInEnum();
            // "n1", "n2"... : the tree's own keys ("b12", "f3"...) never clash with them
            i.RuleFor(x => x.Key).NotEmpty().MaximumLength(20).Matches("^n[0-9]+$").WithMessage("A line key looks like n1.");
            i.RuleFor(x => x.ParentKey).MaximumLength(20);
            i.RuleFor(x => x.BuildingId).NotNull().WithMessage("Choose the building.").When(x => OnBuilding.Contains(x.Kind));
            i.RuleFor(x => x).Must(x => x.BuildingId is not null || !string.IsNullOrEmpty(x.ParentKey))
                .WithMessage("Choose the building of the new floor.").When(x => x.Kind == AssetChangeKind.AddFloor);
            i.RuleFor(x => x.FloorId).NotNull().WithMessage("Choose the floor.").When(x => OnFloor.Contains(x.Kind));
            i.RuleFor(x => x).Must(x => x.FloorId is not null || !string.IsNullOrEmpty(x.ParentKey))
                .WithMessage("Choose the floor of the new room.").When(x => x.Kind == AssetChangeKind.AddRoom);
            i.RuleFor(x => x.RoomId).NotNull().WithMessage("Choose the room.").When(x => OnRoom.Contains(x.Kind));
            i.RuleFor(x => x.AssetId).NotNull().WithMessage("Choose the asset.").When(x => OnAsset.Contains(x.Kind));
            i.RuleFor(x => x).Must(x => x.RoomId is not null || !string.IsNullOrEmpty(x.ParentKey))
                .WithMessage("Choose the room of the asset.").When(x => AssetValues.Contains(x.Kind));
            i.RuleFor(x => x.Name).NotEmpty().WithMessage("Fill in the name.")
                .When(x => x.Kind is not (AssetChangeKind.DeleteBuilding or AssetChangeKind.DeleteFloor
                    or AssetChangeKind.DeleteRoom or AssetChangeKind.RetireAsset or AssetChangeKind.DeleteAsset));
            i.RuleFor(x => x.Name).Must(n => n!.Trim().Length >= 2).WithMessage("At least 2 characters.").MaximumLength(100)
                .When(x => BuildingValues.Contains(x.Kind) && !string.IsNullOrEmpty(x.Name));
            i.RuleFor(x => x.Address).MaximumLength(200);
            i.RuleFor(x => x.Name).MaximumLength(10).Matches("^[A-Za-z0-9]+$")
                .WithMessage("Letters and digits only, e.g. G, B1, 12.")
                .When(x => x.Kind is AssetChangeKind.AddFloor or AssetChangeKind.RenameFloor && !string.IsNullOrEmpty(x.Name));
            i.RuleFor(x => x.Name).MaximumLength(50)
                .When(x => x.Kind is AssetChangeKind.AddRoom or AssetChangeKind.RenameRoom);
            i.RuleFor(x => x.Name).Must(n => n!.Trim().Length >= 2).WithMessage("At least 2 characters.").MaximumLength(150)
                .When(x => AssetValues.Contains(x.Kind) && !string.IsNullOrEmpty(x.Name));
            i.RuleFor(x => x.TypeId).NotNull().WithMessage("Choose the asset type.").When(x => AssetValues.Contains(x.Kind));
        });
    }
}

public sealed class RejectChangeListRequestValidator : AbstractValidator<RejectChangeListRequest>
{
    public RejectChangeListRequestValidator() =>
        RuleFor(x => x.Reason).NotEmpty().Must(r => r.Trim().Length >= 10).WithMessage("At least 10 characters.").MaximumLength(500);
}
