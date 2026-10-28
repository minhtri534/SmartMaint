namespace SmartMaint.Api.Entities.Users;

/// <summary>
/// The 5 fixed roles (D1). Stored as snake_case text with a CHECK constraint: customer, staff, ...
/// </summary>
public enum UserRole
{
    Customer,
    Staff,
    Technician,
    Manager,
    Admin,
}
