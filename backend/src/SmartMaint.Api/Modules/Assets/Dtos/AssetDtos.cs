using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Entities.Cases;

namespace SmartMaint.Api.Modules.Assets.Dtos;

// F1 assets. Names match frontend/src/features/assets/types.ts.

/// <summary>A link to another asset ("replaces" / "replaced by", kept for assets replaced earlier).</summary>
public sealed record AssetRefResponse(long Id, string Name);

/// <summary>AST-02 filters.</summary>
public sealed record AssetListQuery : PageRequest
{
    public long? BuildingId { get; init; }
    public long? RoomId { get; init; }
    public long? CategoryId { get; init; }
    public long? TypeId { get; init; }

    /// <summary>Matches the asset name.</summary>
    public string? Search { get; init; }

    public bool IncludeRetired { get; init; }

    public AssetListStatus? Status { get; init; }

    /// <summary>"name_asc", "name_desc", "warranty_asc" or "warranty_desc"; empty = by place.</summary>
    public string? Sort { get; init; }
}

/// <summary>The status filter of the asset list: the same three states as the pill on each row.</summary>
public enum AssetListStatus
{
    InService,
    OpenCase,
    Retired,
}

public sealed record AssetLocationResponse(long BuildingId, string BuildingName, string FloorName, long RoomId, string RoomName);

/// <summary>One row of AST-02.</summary>
public record AssetSummaryResponse(
    long Id,
    string Name,
    long TypeId,
    string TypeName,
    string CategoryName,
    string? ManufacturerName,
    AssetLocationResponse Location,
    DateOnly? WarrantyEndDate,
    bool UnderWarranty,
    bool HasCopyright,
    DateTimeOffset? RetiredAt,
    int OpenCases);

public enum AssetAction
{
    /// <summary>Staff of the building: put this asset in a change list (edit, retire or delete it there).</summary>
    ProposeChange,

    /// <summary>Staff, F5 way 2: open a survey case (the asset fails often).</summary>
    OpenSurvey,
}

/// <summary>A case of this asset (G5 maintenance history). Never sent to the Administrator (BR-11).</summary>
public sealed record AssetCaseResponse(
    long Id,
    string Code,
    string Title,
    CaseStatus Status,
    DateTimeOffset SubmittedAt,
    DateTimeOffset? ClosedAt,
    string? RootCause);

/// <summary>
/// AST-03. <see cref="History"/> is null for the Administrator (no case content, BR-11); <see cref="CaseCount"/> is
/// always there.
/// </summary>
public sealed record AssetDetailResponse(
    long Id,
    string Name,
    long TypeId,
    string TypeName,
    string CategoryName,
    string? ManufacturerName,
    AssetLocationResponse Location,
    DateOnly? WarrantyEndDate,
    bool UnderWarranty,
    bool HasCopyright,
    DateTimeOffset? RetiredAt,
    int OpenCases,
    long? ManufacturerId,
    DateTimeOffset CreatedAt,
    int CaseCount,
    IReadOnlyList<AssetCaseResponse>? History,
    int RepairsLast12Months,
    bool FrequentFailure,
    int? ExpectedLifeYears,
    AssetRefResponse? Replaces,
    AssetRefResponse? ReplacedBy,
    IReadOnlyList<AssetAction> AllowedActions)
    : AssetSummaryResponse(
        Id, Name, TypeId, TypeName, CategoryName, ManufacturerName, Location, WarrantyEndDate, UnderWarranty,
        HasCopyright, RetiredAt, OpenCases);

/// <summary>AST-04. ERD: Assets(Name, RoomID, TypeID, ManufacturerID, HasCopyright, WarrantyEndDate).</summary>
public sealed record AssetRequest(
    string Name,
    long RoomId,
    long TypeId,
    long? ManufacturerId,
    bool HasCopyright,
    DateOnly? WarrantyEndDate);

/// <summary>AST-07: where a scanned QR label should send the caller.</summary>
public sealed record QrTargetResponse(
    long Id,
    string Name,
    long TypeId,
    AssetLocationResponse Location,
    bool IsRetired,
    bool CanReport,
    bool CanOpenDetail);
