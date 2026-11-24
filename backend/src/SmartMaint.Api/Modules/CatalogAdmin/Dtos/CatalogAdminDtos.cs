using SmartMaint.Api.Entities.Cases;

namespace SmartMaint.Api.Modules.CatalogAdmin.Dtos;

// ADM-06 Maintenance categories: the lists people choose from. "UsedBy" = how many rows point at it (0 = can delete).

/// <summary>ERD: CaseCategories (+ NOT IN ERD v5 asset type, archive). AssetTypeId empty = every asset type.</summary>
public sealed record CaseCategoryAdminResponse(
    long Id,
    string GroupName,
    string Name,
    string? Description,
    long? AssetTypeId,
    string? AssetTypeName,
    DateTimeOffset? ArchivedAt,
    int UsedBy);

public sealed record CaseCategoryRequest(string Name, string? Description, long? AssetTypeId);

/// <summary>ERD: AssetCategory(Code, CategoryName, Description).</summary>
public sealed record AssetCategoryResponse(long Id, string Code, string CategoryName, string? Description, int UsedBy, int AssetCount);

public sealed record AssetCategoryRequest(string Code, string CategoryName, string? Description);

/// <summary>ERD: AssetType(CategoryID, Code, Name, Description). UsedBy = its assets + the faults linked to it.</summary>
public sealed record AssetTypeResponse(
    long Id, long CategoryId, string CategoryName, string Code, string Name, string? Description, int? ExpectedLifeYears, int UsedBy, int AssetCount);

/// <summary>ExpectedLifeYears: NOT IN ERD v5 (F5 R03), optional.</summary>
public sealed record AssetTypeRequest(long CategoryId, string Code, string Name, string? Description, int? ExpectedLifeYears = null);

/// <summary>ERD: Manufacturers(Name, PhoneNumber, Email).</summary>
public sealed record ManufacturerResponse(long Id, string Name, string? PhoneNumber, string? Email, int UsedBy);

public sealed record ManufacturerRequest(string Name, string? PhoneNumber, string? Email);

/// <summary>ERD: Specialties(Name).</summary>
public sealed record SpecialtyResponse(long Id, string Name, DateTimeOffset? ArchivedAt, int UsedBy);

public sealed record SpecialtyRequest(string Name);
