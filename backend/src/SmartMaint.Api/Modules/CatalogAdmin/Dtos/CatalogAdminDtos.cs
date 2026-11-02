using SmartMaint.Api.Entities.Cases;

namespace SmartMaint.Api.Modules.CatalogAdmin.Dtos;

// ADM-06 Maintenance categories: the lists people choose from. "UsedBy" = how many rows point at it (0 = can delete).

/// <summary>ERD: CaseCategories (+ NOT IN ERD v5 group, emergency flag, defaults, photo rule).</summary>
public sealed record CaseCategoryAdminResponse(
    long Id,
    string GroupName,
    string Name,
    string? Description,
    bool IsEmergency,
    CasePriority DefaultPriority,
    CostBearer DefaultCostBearer,
    bool PhotoRequired,
    int UsedBy);

public sealed record CaseCategoryRequest(
    string GroupName,
    string Name,
    string? Description,
    bool IsEmergency,
    CasePriority DefaultPriority,
    CostBearer DefaultCostBearer,
    bool PhotoRequired);

/// <summary>ERD: AssetCategory(Code, CategoryName, Description).</summary>
public sealed record AssetCategoryResponse(long Id, string Code, string CategoryName, string? Description, int UsedBy);

public sealed record AssetCategoryRequest(string Code, string CategoryName, string? Description);

/// <summary>ERD: AssetType(CategoryID, Code, Name, Description).</summary>
public sealed record AssetTypeResponse(
    long Id, long CategoryId, string CategoryName, string Code, string Name, string? Description, int UsedBy);

public sealed record AssetTypeRequest(long CategoryId, string Code, string Name, string? Description);

/// <summary>ERD: Manufacturers(Name, PhoneNumber, Email).</summary>
public sealed record ManufacturerResponse(long Id, string Name, string? PhoneNumber, string? Email, int UsedBy);

public sealed record ManufacturerRequest(string Name, string? PhoneNumber, string? Email);

/// <summary>ERD: Specialties(Name).</summary>
public sealed record SpecialtyResponse(long Id, string Name, int UsedBy);

public sealed record SpecialtyRequest(string Name);
