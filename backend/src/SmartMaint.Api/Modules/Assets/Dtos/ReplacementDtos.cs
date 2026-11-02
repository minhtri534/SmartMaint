using SmartMaint.Api.Entities.Cases;

namespace SmartMaint.Api.Modules.Assets.Dtos;

// A06 / A07 replacement requests (F5 → F1). Names match frontend/src/features/assets/types.ts.

/// <summary>A link to another asset (A07: "replaces" / "replaced by").</summary>
public sealed record AssetRefResponse(long Id, string Name);

/// <summary>
/// A closed case replaced the asset. The Administrator sees the asset, the model and who asked, not the case content
/// (BR-11): <see cref="CaseCode"/> is a reference to quote to the staff, <see cref="CaseId"/> is empty for them.
/// </summary>
public sealed record ReplacementRequestResponse(
    long Id,
    string CaseCode,
    long? CaseId,
    AssetRefResponse OldAsset,
    long OldAssetTypeId,
    string OldAssetTypeName,
    long? OldManufacturerId,
    AssetLocationResponse Location,
    string NewModel,
    string? NewSerial,
    string RequestedBy,
    DateTimeOffset RequestedAt,
    ReplacementStatus Status,
    AssetRefResponse? NewAsset,
    DateTimeOffset? FinishedAt,
    string? DismissReason);

/// <summary>The new asset, registered in the room of the old one; the old one is retired in the same step.</summary>
public sealed record CompleteReplacementRequest(
    string Name, long TypeId, long? ManufacturerId, bool HasCopyright, DateOnly? WarrantyEndDate);

public sealed record DismissReplacementRequest(string Reason);
