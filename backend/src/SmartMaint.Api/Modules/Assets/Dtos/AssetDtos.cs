using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Entities.Cases;

namespace SmartMaint.Api.Modules.Assets.Dtos;

// F1 assets. Names match frontend/src/features/assets/types.ts.

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
    Edit,
    Retire,
    Delete,

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

/// <summary>
/// AST-05. The web reads the CSV file and sends its text. DryRun = only check; otherwise the rows are imported
/// all together, or none when any row has an error.
/// </summary>
public sealed record ImportAssetsRequest(string Csv, bool DryRun);

public sealed record ImportRowResult(int Line, string Name, IReadOnlyList<string> Errors);

public sealed record ImportAssetsResponse(int RowCount, int ErrorCount, bool Imported, IReadOnlyList<ImportRowResult> Rows);

/// <summary>AST-07: where a scanned QR label should send the caller.</summary>
public sealed record QrTargetResponse(
    long Id,
    string Name,
    AssetLocationResponse Location,
    bool IsRetired,
    bool CanReport,
    bool CanOpenDetail);
