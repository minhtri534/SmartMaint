namespace SmartMaint.Api.Entities.Users;

/// <summary>
/// The 5 fixed roles (D1). Stored as snake_case text with a CHECK constraint: customer, officer, ...
/// </summary>
public enum UserRole
{
    Customer,
    Officer,
    Technician,
    Manager,
    Admin,
}
