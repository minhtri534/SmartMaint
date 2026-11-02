using Microsoft.EntityFrameworkCore;
using SmartMaint.Api.Data;
using SmartMaint.Api.Entities;
using SmartMaint.Api.Entities.Accounts;
using SmartMaint.Api.Entities.Buildings;
using SmartMaint.Api.Entities.Cases;
using SmartMaint.Api.Entities.Core;
using SmartMaint.Api.Modules.Audit;
using SmartMaint.Api.Modules.CatalogAdmin.Dtos;

namespace SmartMaint.Api.Modules.CatalogAdmin;

/// <summary>
/// Each list follows the same three steps: Save = create (id null) or update, with a duplicate check;
/// Delete = only when nothing uses the item. <see cref="SaveAsync{T}"/> / <see cref="DeleteAsync{T}"/> hold the shared part.
/// </summary>
internal sealed class CatalogAdminService(AppDbContext db, IAuditService audit) : ICatalogAdminService
{
    // ----- service types (case categories) ----------------------------------------------------------------------

    public async Task<IReadOnlyList<CaseCategoryAdminResponse>> ListCaseCategoriesAsync(CancellationToken cancellationToken) =>
        await db.CaseCategories.AsNoTracking()
            .OrderBy(c => c.GroupName).ThenBy(c => c.Name)
            .Select(c => new CaseCategoryAdminResponse(
                c.Id, c.GroupName, c.Name, c.Description, c.IsEmergency, c.DefaultPriority.Code, c.DefaultCostBearer,
                c.PhotoRequired, db.Cases.Count(x => x.CategoryId == c.Id)))
            .ToListAsync(cancellationToken);

    public async Task<CaseCategoryAdminResponse> SaveCaseCategoryAsync(
        long? id, CaseCategoryRequest request, CancellationToken cancellationToken)
    {
        var group = request.GroupName.Trim();
        var name = request.Name.Trim();
        var priority = await db.PriorityLevels.SingleAsync(p => p.Code == request.DefaultPriority, cancellationToken);
        var category = await SaveAsync(
            db.CaseCategories, id, AuditTarget.CaseCategory,
            c => c.GroupName.ToLower() == group.ToLower() && c.Name.ToLower() == name.ToLower(),
            c =>
            {
                c.GroupName = group;
                c.Name = name;
                c.Description = Blank(request.Description);
                // An emergency type always opens an emergency case (N26).
                c.IsEmergency = request.IsEmergency;
                c.DefaultPriorityId = priority.Id;
                c.DefaultCostBearer = request.DefaultCostBearer;
                c.PhotoRequired = request.PhotoRequired;
            },
            c => new { c.Id, c.GroupName, c.Name, c.IsEmergency, c.DefaultPriorityId, c.DefaultCostBearer, c.PhotoRequired },
            cancellationToken);
        return (await ListCaseCategoriesAsync(cancellationToken)).Single(c => c.Id == category.Id);
    }

    public Task DeleteCaseCategoryAsync(long id, CancellationToken cancellationToken) =>
        DeleteAsync(db.CaseCategories, id, AuditTarget.CaseCategory, () => db.Cases.CountAsync(c => c.CategoryId == id, cancellationToken),
            c => new { c.Id, c.GroupName, c.Name }, cancellationToken);

    // ----- asset categories -------------------------------------------------------------------------------------

    public async Task<IReadOnlyList<AssetCategoryResponse>> ListAssetCategoriesAsync(CancellationToken cancellationToken) =>
        await db.AssetCategories.AsNoTracking()
            .OrderBy(c => c.CategoryName)
            .Select(c => new AssetCategoryResponse(
                c.Id, c.Code, c.CategoryName, c.Description, db.AssetTypes.Count(t => t.CategoryId == c.Id)))
            .ToListAsync(cancellationToken);

    public async Task<AssetCategoryResponse> SaveAssetCategoryAsync(
        long? id, AssetCategoryRequest request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        var category = await SaveAsync(
            db.AssetCategories, id, AuditTarget.AssetCategory,
            c => c.Code == code,
            c =>
            {
                c.Code = code;
                c.CategoryName = request.CategoryName.Trim();
                c.Description = Blank(request.Description);
            },
            c => new { c.Id, c.Code, c.CategoryName, c.Description },
            cancellationToken);
        return (await ListAssetCategoriesAsync(cancellationToken)).Single(c => c.Id == category.Id);
    }

    public Task DeleteAssetCategoryAsync(long id, CancellationToken cancellationToken) =>
        DeleteAsync(db.AssetCategories, id, AuditTarget.AssetCategory,
            () => db.AssetTypes.CountAsync(t => t.CategoryId == id, cancellationToken),
            c => new { c.Id, c.Code, c.CategoryName }, cancellationToken);

    // ----- asset types ------------------------------------------------------------------------------------------

    public async Task<IReadOnlyList<AssetTypeResponse>> ListAssetTypesAsync(CancellationToken cancellationToken) =>
        await db.AssetTypes.AsNoTracking()
            .OrderBy(t => t.Category.CategoryName).ThenBy(t => t.Name)
            .Select(t => new AssetTypeResponse(
                t.Id, t.CategoryId, t.Category.CategoryName, t.Code, t.Name, t.Description, t.ExpectedLifeYears,
                db.Assets.Count(a => a.TypeId == t.Id)))
            .ToListAsync(cancellationToken);

    public async Task<AssetTypeResponse> SaveAssetTypeAsync(long? id, AssetTypeRequest request, CancellationToken cancellationToken)
    {
        if (!await db.AssetCategories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken))
        {
            throw CatalogAdminErrors.UnknownCategory();
        }

