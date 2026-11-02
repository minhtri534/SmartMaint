using SmartMaint.Api.Common.Models;
using SmartMaint.Api.Modules.Assets.Dtos;

namespace SmartMaint.Api.Modules.Assets;

/// <summary>
/// F1 (Huy) — assets. Staff and managers read the assets of their buildings, the Administrator all of them and is the
/// only one who changes them (Leader 02/11). An asset with history is retired, never deleted (BR-10).
/// </summary>
public interface IAssetService
{
    /// <summary>AST-02.</summary>
    Task<PagedResult<AssetSummaryResponse>> ListAsync(AssetListQuery query, CancellationToken cancellationToken);

    /// <summary>AST-03, with the repair history for staff and managers (G5).</summary>
    Task<AssetDetailResponse> GetAsync(long id, CancellationToken cancellationToken);

    /// <summary>AST-04.</summary>
    Task<AssetDetailResponse> CreateAsync(AssetRequest request, CancellationToken cancellationToken);

    Task<AssetDetailResponse> UpdateAsync(long id, AssetRequest request, CancellationToken cancellationToken);

    /// <summary>Blocked while the asset has open cases.</summary>
    Task<AssetDetailResponse> RetireAsync(long id, CancellationToken cancellationToken);

    /// <summary>Only for an asset that never had a case (a mistake when creating it).</summary>
    Task DeleteAsync(long id, CancellationToken cancellationToken);

    /// <summary>AST-05: check (DryRun) or import a CSV, all rows or none.</summary>
    Task<ImportAssetsResponse> ImportAsync(ImportAssetsRequest request, CancellationToken cancellationToken);

    /// <summary>AST-07: any signed-in role scans a label; says whether the caller can report on it or open it.</summary>
    Task<QrTargetResponse> GetQrTargetAsync(long id, CancellationToken cancellationToken);
}