        var code = request.Code.Trim().ToUpperInvariant();
        var type = await SaveAsync(
            db.AssetTypes, id, AuditTarget.AssetType,
            t => t.Code == code,
            t =>
            {
                t.CategoryId = request.CategoryId;
                t.Code = code;
                t.Name = request.Name.Trim();
                t.Description = Blank(request.Description);
                t.ExpectedLifeYears = request.ExpectedLifeYears;
            },
            t => new { t.Id, t.CategoryId, t.Code, t.Name, t.Description, t.ExpectedLifeYears },
            cancellationToken);
        return (await ListAssetTypesAsync(cancellationToken)).Single(t => t.Id == type.Id);
    }

    public Task DeleteAssetTypeAsync(long id, CancellationToken cancellationToken) =>
        DeleteAsync(db.AssetTypes, id, AuditTarget.AssetType, () => db.Assets.CountAsync(a => a.TypeId == id, cancellationToken),
            t => new { t.Id, t.Code, t.Name }, cancellationToken);

    // ----- manufacturers ----------------------------------------------------------------------------------------

    public async Task<IReadOnlyList<ManufacturerResponse>> ListManufacturersAsync(CancellationToken cancellationToken) =>
        await db.Manufacturers.AsNoTracking()
            .OrderBy(m => m.Name)
            .Select(m => new ManufacturerResponse(m.Id, m.Name, m.PhoneNumber, m.Email, db.Assets.Count(a => a.ManufacturerId == m.Id)))
            .ToListAsync(cancellationToken);

    public async Task<ManufacturerResponse> SaveManufacturerAsync(
        long? id, ManufacturerRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        var maker = await SaveAsync(
            db.Manufacturers, id, AuditTarget.Manufacturer,
            m => m.Name.ToLower() == name.ToLower(),
            m =>
            {
                m.Name = name;
                m.PhoneNumber = Blank(request.PhoneNumber);
                m.Email = Blank(request.Email)?.ToLowerInvariant();
            },
            m => new { m.Id, m.Name, m.PhoneNumber, m.Email },
            cancellationToken);
        return (await ListManufacturersAsync(cancellationToken)).Single(m => m.Id == maker.Id);
    }

    public Task DeleteManufacturerAsync(long id, CancellationToken cancellationToken) =>
        DeleteAsync(db.Manufacturers, id, AuditTarget.Manufacturer,
            () => db.Assets.CountAsync(a => a.ManufacturerId == id, cancellationToken),
            m => new { m.Id, m.Name }, cancellationToken);

    // ----- specialties ------------------------------------------------------------------------------------------

    public async Task<IReadOnlyList<SpecialtyResponse>> ListSpecialtiesAsync(CancellationToken cancellationToken) =>
        await db.Specialties.AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SpecialtyResponse(s.Id, s.Name, db.TechnicianSpecialties.Count(t => t.SpecialtyId == s.Id)))
            .ToListAsync(cancellationToken);

    public async Task<SpecialtyResponse> SaveSpecialtyAsync(long? id, SpecialtyRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        var specialty = await SaveAsync(
            db.Specialties, id, AuditTarget.Specialty,
            s => s.Name.ToLower() == name.ToLower(),
            s => s.Name = name,
            s => new { s.Id, s.Name },
            cancellationToken);
        return (await ListSpecialtiesAsync(cancellationToken)).Single(s => s.Id == specialty.Id);
    }

    public Task DeleteSpecialtyAsync(long id, CancellationToken cancellationToken) =>
        DeleteAsync(db.Specialties, id, AuditTarget.Specialty,
            () => db.TechnicianSpecialties.CountAsync(t => t.SpecialtyId == id, cancellationToken),
            s => new { s.Id, s.Name }, cancellationToken);

    // ----- shared steps -----------------------------------------------------------------------------------------

    /// <summary>Create (id null) or update one item, refuse a duplicate, write the audit row with old / new values.</summary>
    private async Task<T> SaveAsync<T>(
        DbSet<T> set,
        long? id,
        AuditTarget target,
        System.Linq.Expressions.Expression<Func<T, bool>> sameKey,
        Action<T> apply,
        Func<T, object> snapshot,
        CancellationToken cancellationToken)
        where T : Entity, new()
    {
        var duplicate = await set.Where(sameKey).AnyAsync(x => x.Id != (id ?? 0), cancellationToken);
        if (duplicate)
        {
            throw CatalogAdminErrors.Duplicate();
        }

        T item;
        object? before = null;
        if (id is { } existingId)
        {
            item = await set.SingleOrDefaultAsync(x => x.Id == existingId, cancellationToken) ?? throw CatalogAdminErrors.NotFound();
            before = snapshot(item);
            apply(item);
        }
        else
        {
            item = new T();
            apply(item);
            set.Add(item);
            await db.SaveChangesAsync(cancellationToken);
        }

        audit.Record(target, id is null ? AuditOperation.Create : AuditOperation.Update, before, snapshot(item));
        await db.SaveChangesAsync(cancellationToken);
        return item;
    }

    /// <summary>Delete one item only when <paramref name="usedBy"/> is 0.</summary>
    private async Task DeleteAsync<T>(
        DbSet<T> set, long id, AuditTarget target, Func<Task<int>> usedBy, Func<T, object> snapshot, CancellationToken cancellationToken)
        where T : Entity
    {
        var item = await set.SingleOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw CatalogAdminErrors.NotFound();
        var count = await usedBy();
        if (count > 0)
        {
            throw CatalogAdminErrors.InUse(count);
        }

        set.Remove(item);
        audit.Record(target, AuditOperation.Delete, snapshot(item), null);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
